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
        //모든 selectableItem을 순회하면서 각각의 UI를 초기화한다. 
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

    // 이 함수를 호출헤 http 요청을 보낸다. 
    public void sendHttpRequest()
    {
        StartCoroutine(registerItemsCoroutine());
    }

    public void onRegisterButtonClicked()
    {
        //아이템을 아직 선택하지 않은 경우
        if(seleted == null)
        {
            ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
            ExchangeMenu.instance.alertMenu.contentText.text = "아이템을 선택하세요.";
            return;
        }

        //가격이 음수인 경우
        if (int.Parse(priceInputField.text) < 0)
        {
            ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
            ExchangeMenu.instance.alertMenu.contentText.text = "가격이 음수입니다.";
            return;
        }

        //착용 중인 장비인 경우
        if(User.instance.armor_id == seleted.data.id || User.instance.weapon_id == seleted.data.id)
        {
            ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
            ExchangeMenu.instance.alertMenu.contentText.text = "착용중인 장비는 등록할 수 없습니다.";
            return;
        }

        StartCoroutine(registerItemsCoroutine());
    }

    private IEnumerator registerItemsCoroutine()
    {
        WWWForm form = new WWWForm();
        // 데이터 넣기 예시
        form.AddField("character_id", User.instance.character_id);
        form.AddField("item_id", seleted.data.id);
        form.AddField("price",  int.Parse(priceInputField.text));

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/registerItem.php", form);
        yield return www.SendWebRequest();
        //연결 에러
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("http request connection error");
        }
        //성공한 경우
        else
        {
            Debug.Log(www.downloadHandler.text);
            //응답 json string parsing하기
            JObject decoded = JObject.Parse(www.downloadHandler.text);

            bool success = decoded["success"].ToObject<bool>();

            if(success)
            {
                ExchangeMenu.instance.alertMenu.gameObject.SetActive(true);
                ExchangeMenu.instance.alertMenu.contentText.text = "아이템을 등록했습니다.";

                //인벤토리에서 해당 아이템 삭제
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
