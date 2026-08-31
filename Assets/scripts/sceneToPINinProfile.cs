using UnityEngine;
using UnityEngine.SceneManagement; // ضروري عشان نقدر نغير المشهد

public class SceneControllerTester : MonoBehaviour
{
    // دالة الانتقال للمشهد الثاني
    public void GoToPinSceneinProfile()
    {
        // "PIN" هو اسم المشهد اللي في الصورة الثانية
        // تأكد أن الاسم مطابق تماماً لاسم الملف عندك
        SceneManager.LoadScene("PIN in profile");
    }
}