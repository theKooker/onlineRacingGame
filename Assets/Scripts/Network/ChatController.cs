using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatController : MonoBehaviour, IChatClientListener
{

    [SerializeField] private string nickname;
    private ChatClient chatClient;
    public TMP_InputField textField;
    [SerializeField] private Vector2 lastMessage;
    public GameObject senderPrefab;
    public GameObject viewScroll;
    public List<GameObject> messages = new List<GameObject>();

    private void Awake()
    {
        nickname = PlayerPrefs.GetString("USERNAME");
    }

    private void Start()
    {
        chatClient = new ChatClient(this);
        ConnectToPhotonChat();
    }

    public void SendDirectMessage(string recipient, string message)
    {
        chatClient.SendPrivateMessage(recipient, message);
    }

    public void SendMessage()
    {
        var text = textField.text;
        if (text.Length != 0)
        {
            var message = Instantiate(senderPrefab, senderPrefab.transform.position, Quaternion.identity);
            lastMessage = new Vector2(message.transform.position.x, message.transform.position.y);
            message.transform.parent = viewScroll.transform;
            message.GetComponent<RectTransform>().anchoredPosition = lastMessage;
            message.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
            //message.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 19;
            messages.Add(message);
        }
    }
    private void ConnectToPhotonChat()
    {
        Debug.Log("Connecting to Photon Chat!");
        chatClient.AuthValues = new Photon.Chat.AuthenticationValues(nickname);
        ChatAppSettings chatSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
        chatClient.ConnectUsingSettings(chatSettings);
    }

    private void Update()
    {
        chatClient.Service();
    }
    
    public void DebugReturn(DebugLevel level, string message)
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnected()
    {
        Debug.Log("You have disconnected to the Photon Chat");
    }

    public void OnConnected()
    {
        Debug.Log("You have connected to the Photon Chat");
    }

    public void OnChatStateChange(ChatState state)
    {
        throw new System.NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        throw new System.NotImplementedException();
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (!string.IsNullOrEmpty(message.ToString()))
        {
            // Channel Name format [Sender:Receiver]
            string[] splitNames = channelName.Split(new char[] { ':' });
            string senderName = splitNames[0];
            if (!sender.Equals(senderName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"{sender}: {message}");
            }
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        throw new System.NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
}
