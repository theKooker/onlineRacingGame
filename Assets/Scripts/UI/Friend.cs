using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Friend : MonoBehaviour
{
    private string friendUsername;
    private int profilePicId;
    private TextMeshProUGUI userNameText;
    private Image profilepic;

    private void Awake()
    {
        userNameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        profilepic = transform.GetChild(1).GetComponent<Image>();
    }

    public void initialize(string _value , int profilePicID)
    {
        this.friendUsername = _value;
        userNameText.text = friendUsername;
        profilepic.sprite = Global.instance.profilePics[profilePicID];
    }

    public void chatWithFriend()
    {
        PlayerPrefs.SetString("receive", friendUsername);
        Global.instance.LoadLevel("ChatScreen");
    }
}
