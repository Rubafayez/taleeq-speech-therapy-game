using UnityEngine;
using TMPro; // مكتبة النصوص اللي تستخدمينها

public class PINAutoMove : MonoBehaviour
{
    // مصفوفة نحط فيها الخانات الأربعة بالترتيب
    public TMP_InputField[] pinFields;

    void Start()
    {
        for (int i = 0; i < pinFields.Length; i++)
        {
            int index = i; // حفظ الترتيب الحالي
            
            // إضافة "مستمع" يشتغل كل ما تغير النص في الخانة
            pinFields[i].onValueChanged.AddListener((text) => 
            {
                // إذا كتب رقم (طول النص صار 1) ومو هو آخر مربع
                if (text.Length == 1 && index < pinFields.Length - 1)
                {
                    // رح للمربع اللي بعده وفعله
                    pinFields[index + 1].Select();
                    pinFields[index + 1].ActivateInputField(); 
                }
            });
        }
    }
}