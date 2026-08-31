using UnityEngine;
using UnityEngine.UI;

public class imageSystem : MonoBehaviour
{
    public Sprite[] images;
    public Image[] btnImage;
    public Image BtnOpen;
    public GameObject selectedImage;
    int imageNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < images.Length; i++)
        {
            btnImage[i].sprite = images[i];
        }
    }

    public void openImagePressed()
    {
        Invoke(nameof(setSelectAfterOpen) , 0.2f);
    }

    void setSelectAfterOpen()
    {
        selectedImage.transform.position = btnImage[imageNum].transform.position;
    }

    public void setImage(int imagenum)
    {
        imageNum = imagenum;
        selectedImage.transform.position = btnImage[imagenum].transform.position;
        BtnOpen.sprite = images[imagenum];

    }

    public void save()
    {
        FindFirstObjectByType<childReg>().imageNum = imageNum;
    }
}
