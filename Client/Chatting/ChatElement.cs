using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatElement : MonoBehaviour
{
    public void UpdateUI(ChatData chatData)
    {
        this.chatData = chatData;

        for (int i = 0; i < chatData.sender.Length; i++)
        {
            byte val = (byte)chatData.sender[i];
            if(val == 0)
            {
                chatData.sender = chatData.sender.Substring(0, i);
                break;
            }
        }

        string newText = "[" + chatData.time + "]";

        if (chatData.type == ChatData.eType.All)
        {
            newText += "[전체]" + chatData.sender + ": " + chatData.content;

            text.color = new Color(0.9529f, 0.6078f, 0.0f, 1.0f);
        }
        else if (chatData.type == ChatData.eType.Normal)
        {
            text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            newText += chatData.sender + ": " + chatData.content;
        }
        else if (chatData.type == ChatData.eType.Direct)
        {
            text.color = new Color(0.8431f, 0.5764f, 0.9372f, 1.0f);

            //보낸 귓속말의 경우
            if(chatData.sender == User.instance.nickname)
            {
                newText += ("당신이" + chatData.receiver + "에게: " + chatData.content);
            }
            //받은 귓속말의 경우
            else
            {
                newText += (chatData.sender + "가 당신에게: " + chatData.content);
            }
        }
        else if(chatData.type == ChatData.eType.System)
        {
            if (chatData.systemMessageType == ChatData.eSystemMessageType.Alert)
            {
                text.color = new Color(1.0f, 0.3839f, 0.2767f, 1.0f);
                newText = chatData.content;
            }
            else if(chatData.systemMessageType == ChatData.eSystemMessageType.Info)
            {
                text.color = new Color(0.8039f, 0.7450f, 0.2784f);
                newText = chatData.content;
            }
        }

        text.text = newText;
        lines = text.GetTextInfo(newText).lineCount;
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, 20 * lines);
    }

    public ChatData chatData;   // 해당 chatElement가 표현하는 chatData
    public int lines;   // 해당 채팅이 채팅창에서 차지하는 줄 수
    public TMPro.TMP_Text text;
}
