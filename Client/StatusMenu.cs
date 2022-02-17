using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusMenu : MonoBehaviour
{
    public static StatusMenu instance;

    public Text nicknameText;
    public Text levelText;
    public Text expText;
    public Text pointText;
    public Text strText;
    public Text dexText;
    public Text intText;
    public Text luckText;

    public void Awake()
    {
        instance = GetComponent<StatusMenu>();
    }

    public void Organize()
    {
        nicknameText.text = User.instance.nickname;
        levelText.text = User.instance.level.ToString();
        expText.text = User.instance.exp.ToString() + " / " + (User.instance.level * 10).ToString();

        int itemStr = 0;
        int itemDex = 0;
        int itemInt = 0;
        int itemLuck = 0;

        ItemData weapon = Inventory.instance.getWeapon();
        ItemData armor = Inventory.instance.getArmor();

        if(weapon != null) {
            itemStr += weapon.strong;
            itemDex += weapon.dexility;
            itemInt += weapon.intellect;
            itemLuck += weapon.luck;
        }

        if(armor != null)
        {
            itemStr += armor.strong;
            itemDex += armor.dexility;
            itemInt += armor.intellect;
            itemLuck += armor.luck;
        }

        strText.text = User.instance.strong.ToString() + "(+" + itemStr.ToString() + ")";
        dexText.text = User.instance.dexility.ToString() + "(+" + itemDex.ToString() + ")";
        intText.text = User.instance.intellect.ToString() + "(+" + itemInt.ToString() + ")";
        luckText.text = User.instance.luck.ToString() + "(+" + itemLuck.ToString() + ")";

        pointText.text = User.instance.status_point.ToString();
    }

    public void useStatusPoint(int type)
    {
        if(User.instance.status_point > 0)
        {
            User.instance.status_point -= 1;
            byte[] outBuffer = new byte[2];
            outBuffer[0] = 12;
            outBuffer[1] = (byte)type;
            ClientSocket.instance.SendRequest(outBuffer);

            if(type == 0)
            {
                User.instance.strong += 1;
            }
            else if (type == 1)
            {
                User.instance.dexility += 1;
            }
            else if (type == 2)
            {
                User.instance.intellect += 1;
            }
            else if (type == 3)
            {
                User.instance.luck += 1;
            }

            StatusMenu.instance.Organize();
        }
    }
}
