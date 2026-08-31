using UnityEngine;

public class RegistrationState : MonoBehaviour
{
    public static RegistrationState Instance;

    public string parentName;
    public string email;
    public string parentPassword;

    // ✅ alias عشان كودكم القديم
    public string parentEmail { get => email; set => email = value; }

    public bool IsParentStepValid = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Clear()
    {
        parentName = "";
        email = "";
        parentPassword = "";
        IsParentStepValid = false;
    }
}
