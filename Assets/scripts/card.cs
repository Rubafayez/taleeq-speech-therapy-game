using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class card : MonoBehaviour
{
    int imagenum;
    public Image cardImage;
    public TextMeshProUGUI ChildNameText;
    public TextMeshProUGUI BirthdayText;
    public TextMeshProUGUI GenderText;
    string childname;
    int day_;
    int month_;
    int year_;

    string gender_;
    public void setDataAndUpdateText(int imageNum , string childName , string gender , int day ,int month,int year)
    {
        imagenum = imageNum;
        childname = childName;
        cardImage.sprite = FindFirstObjectByType<createCardSystem>().images[imageNum];
        ChildNameText.text = childName;
        BirthdayText.text = day + " / " + month + " / " + year;
        GenderText.text = gender;

        day_ = day;
        month_ = month;
        year_ = year;

        gender_ = gender;
    }

    public void removeCard()
    {
        FindFirstObjectByType<createCardSystem>().removePressed(childname);
    }

    public void changeFileOpen()
    {
        FindFirstObjectByType<changeFileSystem>().changeFilePressed(imagenum, childname, day_, month_, year_, gender_);
    }
}
