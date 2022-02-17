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

        //�̹� ģ���� ĳ�����̸� ��ư�� ��Ȱ��ȭ �Ѵ�.
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
        //������ ģ�� ��û�� ���� ��쿡�� 
        if (!ScrollFriendContent.instance.isThereWaiting(characterInfoMenu.selected_character_id))
        {
            ScrollFriendContent.instance.addWaiting(characterInfoMenu.nicknameText.text, characterInfoMenu.selected_character_id);

            byte[] outBuffer = new byte[6];
            outBuffer[0] = 10;
            outBuffer[1] = 0;

            byte[] intByte = BitConverter.GetBytes(characterInfoMenu.selected_character_id);
            Buffer.BlockCopy(intByte, 0, outBuffer, 2, 4);

            ClientSocket.instance.SendRequest(outBuffer);

            //todo: ģ�� ��û�� ���½��ϴ� �˸�
            Debug.Log("ģ�� ��û�� ���½��ϴ�.");
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
        Room.instance.userDatas.Clear();    //���� �뿡 �ִ� ���� ��� �ʱ�ȭ�ϱ�
        Room.instance.room_id_to_move = 0; //�̵��� ���� id ����
        Room.instance.isGettingInput = false;    //����ȭ ������ ���� �ʰ� �����ϱ�
        ClientInput.instance.isReady = false;    //�ٸ� ������ �̵� �Ϸ� ������ Ű���� �����͸� ������ ����

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
        // ������ �ֱ� ����
        form.AddField("characterID", User.instance.character_id);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/getItemList.php", form);
        yield return www.SendWebRequest();

        //���� ����
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("http request connection error");
        }
        //������ ���
        else
        {
            JObject decoded = null;

            //���� json string parsing�ϱ�
            try
            {
                decoded = JObject.Parse(www.downloadHandler.text);
            }
            catch(Exception e)
            {
                e.ToString();
                Debug.Log(www.downloadHandler.text);
            }

            //key�� value���. int���� �ٸ� Ÿ�Ե� ����
            bool success = decoded["success"].ToObject<bool>();

            if (success == true)
            {
                Inventory.instance.Clear();

                //�迭�� ���
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
