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
    public int pageNum; //���� �˻��� ������ �˻� ����� �� ������ ��

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
        //��� ���ϵ� ��ü ����
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
        //��� ���ϵ� ��ü ����
        clearUI();

        goldText.text = User.instance.gold.ToString();

        //element��ü ���� �����
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
        buyCancleButton.GetComponentInChildren<Text>().text = "��� ���";
        requestItemData();
    }

    public void onExchangeButtonClicked()
    {
        status = EStatus.exchange;
        selectedItem = null;
        buyCancleButton.GetComponentInChildren<Text>().text = "����";
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

        if(buttonText == "����")
        {
            int index = Inventory.instance.GetBlankIndex();
            if (index == -1)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "�κ��丮�� �� ������ �����ϴ�. ";
                return;
            }

            if (selectedItem == null)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "�������� ������ �ּ���. ";
                return;
            }

            if(User.instance.gold < selectedItem.price)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "��尡 �����մϴ�. ";
                return;
            }

            StartCoroutine(buyItemCoroutine(selectedItem, index));
        }
        else if(buttonText == "��� ���")
        {
            int index = Inventory.instance.GetBlankIndex();
            if(index == -1)
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "�κ��丮�� �� ������ �����ϴ�. ";
                return;
            }

            if (selectedItem == null) 
            {
                alertMenu.gameObject.SetActive(true);
                alertMenu.contentText.text = "�������� ������ �ּ���. ";
                return;
            }

            StartCoroutine(cancleRegisterCoroutine(selectedItem, index));
        }
        else if (buttonText == "��� ����")
        {
            StartCoroutine(receivePaymentCoroutine(selectedItem));
        }
    }


    private IEnumerator cancleRegisterCoroutine(RegisteredItemData itemData, int index)
    {
        WWWForm form = new WWWForm();
        // ������ �ֱ� ����
        form.AddField("item_id", itemData.item_id);
        form.AddField("index_in_inventory", index);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/cancleRegisteredItem.php", form);
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
                //����� ������ �����͸� �κ��丮�� �߰��ϰ� UI�� ������Ʈ�Ѵ�. 
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
                    alertMenu.contentText.text = "�̹� �ȸ� �������Դϴ�. ";
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

            //key�� value���. int���� �ٸ� Ÿ�Ե� ����
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
        // ������ �ֱ� ����
        form.AddField("item_id", itemData.item_id);
        form.AddField("index_in_inventory", index);
        form.AddField("character_id", User.instance.character_id);
        form.AddField("price", itemData.price);

        UnityWebRequest www = UnityWebRequest.Post(ClientSocket.phpIP + "/buyRegisteredItem.php", form);
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
                    alertMenu.contentText.text = "�̹� �ȷȰų� ��� ��ҵ� �������Դϴ�. ";
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
