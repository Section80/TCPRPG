using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChattingTap : MonoBehaviour
{
    public Transform content;
    public int index;

    public string tapName;

    public bool bShowAll;
    public bool bShowNormal;
    public bool bShowDirect;
    public bool bShowSystem;
    public bool bOffAlert;

    public void OnSendChat(ChatData chatData)
    {
        if(chatData.type == ChatData.eType.All && !bShowAll)
        {
            return;
        }

        if (chatData.type == ChatData.eType.Normal && !bShowNormal)
        {
            return;
        }

        if (chatData.type == ChatData.eType.Direct && !bShowDirect)
        {
            return;
        }

        if (chatData.type == ChatData.eType.System && !bShowSystem)
        {
            return;
        }

        if(chatData.type == ChatData.eType.System)
        {
            return;
        }

        AddElement(chatData);
    }

    public void OnGetChat(ChatData chatData) 
    {
        if (chatData.type == ChatData.eType.All && !bShowAll)
        {
            return;
        }

        if (chatData.type == ChatData.eType.Normal && !bShowNormal)
        {
            return;
        }

        if (chatData.type == ChatData.eType.Direct && !bShowDirect)
        {
            return;
        }

        if (chatData.type == ChatData.eType.System && !bShowSystem)
        {
            return;
        }

        AddElement(chatData);

        //자기가 보낸 메세시가 알람을 활성화 시킬 필요는 없다.
        if (ChattingMenu.instance.currentTap != this)
        {
            bNewMessage = true;
        }
    }

    public void OnSystemMessage(ChatData chatData) {
        if (chatData.type == ChatData.eType.System && !bShowSystem)
        {
            return;
        }

        AddElement(chatData);
    }

    public void AddElement(ChatData chatData)
    {
        GameObject newElement = Instantiate(ChattingMenu.instance.chatElementPrefap, content, false);
        ChatElement newChatElement = newElement.GetComponent<ChatElement>();
        newChatElement.UpdateUI(chatData);
        newElement.transform.SetSiblingIndex(0);

        if(content.childCount > 10)
        {
            GameObject go = content.GetChild(content.childCount - 1).gameObject;
            DestroyImmediate(go);
        }


        int localY = 0;

        for(int i = 0; i < content.childCount; i++)
        {
            Transform transform = content.GetChild(i);
            ChatElement element = transform.GetComponent<ChatElement>();

            element.GetComponent<RectTransform>().localPosition = new Vector3(0, localY + 2, 0);
            localY += element.lines * 20;
        }

        RectTransform rect = content.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, localY);
    }

    public void Clear()
    {
        while(content.childCount != 0)
        {
            DestroyImmediate(content.GetChild(0).gameObject);
        }
    }

    private bool bNewMessage;
}
