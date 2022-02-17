using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class InvitableFriendElement : MonoBehaviour
{
    public static int invitedCharacterID = 0;
    public Text nicknameText;
    public int characterID = 0;

    // 초대 버튼 눌렀을 때
    public void OnClickInvite()
    {
        FriendData friendData = getFriendDataByID(characterID);

        // 친구가 여전히 친구인지 확인한다
        if(friendData == null)
        {
            InviteFriendMenu.instance.alertMenu.gameObject.SetActive(true);
            InviteFriendMenu.instance.alertMenu.contentText.text = "상대방이 친구를 삭제해 초대할 수 없습니다. ";
            InviteFriendMenu.instance.UpdateUI();
            return;
        }

        // 친구가 여전히 온라인이 아닌 경우
        if (!friendData.isOnline) {
            InviteFriendMenu.instance.alertMenu.gameObject.SetActive(true);
            InviteFriendMenu.instance.alertMenu.contentText.text = "친구가 온라인이 아닙니다. ";
            InviteFriendMenu.instance.UpdateUI();
            return;
        }

        //친구 초대 패킷을 보낸다. 
        byte[] outBuffer = new byte[7];
        outBuffer[0] = 14;
        outBuffer[1] = 0;
        byte[] intBuffer = BitConverter.GetBytes(characterID);
        Buffer.BlockCopy(intBuffer, 0, outBuffer, 2, 4);
        
        if(DungeonMenu.instance.selectedDungeon == DungeonMenu.eDungeonName.SlimeDungeon)
        {
            outBuffer[6] = 0;
        }
        else if(DungeonMenu.instance.selectedDungeon == DungeonMenu.eDungeonName.OakDungeon)
        {
            outBuffer[6] = 1;
        }

        ClientSocket.instance.SendRequest(outBuffer);

        InviteFriendMenu inviteMenu = InviteFriendMenu.instance;
        OnInviteMenu menu = inviteMenu.onInviteMenu;
        menu.gameObject.SetActive(true);

        invitedCharacterID = characterID;
    }

    public void SetLocalPosition(int index)
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.localPosition = new Vector3(0, -index * 60.0f, 0);
    }

    private FriendData getFriendDataByID(int characterID)
    {
        foreach(FriendData iFriendData in User.instance.friendDatas)
        {
            if(iFriendData.character_id == characterID)
            {
                return iFriendData;
            }
        }

        return null;
    }
}
