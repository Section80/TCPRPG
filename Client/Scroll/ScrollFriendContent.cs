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
                element.isOnlineText.text = "온라인";
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
                element.isOnlineText.text = "오프라인";
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

        //child 중 해당 character_id를 가진 것을 찾는다. 
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

        //child 중 해당 character_id를 가진 것을 찾는다. 
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

        //child 중 해당 character_id를 가진 것을 찾는다. 
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

        //child 중 해당 character_id를 가진 것을 찾는다. 
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
        //친구가 삭제된 경우, friendElement를 삭제해야 한다.
        List<GameObject> toDestory = new List<GameObject>();

        //모든 child를 순회하면서 해당 friendData가 여전히 존재하는지 확인한다. 
        for(int i = 0; i < transform.childCount; i++)
        {
            bool exist = false;
            FriendElement element = transform.GetChild(i).GetComponent<FriendElement>();

            if (element.type == FriendElement.EType.Friend)
            {
                //해당 element가 여전히 friendData에 있는지 확인한다. 없으면 toDestroy에 추가한다. 
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

        //더이상 존재하지 않는 data에 해당하는 gameObject들을 삭제한다.
        foreach(GameObject go in toDestory)
        {
            DestroyImmediate(go);
        }


        //새로 추가된 friendData의 경우, 아직 friendElement가 존재하지 않는다. 이 경우 추가해야 한다. 
        //데이터에 해당하는 friendData가 존재하는 경우, 온라인 상태가 업데이트 되었을 수 있으니 텍스트를 업데이트 한다. 
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
                            element.isOnlineText.text = "온라인";
                        }
                        else
                        {
                            element.isOnlineText.text = "오프라인";
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
                    element.isOnlineText.text = "온라인";
                }
                else
                {
                    element.isOnlineText.text = "오프라인";
                }
            }
        }

        //width를 다시 조정한다.
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, User.instance.characterDatas.Count * 45.0f);
        //position을 다시 조정한다. 
        rectTransform.localPosition = new Vector3(0.0f, -40.0f, 0.0f);

        //child들의 위치를 설정한다. 
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject go = transform.GetChild(i).gameObject;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0.0f, +-2.5f - i * 45.0f, 0.0f);
        }
    }
}
