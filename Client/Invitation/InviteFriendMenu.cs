using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InviteFriendMenu : MonoBehaviour
{
    public static InviteFriendMenu instance;

    // User.instance.friendDatas�� ����
    // transform�� invitableFriendElement�� �߰��Ѵ�. 
    public void UpdateUI()
    {
        int count = 0;
        //child��ü�� ��� �ı��Ѵ�. 
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

    //ģ�� �ʴ� ���� ��Ŷ�� �޾��� �� ȣ��Ǵ� �Լ�
    public void OnInviteFail()
    {
        OnInviteMenu.instance.gameObject.SetActive(false);
        alertMenu.contentText.text = "������ �̹� ���� ���Դϴ�. ";
        alertMenu.gameObject.SetActive(true);
    }

    //ģ�� �ʴ븦 �����޾��� �� ȣ��Ǵ� �Լ�
    public void OnRejectInvite()
    {
        onInviteMenu.gameObject.SetActive(false);

        OnInviteMenu.instance.gameObject.SetActive(false);
        alertMenu.contentText.text = "������ �ʴ븦 �����߽��ϴ�. ";
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
