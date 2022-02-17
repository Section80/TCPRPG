using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;


public class ItemRegisterationMenu : MonoBehaviour
{
    public static ItemRegisterationMenu instance;

    public Sprite armorImage;
    public Sprite warroir_weaponImage;
    public Sprite magican_weaponImage;

    public SelectableItem seleted;
    public ItemInfoMenu itemInfoMenu;
    public Transform selectableHolder;
    public InputField priceInputField;

    private void Awake()
    {
        instance = GetComponent<ItemRegisterationMenu>();
    }

    public void onCancleButtonClicked()
    {
        gameObject.SetActive(false);
        ExchangeMenu.instance.gameObject.SetActive(true);
    }

    public void updateUI()
    {
        //��� selectableItem�� ��ȸ�ϸ鼭 ������ UI�� �ʱ�ȭ�Ѵ�. 
        for(int i = 0; i < 16; i++)
        {
            SelectableItem selectable = selectableHolder.GetChild(i).GetComponent<SelectableItem>();
            ItemData itemData = null;
            foreach(ItemData data in Inventory.instance.itemDatas)
            {
                if(data.index_in_inventory == i)
                {
                    itemData = data;
                }
            }

            selectable.updateUI(itemData);
        }
    }

    // �� �Լ��� ȣ���� http ��û�� ������. 
    public void sendHttpRequest()
    {
        StartCoroutine(registerItemsCoroutine());
    }

    public void onRegisterButtonClicked()
    {
        //�������� ���� �������� ���� ���
        if(seleted == null)
        {
            ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
            ExchangeMenu.instance.alertMenu.contentText.text = "�������� �����ϼ���.";
            return;
        }

        //������ ������ ���
        if (int.Parse(priceInputField.text) < 0)
        {
            ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
            ExchangeMenu.instance.alertMenu.contentText.text = "������ �����Դϴ�.";
            return;
        }

        //���� ���� ����� ���
        if(User.instance.armor_id == seleted.data.id || User.instance.weapon_id == seleted.data.id)
        {
            ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
            ExchangeMenu.instance.alertMenu.contentText.text = "�������� ���� ����� �� �����ϴ�.";
            return;
        }

        StartCoroutine(registerItemsCoroutine());
    }

    private IEnumerator registerItemsCoroutine()
    {
        WWWForm form = new WWWForm();
        // ������ �ֱ� ����
        form.AddField("character_id", User.instance.character_id);
        form.AddField("item_id", seleted.data.id);
        form.AddField("price",  int.Parse(priceInputField.text));

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/registerItem.php", form);
        yield return www.SendWebRequest();
        //���� ����
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("http request connection error");
        }
        //������ ���
        else
        {
            Debug.Log(www.downloadHandler.text);
            //���� json string parsing�ϱ�
            JObject decoded = JObject.Parse(www.downloadHandler.text);

            bool success = decoded["success"].ToObject<bool>();

            if(success)
            {
                ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
                ExchangeMenu.instance.alertMenu.contentText.text = "�������� ����߽��ϴ�.";

                //�κ��丮���� �ش� ������ ����
                Inventory.instance.itemDatas.Remove(seleted.data);
                seleted.iconImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                seleted = null;

                updateUI();
                InventoryMenu.OrganizeChild();
            } else
            {
                Debug.Log(decoded["message"].ToObject<string>());
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

}
