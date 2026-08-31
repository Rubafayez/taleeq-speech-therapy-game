using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // نستخدم FirebaseAuth من الـ Singleton المركزي
    private FirebaseAuth Auth => FirebaseInitializer.Auth;

    public void Register()
    {
        if (!FirebaseInitializer.IsReady)
        {
            Debug.LogError("Firebase not ready yet.");
            return;
        }

        string email    = emailInput.text;
        string password = passwordInput.text;

        Auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Register failed: " + task.Exception);
                    return;
                }
                Debug.Log("User created: " + task.Result.User.Email);
            });
    }
}
