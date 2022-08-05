
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class InGame : MonoBehaviourPunCallbacks
{
    
    [SerializeField] private GameObject[] emojieObjects;
    public GameObject currentEmoji;
    [SerializeField] public  TextMeshProUGUI countingText;
    [SerializeField] private AudioClip ringtone;
    [SerializeField] private GameObject guideBreaks;
    [SerializeField] private GameObject guideBoosts;
    private float timeOfEmoji;

    public GameObject player;
    private string player1username = "";
    private string player2username = "";
    public string gameDecision = "";
    private bool scoreUpdated = false;
    public void Start()
    {
        StartCoroutine(Pause());
    }

    public GameObject GetCurrentEmoji()
    {
        if (GetComponent<EmojiReaction>().getEmojiId() != -1)
        {
            Debug.Log(GetComponent<EmojiReaction>().getEmojiId());
            currentEmoji = emojieObjects[GetComponent<EmojiReaction>().emojiId];
            return currentEmoji;         
        }
        else
        {
            return null;
        }

    }
    private void Update()
    {
        
        if (player.GetComponent<CarsCheckpointsTriggers>().scoreTimeEndMatch > 0)
        {

            countingText.text = "Finish";

        }
        StartCoroutine(GetDecision());
        UpdateEmoji();

    }

    private IEnumerator GetDecision()
    {
        var DBTask = FirebaseManager.DBreference.Child("matches").Child(PhotonNetwork.CurrentRoom.Name).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot s = DBTask.Result;
        if (s.Child("decision").Value.ToString() != "") {
            StartCoroutine(UpdateScore());
            countingText.text = "Finish";
        }
        
       


    }
    




    private IEnumerator UpdateScore()
    {
        var task2 = FirebaseManager.DBreference.Child("matches").Child(PlayerPrefs.GetString("matchid")).GetValueAsync();
        yield return new WaitUntil(predicate: () => task2.IsCompleted);
        DataSnapshot r = task2.Result;
        player1username = r.Child("player1").Value.ToString();
        player2username = r.Child("player2").Value.ToString();
        gameDecision = r.Child("decision").Value.ToString();
        var task = FirebaseManager.DBreference.Child("users").OrderByChild("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => task.IsCompleted);
        DataSnapshot snapshot = task.Result;
        var player1uid = "";
        var player2uid = "";
        var player1money = 0;
        var player2money = 0;
        Debug.Log("player1username = "+player1username);
        Debug.Log("player2username = "+player2username);
        foreach (DataSnapshot res in snapshot.Children.Reverse<DataSnapshot>())
        {
            if (player1username == res.Child("username").Value.ToString())
            {
                player1uid = res.Key;
                player1money = int.Parse(res.Child("money").Value.ToString());
            }
            if (player2username == res.Child("username").Value.ToString())
            {
                player2uid = res.Key;
                player2money = int.Parse(res.Child("money").Value.ToString());
            }

            if (player1uid != "" && player2uid != "")
            {
                break;
            }
        }

        if (!scoreUpdated)
        {
            scoreUpdated = true;
            if (gameDecision == player1username)
            {
                StartCoroutine(FirebaseManager.UpdateMoneyDatabase((player1money + 1000).ToString(), player1uid));
                StartCoroutine(FirebaseManager.UpdateMoneyDatabase((player2money + 500).ToString(), player2uid));
            }
            else
            {
                StartCoroutine(FirebaseManager.UpdateMoneyDatabase((player1money + 500).ToString(), player1uid));
                StartCoroutine(FirebaseManager.UpdateMoneyDatabase((player2money + 1000).ToString(), player2uid));
            }


        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(GameOver());



    }
    IEnumerator GameOver()
    {
        // PhotonNetwork.Disconnect();
        PhotonNetwork.LeaveRoom();
        // while(PhotonNetwork.IsConnected)
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }
        Global.instance.LoadLevel("GameOver");
    }

    private void UpdateEmoji()
    {
        if (!currentEmoji)
        {

            if (GetCurrentEmoji())
            {
                currentEmoji = PhotonNetwork.Instantiate(GetCurrentEmoji().name, player.transform.position + Vector3.up * 4,player.transform.rotation);
                currentEmoji.transform.position = player.transform.position + Vector3.up * 4;
                timeOfEmoji = Time.fixedTime;

            }
        }
        else
        {
            currentEmoji.transform.position = player.transform.position + Vector3.up * 4;
            if (Time.fixedTime - timeOfEmoji > 3.0f)
            {
                Destroy(currentEmoji);
                PhotonNetwork.Destroy(currentEmoji);
                GetComponent<EmojiReaction>().removeEmoji();
                SetCurrentEmoji(null);
            }
        }
    }
    private IEnumerator Pause()
    {
        player = GameObject.FindWithTag("PLAYER");

        GetComponent<AudioSource>().PlayOneShot(ringtone);
        player.GetComponent<RacingCarController>().enabled = false;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        countingText.text = "1";
        yield return new WaitForSeconds(1);
        countingText.text = "2";
        yield return new WaitForSeconds(1);
        countingText.text = "3";
        yield return new WaitForSeconds(1);
        countingText.text = "";
        player.GetComponent<RacingCarController>().enabled = true;
        guideBoosts.SetActive(false);
        guideBreaks.SetActive(false);
    }

    public void SetCurrentEmoji(GameObject value)
    {
        this.currentEmoji = null;
    }

}
