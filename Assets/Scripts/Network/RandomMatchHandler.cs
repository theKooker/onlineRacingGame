using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class RandomMatchHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject findMatchBtn;
    [SerializeField] private GameObject searching;
    [SerializeField] private GameObject responseText;

    
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            findMatchBtn.SetActive(false);
            searching.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();  
        }

    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("We are connected to Photon! on " + PhotonNetwork.CloudRegion +" Server");
        PhotonNetwork.AutomaticallySyncScene = true;
        findMatchBtn.SetActive(true);
    }

    public void FindMatch()
    {
        searching.SetActive(true);
        findMatchBtn.SetActive(false);

        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Searching for a Game");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        MakeRoom();
    }

    void MakeRoom()
    {
        RoomOptions roomOptions =  new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom("ORG_match" + System.DateTime.UtcNow.ToString("HH:mm:ss dd MMMM, yyyy"), roomOptions);
        Debug.Log("Room Created, Waiting For Another Player");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.UserId +" joined the room");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {

            Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount + "/2 Starting Game");
            PhotonNetwork.LoadLevel("level1");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName+ "Joined to "+ PhotonNetwork.CurrentRoom.Name);
        StartCoroutine(FirebaseManager.CreateMatchStateForPlayer(PlayerPrefs.GetString("username"),
            PhotonNetwork.CurrentRoom.Name));
    }

    public void StopSearch()
    {
        searching.SetActive(false);
        findMatchBtn.SetActive(true);
        PhotonNetwork.LeaveRoom();
        Debug.Log("Stopped, Back to Menu");
    }
}
