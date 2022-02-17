using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryMenu : MonoBehaviour
{
    public static InventoryMenu instance;

    public static void OrganizeChild()
    {
        bool isActive = HudUI.instance.inventoryMenu.gameObject.activeInHierarchy;
        instance.gameObject.SetActive(true);
        instance.organizeChild();
        instance.gameObject.SetActive(isActive);
    }

    public static void UpdateGold()
    {
        bool isActive = HudUI.instance.inventoryMenu.gameObject.activeInHierarchy;
        instance.gameObject.SetActive(true);
        instance.updateGold();
        instance.gameObject.SetActive(isActive);
    }

    public Sprite armorImage;
    public Sprite warriorWeaponImage;
    public Sprite magicianWeaponImage;
    public Text goldText;
    public GameObject elementHolder;

    private void Awake()
    {
        instance = GetComponent<InventoryMenu>();
    }

    public void updateGold()
    {
        goldText.text = User.instance.gold.ToString();
    }

    private void organizeChild()
    {
        for (int i = 0; i < 16; i++)
        {
            ItemElement itemElement = elementHolder.transform.GetChild(i).GetComponent<ItemElement>();
            ItemData data = null;

            foreach (ItemData itemData in Inventory.instance.itemDatas)
            {
                if (itemData.index_in_inventory == i)
                {
                    data = itemData;
                    break;
                }
            }

            itemElement.initialize(data);
        }

        updateGold();
    }
}
