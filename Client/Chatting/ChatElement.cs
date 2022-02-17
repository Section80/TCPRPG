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
            newText += "[��ü]" + chatData.sender + ": " + chatData.content;

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

            //���� �ӼӸ��� ���
            if(chatData.sender == User.instance.nickname)
            {
                newText += ("�����" + chatData.receiver + "����: " + chatData.content);
            }
            //���� �ӼӸ��� ���
            else
            {
                newText += (chatData.sender + "�� ��ſ���: " + chatData.content);
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

    public ChatData chatData;   // �ش� chatElement�� ǥ���ϴ� chatData
    public int lines;   // �ش� ä���� ä��â���� �����ϴ� �� ��
    public TMPro.TMP_Text text;
}
