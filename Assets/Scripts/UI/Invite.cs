using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class Invite : MonoBehaviour
{
    private string inviteUsername;
    private TextMeshProUGUI userNameText;

    private void Awake()
    {
        userNameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void initialize(string _value)
    {
        this.inviteUsername = _value;
        userNameText.text = inviteUsername;
    }

    public void Accept()
    {
        Debug.Log("Invitation accepted");
        handleAcceptInvitation();
    }

    public void handleAcceptInvitation()
    {
        StartCoroutine(removeFromFriendRequests());
        StartCoroutine(addFriendToBothFriendsList());
    }

    private IEnumerator removeFromFriendRequests(bool destroy = false)
    {
        var DBTask = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot snapshot = DBTask.Result;
        List<String> inviteusernames = snapshot.Child("userFriendRequests").Value.ToString().Split().ToList();
        Debug.Log(inviteusernames.Count);
        inviteusernames.Remove(inviteUsername);
        Debug.Log(inviteusernames.Count);
        StartCoroutine(FirebaseManager.UpdateUserfriendRequestsDatabase(inviteusernames.ToArray()));
        if (destroy)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator addFriendToBothFriendsList()
    {
        var DBTask = FirebaseManager.DBreference.Child("users").OrderByChild("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot snapshot = DBTask.Result;
        var userId = "";
        List<string> userFriendsList = new List<string>();
        foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
        {
            if (inviteUsername == childSnapshot.Child("username").Value.ToString())
            {
                userId = childSnapshot.Key;
                userFriendsList = childSnapshot.Child("userfriends").Value.ToString().Split().ToList();
            }
        }

        userFriendsList.Add(PlayerPrefs.GetString("username"));
        StartCoroutine(FirebaseManager.UpdateUserfriendsDatabase(userFriendsList.ToArray(), userId));
        DBTask = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        snapshot = DBTask.Result;
        userFriendsList = snapshot.Child("userfriends").Value.ToString().Split().ToList();
        userFriendsList.Add(inviteUsername);
        StartCoroutine(FirebaseManager.UpdateUserfriendsDatabase(userFriendsList.ToArray()));
        Destroy(gameObject);
    }

    public void Decline()
    {
        StartCoroutine(removeFromFriendRequests(true));
    }
}