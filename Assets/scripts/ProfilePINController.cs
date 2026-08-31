using UnityEngine;
using UnityEngine.SceneManagement; // ضروري للانتقال بين الصفحات

public class ProfilePINController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("اكتب هنا اسم مشهد تأكيد الهوية بدقة")]
    [SerializeField] private string confirmIdentityScene = "Confirm identity in PIN change";

    // هذه الدالة تربطها بزر "تعديل الرمز"
    public void OnEditButtonClicked()
    {
        // 1. نحدد أن العملية هي "تعديل"
        // نستخدم المتغير الموجود في السكربت الآخر (VerifySecurityAnswer)
        VerifySecurityAnswer.CurrentOperation = VerifySecurityAnswer.OperationType.Edit;

        // 2. ننتقل لصفحة تأكيد الهوية
        Debug.Log("تم اختيار: تعديل الرمز");
        SceneManager.LoadScene(confirmIdentityScene);
    }

    // هذه الدالة تربطها بزر "حذف القفل"
    public void OnDeleteButtonClicked()
    {
        // 1. نحدد أن العملية هي "حذف"
        VerifySecurityAnswer.CurrentOperation = VerifySecurityAnswer.OperationType.Delete;

        // 2. ننتقل لصفحة تأكيد الهوية
        Debug.Log("تم اختيار: حذف القفل");
        SceneManager.LoadScene(confirmIdentityScene);
    }
}