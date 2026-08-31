using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class calanderSystem : MonoBehaviour
{
    public Text yearsText;
    public int maxYears = 2026;
    public int years = 2026;

    public Text monthText;
    public string[] months;
    public int monthNum = 0;

    public GameObject dayHome;
    public GameObject dayBtnPrefab;
    public GameObject daySelect;
    public int DayNum = 1;

    public TextMeshProUGUI CalanderText;

    public childReg reg; // ربط مباشر بدل Find

    private void Start()
    {
        updateAllText();
        createDayBtn();
    }

    public void plusYears()
    {
        if (years == maxYears) return;
        years++;
        updateAllText();
    }

    public void minusYears()
    {
        if (years < 1900) return;
        years--;
        updateAllText();
    }

    void updateAllText()
    {
        yearsText.text = years.ToString();
        monthText.text = months[monthNum];
    }

    public void plusMonth()
    {
        monthNum = (monthNum == 11) ? 0 : monthNum + 1;
        updateAllText();
    }

    public void minusMonth()
    {
        monthNum = (monthNum == 0) ? 11 : monthNum - 1;
        updateAllText();
    }

    void createDayBtn()
    {
        for (int i = 0; i < 31; i++)
        {
            GameObject newDayBtn = Instantiate(dayBtnPrefab, dayHome.transform);
            newDayBtn.GetComponent<dayId>().ourDayNum = i + 1;
        }
    }

    public void selectDayNum(int dayNum, GameObject btnObject)
    {
        DayNum = dayNum;
        daySelect.transform.position = btnObject.transform.position;
    }

    public void SaveAndupdateCalander()
    {
        updateAllText();

        string dateStr = DayNum + "/" + (monthNum + 1) + "/" + years;
        CalanderText.text = dateStr;

        if (reg != null)
        {
            reg.age = maxYears - years;

            if (reg.birthDateText != null)
                reg.birthDateText.text = dateStr;
        }

        gameObject.SetActive(false);
    }
}
