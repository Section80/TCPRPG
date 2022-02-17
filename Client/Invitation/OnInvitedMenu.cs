using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class OnInvitedMenu : MonoBehaviour
{
    public static OnInvitedMenu instance;

    public Text nicknameText;
    public Text dungeonTypeText;

    public void OnInvited(int characterID, DungeonMenu.eDungeonName dungeonName)
    {
        invitedDatas.Add(new InvitedData(characterID, dungeonName));

        if(gameObject.activeInHierarchy == false)
        {
            gameObject.SetActive(true);
        }

        currentData = instance.getFriendDataByID(characterID);

        updateUI();
    }

    public void OnCancleInvite(int inviterID)
    {
        InvitedData data = null;
        foreach(InvitedData iData in invitedDatas)
        {
            if(iData.characterID == inviterID)
            {
                data = iData;
            }
        }

        invitedDatas.Remove(data);

        if(currentData.character_id == inviterID)
        {
            if (invitedDatas.Count > 0)
            {
                currentData = getFriendDataByID(invitedDatas[invitedDatas.Count - 1].characterID);
            }
            else
            {
                currentData = null;
            }
            updateUI();
        }
    }

    public void OnClickAccept()
    {
        byte[] outBuffer = new byte[6];

        outBuffer[0] = 14;
        outBuffer[1]  = 2;
        byte[] intBuffer = BitConverter.GetBytes(currentData.character_id);
        Buffer.BlockCopy(intBuffer, 0, outBuffer, 2, intBuffer.Length);

        ClientSocket.instance.SendRequest(outBuffer);

        //새로운 던전 갈 준비
        DungeonMenu.instance.selectedDungeon = invitedDatas[invitedDatas.Count - 1].dungeonName;

        invitedDatas.Clear();

        updateUI();
    }

    public void OnClickReject()
    {
        byte[] outBuffer = new byte[6];

        outBuffer[0] = 14;
        outBuffer[1] = 1;
        byte[] intBuffer = BitConverter.GetBytes(currentData.character_id);
        Buffer.BlockCopy(intBuffer, 0, outBuffer, 2, intBuffer.Length);

        ClientSocket.instance.SendRequest(outBuffer);

        invitedDatas.Remove(invitedDatas[invitedDatas.Count - 1]);
        if (invitedDatas.Count > 0)
        {
            currentData = getFriendDataByID(invitedDatas[invitedDatas.Count - 1].characterID);
        }
        else
        {
            currentData = null;
        }
        updateUI();
    }

    private void updateUI()
    {
        if(invitedDatas.Count == 0 || currentData == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            nicknameText.text = currentData.nickname;
            if(invitedDatas[invitedDatas.Count - 1].dungeonName == DungeonMenu.eDungeonName.SlimeDungeon)
            {
                dungeonTypeText.text = "슬라임 던전";
            }
            else if(invitedDatas[invitedDatas.Count - 1].dungeonName == DungeonMenu.eDungeonName.OakDungeon)
            {
                dungeonTypeText.text = "오크 던전";
            }
        }
    }

    private void OnEnable()
    {
        instance = GetComponent<OnInvitedMenu>();
    }

    private FriendData getFriendDataByID(int characterID)
    {
        foreach(FriendData iData in User.instance.friendDatas)
        {
            if(iData.character_id == characterID)
            {
                return iData;
            }
        }

        return null;
    }

    private void Awake()
    {
        if(invitedDatas == null)
        {
            invitedDatas = new List<InvitedData>();
        }
        
    }

    private List<InvitedData> invitedDatas = null;
    private FriendData currentData;
}
