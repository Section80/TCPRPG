using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollFriendContent : MonoBehaviour
{
    public static ScrollFriendContent instance;

    public GameObject friendElement;
    public RectTransform rectTransform;

    public void Awake()
    {
        instance = GetComponent<ScrollFriendContent>();
    }

    public bool isThereWaiting(int character_id)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            FriendElement element = transform.GetChild(i).GetComponent<FriendElement>();

            if(element.type == FriendElement.EType.Waiting)
            {
                if(character_id == element.character_id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void Start()
    {
        //
    }

    public void onFriendOnline(int character_id)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            FriendElement element = transform.GetChild(i).GetComponent<FriendElement>();

            if (element.character_id == character_id)
            {
                element.isOnlineText.text = "�¶���";
            }
        }
    }

    public void onFriendOffline(int character_id)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            FriendElement element = transform.GetChild(i).GetComponent<FriendElement>();

            if (element.character_id == character_id)
            {
                element.isOnlineText.text = "��������";
            }
        }
    }

    public void addWaiting(string nickname, int character_id)
    {
        FriendElement element = Instantiate(friendElement, transform).GetComponent<FriendElement>();
        element.SetType(FriendElement.EType.Waiting);
        element.nicknameText.text = nickname;
        element.character_id = character_id;

        OrganizeChild();
    }

    public void addRequest(string nickname, int character_id)
    {
        FriendElement element = Instantiate(friendElement, transform).GetComponent<FriendElement>();
        element.SetType(FriendElement.EType.Request);
        element.nicknameText.text = nickname;
        element.character_id = character_id;

        OrganizeChild();
    }

    public void handleCancleRequest(int character_id)
    {
        FriendElement element = null;

        //child �� �ش� character_id�� ���� ���� ã�´�. 
        for(int i = 0; i < transform.childCount; i++)
        {
            FriendElement _element = transform.GetChild(i).GetComponent<FriendElement>();

            if(_element.character_id == character_id)
            {
                element = _element;
            }
        }

        DestroyImmediate(element.gameObject);
        OrganizeChild();
    }

    public void handleAcceptRequest(int character_id)
    {
        FriendElement element = null;

        //child �� �ش� character_id�� ���� ���� ã�´�. 
        for (int i = 0; i < transform.childCount; i++)
        {
            FriendElement _element = transform.GetChild(i).GetComponent<FriendElement>();

            if (_element.character_id == character_id)
            {
                element = _element;
            }
        }

        element.SetType(FriendElement.EType.Friend);
        User.instance.friendDatas.Add(new FriendData(element.nicknameText.text, character_id, true));
        OrganizeChild();
    }

    public void handleRejectRequest(int character_id) 
    {
        FriendElement element = null;

        //child �� �ش� character_id�� ���� ���� ã�´�. 
        for (int i = 0; i < transform.childCount; i++)
        {
            FriendElement _element = transform.GetChild(i).GetComponent<FriendElement>();

            if (_element.character_id == character_id)
            {
                element = _element;
            }
        }

        DestroyImmediate(element.gameObject);
        OrganizeChild();
    }

    public void handleDeleteFriend(int character_id)
    {
        FriendElement element = null;

        //child �� �ش� character_id�� ���� ���� ã�´�. 
        for (int i = 0; i < transform.childCount; i++)
        {
            FriendElement _element = transform.GetChild(i).GetComponent<FriendElement>();

            if (_element.character_id == character_id)
            {
                element = _element;
            }
        }

        DestroyImmediate(element.gameObject);

        for(int i = 0; i < User.instance.friendDatas.Count; i++)
        {
            FriendData data = User.instance.friendDatas[i];

            if(data.character_id == character_id)
            {
                User.instance.friendDatas.Remove(data);
            }
        }

        OrganizeChild();
    }

    public void OrganizeChild()
    {
        //ģ���� ������ ���, friendElement�� �����ؾ� �Ѵ�.
        List<GameObject> toDestory = new List<GameObject>();

        //��� child�� ��ȸ�ϸ鼭 �ش� friendData�� ������ �����ϴ��� Ȯ���Ѵ�. 
        for(int i = 0; i < transform.childCount; i++)
        {
            bool exist = false;
            FriendElement element = transform.GetChild(i).GetComponent<FriendElement>();

            if (element.type == FriendElement.EType.Friend)
            {
                //�ش� element�� ������ friendData�� �ִ��� Ȯ���Ѵ�. ������ toDestroy�� �߰��Ѵ�. 
                foreach (FriendData data in User.instance.friendDatas)
                {
                    if (element.nicknameText.text == data.nickname)
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    toDestory.Add(element.gameObject);
                }
            }
        }

        //���̻� �������� �ʴ� data�� �ش��ϴ� gameObject���� �����Ѵ�.
        foreach(GameObject go in toDestory)
        {
            DestroyImmediate(go);
        }


        //���� �߰��� friendData�� ���, ���� friendElement�� �������� �ʴ´�. �� ��� �߰��ؾ� �Ѵ�. 
        //�����Ϳ� �ش��ϴ� friendData�� �����ϴ� ���, �¶��� ���°� ������Ʈ �Ǿ��� �� ������ �ؽ�Ʈ�� ������Ʈ �Ѵ�. 
        foreach (FriendData data in User.instance.friendDatas)
        {
            bool exist = false;

            for(int i = 0; i < transform.childCount; i++)
            {
                FriendElement element = transform.GetChild(i).GetComponent<FriendElement>();
                element.type = FriendElement.EType.Friend;
                if (element.type == FriendElement.EType.Friend)
                {
                    if (element.nicknameText.text == data.nickname)
                    {
                        if (data.isOnline)
                        {
                            element.isOnlineText.text = "�¶���";
                        }
                        else
                        {
                            element.isOnlineText.text = "��������";
                        }
                        exist = true;
                    }
                }
            }

            if(!exist)
            {
                GameObject gameObject = Instantiate(friendElement, transform);
                FriendElement element = gameObject.GetComponent<FriendElement>();
                element.SetType(FriendElement.EType.Friend);
                element.nicknameText.text = data.nickname;
                element.character_id = data.character_id;
                if (data.isOnline)
                {
                    element.isOnlineText.text = "�¶���";
                }
                else
                {
                    element.isOnlineText.text = "��������";
                }
            }
        }

        //width�� �ٽ� �����Ѵ�.
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, User.instance.characterDatas.Count * 45.0f);
        //position�� �ٽ� �����Ѵ�. 
        rectTransform.localPosition = new Vector3(0.0f, -40.0f, 0.0f);

        //child���� ��ġ�� �����Ѵ�. 
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject go = transform.GetChild(i).gameObject;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0.0f, +-2.5f - i * 45.0f, 0.0f);
        }
    }
}
