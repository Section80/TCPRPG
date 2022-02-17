using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMenu : MonoBehaviour
{
    public static DungeonMenu instance;

    public AlertMenu alertMenu;

    public Button slimeDungeonButton;
    public Button oakDungeonButton;

    public FindingMatchMenu findingMatchMenu;
    public FoundMatchMenu foundMatchMenu;
    public InviteFriendMenu inviteFriendMenu;

    public enum eStatus
    {
        Waiting,
        WaitingAccept,
        Entering
    }

    public eStatus status = eStatus.Waiting;

    public enum eDungeonName
    {
        None, SlimeDungeon, OakDungeon
    }

    private byte EDungeonNameToByte(eDungeonName dungeonName)
    {
        if(dungeonName == eDungeonName.SlimeDungeon)
        {
            return 0;
        }
        else if(dungeonName == eDungeonName.OakDungeon)
        {
            return 1;
        }

        return 99;
    }

    public eDungeonName selectedDungeon = eDungeonName.None;

    private void OnEnable()
    {
        slimeDungeonButton.image.color = new Color(1.0f, 1.0f, 1.0f);
        oakDungeonButton.image.color = new Color(1.0f, 1.0f, 1.0f);
        selectedDungeon = eDungeonName.None;

        if(instance == null)
        {
            instance = GetComponent<DungeonMenu>();
        }
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
        inviteFriendMenu.OnClickClose();
        alertMenu.CloseMenu();
    }
    
    public void OnClickSoloPlay()
    {
        if(selectedDungeon == eDungeonName.None)
        {
            alertMenu.gameObject.SetActive(true);
            alertMenu.contentText.text = "던전을 선택해 주세요.";
            return;
        }

        if(status == eStatus.Waiting)
        {
            //혼자하기 패킷 보내기
            byte[] outBuffer = new byte[3];
            outBuffer[0] = 13;
            outBuffer[1] = 1;
            outBuffer[2] = EDungeonNameToByte(selectedDungeon);

            ClientSocket.instance.SendRequest(outBuffer);

            foundMatchMenu.gameObject.SetActive(true);
            foundMatchMenu.SetUI(FoundMatchMenu.eStatus.WaitingRoomStart);
        }
    }

    public void OnClickFindMatch()
    {
        if (selectedDungeon == eDungeonName.None)
        {
            alertMenu.gameObject.SetActive(true);
            alertMenu.contentText.text = "던전을 선택해 주세요.";
            return;
        }

        if (status == eStatus.Waiting)
        {
            //매치 찾기 요청 패킷을 보낸다. 
            byte[] outBuffer = new byte[3];
            outBuffer[0] = 13;
            outBuffer[1] = 2;
            outBuffer[2] = EDungeonNameToByte(selectedDungeon);

            ClientSocket.instance.SendRequest(outBuffer);

            findingMatchMenu.gameObject.SetActive(true);
            findingMatchMenu.text.text = "매치 찾는 중...";
        }
    }

    public void OnClickInviteFriend()
    {
        if (selectedDungeon == eDungeonName.None)
        {
            alertMenu.gameObject.SetActive(true);
            alertMenu.contentText.text = "던전을 선택해 주세요.";
            return;
        }

        inviteFriendMenu.gameObject.SetActive(true);
        inviteFriendMenu.UpdateUI();
    }

    public void OnMatchFound()
    {
        findingMatchMenu.gameObject.SetActive(false);
        foundMatchMenu.gameObject.SetActive(true);
        foundMatchMenu.SetUI(FoundMatchMenu.eStatus.WaitingUserResponse);
    }

    public void OnMatchStartFailed()
    {
        foundMatchMenu.gameObject.SetActive(false);
        findingMatchMenu.gameObject.SetActive(true);

        findingMatchMenu.text.text = "다른 참가자가 매치를 거절했습니다. \n매치 찾는 중...";
    }

    public void OnClickSlimeDungeon()
    {
        slimeDungeonButton.image.color = new Color(0.5f, 0.5f, 0.5f);
        oakDungeonButton.image.color = new Color(1.0f, 1.0f, 1.0f);
        selectedDungeon = eDungeonName.SlimeDungeon;
    }

    public void OnClickOakDungeon()
    {
        oakDungeonButton.image.color = new Color(0.5f, 0.5f, 0.5f);
        slimeDungeonButton.image.color = new Color(1.0f, 1.0f, 1.0f);
        selectedDungeon = eDungeonName.OakDungeon;
    }
}
