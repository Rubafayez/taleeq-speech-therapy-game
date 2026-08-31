using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    public void GoToWhoUare()
    {
        SceneManager.LoadScene("WhoUare");
    }
}
