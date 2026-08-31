using UnityEngine;
using UnityEngine.UI;

public class dayId : MonoBehaviour
{
    public int ourDayNum;
    void Start()
    {
        GetComponentInChildren<Text>().text = ourDayNum.ToString();
    }

    public void selectDay()
    {
        FindFirstObjectByType<calanderSystem>().selectDayNum(ourDayNum, gameObject);
    }
}
