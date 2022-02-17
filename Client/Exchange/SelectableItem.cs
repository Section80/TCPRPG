using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectableItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public enum EType
    {
        none,
        armor,
        warrior_weapon,
        magican_weapon
    }

    public ItemData data;
    public Image iconImage;

    public EType type = EType.none;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            //��ĭ�� �ƴ� ��쿡��
            if(type != EType.none)
            {
                //�������� �������� �ƴ� ��쿡��
                if (User.instance.armor_id != data.id && User.instance.weapon_id != data.id)
                {
                    if (ItemRegisterationMenu.instance.seleted != null)
                    {
                        //������ ���õǾ��� �������� ���� ������ �ٲ۴�. 
                        ItemRegisterationMenu.instance.seleted.iconImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    }

                    //�ڽ��� ���� ��Ӱ� �ٲ۴�. 
                    iconImage.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

                    //���� ���õ� �������� �ڱ� �ڽ����� �ٲ۴�.
                    ItemRegisterationMenu.instance.seleted = this;
                } 
                //�������� �������� �������� ���
                else
                {
                    if (ItemRegisterationMenu.instance.seleted != null)
                    {
                        //������ ���õǾ��� �������� ���� ������ �ٲ۴�. 
                        ItemRegisterationMenu.instance.seleted.iconImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                    ItemRegisterationMenu.instance.seleted = null;
                }
            }
        }
    }

    public void updateUI(ItemData data)
    {
        if(data == null)
        {
            iconImage.sprite = null;
            type = EType.none;
            return;
        }

        this.data = data;

        if(data.is_weapon)
        {
            if(data.job == 0)
            {
                type = EType.warrior_weapon;
                iconImage.sprite = ItemRegisterationMenu.instance.warroir_weaponImage;
            } 
            else if(data.job == 1)
            {
                type = EType.magican_weapon;
                iconImage.sprite = ItemRegisterationMenu.instance.magican_weaponImage;
            }
        }
        else
        {
            type = EType.armor;
            iconImage.sprite = ItemRegisterationMenu.instance.armorImage;
        }

        //���� ���� ����� ���
        if (User.instance.armor_id == data.id || User.instance.weapon_id == data.id)
        {
            iconImage.color = new Color(6.0f, 0.3f, 0.3f, 1.0f);
        }
        else
        {
            iconImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //��ĭ�� �ƴ� ��쿡��
        if (type != EType.none)
        {
            ItemRegisterationMenu.instance.itemInfoMenu.gameObject.SetActive(true);
            ItemRegisterationMenu.instance.itemInfoMenu.setText(data);
            ItemRegisterationMenu.instance.itemInfoMenu.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (type != EType.none)
        {
            ItemRegisterationMenu.instance.itemInfoMenu.gameObject.SetActive(false);
        }
    }
}
