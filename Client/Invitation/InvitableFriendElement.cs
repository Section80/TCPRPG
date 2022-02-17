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

    // �ʴ� ��ư ������ ��
    public void OnClickInvite()
    {
        FriendData friendData = getFriendDataByID(characterID);

        // ģ���� ������ ģ������ Ȯ���Ѵ�
        if(friendData == null)
        {
            InviteFriendMenu.instance.alertMenu.gameObject.SetActive(true);
            InviteFriendMenu.instance.alertMenu.contentText.text = "������ ģ���� ������ �ʴ��� �� �����ϴ�. ";
            InviteFriendMenu.instance.UpdateUI();
            return;
        }

        // ģ���� ������ �¶����� �ƴ� ���
        if (!friendData.isOnline) {
            InviteFriendMenu.instance.alertMenu.gameObject.SetActive(true);
            InviteFriendMenu.instance.alertMenu.contentText.text = "ģ���� �¶����� �ƴմϴ�. ";
            InviteFriendMenu.instance.UpdateUI();
            return;
        }

        //ģ�� �ʴ� ��Ŷ�� ������. 
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
