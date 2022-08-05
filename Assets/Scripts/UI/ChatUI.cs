
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private Transform inviteContainer;
    [SerializeField] private Chat uiChatPrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Vector2 originalSize;
    [SerializeField] private Vector2 increaseSize;
    public TMP_InputField chatMessageField;
    private void Awake()
    {
        //FETCH OLD MESSAGES
        //StartCoroutine(FetchOldMessages());
        contentRect = inviteContainer.GetComponent<RectTransform>();
        originalSize = contentRect.sizeDelta;
        increaseSize = new Vector2(0, uiChatPrefab.GetComponent<RectTransform>().sizeDelta.y);
    }

    private void Start()
    {
        ListenForNewMessages(InstantiateMessage, Debug.Log);
    }

    public void sendMessage()
    {
        if (chatMessageField.text.Trim().Length != 0)
        {
            StartCoroutine(FirebaseManager.UpdateMessages(PlayerPrefs.GetString("username", ""), PlayerPrefs.GetString("receive", ""),chatMessageField.text.Trim()));
            chatMessageField.text = "";
        }
    }

    public void InstantiateMessage(string messageID)
    {
        StartCoroutine(InstantiateNewMessage(messageID));
    }

    public IEnumerator InstantiateNewMessage(string messageID)
    {
        yield return new WaitForSeconds(0.5f);
        var DBTask = FirebaseManager.DBreference.Child("messages").Child(messageID).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DataSnapshot snapshot = DBTask.Result;
        Debug.Log(DBTask.Result.Value == null);
        Debug.Log(snapshot.Child("sender"));
        Debug.Log(snapshot.Child("receiver"));
        var text = snapshot.Child("text").Value.ToString();
        var sender = snapshot.Child("sender").Value.ToString();
        var receiver = snapshot.Child("receiver").Value.ToString();
        if (
            (sender.Equals(PlayerPrefs.GetString("username", ""))
             &&
             receiver.Equals(PlayerPrefs.GetString("receive", ""))
             ) ||
            (sender.Equals(PlayerPrefs.GetString("receive", "")) &&
             receiver.Equals(PlayerPrefs.GetString("username", ""))
             )
            )
        {
            Debug.Log("SO "+sender + "sends to "+receiver+ " :"+text);
            var message = $"{sender} : {text}";
            Chat m = Instantiate(uiChatPrefab, inviteContainer);
            m.SetTextMessage(message);
            m.sender = sender;
            m.receiver = receiver;
            contentRect.sizeDelta += increaseSize;
        }


    }


    public void ListenForNewMessages(Action<string> callback, Action<AggregateException> fallback)
    {
        void CurrentListener(object o, ChildChangedEventArgs args)
        {
            Debug.Log("new message");   
            callback(args.Snapshot.Key);
        }

        FirebaseManager.DBreference.Child("messages").ChildAdded += CurrentListener;
    }
}
