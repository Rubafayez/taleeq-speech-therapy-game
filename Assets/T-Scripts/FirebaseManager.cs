using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_Dropdown genderDropdown;

    DatabaseReference dbReference;

    void Start()
    {
        // ✅ نستخدم FirebaseInitializer المركزي بدل ما نبدأ Firebase من جديد
        StartCoroutine(WaitForFirebase());
    }

    private System.Collections.IEnumerator WaitForFirebase()
    {
        // ننتظر حتى FirebaseInitializer ينهي التهيئة
        float timeout = 10f;
        while (!FirebaseInitializer.IsReady && timeout > 0f)
        {
            timeout -= UnityEngine.Time.unscaledDeltaTime;
            yield return null;
        }

        if (!FirebaseInitializer.IsReady)
        {
            Debug.LogError("[FirebaseManager] Firebase not ready after timeout.");
            yield break;
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("[FirebaseManager] Ready using shared FirebaseInitializer.");
    }

    public void SaveUserData()
    {
        if (dbReference == null)
        {
            Debug.LogError("[FirebaseManager] DB not ready yet.");
            return;
        }

        string userName = nameInputField.text;
        string gender = genderDropdown.options[genderDropdown.value].text;

        dbReference.Child("users").Child(userName).Child("gender").SetValueAsync(gender);
        Debug.Log("تم حفظ بيانات " + userName + " بنجاح!");
    }
}