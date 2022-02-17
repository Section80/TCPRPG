using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FriendElement : MonoBehaviour
{
    public enum EType
    {
        Friend, //이미 친구인 유저
        Request, //친구 요청을 받은 경우
        Waiting, //유저가 친구 요청을 보냈고 응답을 기다리는 경우
        None
    }

    public EType type = EType.None;

    public Text nicknameText;
    public Text isOnlineText;
    public int character_id;
    public GameObject friendObject; //친구인 경우 active 해야 하는 object
    public GameObject requestObject;    //친구 요청인 경우 active 해야 하는 object
    public GameObject waitingObject;

    public Button deleteButton;
    public Button cancleRequestButton;
    public Button acceptButton;
    public Button rejectButton;

    public void SetType(EType type)
    {
        if(type == EType.Friend)
        {
            friendObject.SetActive(true);
            requestObject.SetActive(false);
            waitingObject.SetActive(false);

            deleteButton.onClick.AddListener(delegate { onClickDelete(); });
            type = EType.Friend;
        } else if(type == EType.Request)
        {
            requestObject.SetActive(true);
            friendObject.SetActive(false);
            waitingObject.SetActive(false);
            type = EType.Request;

            acceptButton.onClick.AddListener(delegate { onClickAccept(); });
            rejectButton.onClick.AddListener(delegate { onClickReject(); });
        } else if(type == EType.Waiting)
        {
            requestObject.SetActive(false);
            friendObject.SetActive(false);
            waitingObject.SetActive(true);
            type = EType.Waiting;
            cancleRequestButton.onClick.AddListener(delegate { onClickCancle(); });
        }
    }

    public void onClickCancle()
    {
        byte[] outBuffer = new byte[6];
        outBuffer[0] = 10;
        outBuffer[1] = 1;

        byte[] to = BitConverter.GetBytes(character_id);
        Buffer.BlockCopy(to, 0, outBuffer, 2, 4);

        ClientSocket.instance.SendRequest(outBuffer);
        DestroyImmediate(gameObject);
        ScrollFriendContent.instance.OrganizeChild();
    }

    public void onClickAccept()
    {
        byte[] outBuffer = new byte[6];
        outBuffer[0] = 10;
        outBuffer[1] = 2;

        byte[] from = BitConverter.GetBytes(character_id);
        Buffer.BlockCopy(from, 0, outBuffer, 2, 4);

        ClientSocket.instance.SendRequest(outBuffer);
        User.instance.friendDatas.Add(new FriendData(nicknameText.text, character_id, true));
        DestroyImmediate(gameObject);
        ScrollFriendContent.instance.OrganizeChild();
    }

    public void onClickReject()
    {
        byte[] outBuffer = new byte[6];
        outBuffer[0] = 10;
        outBuffer[1] = 3;

        byte[] from = BitConverter.GetBytes(character_id);
        Buffer.BlockCopy(from, 0, outBuffer, 2, 4);

        ClientSocket.instance.SendRequest(outBuffer);
        DestroyImmediate(gameObject);
        ScrollFriendContent.instance.OrganizeChild();
    }

    public void onClickDelete()
    {
        byte[] outBuffer = new byte[6];
        outBuffer[0] = 10;
        outBuffer[1] = 4;

        byte[] who = BitConverter.GetBytes(character_id);
        Buffer.BlockCopy(who, 0, outBuffer, 2, 4);

        ClientSocket.instance.SendRequest(outBuffer);
        DestroyImmediate(gameObject);

        for(int i = 0; i < User.instance.friendDatas.Count; i++)
        {
            FriendData data = User.instance.friendDatas[i];
            if(data.character_id == character_id)
            {
                User.instance.friendDatas.Remove(data);
            }
        }

        ScrollFriendContent.instance.OrganizeChild();
    }
}
