using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiReaction : MonoBehaviour
{
    public int emojiId;
    public void OnClickOnEmoji(int id)
    {
        this.emojiId = id;
    }

    public int getEmojiId()
    {
        return this.emojiId;
    }
    private void Awake()
    {
        emojiId = -1;
    }

    public void removeEmoji()
    {
        emojiId = -1;
    }
}
