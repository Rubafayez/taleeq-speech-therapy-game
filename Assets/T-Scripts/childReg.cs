using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine.SceneManagement;

public class childReg : MonoBehaviour
{
    [Header("Child UI")]
    public TMP_InputField nameInput;
    public TMP_Dropdown   genderDropdown;

    // ═══════════════════════════════════════════════════════════════
    // Calander Btn
    // ═══════════════════════════════════════════════════════════════
    public TMP_Text       birthDateText;
    public int            age;

    // ═══════════════════════════════════════════════════════════════
    // gg
    // ═══════════════════════════════════════════════════════════════
    public TMP_Text       birthDateText_gg;
    public int            age_gg;

    [Header("Avatar Settings")]
    public int selectedAvatarID = 1;
    public int imageNum = 0;       // رقم الصورة المختارة (تستخدمه imageSystem)

    [Header("Edit Mode")]
    public string oldName = "";    // الاسم القديم عند التعديل (تستخدمه changeFileSystem)

    [Header("Optional UI")]
    public TMP_Text errorText;

    // ═══════════════════════════════════════════════════════════════
    // Outlines — تظهر حمراء لما يغلط اليوزر
    // ═══════════════════════════════════════════════════════════════
    [Header("Outlines (اختياري)")]
    public Outline nameOutline;       // حول حقل الاسم
    public Outline genderOutline;     // حول الـ Dropdown
    public Outline birthDateOutline;  // حول نص التاريخ

    [Header("Pop-up Settings")]
    public GameObject pinPopup;
    public string     nextSceneName = "Register 2"; // احتياط فقط

    // Realtime Database عبر الـ Singleton
    private DatabaseReference db   => FirebaseInitializer.DB;
    private FirebaseAuth      auth => FirebaseInitializer.Auth;

    // ═══════════════════════════════════════════════════════════════
    // دالة اختيار الأفاتار
    // ═══════════════════════════════════════════════════════════════
    public void OnAvatarSelected(int avatarID)
    {
        selectedAvatarID = avatarID;
        Debug.Log("تم اختيار الصورة رقم: " + avatarID);
    }

