using UnityEngine;
using UnityEngine.SceneManagement; // ضروري عشان نقدر ننتقل بين المشاهد

public class PopupManager : MonoBehaviour
{
    // هذا المتغير هو "الصندوق" اللي بنحط فيه الـ Pop-up حقك
    public GameObject pinPopupWindow;

    // دالة 1: نربطها بزر "سجل"
    public void OnRegisterButtonClicked()
    {
        // هنا المفروض يكون كود التسجيل حقك (Firebase)
        // بعد ما يخلص تسجيل، نظهر النافذة:
        pinPopupWindow.SetActive(true);
    }

    // دالة 2: نربطها زر "تفعيل القفل"
    public void GoToPinScene()
    {
        // تأكد أن اسم المشهد عندك هو PIN كما هو مكتوب هنا
        SceneManager.LoadScene("PIN");
    }

    // دالة 3: نربطها بزر "لاحقاً" → تروح لصفحة WhoUare
    public void SkipAndGoHome()
    {
        pinPopupWindow.SetActive(false);
        // الانتقال لصفحة "من أنت؟" (WhoUare) - Index 0 في Build Profiles
        SceneManager.LoadScene(0);
    }
}