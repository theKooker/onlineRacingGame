using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winText;

    [SerializeField]
    private Image place1;
    [SerializeField]
    private TextMeshProUGUI place1text;
    [SerializeField] private Image place2;
    [SerializeField]
    private TextMeshProUGUI place2text;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FetchData());
        
    }

    private IEnumerator FetchData()
    {
        var DBTask2 = FirebaseManager.DBreference.Child("matches").Child(PlayerPrefs.GetString("matchid")).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DataSnapshot snap = DBTask2.Result;
        var player1username = snap.Child("player1").Value.ToString();
        var player2username = snap.Child("player2").Value.ToString();
        var task = FirebaseManager.DBreference.Child("users").OrderByChild("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => task.IsCompleted);
        DataSnapshot snapshot = task.Result;
        var player1uid = "";
        var player2uid = "";
        var player1pic = 0;
        var player2pic = 0;
        foreach (DataSnapshot res in snapshot.Children.Reverse<DataSnapshot>())
        {
            if (player1username == res.Child("username").Value.ToString())
            {
                player1uid = res.Key;
                player1pic = int.Parse(res.Child("profilePic").Value.ToString());
            }
            if (player2username == res.Child("username").Value.ToString())
            {
                player2uid = res.Key;
                player2pic = int.Parse(res.Child("profilePic").Value.ToString());
            }

            if (res.Child("username").Value.ToString() == PlayerPrefs.GetString("username"))
            {
                PlayerPrefs.SetString("money", res.Child("money").Value.ToString());
            }
            

            if (player1uid != "" && player2uid != "")
            {
                break;
            }
        }

        if (snap.Child("decision").Value.ToString() == player1username)
        {
            place1.sprite = Global.instance.profilePics[player1pic];
            place1text.text = player1username;
            place2.sprite = Global.instance.profilePics[player2pic];
            place2text.text = player2username;
        }
        else
        {
            place2.sprite = Global.instance.profilePics[player1pic];
            place2text.text = player1username;
            place1.sprite = Global.instance.profilePics[player2pic];
            place1text.text = player2username;
        }

        yield return new WaitForSeconds(1f);
        if (snap.Child("decision").Value.ToString() == PlayerPrefs.GetString("username"))
        {
            winText.text = "You Win!";
        }
        else
        {
            winText.text = "You Lose!";
        }
    }
}
