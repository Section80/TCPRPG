using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class HudUI : MonoBehaviour
{
    public static HudUI instance;

    public Text levelText;
    public Image hpBar;
    public Text hpText;
    public Image mpBar;
    public Text mpText;
    public Image expBar;
    public Text expText;
    public CharacterInfoMenu characterInfoMenu;

    public GameObject friendMenu;
    public GameObject inventoryMenu;
    public GameObject itemInfoMenu;
    public GameObject statusMenu;
    public OnInvitedMenu onInvitedMenu;

    public BackToTownUI backToTownUI;

    public void onFriendMenuClicked()
    {
        if(friendMenu)
        {
            friendMenu.SetActive(true);
        }
    }

    public void onCloseFriendMenuClicked()
    {
        if(friendMenu)
        {
            friendMenu.SetActive(false);
        }
    }

    public void onInventoryMenuClicked()
    {
        if(inventoryMenu)
        {
            inventoryMenu.SetActive(true);
            InventoryMenu.OrganizeChild();
        }
    }

    public void onCloseInventoryMenuClicked()
    {
        if(inventoryMenu)
        {
            inventoryMenu.SetActive(false);
        }
    }

    public void onStatusMenuClicked()
    {
        if(statusMenu)
        {
            statusMenu.SetActive(true);
            statusMenu.GetComponent<StatusMenu>().Organize();
        }
    }

    public void onCloseStatusMenuClicked()
    {
        if(statusMenu)
        {
            statusMenu.SetActive(false);
        }
    }

    public void showCharacterInfoMenu(string nickname, int character_id)
    {
        characterInfoMenu.gameObject.SetActive(true);
        characterInfoMenu.nicknameText.text = nickname;
        characterInfoMenu.selected_character_id = character_id;

        //이미 친구인 캐릭터이면 버튼은 비활성화 한다.
        bool isFriend = false;
        foreach(FriendData data in User.instance.friendDatas)
        {
            if(nickname.Equals(data.nickname))
            {
                isFriend = true;
            }
        }

        if(isFriend)
        {
            characterInfoMenu.friendRequestButton.enabled = false;
        } else
        {
            characterInfoMenu.friendRequestButton.enabled = true;
        }
    }

    public void onClickFriendRequestButton()
    {
        bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;

        HudUI.instance.onFriendMenuClicked();
        //동인한 친구 요청이 없는 경우에만 
        if (!ScrollFriendContent.instance.isThereWaiting(characterInfoMenu.selected_character_id))
        {
            ScrollFriendContent.instance.addWaiting(characterInfoMenu.nicknameText.text, characterInfoMenu.selected_character_id);

            byte[] outBuffer = new byte[6];
            outBuffer[0] = 10;
            outBuffer[1] = 0;

            byte[] intByte = BitConverter.GetBytes(characterInfoMenu.selected_character_id);
            Buffer.BlockCopy(intByte, 0, outBuffer, 2, 4);

            ClientSocket.instance.SendRequest(outBuffer);

            //todo: 친구 요청을 보냈습니다 알림
            Debug.Log("친구 요청을 보냈습니다.");
            closeCharacterInfoMenu();
        }

        if (!isMenuOpend)
        {
            HudUI.instance.onCloseFriendMenuClicked();
        }
    }

    public void closeCharacterInfoMenu()
    {
        characterInfoMenu.gameObject.SetActive(false);
    }



    // Update is called once per frame
    void Update()
    {
        if (User.instance != null)
        {
            if (User.instance.entity != null)
            {
                Entity entity = User.instance.entity;
                hpText.text = 0.ToString();

                expText.text = User.instance.selectedCharacter.exp.ToString();
                //expBar.rectTransform.sizeDelta = new Ve
            }

            UserEntity ue = User.instance.entity;

            if (Room.instance.id != 0)
            {
                mpText.text = ue.mp.ToString();
                hpText.text = ue.hp.ToString();
                if (ue.max_hp != 0)
                {
                    hpBar.rectTransform.sizeDelta = new Vector2((ue.hp / (float)ue.max_hp) * 300.0f, 30.0f);
                }

                if (ue.max_mp != 0)
                {
                    mpBar.rectTransform.sizeDelta = new Vector2((ue.mp / (float)ue.max_mp) * 300.0f, 20.0f);
                }
            }
            else
            {
                int strong = User.instance.strong;
                int intellect = User.instance.intellect;

                ItemData weapon = Inventory.instance.getWeapon();
                ItemData armor = Inventory.instance.getArmor();
                
                if (weapon != null)
                {
                    strong += weapon.strong;
                    intellect += weapon.intellect;
                }
                if (armor != null)
                {
                    strong += armor.strong;
                    intellect += armor.intellect;
                }

//                Debug.Log(strong + " " + intellect);

                int maxHP = 100 + strong * 10;
                int maxMP = 100 + intellect * 10;

                hpText.text = maxHP.ToString();
                mpText.text = maxMP.ToString();

                hpBar.rectTransform.sizeDelta = new Vector2(300.0f, 30.0f);
                mpBar.rectTransform.sizeDelta = new Vector2(300.0f, 20.0f);
            }

            int maxExp = User.instance.level * 50;

            expText.text = User.instance.exp.ToString() + " / " + (maxExp).ToString();
            expBar.rectTransform.sizeDelta = new Vector2((User.instance.exp / (float)maxExp) * 1024.0f, 20.0f);

            levelText.text = "Lv. " + User.instance.level.ToString();
        }
    }

    public void toTown()
    {
        Room.instance.userDatas.Clear();    //현재 룸에 있는 유저 목록 초기화하기
        Room.instance.room_id_to_move = 0; //이동할 룸의 id 설정
        Room.instance.isGettingInput = false;    //동기화 데이터 받지 않게 설정하기
        ClientInput.instance.isReady = false;    //다른 룸으로 이동 완료 전까지 키보드 데이터를 보내지 않음

        byte[] outBuffer = new byte[2];
        outBuffer[0] = 8;
        outBuffer[1] = 0;
        ClientSocket.instance.SendRequest(outBuffer);

        byte[] outBuffer2 = new byte[1 + 4 + 1];
        outBuffer2[0] = 9;
        byte[] intBytes = BitConverter.GetBytes(0);
        Buffer.BlockCopy(intBytes, 0, outBuffer2, 1, 4);
        outBuffer2[5] = (byte)0;
        ClientSocket.instance.SendRequest(outBuffer2);
    }

    public void GetItemList()
    {
        StartCoroutine(getItemListCoroutine());
    }

    private IEnumerator getItemListCoroutine()
    {
        WWWForm form = new WWWForm();
        // 데이터 넣기 예시
        form.AddField("characterID", User.instance.character_id);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/getItemList.php", form);
        yield return www.SendWebRequest();

        //연결 에러
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("http request connection error");
        }
        //성공한 경우
        else
        {
            JObject decoded = null;

            //응답 json string parsing하기
            try
            {
                decoded = JObject.Parse(www.downloadHandler.text);
            }
            catch(Exception e)
            {
                e.ToString();
                Debug.Log(www.downloadHandler.text);
            }

            //key로 value얻기. int말고 다른 타입도 가능
            bool success = decoded["success"].ToObject<bool>();

            if (success == true)
            {
                Inventory.instance.Clear();

                //배열의 경우
                JToken datas = decoded["data"];
                foreach (JToken data in datas)
                {
                    ItemData itemData = new ItemData();

                    itemData.id = data["id"].ToObject<int>();
                    itemData.index_in_inventory = data["index_in_inventory"].ToObject<int>();
                    itemData.name = data["name"].ToObject<string>();
                    int isWeapon = data["is_weapon"].ToObject<int>();

                    if(isWeapon == 1)
                    {
                        itemData.is_weapon = true;
                    }
                    else if(isWeapon == 0)
                    {
                        itemData.is_weapon = false;
                    }

                    itemData.job = data["job"].ToObject<int>();
                    itemData.level_limit = data["level_limit"].ToObject<int>();
                    itemData.strong = data["strong"].ToObject<int>();
                    itemData.dexility = data["dexility"].ToObject<int>();
                    itemData.intellect = data["intellect"].ToObject<int>();
                    itemData.luck = data["luck"].ToObject<int>();

                    Inventory.instance.AddItemData(itemData);
                }

                InventoryMenu.OrganizeChild();
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    private void Awake()
    {
        instance = GetComponent<HudUI>();

        inventoryMenu.gameObject.SetActive(true);
        inventoryMenu.gameObject.SetActive(false);

        friendMenu.gameObject.SetActive(true);
        friendMenu.gameObject.SetActive(false);

        itemInfoMenu.gameObject.SetActive(true);
        itemInfoMenu.gameObject.SetActive(false);

        statusMenu.gameObject.SetActive(true);
        statusMenu.gameObject.SetActive(false);
    }
}
