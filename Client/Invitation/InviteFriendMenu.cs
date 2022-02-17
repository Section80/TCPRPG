using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InviteFriendMenu : MonoBehaviour
{
    public static InviteFriendMenu instance;

    // User.instance.friendDatas에 따라서
    // transform에 invitableFriendElement를 추가한다. 
    public void UpdateUI()
    {
        int count = 0;
        //child객체를 모두 파괴한다. 
        while (contentTransform.childCount != 0)
        {
            DestroyImmediate(contentTransform.GetChild(0).gameObject);
        }

        foreach (FriendData iFriendData in User.instance.friendDatas)
        {
            if (iFriendData.isOnline)
            {
                GameObject element = Instantiate(invitableFriendElement, contentTransform, false);
                InvitableFriendElement _element = element.GetComponent<InvitableFriendElement>();
                _element.SetLocalPosition(count);
                _element.nicknameText.text = iFriendData.nickname;
                _element.characterID = iFriendData.character_id;

                count++;
            }

            contentTransform.sizeDelta = new Vector2(250, 60 * count);
        }
    }

    //친구 초대 실패 패킷을 받았을 때 호출되는 함수
    public void OnInviteFail()
    {
        OnInviteMenu.instance.gameObject.SetActive(false);
        alertMenu.contentText.text = "유저가 이미 게임 중입니다. ";
        alertMenu.gameObject.SetActive(true);
    }

    //친구 초대를 거절받았을 때 호출되는 함수
    public void OnRejectInvite()
    {
        onInviteMenu.gameObject.SetActive(false);

        OnInviteMenu.instance.gameObject.SetActive(false);
        alertMenu.contentText.text = "상대방이 초대를 거절했습니다. ";
        alertMenu.gameObject.SetActive(true);
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }

    public AlertMenu alertMenu;
    public RectTransform contentTransform;
    public GameObject invitableFriendElement;
    public OnInviteMenu onInviteMenu;

    private void Awake()
    {
        instance = GetComponent<InviteFriendMenu>();
    }

    private void OnEnable()
    {
        instance = GetComponent<InviteFriendMenu>();
    }
}
