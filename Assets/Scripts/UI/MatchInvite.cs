using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Photon.Pun;
using TMPro;
using UnityEngine;


// Original Code by Mohamed, copied and modified

public class MatchInvite : MonoBehaviourPunCallbacks
{
    private string _matchInvitation;
    private string _inviteUsername;
    private string _matchRoomName;
    private TextMeshProUGUI _userNameText;
    [SerializeField] public GameObject acceptButton;
    [SerializeField] public GameObject waitImage;
    [SerializeField] public GameObject connectedImage;
    [SerializeField] public GameObject disconnectedImage;
    [SerializeField] public GameObject warningImage;
    private bool _accepted = false;


    private void Awake()
    {
        _userNameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void Initialize(string value)
    {
        this._matchInvitation = value;
        string[] temp = value.Split(new[] { "#" }, StringSplitOptions.None);
        this._inviteUsername = temp[0];
        this._matchRoomName = temp[1];
        _userNameText.text = _inviteUsername;
        if (this._matchRoomName == "")
        {
            Destroy(gameObject);
        }
    }

    public void Accept()
    {
        Debug.Log("Invitation accepted");
        HandleAcceptInvitation();
    }

    public override void OnJoinedRoom()
    {
        waitImage.SetActive(false);
        this.disconnectedImage.SetActive(false);
        this.connectedImage.SetActive(true);
        Debug.Log("But now I am in Room: " + PhotonNetwork.InRoom);
        Debug.Log("with: " + PhotonNetwork.CurrentRoom.Players);
        PhotonNetwork.AutomaticallySyncScene = true;

        // base.OnJoinedRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        waitImage.SetActive(false);
        this.connectedImage.SetActive(false);
        this.disconnectedImage.SetActive(true);
        Debug.Log("Something happened: " + returnCode + " - " + message);
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnConnectedToMaster()
    {
        if (_accepted)
        {
            Debug.Log("We are connected to Photon! on " + PhotonNetwork.CloudRegion + " Server");
            // PhotonNetwork.AutomaticallySyncScene = true; not now? todo

            Debug.Log("Am In Room: " + PhotonNetwork.InRoom);
            Debug.Log("Trying to connect to: " + _matchRoomName);
            PhotonNetwork.JoinRoom(this._matchRoomName);
        }
    }

    private void HandleAcceptInvitation()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name != this._matchRoomName)
        {
            // PhotonNetwork.LeaveRoom();
            this.warningImage.SetActive(true);
            return;
        }
        else if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == this._matchRoomName)
        {
            _accepted = true;

            this.acceptButton.SetActive(false);
            this.connectedImage.SetActive(true);
            this.disconnectedImage.SetActive(false);
            this.waitImage.SetActive(false);
            OnConnectedToMaster();
        }
        else
        {
            _accepted = true;
            StartCoroutine(RemoveFromMatchInvitations());

            this.acceptButton.SetActive(false);
            this.connectedImage.SetActive(false);
            this.disconnectedImage.SetActive(false);
            this.waitImage.SetActive(true);

            try
            {
                if (!PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.ConnectUsingSettings();
                }
                else
                {
                    OnConnectedToMaster();
                }
            }
            catch (Exception)
            {
                Debug.Log("Invitation Broken or not up to date any longer");
                Decline();
            }
        }
    }

    private bool IsMatchInvitation(string matchInvitation)
    {
        return matchInvitation.Equals(_matchInvitation);
    }

    private IEnumerator RemoveFromMatchInvitations(bool destroy = false)
    {
        var dbTask = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);
        DataSnapshot snapshot = dbTask.Result;
        List<String> matchInvitations = snapshot.Child("matchInvitations").Value.ToString().Split().ToList();
        Debug.Log(matchInvitations.Count);
        matchInvitations.RemoveAll(IsMatchInvitation);
        Debug.Log(matchInvitations.Count);
        StartCoroutine(
            FirebaseManager.UpdateMatchInvitationsDatabase(matchInvitations.ToArray(), FirebaseManager.User.UserId));

        if (destroy)
        {
            Destroy(gameObject);
        }
    }

    public void Decline()
    {
        _accepted = false;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        StartCoroutine(RemoveFromMatchInvitations(true));
    }
}