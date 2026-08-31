using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UpdateUserPIN : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField[] pinInputs;
    [SerializeField] private Outline[] pinOutlines;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "PIN in profile";

    // ✅ Realtime Database بدل Firestore
    private DatabaseReference db => FirebaseInitializer.DB;
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        ResetUI();
        SetupPinInputs();

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ValidateAndUpdatePIN);
    }

    void SetupPinInputs()
    {
        if (pinInputs == null) return;
        for (int i = 0; i < pinInputs.Length; i++)
        {
            if (pinInputs[i] == null) continue;
            int index = i;
            pinInputs[i].onValueChanged.AddListener((val) =>
            {
                ResetUI();
                if (val.Length == 1 && index < pinInputs.Length - 1)
                {
                    pinInputs[index + 1].Select();
                    pinInputs[index + 1].ActivateInputField();
                }
            });
        }
    }

    void ResetUI()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }
        if (pinOutlines != null)
            foreach (var o in pinOutlines)
                if (o != null) o.enabled = false;
    }

    void ValidateAndUpdatePIN()
    {
        string newPin = "";
        bool hasEmpty = false;

        for (int i = 0; i < pinInputs.Length; i++)
        {
            var input   = pinInputs[i];
            var outline = (pinOutlines != null && i < pinOutlines.Length) ? pinOutlines[i] : null;

            if (string.IsNullOrEmpty(input.text))
            {
                hasEmpty = true;
                if (outline != null) outline.enabled = true;
            }
            else
            {
                if (outline != null) outline.enabled = false;
                newPin += input.text;
            }
        }

        if (hasEmpty || newPin.Length < 4)
        {
            if (feedbackText != null)
            {
                feedbackText.text = " ﺕﺎﻧﺎﻴﺒﻟﺍ ﻊﻴﻤﺟ ﺔﺌﺒﻌﺗ ﻰﺟﺮﻳ";//يرجى تعبئة جميع البيانات 
                feedbackText.gameObject.SetActive(true);
            }
            return;
        }

        UpdatePinInDB(newPin);
    }

    // ✅ Realtime Database بدل Firestore
    void UpdatePinInDB(string pinCode)
    {
        if (auth.CurrentUser == null) return;
        string userId = auth.CurrentUser.UserId;

        if (confirmButton != null) confirmButton.interactable = false;

        db.Child("PIN").Child(userId).Child("pinCode")
          .SetValueAsync(pinCode)
          .ContinueWithOnMainThread(task =>
        {
            if (confirmButton != null) confirmButton.interactable = true;

            if (task.IsFaulted)
            {
                Debug.LogError("Error: " + task.Exception);
                if (feedbackText != null)
                {
                    feedbackText.text = "حدث خطأ ما";
                    feedbackText.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.Log("PIN Updated Successfully");
                SceneManager.LoadScene(nextSceneName);
            }
        });
    }
}