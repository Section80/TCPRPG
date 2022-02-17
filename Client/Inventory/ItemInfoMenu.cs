using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoMenu : MonoBehaviour
{
    public static ItemInfoMenu instance;

    public Text nameText;
    public Text typeText;
    public Text levelText;
    public Text strText;
    public Text dexText;
    public Text intText;
    public Text luckText;

    private void Awake()
    {
        instance = GetComponent<ItemInfoMenu>();
    }

    public void setText(ItemData itemData)
    {
        nameText.text = itemData.name;
        levelText.text = itemData.level_limit.ToString();
        strText.text = itemData.strong.ToString();
        dexText.text = itemData.dexility.ToString();
        intText.text = itemData.intellect.ToString();
        luckText.text = itemData.luck.ToString();

        if(itemData.is_weapon == false)
        {
            typeText.text = "规绢备";
        } else
        {
            if(itemData.job == 0)
            {
                typeText.text = "况府绢 公扁";
            }
            else if(itemData.job == 1)
            {
                typeText.text = "概瘤记 公扁";
            }
        }
    }
}
