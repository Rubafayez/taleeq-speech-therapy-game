using UnityEngine;
using TMPro;

public class PasswordVisibility : MonoBehaviour
{
    public TMP_InputField passwordField;
    private bool isVisible = false; 
    public void TogglePasswordVisibility()
    {
        isVisible = !isVisible; 
        if (isVisible)
        {
            passwordField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordField.contentType = TMP_InputField.ContentType.Password;
        }
        
        passwordField.ForceLabelUpdate(); 
    }
}