using UnityEngine;

public class ProfileCustomRaceScript : MonoBehaviour
{
    // [SerializeField] private GameObject profilePicture;
    // [SerializeField] private GameObject profileUserName;
    // [SerializeField] private GameObject onlineOrReadyIndicator;
    // [SerializeField] private GameObject inviteButtonDisableOnInvitedHideOnInMatch;
    // [SerializeField] private GameObject CustomMatchMenuPrefab;


    public void InviteToMatch()
    {
        StartCoroutine(transform.parent.parent.parent.parent.parent.GetChild(0).GetComponent<CustomMatchScript>()
            .SendInviteToMatch(transform.name));
    }

    // // Start is called before the first frame update
    // void Start()
    // {
    //     // todo fetch profile username and profile picture
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     // todo update online status
    //     // todo update ready status
    // }
}