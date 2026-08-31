using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class PinSetupManager : MonoBehaviour
{
    [Header("1. PIN Inputs (4 Fields)")]
    public TMP_InputField[] pinInputs;
    public Outline[] pinOutlines; // إطارات حقول الـ PIN الحمراء

    [Header("2. Security Question")]
    public TMP_Dropdown securityQuestionDropdown;
    public Outline dropdownOutline;

    [Header("3. Answer Input")]
    public TMP_InputField answerInput;
    public Outline answerOutline;

    [Header("4. Error Feedback")]
    public TextMeshProUGUI errorMessageText;

    [Header("5. Success Popup")]
    public GameObject successPopup;

    [Header("6. Scene Navigation")]
    public string nextSceneName = "Scenes/WhoUare"; // احتياطي
    public int nextSceneIndex = 0; // WhoUare = 0 في Build Profiles

    // Realtime Database
    private DatabaseReference db => FirebaseInitializer.DB;
    private string currentParentID;

    void Start()
    {
        if (successPopup != null) successPopup.SetActive(false);
        if (errorMessageText != null) errorMessageText.gameObject.SetActive(false);

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            currentParentID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // إخفاء الخطأ بمجرد أن يبدأ المستخدم بالكتابة
        foreach (var input in pinInputs)
            if (input != null)
                input.onValueChanged.AddListener((val) => HideError());

        if (securityQuestionDropdown != null)
            securityQuestionDropdown.onValueChanged.AddListener((val) => HideError());

        if (answerInput != null)
            answerInput.onValueChanged.AddListener((val) => HideError());
    }

    // =========================================================
    // زر "تفعيل القفل" → يتحقق أولاً ثم يحفظ
    // =========================================================
    public void OnActivateClick()
    {
        // --- خطوة 1: التحقق من الحقول ---
        bool hasError = false;
        ResetOutlines();

        string fullPin = "";
        bool pinIncomplete = false;

        foreach (var input in pinInputs)
        {
            if (input != null)
            {
                fullPin += input.text;
                if (string.IsNullOrEmpty(input.text))
                    pinIncomplete = true;
            }
        }

        if (pinIncomplete || fullPin.Length < 4)
        {
            hasError = true;
            foreach (var o in pinOutlines) if (o != null) o.enabled = true;
        }

        if (securityQuestionDropdown != null && securityQuestionDropdown.value == 0)
        {
            hasError = true;
            if (dropdownOutline != null) dropdownOutline.enabled = true;
        }

        if (answerInput == null || string.IsNullOrWhiteSpace(answerInput.text))
        {
            hasError = true;
            if (answerOutline != null) answerOutline.enabled = true;
        }

        if (hasError)
        {
            ShowError("ﺕﺎﻧﺎﻴﺒﻟﺍ ﻊﻴﻤﺟ ﺔﺌﺒﻌﺗ ﻰﺟﺮﻳ"); //يرجى تعبئة جميع البيانات
            return;
        }

        // --- خطوة 2: الحفظ في Firebase ---
        SavePin(fullPin);
    }

    // =========================================================
    // زر "تأكيد" داخل الـ Popup → ينتقل لـ WhoUare
    // =========================================================
    public void OnConfirmSuccessPopup()
    {
        if (successPopup != null) successPopup.SetActive(false);
        Debug.Log("الانتقال للـ Scene index: " + nextSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }

    // اسم بديل
    public void OnConfirm() { OnConfirmSuccessPopup(); }
    public void OnConfirmButtonPressed() { OnConfirmSuccessPopup(); }

    // =========================================================
    // زر "لاحقاً" → ينتقل مباشرة لـ WhoUare بدون حفظ
    // =========================================================
    public void OnSkipButtonPressed()
    {
        Debug.Log("لاحقاً - الانتقال للـ Scene index: " + nextSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }

    // =========================================================
    // حفظ البيانات في Firebase Realtime Database
    // =========================================================
    void SavePin(string pin)
    {
        if (string.IsNullOrEmpty(currentParentID))
        {
            Debug.LogError("المستخدم غير مسجل دخول!");
            return;
        }

        string selectedQuestion = securityQuestionDropdown.options[securityQuestionDropdown.value].text;

        var pinData = new Dictionary<string, object>
        {
            { "parentID",         currentParentID },
            { "pinCode",          pin },
            { "securityQuestion", selectedQuestion },
            { "securityAnswer",   answerInput.text },
            { "createdAt",        ServerValue.Timestamp }
        };

        db.Child("PIN").Child(currentParentID).SetValueAsync(pinData)
          .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("تم حفظ الرمز بنجاح.");

                db.Child("parents").Child(currentParentID).Child("isLocked")
                  .SetValueAsync(true).ContinueWithOnMainThread(task2 =>
                {
                    if (task2.IsCompleted && !task2.IsFaulted)
                    {
                        Debug.Log("تم تحديث البروفايل.");
                        if (successPopup != null)
                            successPopup.SetActive(true);
                    }
                    else
                        Debug.LogError("فشل تحديث isLocked: " + task2.Exception);
                });
            }
            else
                Debug.LogError("فشل الحفظ: " + task.Exception);
        });
    }

    // =========================================================
    // دوال مساعدة
    // =========================================================
    void ShowError(string msg)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = msg;
            errorMessageText.gameObject.SetActive(true);
        }
    }

    void HideError()
    {
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);
        ResetOutlines();
    }

    void ResetOutlines()
    {
        if (pinOutlines != null)
            foreach (var o in pinOutlines)
                if (o != null) o.enabled = false;

        if (dropdownOutline != null) dropdownOutline.enabled = false;
        if (answerOutline != null) answerOutline.enabled = false;
    }
}