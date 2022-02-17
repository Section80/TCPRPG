using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RegisteredItemElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Image coverImage;

    public Text nameText;
    public Text levelLimitText;
    public Text typeText;
    public Text strText;
    public Text dexText;
    public Text intText;
    public Text luckText;
    public Text priceText;

    public RegisteredItemData data;
    public bool is_selected = false;

    public void updateUI(RegisteredItemData data)
    {
        this.data = data;

        nameText.text = data.item_name;
        levelLimitText.text = data.level_limit.ToString();

        if (data.is_weapon)
        {
            if (data.job == 0)
            {
                typeText.text = "워리어 무기";
            }
            else if (data.job == 1)
            {
                typeText.text = "매지션 무기";
            }
        }
        else
        {
            typeText.text = "방어구";
        }

        strText.text = data.strong.ToString();
        dexText.text = data.dexility.ToString();
        intText.text = data.intellect.ToString();
        luckText.text = data.luck.ToString();
        priceText.text = data.price.ToString();

        if(data.isSold == true)
        {
            coverImage.color = new Color(0.0f, 0.5f, 0.0f, 0.3f);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (!is_selected)
        {
            Color color = coverImage.color;

            float a = 0.125f;
            if(color.g == 0.5f)
            {
                a += 0.3f;
            }
            coverImage.color = new Color(color.r, color.g, color.b, a);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (!is_selected)
        {
            Color color = coverImage.color;

            float a = 0.0f;
            if (color.g == 0.5f)
            {
                a += 0.3f;
            }

            coverImage.color = new Color(color.r, color.g, color.b, a);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        ExchangeMenu.instance.selectedItem = data;
        ExchangeMenu.instance.resetSelectedElement();

        Color color = coverImage.color;
        float a = 0.25f;
        if (color.g == 0.5f)
        {
            a += 0.3f;
        }

        coverImage.color = new Color(color.r, color.g, color.b, a);

        is_selected = true;

        if (ExchangeMenu.instance.status == ExchangeMenu.EStatus.myExchange)
        {
            if (data.isSold == true)
            {
                ExchangeMenu.instance.buyCancleButton.GetComponentInChildren<Text>().text = "대금 수령";
            }
            else
            {
                ExchangeMenu.instance.buyCancleButton.GetComponentInChildren<Text>().text = "등록 취소";
            }
        }
        else if(ExchangeMenu.instance.status == ExchangeMenu.EStatus.exchange)
        {
            ExchangeMenu.instance.buyCancleButton.GetComponentInChildren<Text>().text = "구입";
        }
    }
}
