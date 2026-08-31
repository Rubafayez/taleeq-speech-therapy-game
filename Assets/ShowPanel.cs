using UnityEngine;
using System.Collections;

public class ShowPanel : MonoBehaviour
{
    public GameObject panel;

    public void OpenPanel()
    {
        panel.SetActive(true);
        StartCoroutine(CloseAfterTime());
    }

    IEnumerator CloseAfterTime()
    {
        yield return new WaitForSeconds(5f);
        panel.SetActive(false);
    }
}