    private void Start()
    {
        // إخفاء الألوان الحمراء والرسالة بمجرد التعديل
        if (nameInput != null)
        {
            nameInput.onValueChanged.AddListener(delegate {
                ResetOutlines();
                HideError();
            });
        }

        if (genderDropdown != null)
        {
            genderDropdown.onValueChanged.AddListener(delegate {
                ResetOutlines();
                HideError();
            });
        }
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
            errorText.text = "";
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // زر "سجل"
    // ═══════════════════════════════════════════════════════════════
    public void SavePlayer()
    {
        ResetOutlines(); // أخفي أي أوتلاين من قبل

        if (!FirebaseInitializer.IsReady)
        {
            ShowError("Firebase ليس جاهزاً بعد. انتظري لحظة.");
            return;
        }

        if (RegistrationState.Instance == null || !RegistrationState.Instance.IsParentStepValid)
        {
            ShowError("بيانات إنشاء الحساب غير مكتملة. ارجعي للخطوة الأولى.");
            return;
        }

        string childName = nameInput  != null ? nameInput.text.Trim() : "";
        string gender    = (genderDropdown != null && genderDropdown.options.Count > 0)
                           ? genderDropdown.options[genderDropdown.value].text : "";
        string birthDate = birthDateText != null ? birthDateText.text.Trim() : "";

        // ─── فحص كل الحقول مرة وحدة ─────────────────────────────────
        bool hasError = false;

        if (string.IsNullOrEmpty(childName))
        {
            SetOutline(nameOutline, true);   // 🔴 اسم فاضي
            hasError = true;
        }

        // الجنس: إذا القيمة 0 يعني ما اتختار شي
        if (genderDropdown == null || genderDropdown.value == 0)
        {
            SetOutline(genderOutline, true); // 🔴 جنس ما اتختار
            hasError = true;
        }

        bool birthInvalid = age <= 0
                         || string.IsNullOrEmpty(birthDate)
                         || birthDate.Contains("DD")
                         || birthDate.Contains("MM");
        if (birthInvalid)
        {
            SetOutline(birthDateOutline, true); // 🔴 تاريخ غير مختار
            hasError = true;
        }

        if (hasError)
        {
            ShowError("يرجى تعبئة جميع البيانات");
            return;
        }
        // ─────────────────────────────────────────────────────────────

        CreateParentAuthThenSaveAll(childName, age, gender, birthDate);
    }

    // ═══════════════════════════════════════════════════════════════
    // إنشاء حساب الأب ثم الحفظ
    // ═══════════════════════════════════════════════════════════════
    private void CreateParentAuthThenSaveAll(string childName, int childAge, string gender, string birthDate)
    {
        string parentName = RegistrationState.Instance.parentName;
        string email      = RegistrationState.Instance.email;
        string password   = RegistrationState.Instance.parentPassword;

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                ShowError(GetAuthError(task.Exception));
                return;
            }

            string parentUid = task.Result.User.UserId;
            SaveToRealtimeDatabase(parentUid, parentName, email, childName, childAge, gender, birthDate);
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // الحفظ في Realtime Database
    // المسار: /parents/{uid}/  +  /parents/{uid}/children/{childId}/
    // ═══════════════════════════════════════════════════════════════
    private void SaveToRealtimeDatabase(string parentUid, string parentName, string email,
                                         string childName, int childAge, string gender, string birthDate)
    {
        // بيانات الأب
        var parentData = new Dictionary<string, object>
        {
            { "uid",       parentUid  },
            { "name",      parentName },
            { "email",     email      },
            { "isLocked",  false      }   // القفل مش مفعّل بالبداية
        };

        // بيانات الطفل
        string childId = db.Child("parents").Child(parentUid).Child("children").Push().Key;
        var childData = new Dictionary<string, object>
        {
            { "name",      childName        },
            { "age",       childAge         },
            { "gender",    gender           },
            { "birthDate", birthDate        },
            { "parentUid", parentUid        },
            { "avatarID",  selectedAvatarID },
            { "childId",   childId          }
        };

        // حفظ الأب أولاً
        db.Child("parents").Child(parentUid).SetValueAsync(parentData)
          .ContinueWithOnMainThread(parentTask =>
        {
            if (parentTask.IsFaulted || parentTask.IsCanceled)
            {
                ShowError("فشل حفظ بيانات الأب.");
                return;
            }

            // ثم حفظ الطفل
            db.Child("parents").Child(parentUid).Child("children").Child(childId).SetValueAsync(childData)
              .ContinueWithOnMainThread(childTask =>
            {
                if (childTask.IsFaulted || childTask.IsCanceled)
                {
                    ShowError("فشل حفظ بيانات الطفل.");
                    return;
                }

                Debug.Log("Parent + Child saved successfully!");
                RegistrationState.Instance.Clear();

                // إظهار الـ Popup أو الانتقال
                if (pinPopup != null)
                    pinPopup.SetActive(true);
                else
                    SceneManager.LoadScene(nextSceneName);
            });
        });
    }

    private void ShowError(string msg)
    {
        Debug.LogError(msg);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(true); // ← أظهر العنصر لو كان مخفياً
            
            // عشان نمنع الـ RTLTextMeshPro من مسح النص أو عكسه مرتين (نفس اللي كان يصير بـ TaleeqLogin بالضبط)
            var rtlText = errorText.GetComponent<RTLTMPro.RTLTextMeshPro>();
            if (rtlText != null)
            {
                // الـ RTLTextMeshPro إذا عطيناه النص العادي من خاصية Text بيصلحه بنفسه
                rtlText.text = msg;
                rtlText.UpdateText();
            }
            else
            {
                // حطينا ArabicFixer.Fix بحيث النص يترتب صح وما يتقطع للي ما عنده سكربت RTL
                errorText.text = ArabicFixer.Fix(msg, false, false);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // دالة لإخفاء خطأ التقويم (ربطها بزر التقويم في اليونيتي)
    // ═══════════════════════════════════════════════════════════════
    public void HideDateError()
    {
        SetOutline(birthDateOutline, false);
        HideError();
    }

    // ═══════════════════════════════════════════════════════════════
    // مساعدات الـ Outline
    // ═══════════════════════════════════════════════════════════════
    private void SetOutline(Outline outline, bool visible)
    {
        if (outline != null)
            outline.enabled = visible;
    }

    private void ResetOutlines()
    {
        SetOutline(nameOutline,      false);
        SetOutline(genderOutline,    false);
        SetOutline(birthDateOutline, false);
    }

    private string GetAuthError(System.AggregateException ex)
    {
        if (ex == null) return "Unknown error";
        foreach (var e in ex.InnerExceptions)
            if (e is Firebase.FirebaseException fe) return "Auth Error: " + fe.Message;
        return ex.Message;
    }
}