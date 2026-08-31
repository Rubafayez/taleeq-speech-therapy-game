using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

/// <summary>
/// مدير الأفاتار — يحفظ في Realtime Database
/// </summary>
public class AvatarManager : MonoBehaviour
{
    [Header("UI References")]
    public Image childProfileImage;  // صورة الطفل اللي راح تتغير
    public Sprite[] allAvatars;      // الـ 8 صور بالترتيب

    // Firebase
    private DatabaseReference db => FirebaseInitializer.DB;
    private FirebaseAuth auth;

    // معلومات الطفل الحالي
    [HideInInspector]
    public string currentChildID;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// اربطها بأزرار الأفاتار الـ 8 في On Click
    /// </summary>
    public void SelectAndSaveAvatar(int avatarID)
    {
        if (string.IsNullOrEmpty(currentChildID))
        {
            Debug.LogError("❌ لم يتم تحديد الطفل! استخدم SetCurrentChild() أولاً");
            return;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogError("❌ المستخدم غير مسجل دخول!");
            return;
        }

        string parentUid = auth.CurrentUser.UserId;

        Debug.Log($"⏳ جاري حفظ الأفاتار {avatarID} للطفل {currentChildID}...");

        // حفظ في Realtime Database
        db.Child("parents").Child(parentUid)
          .Child("children").Child(currentChildID)
          .Child("avatarID").SetValueAsync(avatarID)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted && !task.IsFaulted)
              {
                  Debug.Log($"✅ تم حفظ الأفاتار {avatarID} في Realtime Database!");

                  // تغيير الصورة في الواجهة فوراً
                  if (childProfileImage != null && allAvatars != null &&
                      avatarID >= 1 && avatarID <= allAvatars.Length)
                  {
                      childProfileImage.sprite = allAvatars[avatarID - 1];
                      Debug.Log("✅ تم تحديث الصورة في الواجهة!");
                  }

                  // إغلاق النافذة
                  this.gameObject.SetActive(false);
              }
              else if (task.IsFaulted)
              {
                  Debug.LogError($"❌ فشل حفظ الأفاتار: {task.Exception}");
              }
          });
    }

    /// <summary>
    /// يحدد الطفل الحالي قبل فتح البوب أب
    /// </summary>
    public void SetCurrentChild(string childID, Image childImage)
    {
        currentChildID = childID;
        childProfileImage = childImage;

        // فتح البوب أب
        this.gameObject.SetActive(true);

        Debug.Log($"✅ تم تحديد الطفل: {childID}");
    }
}