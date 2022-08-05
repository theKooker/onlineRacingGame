using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;

public class FriendsUI : MonoBehaviour
{
    [SerializeField] private Transform inviteContainer;
    [SerializeField] private Friend uiFriendPrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Vector2 originalSize;
    [SerializeField] private Vector2 increaseSize;
    private List<string> friends;
    private List<Friend> friendsPrefabs = new List<Friend>();
    private void Awake()
    {
        
        
        //TODO: FETCH USERNAMES INVITES FROM FIREBASE
        StartCoroutine(FetchFriends());
        contentRect = inviteContainer.GetComponent<RectTransform>();
        originalSize = contentRect.sizeDelta;
        increaseSize = new Vector2(0, uiFriendPrefab.GetComponent<RectTransform>().sizeDelta.y);

    }
    private IEnumerator FetchFriends()
    {
        var DBTask = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot snapshot = DBTask.Result;
        friends = snapshot.Child("userfriends").Value.ToString().Split().ToList();
        foreach (var f in friends)
        {
            if (f.Length == 0) continue;
            StartCoroutine(FetchFriendsDetails(f));
        }
    }

    private IEnumerator FetchFriendsDetails(string username)
    {
        var task = FirebaseManager.DBreference.Child("users").OrderByChild("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => task.IsCompleted);
        DataSnapshot snapshot = task.Result;

        var profilePicID = 0;
        foreach (DataSnapshot res in snapshot.Children.Reverse<DataSnapshot>())
        {
            if (username == res.Child("username").Value.ToString())
            {
                profilePicID = int.Parse(res.Child("profilePic").Value.ToString());
                break;
            }
        }
        Friend friend = Instantiate(uiFriendPrefab, inviteContainer);
        friend.initialize(username, profilePicID);
        contentRect.sizeDelta += increaseSize;
        friendsPrefabs.Add(friend);
        
    }
}
