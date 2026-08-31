using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// يُوقف iPad Keyboard Assistant Bar لمنع كراش bad_weak_ptr مع Firebase
/// ضعيه على أي GameObject في كل scene فيه InputField
/// </summary>
public class KeyboardAssistantDisabler : MonoBehaviour
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _DisableKeyboardAssistantBar();
#endif

    private void Start()
    {
        DisableAssistantBar();
    }

    private void OnEnable()
    {
        // نستدعيه عند كل مرة يُفعَّل فيه هذا الـ GameObject
        DisableAssistantBar();

        // وأيضاً نراقب حدث ظهور الكيبورد
        TouchScreenKeyboard.hideInput = false; // نتركه يظهر لكن بدون شريط
    }

    public void DisableAssistantBar()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _DisableKeyboardAssistantBar();
        Debug.Log("[KeyboardFix] iPad keyboard assistant bar disabled.");
#endif
    }
}
