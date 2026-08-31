using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class createCardSystem : MonoBehaviour
{
    public GameObject card;
    public Sprite[] images;
    List<GameObject> cards = new List<GameObject>();
    public GameObject CardHome;

    FirebaseFirestore db;
    string holdChildNameForRemove;
    public GameObject RemoveMsg;
    public TextMeshProUGUI removeChildNameText;
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        Invoke(nameof(createCard), 0.2f);
    }

    // read all in collection and call read and create card
    public void createCard()
    {
        // first remove all card
        removeAllCard();

        int childCount = 0;
        string email = PlayerPrefs.GetString("email");

        if(email == string.Empty)
        {
            email = "test@gmail.com";
        }

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("No email found in PlayerPrefs");
            return;
        }

        // الوصول إلى Collection اللاعبين
        CollectionReference playersRef = db.Collection("Users").Document(email).Collection("player");

        // جلب جميع الـ Documents في Collection
        playersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching players: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                Debug.Log("Total Players: " + snapshot.Count);
                childCount = snapshot.Count;
                
                

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    //child name
                    ReadAndCreatePlayersFromFirestore(doc.Id);
                    string playerName = doc.Id;
                    Debug.Log("Player Name: " + playerName);
                }
            }
        });

    }

    // read data and create card
    public void ReadAndCreatePlayersFromFirestore(string playerName)
    {
        string email = PlayerPrefs.GetString("email");

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("No email found in PlayerPrefs");
            return;
        }

        DocumentReference playerDoc = db.Collection("Users").Document(email).Collection("player").Document(playerName);

        playerDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Read Error: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DocumentSnapshot doc = task.Result;

                if (doc.Exists)
                {
                    Dictionary<string, object> data = doc.ToDictionary();

                    // create card here
                    GameObject newCard = Instantiate(card, CardHome.transform) as GameObject;
                    int imagenum = int.Parse(data["image"].ToString());
                    int day = int.Parse(data["ageDay"].ToString());
                    int month = int.Parse(data["ageMonth"].ToString());
                    int year = int.Parse(data["ageYear"].ToString());
                    newCard.GetComponent<card>().setDataAndUpdateText(imagenum, data["name"].ToString(), data["gender"].ToString(),day, month, year);

                    cards.Add(newCard);

                    
                }
                else
                {
                    Debug.Log("No player found!");
                }
            }
        });
    }

    public void removePressed(string childName)
    {
        holdChildNameForRemove = childName;
        removeChildNameText.text = childName;
        RemoveMsg.SetActive(true);

    }

    public void setHoldName(string childName)
    {
        holdChildNameForRemove = childName;
        DeletePlayer(holdChildNameForRemove);
    }

    public void remove()
    {
        // remove card
        DeletePlayer(holdChildNameForRemove);

        // create card
        createCard();
    }

    void removeAllCard()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Destroy(cards[i]);
        }

        cards.Clear();
    }

    public void DeletePlayer(string playerName)
    {
        string email = PlayerPrefs.GetString("email");

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("No email found!");
            return;
        }

        DocumentReference playerDoc =
            db.Collection("Users")
              .Document(email)
              .Collection("player")
              .Document(playerName);

        playerDoc.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Delete Error: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Player deleted: " + playerName);
            }
        });
    }
}
