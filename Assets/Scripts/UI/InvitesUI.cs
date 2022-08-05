using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;

public class InvitesUI : MonoBehaviour
{
    [SerializeField] private Transform inviteContainer;
    [SerializeField] private Invite uiInvitePrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Vector2 originalSize;
    [SerializeField] private Vector2 increaseSize;
    public TMP_InputField usernameInvitationField;
    public TMP_Text warningText;
    private List<string> inviteusernames = new List<string>();
    private List<string> friends = new List<string>();
    private List<Invite> invites = new List<Invite>();

    private void Awake()
    {
        //TODO: FETCH USERNAMES INVITES FROM FIREBASE
        StartCoroutine(FetchInvitation());

        //BEGIN MOCKDATA
        /**inviteusernames.Add("USER1");
        inviteusernames.Add("USER2");
        inviteusernames.Add("USER3");
        inviteusernames.Add("USER1");
        inviteusernames.Add("USER2");
        inviteusernames.Add("USER3");        
        inviteusernames.Add("USER1");
        inviteusernames.Add("USER2");
        inviteusernames.Add("USER3");*/
        //END MOCKDATA
        contentRect = inviteContainer.GetComponent<RectTransform>();
        originalSize = contentRect.sizeDelta;
        increaseSize = new Vector2(0, uiInvitePrefab.GetComponent<RectTransform>().sizeDelta.y);
    }

    private IEnumerator FetchInvitation()
    {
        var DBTask = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot snapshot = DBTask.Result;
        inviteusernames = snapshot.Child("userFriendRequests").Value.ToString().Split().ToList();
        friends = snapshot.Child("userfriends").Value.ToString().Split().ToList();
        foreach (var inviteusername in inviteusernames)
        {
            if (inviteusername.Length != 0)
            {
                Invite invite = Instantiate(uiInvitePrefab, inviteContainer);
                invite.initialize(inviteusername);
                contentRect.sizeDelta += increaseSize;
                invites.Add(invite);
            }
        }
    }

    public void handleInvitationSend()
    {
        if (usernameInvitationField.text.Length == 0)
        {
            warningText.text = "Please enter username";
        }
        else if (friends.Contains(usernameInvitationField.text))
        {
            warningText.text = "User is already friend";
        }
        else
        {
            StartCoroutine(getTask());
        }
    }

    private IEnumerator getTask()
    {
        var DBTask = FirebaseManager.DBreference.Child("users").OrderByChild("username").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot snapshot = DBTask.Result;
        var userId = "";
        List<string> userFriendRequests = new List<string>();
        foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
        {
            if (usernameInvitationField.text == childSnapshot.Child("username").Value.ToString())
            {
                userId = childSnapshot.Key;
                userFriendRequests = childSnapshot.Child("userFriendRequests").Value.ToString().Split().ToList();
            }
        }

        if (userId.Length == 0)
        {
            warningText.text = "username not found";
        }
        else
        {
            if (userFriendRequests.Contains(usernameInvitationField.text))
            {
                warningText.text = "Invitation already sent";
            }
            else
            {
                userFriendRequests.Add(PlayerPrefs.GetString("username", ""));
                StartCoroutine(FirebaseManager.UpdateUserfriendRequestsDatabase(userFriendRequests.ToArray(), userId));
                warningText.text = "Invitation has been sent";
            }
        }
    }

    private void Update()
    {
        List<Invite> newInvitations = new List<Invite>(invites);
        foreach (var invitations in newInvitations)
        {
            if (invitations == null)
            {
                invites.Remove(invitations);
            }
        }
    }
}