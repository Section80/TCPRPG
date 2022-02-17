using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;


public class ItemElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Image image;

    public EType type = EType.None;
    public ItemData itemData;

    public enum EType
    {
        None,
        WarriorWeapon,
        MagicianWeapon,
        Armor
    }

    public void setType(EType type)
    {
        this.type = type;
        bool isActive = HudUI.instance.inventoryMenu.activeInHierarchy;

        HudUI.instance.inventoryMenu.SetActive(true);
        
        if(type == EType.WarriorWeapon)
        {
            image.sprite = InventoryMenu.instance.warriorWeaponImage;
        }
        else if(type == EType.MagicianWeapon)
        {
            image.sprite = InventoryMenu.instance.magicianWeaponImage;
        }
        else if(type == EType.Armor)
        {
            image.sprite = InventoryMenu.instance.armorImage;
        }
        else if(type == EType.None)
        {
            image.sprite = null;
            image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        HudUI.instance.inventoryMenu.SetActive(isActive);
    }

    public void initialize(ItemData itemData)
    {
        this.itemData = itemData;

        if(itemData == null)
        {
            setType(EType.None);
            return;
        }

        if(itemData.is_weapon)
        {
            if(itemData.job == 0)
            {
                setType(EType.WarriorWeapon);
            } else if (itemData.job == 1) {
                setType(EType.MagicianWeapon);
            }
        } else
        {
            setType(EType.Armor);
        }


        if (User.instance.weapon_id == itemData.id)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else if (User.instance.armor_id == itemData.id)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            image.color = new Color(1.0f, 1.0f, 1.0f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (type != EType.None)
        {
            HudUI.instance.itemInfoMenu.SetActive(true);
            HudUI.instance.itemInfoMenu.GetComponent<ItemInfoMenu>().setText(itemData);
            HudUI.instance.itemInfoMenu.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HudUI.instance.itemInfoMenu.SetActive(false);
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        bool isActive = HudUI.instance.statusMenu.activeInHierarchy;

        //Ÿ�� Ȯ��
        if (type == EType.None)
        {
            return;
        }

        //���� ���� �������� ���
        if (User.instance.weapon_id == itemData.id)
        {
            User.instance.weapon_id = 0;

            HudUI.instance.statusMenu.SetActive(true);
            StatusMenu.instance.Organize();

            HudUI.instance.statusMenu.SetActive(isActive);
            image.color = new Color(1f, 1f, 1f);

            //���� �ڵ�
            /*
            Byte[] outBuffer = new byte[7];
            outBuffer[0] = 11;
            outBuffer[1] = 0;
            outBuffer[2] = 1;
            byte[] intBytes = BitConverter.GetBytes(0);
            Buffer.BlockCopy(intBytes, 0, outBuffer, 3, 4);
            ClientSocket.instance.SendRequest(outBuffer);
            */

            StartCoroutine(equipItemCoroutine(0));

            return;

        }
        if (User.instance.armor_id == itemData.id)
        {
            User.instance.armor_id = 0;

            HudUI.instance.statusMenu.SetActive(true);
            StatusMenu.instance.Organize();

            HudUI.instance.statusMenu.SetActive(isActive);
            image.color = new Color(1f, 1f, 1f);

            //���� �ڵ�
            /*
            Byte[] outBuffer = new byte[7];
            outBuffer[0] = 11;
            outBuffer[1] = 0;
            outBuffer[2] = 0;
            byte[] intBytes = BitConverter.GetBytes(0);
            Buffer.BlockCopy(intBytes, 0, outBuffer, 3, 4);
            ClientSocket.instance.SendRequest(outBuffer);
            */

            StartCoroutine(equipItemCoroutine(0));
            return;
        }

        //���� Ȯ��
        if (User.instance.level < itemData.level_limit)
        {
            return;
        }

        //������ ���: ���� Ȯ��
        if (itemData.is_weapon == true)
        {
            if (User.instance.job != itemData.job)
            {
                return;
            }
            else
            {
                User.instance.weapon_id = itemData.id;
                /*
                Byte[] outBuffer = new byte[7];
                outBuffer[0] = 11;
                outBuffer[1] = 0;
                outBuffer[2] = 1;
                byte[] intBytes = BitConverter.GetBytes(itemData.id);
                Buffer.BlockCopy(intBytes, 0, outBuffer, 3, 4);
                ClientSocket.instance.SendRequest(outBuffer);
                */
                StartCoroutine(equipItemCoroutine(itemData.id));
            }
        } 
        //���� ���� ������ ��� ������ Ȯ���ϸ� ��: �̹� Ȯ����
        else
        {
            User.instance.armor_id = itemData.id;
            /*
            Byte[] outBuffer = new byte[7];
            outBuffer[0] = 11;
            outBuffer[1] = 0;
            outBuffer[2] = 0;
            byte[] intBytes = BitConverter.GetBytes(itemData.id);
            Buffer.BlockCopy(intBytes, 0, outBuffer, 3, 4);
            ClentSocket.instance.SendRequest(outBuffer);
            */
            StartCoroutine(equipItemCoroutine(itemData.id));
        }

        InventoryMenu.OrganizeChild();

        HudUI.instance.statusMenu.SetActive(true);
        StatusMenu.instance.Organize();

        HudUI.instance.statusMenu.SetActive(isActive);
        image.color = new Color(0.5f, 0.5f, 0.5f);
    }

    private IEnumerator equipItemCoroutine(int item_id)
    {
        WWWForm form = new WWWForm();
        form.AddField("character_id", User.instance.character_id);

        int is_weapon = 0;
        if(itemData.is_weapon)
        {
            is_weapon = 1;
        }

        form.AddField("is_weapon", is_weapon);
        form.AddField("item_id", item_id);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/equipItem.php", form);
        yield return www.SendWebRequest();

        //���� ����
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("http request connection error");
        }
        //������ ���
        else
        {
            //���� json string parsing�ϱ�
            JObject decoded = JObject.Parse(www.downloadHandler.text);

            //key�� value���. int���� �ٸ� Ÿ�Ե� ����
            bool success = decoded["success"].ToObject<bool>();

            if(success)
            {

            }
             else
            {
                Debug.Log("error: " + www.downloadHandler.text);
            }

        }
    }

}
