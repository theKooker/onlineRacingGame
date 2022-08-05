using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class CustomMatchScript : MonoBehaviourPunCallbacks
{
    private const int PLAYERSLIMIT = 2; // TODO PLAYER LIMIT


    [SerializeField] public GameObject mapSelectionPanel;
    [SerializeField] public GameObject mapPreviewImageButton;

    [SerializeField] public Sprite mapPreviewImg1;
    [SerializeField] public Sprite mapPreviewImg2;
    [SerializeField] public Sprite mapPreviewImg3;
    [SerializeField] public Sprite mapPreviewImg4;

    [SerializeField] public Scene mapScene1;
    [SerializeField] public Scene mapScene2;
    [SerializeField] public Scene mapScene3;
    [SerializeField] public Scene mapScene4;

    [SerializeField] public GameObject newLobbyPanel;

    [SerializeField] public GameObject globalPrefab;

    [SerializeField] public GameObject competitorsContentList;
    [SerializeField] public GameObject friendsContentList;
    [SerializeField] public GameObject listEntryProfilePrefab;
    private readonly List<CustomUser> _friendsList = new List<CustomUser>();

    [SerializeField] public GameObject startButton;
    [SerializeField] public GameObject playersNum;

    private int _selectedMap;
    private int _mapNameIndex = 1;

    private CustomUser _hostPlayer;

    private int _roomID;
    private string _roomName;


    public void StartGame()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.CurrentRoom.PlayerCount is > 1 and <= PLAYERSLIMIT && PhotonNetwork.IsMasterClient)
        {
            string levelName = "level" + _mapNameIndex;
            Debug.Log(levelName + " ... loading ...");

            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            PhotonNetwork.LoadLevel(levelName); // TODO why does in not load the level for the host?
        }
    }

    private void LoadFriendsView()
    {
        foreach (CustomUser friend in this._friendsList)
        {
            GameObject childGameObject = Instantiate(listEntryProfilePrefab, friendsContentList.transform);
            childGameObject.name = friend.GetName();
            childGameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = friend.GetName();
            var image = childGameObject.transform.GetChild(0).GetComponent<Image>();
            image.sprite = Global.instance.profilePics[friend.GetProfilePictureIndex()];

            childGameObject.transform.GetChild(0).GetChild(0).GetComponent<Image>().color =
                friend.GetOnlineOrReady() ? Color.green : Color.red;
        }
    }


    public IEnumerator SendInviteToMatch(string player)
    {
        if (PhotonNetwork.InRoom)
        {
            // string player = this.transform.parent.name.ToString();

            var temp = FirebaseManager.DBreference.Child("users").OrderByChild("username").GetValueAsync();
            yield return new WaitUntil(predicate: () => temp.IsCompleted);
            DataSnapshot userdata = temp.Result;

            string databaseUserKey = "";
            string matchInvitation = PlayerPrefs.GetString("username") + "#" + _roomName;
            List<string> matchInvitations = new List<string>();

            foreach (DataSnapshot user in userdata.Children.Reverse())
            {
                if (user.Child("username").Value.ToString().Equals(player))
                {
                    databaseUserKey = user.Key;

                    if (user.HasChild("matchInvitations"))
                    {
                        matchInvitations = user.Child("matchInvitations").Value.ToString().Split().ToList();
                    }

                    matchInvitations.Add(matchInvitation);
                }
            }

            if (databaseUserKey.Length != 0)
            {
                StartCoroutine(
                    FirebaseManager.UpdateMatchInvitationsDatabase(matchInvitations.ToArray(), databaseUserKey));
            }
        }
    }


    public void UpdateMapView()
    {
        switch (_selectedMap)
        {
            case 4:
                mapPreviewImageButton.transform.GetComponent<Image>().sprite = mapPreviewImg1;
                _mapNameIndex = 1;
                return;
            case 5:
                mapPreviewImageButton.transform.GetComponent<Image>().sprite = mapPreviewImg2;
                _mapNameIndex = 2;
                return;
            case 8:
                mapPreviewImageButton.transform.GetComponent<Image>().sprite = mapPreviewImg3;
                _mapNameIndex = 3;
                return;
            default:
                // case map = 9
                mapPreviewImageButton.transform.GetComponent<Image>().sprite = mapPreviewImg4;
                _mapNameIndex = 4;
                return;
        }
    }

    public void SelectMap(int sceneNum)
    {
        _selectedMap = sceneNum;
        // ManualUpdateToServer();
    }


    public void CreateRoom()
    {
        _roomID = Random.Range(0, 5000);
        RoomOptions roomOptions = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true, // maybe hidden, but for cases of empty overload we'll keep it visible
            MaxPlayers = 4
        };
        _roomName = "ORG-RoomName_" + _roomID;
        PhotonNetwork.CreateRoom(_roomName, roomOptions);

        Debug.Log("Room Created, Waiting For Other Players");

        // players.add(currentPlayer);
        // ManualUpdateToServer();
    }


    private IEnumerator DownloadFriendsData()
    {
        // get Firebase User-Block
        var users = FirebaseManager.DBreference.Child("users").GetValueAsync();
        yield return new WaitUntil(predicate: () => users.IsCompleted);
        DataSnapshot snapshot = users.Result;
        var yourProfile = snapshot.Child(FirebaseManager.User.UserId);

        List<string> friends = yourProfile.Child("userfriends").Value.ToString().Split().ToList();
        foreach (string friend in friends)
        {
            foreach (var user in users.Result.Children.Reverse<DataSnapshot>())
            {
                if (friend == user.Child("username").Value.ToString())
                {
                    _friendsList.Add(new CustomUser(friend, int.Parse(user.Child("profilePic").Value.ToString())));
                }
            }
        }

        //LOAD VIEWS
        LoadFriendsView();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are connected to Photon! on " + PhotonNetwork.CloudRegion + " Server");
        // PhotonNetwork.AutomaticallySyncScene = true; not now? todo
        CreateRoom();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        StartCoroutine(DownloadFriendsData());
    }

    public void UpdatePlayersNum()
    {
        Debug.Log("InRoom: " + PhotonNetwork.InRoom);
        if (!PhotonNetwork.InRoom)
            return;
        var tempPlayers = PhotonNetwork.CurrentRoom.Players.Count;
        Debug.Log("Current Players: " + tempPlayers);
        this.playersNum.GetComponent<TextMeshProUGUI>().text = tempPlayers.ToString();
        this.startButton.GetComponent<Toggle>().interactable = (tempPlayers is > 1 and <= PLAYERSLIMIT);
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.Players.Clear();
        }

        PhotonNetwork.Disconnect();
    }
}