using UnityEngine;
using UnityEngine.SceneManagement; 
public class NewMonoBehaviourScript : MonoBehaviour
{


    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
