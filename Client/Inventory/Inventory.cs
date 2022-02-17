using System.Collections;
using System.Collections.Generic;


public class Inventory
{
    public static Inventory instance;

    public Inventory()
    {
        instance = this;
        itemDatas = new List<ItemData>();
    }

    public void AddItemData(ItemData data)
    {
        itemDatas.Add(data);
    }

    public void Clear()
    {
        itemDatas.Clear();
    }

    public int Count()
    {
        return itemDatas.Count;
    }

    public ItemData GetItemData(int i)
    {
        return itemDatas[i];
    }

    public ItemData getWeapon()
    {
        if (User.instance.weapon_id != 0)
        {
            foreach (ItemData data in itemDatas)
            {
                if (data.id == User.instance.weapon_id)
                {
                    return data;
                }
            }
        }
        else
        {
            return null;
        }

        return null;
    }

    public ItemData getArmor()
    {
        if (User.instance.armor_id != 0)
        {
            foreach (ItemData data in itemDatas)
            {
                if (data.id == User.instance.armor_id)
                {
                    return data;
                }
            }
        }
        else
        {
            return null;
        }

        return null;
    }

    public int GetBlankIndex()
    {
        //��� �κ��丮 ĭ�� ��ȸ�Ѵ�. 
        for (int i = 0; i < 16; i++)
        {
            bool isBlank = true;

            //�ش� ĭ�� �����ϰ� �ִ� �������� �ִ��� Ȯ���Ѵ�. 
            foreach (ItemData data in itemDatas)
            {
                if(data.index_in_inventory == i)
                {
                    isBlank = false;
                }
            }

            if(isBlank)
            {
                return i;
            }
        }


        return -1;
    }

    public List<ItemData> itemDatas;   //���õ� ĳ���Ͱ� ������ �ִ� ������ data
}
