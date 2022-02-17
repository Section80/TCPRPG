using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class ExchangeMenu : MonoBehaviour
{
    public static ExchangeMenu instance;

    public GameObject registeredItemElements;
    public GameObject registeredItemElementPrefap;

    public AlertMenu alertMenu;

    public Dropdown priceDropdown;
    public Dropdown typeDropdown;
    public InputField minLevelLimitInputField;
    public InputField maxLevelLimitInputField;

    public Button nextButton;
    public Button previousButton;

    public Button buyCancleButton;

    public Text currentPageText;
    public Text goldText;

    public List<RegisteredItemData> itemDatas;

    public RegisteredItemData selectedItem;

    public ItemRegisterationMenu itemRegisterationMenu;

    public int currentPage = 1;
    public int maxpage;
    public int pageNum; //현재 검색한 필터의 검색 결과의 총 페이지 수

    public enum EStatus
    {
        waitingMyExchange,
        waitingExchange,
        myExchange,
        exchange
    }

    public EStatus status = EStatus.exchange;

    private void Awake()
    {
        instance = GetComponent<ExchangeMenu>();

        itemDatas = new List<RegisteredItemData>();
    }

    private void clearUI()
    {
        //모든 차일드 객체 삭제
        GameObject[] delete = new GameObject[registeredItemElements.transform.childCount];

        for (int i = 0; i < registeredItemElements.transform.childCount; i++)
        {
            delete[i] = registeredItemElements.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < delete.Length; i++)
        {
            DestroyImmediate(delete[i]);
        }
    }

    public void updateItemListUI()
    {
        //모든 차일드 객체 삭제
        clearUI();

        goldText.text = User.instance.gold.ToString();

        //element객체 새로 만들기
        for(int i = 0; i < itemDatas.Count; i++)
        {
            RegisteredItemData data = itemDatas[i];

            RegisteredItemElement element = Instantiate(registeredItemElementPrefap, registeredItemElements.transform).GetComponent<RegisteredItemElement>();
            RectTransform tr = element.GetComponent<RectTransform>();

            tr.localPosition = new Vector3(-400.0f, -i * 25.0f, 0.0f);

            element.updateUI(data);
        }
    }

    public void onClickSearchButton()
    {
        StartCoroutine(requestItemDataCoroutine());
    }

    public void resetSelectedElement()
    {
        for (int i = 0; i < registeredItemElements.transform.childCount; i++)
        {
            RegisteredItemElement element = registeredItemElements.transform.GetChild(i).GetComponent<RegisteredItemElement>();

            Color color = element.coverImage.color;
            float a = 0.0f;
            if(color.g == 0.5f)
            {
                a += 0.3f;
            }

            element.coverImage.color = new Color(color.r, color.g, color.b, a);
            element.is_selected = false;
        }
    }

    public void requestItemData()
    {
        StartCoroutine(requestItemDataCoroutine());
    }

    private IEnumerator requestItemDataCoroutine()
    {
        selectedItem = null;

        WWWForm form = new WWWForm();

        if(status == EStatus.myExchange)
        {
            form.AddField("bUserItem", 1);
            status = EStatus.waitingMyExchange;
        }
        else if(status == EStatus.exchange)
        {
            form.AddField("bUserItem", 0);
            status = EStatus.waitingExchange;
        }

        if (priceDropdown.value == 0)
        {
            form.AddField("highPriceOrder", 1);
        }
        else
        {
            form.AddField("highPriceOrder", 0);
        }

        if (typeDropdown.value == 0)
        {
            form.AddField("itemType", 0);
        }
        else if (typeDropdown.value == 1)
        {
            form.AddField("itemType", 1);
        }
        else if (typeDropdown.value == 2)
        {
            form.AddField("itemType", 2);
        }
        else if (typeDropdown.value == 3)
        {
            form.AddField("itemType", 3);
        }

        form.AddField("characterID", User.instance.character_id);
        form.AddField("min", int.Parse(minLevelLimitInputField.text));
        form.AddField("max", int.Parse(maxLevelLimitInputField.text));
        form.AddField("page", currentPage);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/getRegisteredItemList.php", form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("error");
        }
        else
        {
            if(status == EStatus.waitingExchange)
            {
                status = EStatus.exchange;
            } else if(status == EStatus.waitingMyExchange)
            {
                status = EStatus.myExchange;
            }

            Debug.Log(www.downloadHandler.text);
            JObject decoded = JObject.Parse(www.downloadHandler.text);
            bool success = decoded["success"].ToObject<bool>();

            if(success)
            {
                itemDatas.Clear();

                JToken datas = decoded["data"];
                int num = 0;
                foreach(JToken data in datas)
                {
                    num++;
                    RegisteredItemData newData = new RegisteredItemData(data);
                    itemDatas.Add(newData);
                }

                updateItemListUI();
            }
            else
            {
                Debug.Log(decoded["message"].ToObject<string>());
                Debug.Log(www.downloadHandler.text);
            }

            pageNum = decoded["pageNum"].ToObject<int>();

            if(currentPage == 1)
            {
                previousButton.gameObject.SetActive(false);
            }
            if(currentPage == pageNum)
            {
                nextButton.gameObject.SetActive(false);
            }
            else
            {
                nextButton.gameObject.SetActive(true);
                previousButton.gameObject.SetActive(true);
            }

            currentPageText.text = currentPage.ToString() + "/" + pageNum.ToString();
        }
    }

    public void onMyExchangeButtonClicked()
    {
        status = EStatus.myExchange;
        selectedItem = null;
        buyCancleButton.GetComponentInChildren<Text>().text = "등록 취소";
        requestItemData();
    }

    public void onExchangeButtonClicked()
    {
        status = EStatus.exchange;
        selectedItem = null;
        buyCancleButton.GetComponentInChildren<Text>().text = "구입";
        requestItemData();
    }

    public void onRegisterItemButtonClicked()
    {
        gameObject.SetActive(false);
        itemRegisterationMenu.gameObject.SetActive(true);
        itemRegisterationMenu.updateUI();
    }

    public void onBuyCancleButtonClicked()
    {
        string buttonText = buyCancleButton.GetComponentInChildren<Text>().text;

        if(buttonText == "구입")
        {
            int index = Inventory.instance.GetBlankIndex();
            if (index == -1)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "인벤토리에 빈 공간이 없습니다. ";
                return;
            }

            if (selectedItem == null)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "아이템을 선택해 주세요. ";
                return;
            }

            if(User.instance.gold < selectedItem.price)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "골드가 부족합니다. ";
                return;
            }

            StartCoroutine(buyItemCoroutine(selectedItem, index));
        }
        else if(buttonText == "등록 취소")
        {
            int index = Inventory.instance.GetBlankIndex();
            if(index == -1)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "인벤토리에 빈 공간이 없습니다. ";
                return;
            }

            if (selectedItem == null) 
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "아이템을 선택해 주세요. ";
                return;
            }

            StartCoroutine(cancleRegisterCoroutine(selectedItem, index));
        }
        else if (buttonText == "대금 수령")
        {
            StartCoroutine(receivePaymentCoroutine(selectedItem));
        }
    }


    private IEnumerator cancleRegisterCoroutine(RegisteredItemData itemData, int index)
    {
        WWWForm form = new WWWForm();
        // 데이터 넣기 예시
        form.AddField("item_id", itemData.item_id);
        form.AddField("index_in_inventory", index);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/cancleRegisteredItem.php", form);
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
                //취소한 아이템 데이터를 인벤토리에 추가하고 UI를 업데이트한다. 
                Inventory.instance.AddItemData(new ItemData(itemData, index));
                InventoryMenu.OrganizeChild();
                requestItemData();
            } 
            else
            {
                string message = decoded["message"].ToObject<string>();

                if(message == "already sold")
                {
                    alertMenu.gameObject.SetActive(true);
                    alertMenu.contentText.text = "이미 팔린 아이템입니다. ";
                    StartCoroutine(requestItemDataCoroutine());
                }
            }
        }
    }

    private IEnumerator receivePaymentCoroutine(RegisteredItemData itemData)
    {
        WWWForm form = new WWWForm();
        form.AddField("item_id", itemData.item_id);
        form.AddField("character_id", User.instance.character_id);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/receivePayment.php", form);
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

            //key로 value얻기. int말고 다른 타입도 가능
            bool success = decoded["success"].ToObject<bool>();

            if(success)
            {
                User.instance.gold += decoded["price"].ToObject<int>();
                InventoryMenu.UpdateGold();
                requestItemData();
            }
            else
            {

            }
        }
    }

    private IEnumerator buyItemCoroutine(RegisteredItemData itemData, int index)
    {
        WWWForm form = new WWWForm();
        // 데이터 넣기 예시
        form.AddField("item_id", itemData.item_id);
        form.AddField("index_in_inventory", index);
        form.AddField("character_id", User.instance.character_id);
        form.AddField("price", itemData.price);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/buyRegisteredItem.php", form);
        yield return www.SendWebRequest();

        //연결 에러
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("http request connection error");
        }
        //성공한 경우
        else
        {
            //응답 json string parsing하기
            JObject decoded = JObject.Parse(www.downloadHandler.text);

            bool success = decoded["success"].ToObject<bool>();

            if(success)
            {
                Inventory.instance.AddItemData(new ItemData(itemData, index));
                User.instance.gold -= itemData.price;
                InventoryMenu.UpdateGold();
                requestItemData();
            }
            else
            {
                string message = decoded["message"].ToObject<string>();

                if(message == "item not exist")
                {
                    alertMenu.gameObject.SetActive(true);
                    alertMenu.contentText.text = "이미 팔렸거나 등록 취소된 아이템입니다. ";
                    StartCoroutine(requestItemDataCoroutine());
                }
            }
        }
    }

    public void onClickNextButton()
    {
        if(currentPage < pageNum)
        {
            currentPage += 1;
        }

        requestItemData();
    }

    public void onClickPreviousButton()
    {
        if(currentPage > 1)
        {
            currentPage -= 1;
        }

        requestItemData();
    }
}
