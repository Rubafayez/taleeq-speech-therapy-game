using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FamilyDashboardManager : MonoBehaviour
{
    private DatabaseReference db => FirebaseInitializer.DB;
    private FirebaseAuth auth;

    private static string selectedChildID;
    private static Image selectedChildImage;

    private Dictionary<GameObject, string> childProfiles = new Dictionary<GameObject, string>();
    private Dictionary<GameObject, Image> childImages    = new Dictionary<GameObject, Image>();

    [Header("UI References")]
    public TextMeshProUGUI parentNameText;
    public GameObject childProfilePrefab;
    public Transform  profilesContainer;
    public Sprite[]   avatarSprites;
    public GameObject avatarSelectionPopup;

    [Header("Parent Profile")]
    public GameObject lockIcon;            // أيقونة القفل — تظهر فقط إذا isLocked = true
    public GameObject pinVerificationPopup; // نافذة إدخال الـ PIN
    public TMP_InputField[] pinInputs;
    public GameObject pinErrorImage;
    private string correctPin;

    // ═══════════════════════════════════════════════════════════════
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        // إخفاء القفل والـ Popup في البداية
        if (lockIcon != null) lockIcon.SetActive(false);
        if (pinVerificationPopup != null) pinVerificationPopup.SetActive(false);
        if (pinErrorImage != null) pinErrorImage.SetActive(false);

        if (auth.CurrentUser != null)
        {
            LoadParentInfo();
            LoadLockStatus();
            LoadChildren();
        }

        SetupPinNavigation();
    }

    // ═══════════════════════════════════════════════════════════════
    // 1. تحميل اسم الأب
    // ═══════════════════════════════════════════════════════════════
    void LoadParentInfo()
    {
        string uid = auth.CurrentUser.UserId;
        db.Child("parents").Child(uid).Child("name").GetValueAsync()
          .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null && task.Result.Value != null)
            {
                if (parentNameText != null)
                    parentNameText.text = task.Result.Value.ToString();
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // 2. التحقق من isLocked وإظهار أيقونة القفل
    // ═══════════════════════════════════════════════════════════════
    void LoadLockStatus()
    {
        string uid = auth.CurrentUser.UserId;
        db.Child("parents").Child(uid).Child("isLocked").GetValueAsync()
          .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null && task.Result.Value != null)
            {
                bool isLocked = (bool)task.Result.Value;
                Debug.Log($"🔒 isLocked = {isLocked}");

                // أظهر أيقونة القفل فقط إذا الحساب مقفل
                if (lockIcon != null)
                    lockIcon.SetActive(isLocked);
            }
            else
            {
                // لو ما في بيانات = ما في قفل
                if (lockIcon != null)
                    lockIcon.SetActive(false);
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // 3. تحميل الأطفال
    // ═══════════════════════════════════════════════════════════════
    void LoadChildren()
    {
        string uid = auth.CurrentUser.UserId;
        db.Child("parents").Child(uid).Child("children").GetValueAsync()
          .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted || task.Result == null || !task.Result.HasChildren) return;

            foreach (Transform t in profilesContainer) Destroy(t.gameObject);
            childProfiles.Clear();
            childImages.Clear();

            foreach (DataSnapshot child in task.Result.Children)
            {
                string cName    = child.Child("name").Value?.ToString() ?? "؟";
                int    avatarID = child.Child("avatarID").Value != null
                                  ? int.Parse(child.Child("avatarID").Value.ToString()) : 8;
                string cID      = child.Key;
                CreateChildUI(cName, avatarID, cID);
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // بناء واجهة الطفل
    // ═══════════════════════════════════════════════════════════════
    void CreateChildUI(string name, int avatarIndex, string childId)
    {
        GameObject newChild = Instantiate(childProfilePrefab, profilesContainer);

        TextMeshProUGUI nameText = newChild.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null) nameText.text = name;

        Image childImage = null;
        Image[] images = newChild.GetComponentsInChildren<Image>();
        if (images.Length > 1) childImage = images[1];
        else if (images.Length > 0) childImage = images[0];

        if (childImage != null && avatarIndex >= 1 && avatarIndex <= avatarSprites.Length)
            childImage.sprite = avatarSprites[avatarIndex - 1];

        childProfiles[newChild] = childId;
        childImages[newChild]   = childImage;

        // ─── زر الطفل: الضغط على البروفايل → الخريطة ───────────────
        Button profileBtn = newChild.GetComponent<Button>() ?? newChild.AddComponent<Button>();
        profileBtn.onClick.RemoveAllListeners();
        string capturedChildId = childId;
        profileBtn.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString("selectedChildID", capturedChildId);
            PlayerPrefs.Save();
            Debug.Log("👦 الطفل المختار: " + capturedChildId + " → الانتقال لصفحة map");
            SceneManager.LoadScene("map");
        });

        // ربط EditIcon (يوقف انتشار الضغط لمنع تشغيل زر البروفايل)
        Transform editIcon = FindEditIcon(newChild.transform);
        if (editIcon != null)
        {
            Button editBtn = editIcon.GetComponent<Button>() ?? editIcon.gameObject.AddComponent<Button>();
            Image  editImg = editIcon.GetComponent<Image>();
            if (editImg != null) editImg.raycastTarget = true;

            editBtn.onClick.RemoveAllListeners();
            editBtn.onClick.AddListener(() =>
            {
                SelectChild(newChild);
                if (avatarSelectionPopup != null) avatarSelectionPopup.SetActive(true);
            });
            editIcon.SetAsLastSibling();
        }
    }

    Transform FindEditIcon(Transform parent)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
            if (t.name == "EditIcon" || t.name == "Editicon") return t;
        return null;
    }

    public void SelectChild(GameObject childProfile)
    {
        if (childProfile == null) return;
        if (childProfiles.ContainsKey(childProfile)) selectedChildID = childProfiles[childProfile];
        if (childImages.ContainsKey(childProfile)) selectedChildImage = childImages[childProfile];
    }

    // ═══════════════════════════════════════════════════════════════
    // حفظ الأفاتار
    // ═══════════════════════════════════════════════════════════════
    public void SaveNewAvatar(int avatarNumber)
    {
        if (string.IsNullOrEmpty(selectedChildID)) return;

        string uid          = auth.CurrentUser.UserId;
        string childIdLocal = selectedChildID;

        db.Child("parents").Child(uid).Child("children").Child(childIdLocal)
          .Child("avatarID").SetValueAsync(avatarNumber)
          .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted || task.IsFaulted) return;

            // تحديث الصورة في الواجهة
            foreach (var kvp in childProfiles)
            {
                if (kvp.Value == childIdLocal && childImages.ContainsKey(kvp.Key))
                {
                    Image img = childImages[kvp.Key];
                    if (img != null && avatarNumber >= 1 && avatarNumber <= avatarSprites.Length)
                        img.sprite = avatarSprites[avatarNumber - 1];
                    break;
                }
            }

            if (avatarSelectionPopup != null) avatarSelectionPopup.SetActive(false);
        });
    }

    public void ClosePopup()
    {
        if (avatarSelectionPopup != null) avatarSelectionPopup.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════
    // نظام PIN للأب — يُستدعى عند الضغط على بروفايل الأب
    // ═══════════════════════════════════════════════════════════════
    public void OnParentProfileClicked()
    {
        string uid = auth.CurrentUser.UserId;
        Debug.Log("⏳ جاري التحقق من isLocked للـ UID: " + uid);

        db.Child("parents").Child(uid).Child("isLocked").GetValueAsync()
          .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("❌ فشل قراءة isLocked: " + task.Exception?.Message);
                return; // لا تفعل شيئاً عند فشل الاتصال
            }

            bool isLocked = false;
            if (task.Result?.Value != null)
            {
                try   { isLocked = (bool)task.Result.Value; }
                catch { isLocked = false; }
            }

            Debug.Log($"🔒 isLocked = {isLocked}");

            if (isLocked)
            {
                // الحساب مقفل → اجلب الـ PIN وافتح نافذة التحقق
                db.Child("PIN").Child(uid).Child("pinCode").GetValueAsync()
                  .ContinueWithOnMainThread(pinTask =>
                {
                    if (pinTask.IsCompleted && pinTask.Result?.Value != null)
                    {
                        correctPin = pinTask.Result.Value.ToString();
                        if (pinVerificationPopup != null) pinVerificationPopup.SetActive(true);
                        if (pinErrorImage != null) pinErrorImage.SetActive(false);
                        ClearPinInputs();
                    }
                    else
                        Debug.LogError("❌ ما لقيت الـ PIN في Realtime Database");
                });
            }
            else
            {
                // الحساب غير مقفل → لا تفعل شيئاً
                Debug.Log("🔓 الحساب غير مقفل — لا يوجد إجراء");
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // التحقق من الـ PIN المُدخل
    // ═══════════════════════════════════════════════════════════════
    public void VerifyPin()
    {
        string enteredPin = "";
        foreach (var input in pinInputs)
            if (input != null) enteredPin += input.text;

        if (enteredPin == correctPin)
        {
            if (pinErrorImage != null) pinErrorImage.SetActive(false);
            if (pinVerificationPopup != null) pinVerificationPopup.SetActive(false);
            ClearPinInputs();
            Debug.Log("✅ PIN صحيح — تم الإغلاق");
        }
        else
        {
            if (pinErrorImage != null) pinErrorImage.SetActive(true);
            ClearPinInputs();
        }
    }

    void ClearPinInputs()
    {
        foreach (var input in pinInputs) if (input != null) input.text = "";
        if (pinInputs != null && pinInputs.Length > 0) pinInputs[0].ActivateInputField();
    }

    void SetupPinNavigation()
    {
        if (pinInputs == null) return;
        for (int i = 0; i < pinInputs.Length; i++)
        {
            int idx = i;
            if (pinInputs[idx] != null)
                pinInputs[idx].onValueChanged.AddListener(val =>
                {
                    if (val.Length > 0 && idx < pinInputs.Length - 1)
                        pinInputs[idx + 1].ActivateInputField();
                });
        }
    }



    public void ClosePinPopup()
    {
        if (pinVerificationPopup != null) pinVerificationPopup.SetActive(false);
        if (pinErrorImage != null) pinErrorImage.SetActive(false);
        ClearPinInputs();
    }
}