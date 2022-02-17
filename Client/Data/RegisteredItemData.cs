using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class RegisteredItemData
{
    public int item_id;
    public string item_name;
    public int level_limit;
    public type item_type;
    public bool is_weapon;
    public int job;
    public int strong;
    public int dexility;
    public int intellect;
    public int luck;
    public int price;
    public bool isSold;

    public RegisteredItemData() { }

    public RegisteredItemData(JToken token)
    {
        item_id = token["id"].ToObject<int>();

        item_name = token["name"].ToObject<string>();
        level_limit = token["level_limit"].ToObject<int>();

        if (token["is_weapon"].ToObject<int>() == 0)
        {
            is_weapon = false;
            item_type = RegisteredItemData.type.Armor;
        }
        else
        {
            is_weapon = true;
            job = token["job"].ToObject<int>();
            if (job == 0)
            {
                item_type = RegisteredItemData.type.Warrior_weapon;
            }
            else if (job == 1)
            {
                item_type = RegisteredItemData.type.Magician_wapon;
            }
        }

        strong = token["strong"].ToObject<int>();
        dexility = token["dexility"].ToObject<int>();
        intellect = token["intellect"].ToObject<int>();
        luck = token["luck"].ToObject<int>();

        price = token["price"].ToObject<int>();

        if(token["is_sold"].ToObject<int>() == 0)
        {
            isSold = false;
        }
        else
        {
            isSold = true;
        }
    }

    public enum type
    {
        None,
        Warrior_weapon,
        Magician_wapon,
        Armor
    }
}
