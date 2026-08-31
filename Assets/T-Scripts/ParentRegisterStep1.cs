using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParentRegisterStep1 : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField nameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("UI")]
    public TMP_Text errorText;

    // Email check
    private static readonly Regex EmailRegex =
        new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    // Password: >=8 + upper + lower + digit + special
    private static readonly Regex PasswordRegex =
        new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", RegexOptions.Compiled);

    public void OnClickNext()
    {
        if (errorText != null) errorText.text = "";

        string n = nameInput.text.Trim();
        string e = emailInput.text.Trim();
        string p = passwordInput.text;
        string c = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(n))
        { Show("اكتب الاسم"); return; }

        if (!EmailRegex.IsMatch(e))
        { Show("الإيميل غير صحيح. مثال: name@email.com"); return; }

        if (!PasswordRegex.IsMatch(p))
        { Show("كلمة السر لازم تكون 8+ وتحتوي: حرف كبير + حرف صغير + رقم + رمز."); return; }

        if (p != c)
        { Show("تأكيد كلمة السر لازم يطابق كلمة السر."); return; }

        // Save TEMP only
        RegistrationState.Instance.parentName = n;
        RegistrationState.Instance.parentEmail = e;
        RegistrationState.Instance.parentPassword = p;

        RegistrationState.Instance.IsParentStepValid = true;

        // Go to Step2 Scene
        SceneManager.LoadScene("ChildReg");
    }

    private void Show(string msg)
    {
        if (errorText != null) errorText.text = msg;
        else Debug.LogWarning(msg);
    }

}
