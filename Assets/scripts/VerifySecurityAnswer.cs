using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class VerifySecurityAnswer : MonoBehaviour
{
    public enum OperationType { Edit, Delete }
    public static OperationType CurrentOperation = OperationType.Edit;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private TMP_Text questionText;   // ← اربطيه في Inspector لعرض السؤال
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Outline answerOutline;

    [Header("Edit Settings")]
    [SerializeField] private string editSceneName = "editPIN";

    [Header("Delete Settings (Popup)")]
    [SerializeField] private GameObject deleteConfirmPopup;
    [SerializeField] private Button popupYesButton;
    [SerializeField] private Button popupNoButton;
    [SerializeField] private string profileSceneName = "PIN in profile";

    // ✅ Realtime Database بدل Firestore
    private DatabaseReference db => FirebaseInitializer.DB;
    private string storedAnswer = "";

    void Start()
    {
        HideErrorFeedback();
        FetchStoredData();   // ← يجلب السؤال والإجابة معاً

        if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(false);
        if (confirmButton != null) confirmButton.onClick.AddListener(CheckAnswer);
        if (popupYesButton != null) popupYesButton.onClick.AddListener(DeletePINFromDatabase);
        if (popupNoButton != null) popupNoButton.onClick.AddListener(ClosePopup);
        if (answerInput != null) answerInput.onValueChanged.AddListener((val) => HideErrorFeedback());
    }

    void FetchStoredData()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        // ✅ نجلب كل بيانات PIN مرة واحدة (السؤال + الإجابة)
        db.Child("PIN").Child(user.UserId)
          .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.Result == null || !task.Result.Exists) return;

            // عرض السؤال في الـ UI
            if (task.Result.HasChild("securityQuestion"))
            {
                string question = task.Result.Child("securityQuestion").Value?.ToString() ?? "";
                if (questionText != null) questionText.text = question;
            }

            // تخزين الإجابة للمقارنة لاحقاً
            if (task.Result.HasChild("securityAnswer"))
                storedAnswer = task.Result.Child("securityAnswer").Value?.ToString() ?? "";
        });
    }

    void CheckAnswer()
    {
        string userAnswer = answerInput.text.Trim();

        if (string.IsNullOrEmpty(userAnswer))
        { ShowError("ﺔﺤﻴﺤﺻ ﺔﺑﺎﺟﺇ ﻝﺎﺧﺩﺇ ﺀﺎﺟﺮﻟﺍ"); if (answerOutline != null) answerOutline.enabled = true; return; }
//الرجاء إدخال إجابة صحيحة
        if (string.IsNullOrEmpty(storedAnswer))
        { ShowError("جاري تحميل البيانات، حاول مرة أخرى..."); FetchStoredData(); return; }

        if (string.Compare(userAnswer, storedAnswer, System.StringComparison.OrdinalIgnoreCase) == 0)
        {
            if (CurrentOperation == OperationType.Edit)
                SceneManager.LoadScene(editSceneName);
            else if (CurrentOperation == OperationType.Delete)
                if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(true);
        }
        else
        {
            ShowError("الرجاء إدخال إجابة صحيحة");
            if (answerOutline != null) answerOutline.enabled = true;
        }
    }

    // ✅ Realtime Database - حذف PIN وفتح القفل
    void DeletePINFromDatabase()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        string userId = user.UserId;

        // فتح القفل
        db.Child("parents").Child(userId).Child("isLocked")
          .SetValueAsync(false).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("تم فتح القفل (isLocked = false)");
                // حذف بيانات PIN
                db.Child("PIN").Child(userId).RemoveValueAsync();
                SceneManager.LoadScene(profileSceneName);
            }
            else
                Debug.LogError("فشل في فتح القفل: " + task.Exception);
        });
    }

    void ClosePopup() { if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(false); }
    void ShowError(string msg) { if (errorText != null) { errorText.text = msg; errorText.gameObject.SetActive(true); } }
    void HideErrorFeedback() { if (errorText != null) errorText.gameObject.SetActive(false); if (answerOutline != null) answerOutline.enabled = false; }
}