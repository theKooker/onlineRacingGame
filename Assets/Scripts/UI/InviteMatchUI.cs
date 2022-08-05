using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;

public class InviteMatchUI : MonoBehaviour
{
    [SerializeField] private Transform inviteContainer;
    [SerializeField] private MatchInvite uiInviteMatchPrefab;

    [SerializeField] private RectTransform contentRect;

    // [SerializeField] private Vector2 originalSize;
    [SerializeField] private Vector2 increaseSize;

    private List<string> _matchInvitations = new List<string>();

    // private List<string> users = new List<string>();
    private readonly List<MatchInvite> _invites = new List<MatchInvite>();

    private void Awake()
    {
        //TODO: FETCH USERNAMES INVITES FROM FIREBASE
        StartCoroutine(FetchMatchInvitation());

        contentRect = inviteContainer.GetComponent<RectTransform>();
        increaseSize = new Vector2(0, uiInviteMatchPrefab.GetComponent<RectTransform>().sizeDelta.y);
    }

    private IEnumerator FetchMatchInvitation()
    {
        var dbTask = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        DataSnapshot snapshot = dbTask.Result;
        _matchInvitations = snapshot.Child("matchInvitations").Value.ToString().Split().ToList();

        foreach (var matchInvitation in _matchInvitations)
        {
            if (matchInvitation.Length != 0)
            {
                MatchInvite invite = Instantiate(uiInviteMatchPrefab, inviteContainer);
                invite.Initialize(matchInvitation);
                contentRect.sizeDelta += increaseSize;
                _invites.Add(invite);
            }
        }
    }

    private void Update()
    {
        List<MatchInvite> newInvitations = new List<MatchInvite>(_invites);
        foreach (var invitations in newInvitations)
        {
            if (invitations == null)
            {
                _invites.Remove(invitations);
            }
        }
    }
}