using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chat : MonoBehaviour
{
    private TextMeshProUGUI textField;
    public string text;
    public string sender;
    public string receiver;
    private void Awake()
    {
        this.textField = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void SetTextMessage( string _message, string prefix = "")
    {
        this.text = _message;
        this.textField.text = prefix + _message;
    }
    
}
