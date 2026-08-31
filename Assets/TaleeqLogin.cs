using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using System.Collections;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Firebase;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class TaleeqLogin : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;
    public TMP_Text passwordStatusText; // تكست مستقل لرسالة شروط كلمة المرور

    [Header("Outlines (اختياري)")]
    public Outline emailOutline;     // الـ Outline حول حقل الإيميل
    public Outline passwordOutline;  // الـ Outline حول حقل كلمة المرور

    private FirebaseAuth auth;
    private bool isClearingPassword = false; // لمنع إخفاء الرسالة عند مسح الباسورد بالكود

    // ألوان رسمية
    private static readonly Color32 COLOR_ERROR = new Color32(220, 53, 69, 255);   // أحمر
    private static readonly Color32 COLOR_OK    = new Color32(0, 140, 70, 255);    // أخضر غامق
    private static readonly Color32 COLOR_INFO  = new Color32(255, 255, 255, 255); // أبيض
    private static readonly Color32 COLOR_WARN  = new Color32(255, 193, 7, 255);  // أصفر

    void Start()
    {
        DisableStatusRaycast();

        // إخفاء الـ Outlines في البداية
        ClearOutlines();

        // تشخيص — تأكد من الربط
        Debug.Log($"📋 emailOutline = {(emailOutline != null ? emailOutline.name : "NULL")}");
        Debug.Log($"📋 passwordOutline = {(passwordOutline != null ? passwordOutline.name : "NULL")}");

        if (statusText != null)
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
        if (passwordStatusText != null)
        {
            passwordStatusText.text = "";
            passwordStatusText.gameObject.SetActive(false);
        }

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("Firebase initialized successfully.");
                }
                else
                {
                    Debug.LogError("Firebase init failed: " + task.Result);
                    ShowStatus("تعذر الاتصال بالخدمة. يرجى المحاولة لاحقاً.", COLOR_ERROR);
                }
            });

        // ✅ إخفاء اللون الأحمر والرسالة بمجرد ما يبدأ يكتب
        if (emailInput != null)
        {
            emailInput.onValueChanged.AddListener(delegate {
                if (!isClearingPassword)
                {
                    ClearOutlines();
                    HideStatus();
                }
            });
        }
        
        if (passwordInput != null)
        {
            passwordInput.onValueChanged.AddListener(delegate {
                if (!isClearingPassword)
                {
                    ClearOutlines();
                    HideStatus();
                }
            });
        }
    }

    // ✅ زر تسجيل الدخول
    public void LoginUser()
    {
        Debug.Log("LoginUser method called!");

        if (emailInput == null || passwordInput == null || statusText == null)
        {
            Debug.LogError("UI References not assigned in Inspector.");
            return;
        }

        DisableStatusRaycast();

        if (auth == null)
        {
            ShowStatus("جارٍ تهيئة الخدمة. يرجى الانتظار...", COLOR_INFO);
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // ✅ تحقق من الحقول الفارغة
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("يرجى تعبئة جميع البيانات", COLOR_ERROR);
            SetOutline(emailOutline,    string.IsNullOrEmpty(email));
            SetOutline(passwordOutline, string.IsNullOrEmpty(password));
            if (string.IsNullOrEmpty(email)) emailInput.ActivateInputField();
            else passwordInput.ActivateInputField();
            return;
        }

        // ✅ تحقق client-side من الفورمات والشروط
        bool emailOk    = IsValidEmailFormat(email);
        bool passwordOk = IsValidPassword(password);

        if (!emailOk || !passwordOk)
        {
            isClearingPassword = true;
            passwordInput.text = "";
            isClearingPassword = false;

            if (!emailOk && !passwordOk)
            {
                // كلاهم خطأ
                ShowStatus("البريد الإلكتروني أو كلمة المرور غير صحيحة", COLOR_ERROR);
                SetOutline(emailOutline, true);
                SetOutline(passwordOutline, true);
            }
            else if (!emailOk)
            {
                ShowStatus("البريد الإلكتروني غير صحيح", COLOR_ERROR);
                SetOutline(emailOutline, true);
                SetOutline(passwordOutline, false);
                emailInput.ActivateInputField();
            }
            else
            {
                ShowPasswordStatus("يجب أن تحتوي كلمة المرور على حرف كبير وصغير\nورمز وأرقام ولا تقل عن 8 أحرف ولا تزيد عن 20", COLOR_ERROR);
                SetOutline(emailOutline, false);
                SetOutline(passwordOutline, true);
                passwordInput.ActivateInputField();
            }
            return;
        }

        ClearOutlines();
        ShowStatus("جارٍ تسجيل الدخول...", COLOR_INFO);

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                StartCoroutine(ProcessLoginResult(task));
            });
    }

    private IEnumerator ProcessLoginResult(Task<AuthResult> task)
    {
        yield return null;

        if (task.IsFaulted || task.IsCanceled)
        {
            string msg = "تعذر تسجيل الدخول. يرجى التحقق من البيانات.";

            var baseEx = task.Exception?.Flatten()?.InnerExceptions?[0];
            Debug.Log("AUTH ERROR => " + baseEx);

            if (baseEx is FirebaseException fe)
            {
                AuthError code = (AuthError)fe.ErrorCode;
                Debug.Log("AuthError => " + code);

                switch (code)
                {
                    case AuthError.NetworkRequestFailed:
                        msg = "تعذر الاتصال بالخادم يرجى التحقق من اتصال الإنترنت";
                        ClearOutlines();
                        break;

                    case AuthError.TooManyRequests:
                        msg = "تم تجاوز عدد المحاولات يرجى المحاولة لاحقاً";
                        ClearOutlines();
                        break;

                    default:
                        // WrongPassword / UserNotFound / InvalidEmail / InvalidCredential
                        msg = "البريد الإلكتروني أو كلمة المرور غير صحيحة";
                        SetOutline(emailOutline, true);
                        SetOutline(passwordOutline, true);
                        break;
                }
            }

            ShowStatus(msg, COLOR_ERROR);

            // ✅ يرجع يقدر يكتب مباشرة
            isClearingPassword = true;
            passwordInput.text = "";
            isClearingPassword = false;
            passwordInput.ActivateInputField();

            yield break;
        }

        ClearOutlines();

        // ✅ بعد تسجيل الدخول الناجح → صفحة "من أنت"
        SceneManager.LoadScene("WhoUare");
    }

    // ✅ زر "سجل الآن" → الانتقال لصفحة التسجيل
    public void GoToRegister()
    {
        SceneManager.LoadScene("Register 2");
    }

    // ✅ زر "إعادة إنشاء كلمة المرور"
    public void ResetPassword()
    {
        Debug.Log("ResetPassword method called!");
        
        if (emailInput == null || statusText == null)
        {
            Debug.LogError($"emailInput is {(emailInput == null ? "NULL" : "OK")} / statusText is {(statusText == null ? "NULL" : "OK")}");
            return;
        }

        DisableStatusRaycast();

        if (auth == null)
        {
            Debug.LogWarning("auth is NULL! Cannot send reset email.");
            ShowStatus("جارٍ تهيئة الخدمة. يرجى الانتظار...", COLOR_INFO);
            return;
        }

        string email = emailInput.text.Trim();
        Debug.Log($"Entered Email for Reset: '{email}'");

        // ✅ لو ما كتب إيميل
        if (string.IsNullOrEmpty(email))
        {
            ShowStatus("يرجى إدخال البريد الإلكتروني أولاً.", COLOR_ERROR);
            SetOutline(emailOutline, true);
            SetOutline(passwordOutline, false);
            emailInput.ActivateInputField();
            return;
        }

        ClearOutlines();

        ShowStatus("جارٍ إرسال رابط إعادة التعيين...", COLOR_INFO);

        auth.SendPasswordResetEmailAsync(email)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    ShowStatus("تم إرسال رابط التعيين بنجاح\nيرجى التحقق من بريدك الإلكتروني", COLOR_OK);
                }
                else
                {
                    string msg = "تعذر إرسال الرابط. يرجى المحاولة لاحقاً.";

                    var baseEx = task.Exception?.Flatten()?.InnerExceptions?[0];
                    Debug.Log("RESET ERROR => " + baseEx);

                    if (baseEx is FirebaseException fe)
                    {
                        AuthError code = (AuthError)fe.ErrorCode;
                        Debug.Log("Reset AuthError => " + code);

                        switch (code)
                        {
                            case AuthError.InvalidEmail:
                                msg = "يرجى إدخال بريد إلكتروني صحيح";
                                break;

                            case AuthError.UserNotFound:
                                msg = "لا يوجد حساب مرتبط بهذا البريد الإلكتروني";
                                break;

                            case AuthError.NetworkRequestFailed:
                                msg = "تعذر الاتصال بالخادم يرجى التحقق من اتصال الإنترنت.";
                                break;

                            case AuthError.TooManyRequests:
                                msg = "تم تجاوز عدد المحاولات يرجى المحاولة لاحقاً.";
                                break;

                            default:
                                msg = "تعذر إرسال الرابط يرجى المحاولة لاحقاً.";
                                break;
                        }
                    }

                    ShowStatus(msg, COLOR_ERROR);
                }
            });
    }

    // ─── تحقق من الفورمات ────────────────────────────────────────
    private bool IsValidEmailFormat(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length < 8 || password.Length > 20) return false;
        bool hasUpper  = Regex.IsMatch(password, @"[A-Z]");
        bool hasLower  = Regex.IsMatch(password, @"[a-z]");
        bool hasDigit  = Regex.IsMatch(password, @"[0-9]");
        bool hasSymbol = Regex.IsMatch(password, @"[^A-Za-z0-9]");
        return hasUpper && hasLower && hasDigit && hasSymbol;
    }

    // ─── مساعدات الـ Outline ────────────────────────────────────
    private void SetOutline(Outline outline, bool visible)
    {
        if (outline != null)
        {
            outline.enabled = visible;
            Debug.Log($"🔴 SetOutline: {outline.gameObject.name} → {visible} | enabled={outline.enabled}");
        }
        else
        {
            Debug.LogWarning("⚠️ SetOutline: outline is NULL!");
        }
    }

    private void ClearOutlines()
    {
        SetOutline(emailOutline,    false);
        SetOutline(passwordOutline, false);
    }

    // (اختياري) أخفي الرسالة
    public void HideStatus()
    {
        if (statusText != null) statusText.gameObject.SetActive(false);
        if (passwordStatusText != null) passwordStatusText.gameObject.SetActive(false);
    }

    private void ShowStatus(string message, Color color)
    {
        Debug.Log("STATUS => " + message);

        if (passwordStatusText != null) passwordStatusText.gameObject.SetActive(false);

        if (statusText == null) return;

        statusText.gameObject.SetActive(true);

        // ✅ عربي مضبوط + خليه false عشان الأرقام تظل إنجليزية
        statusText.text = ArabicFixer.Fix(message, false, false);

        statusText.color = color;

        // ✅ مع ArabicFixer خليه false
        statusText.isRightToLeftText = false;

        statusText.alignment = TextAlignmentOptions.Center;
        statusText.ForceMeshUpdate();

        DisableStatusRaycast();
    }

    private void ShowPasswordStatus(string message, Color color)
    {
        Debug.Log("PASSWORD STATUS => " + message);

        if (statusText != null) statusText.gameObject.SetActive(false);

        if (passwordStatusText == null) return;

        passwordStatusText.gameObject.SetActive(true);

        // ✅ عربي مضبوط + خليه false عشان الأرقام تظل إنجليزية
        passwordStatusText.text = ArabicFixer.Fix(message, false, false);

        passwordStatusText.color = color;

        // ✅ مع ArabicFixer خليه false
        passwordStatusText.isRightToLeftText = false;

        passwordStatusText.alignment = TextAlignmentOptions.Center;
        passwordStatusText.ForceMeshUpdate();

        DisableStatusRaycast();
    }

    private void DisableStatusRaycast()
    {
        if (statusText != null && statusText as TextMeshProUGUI != null)
        {
            ((TextMeshProUGUI)statusText).raycastTarget = false;
        }

        if (passwordStatusText != null && passwordStatusText as TextMeshProUGUI != null)
        {
            ((TextMeshProUGUI)passwordStatusText).raycastTarget = false;
        }
    }
}