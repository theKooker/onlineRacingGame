using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class PhotonManager : MonoBehaviourPun
{
    [FormerlySerializedAs("SpawnPoints")] [SerializeField] private GameObject[] spawnPoints;
    private PhotonView pV;
    private int player;
    private string playerTag;

    // Start is called before the first frame update
    void Awake()
    {
        pV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
    public void LeaveRace()
    {
        StartCoroutine(DisconnectAndLoad());
    }

    IEnumerator DisconnectAndLoad()
    {
        // PhotonNetwork.Disconnect();
        PhotonNetwork.LeaveRoom();
        // while(PhotonNetwork.IsConnected)
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }
        Global.instance.LoadLevel("Menu");
    }
    void SpawnPlayer()
    {
        
        PlayerPrefs.SetString("matchid",PhotonNetwork.CurrentRoom.Name);
        player = 0;
        playerTag = "PLAYER";
        if (!PhotonNetwork.IsMasterClient)
        {
            player = 1;
        }
        
        GameObject playerObject = PhotonNetwork.Instantiate(PlayerPrefs.GetString("selectedCar", "Car1"), spawnPoints[player].transform.position + Vector3.up, spawnPoints[player].transform.rotation);
        playerObject.GetComponent<CarsCheckpointsTriggers>().carPosition = player;
        playerObject.tag = playerTag;
    }
    
}