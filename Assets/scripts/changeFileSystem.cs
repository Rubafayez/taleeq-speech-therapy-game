using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class changeFileSystem : MonoBehaviour
{
    public calanderSystem calanderSystem;
    public GameObject changeFileObject;

    public void changeFilePressed(int imageNum, string childName, int day, int month, int year, string gender)
    {
        changeFileObject.SetActive(true);

        // image
        FindFirstObjectByType<imageSystem>().setImage(imageNum);
        FindFirstObjectByType<imageSystem>().save();
        
        // name
        FindFirstObjectByType<childReg>().nameInput.text = childName;
        FindFirstObjectByType<childReg>().oldName = childName;

        // birthday
        calanderSystem.DayNum = day;
        calanderSystem.monthNum = month - 1;
        calanderSystem.years = year;
        calanderSystem.SaveAndupdateCalander();

        // gender
        int genderNum = 0;
        if (gender == "Male")
            genderNum = 1;

        FindFirstObjectByType<childReg>().genderDropdown.value = genderNum;
    }

    public void AddChildPressed()
    {
        changeFileObject.SetActive(true);

        // image
        FindFirstObjectByType<imageSystem>().setImage(0);
        FindFirstObjectByType<imageSystem>().save();

        // name
        FindFirstObjectByType<childReg>().nameInput.text = string.Empty;
        FindFirstObjectByType<childReg>().oldName = string.Empty;
        // birthday
        calanderSystem.CalanderText.text = "DD/MM/YYYY";
        calanderSystem.DayNum = 0;
        

        // gender
        int genderNum = 0;
        FindFirstObjectByType<childReg>().genderDropdown.value = genderNum;
    }

    public void save()
    {
        changeFileObject.SetActive(false);
        FindFirstObjectByType<createCardSystem>().createCard();
    }

}
