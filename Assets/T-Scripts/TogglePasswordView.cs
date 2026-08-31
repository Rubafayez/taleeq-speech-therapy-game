using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TogglePasswordView : MonoBehaviour
{
    public TMP_InputField passwordField;
    public Image eyeImage;

    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    private bool isVisible = false;

    public void Toggle()
    {
        isVisible = !isVisible;

        if (isVisible)
        {
            passwordField.contentType = TMP_InputField.ContentType.Standard;
            eyeImage.sprite = eyeOpenSprite;
        }
        else
        {
            passwordField.contentType = TMP_InputField.ContentType.Password;
            eyeImage.sprite = eyeClosedSprite;
        }

        passwordField.ForceLabelUpdate();
    }
}
