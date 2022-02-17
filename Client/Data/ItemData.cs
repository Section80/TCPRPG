using System;

public class ItemData {
    public ItemData() {}

    public ItemData(RegisteredItemData data, int index_in_inventory)
    {
        this.id = data.item_id;
        this.character_id = User.instance.character_id;
        this.name = data.item_name;
        this.index_in_inventory = index_in_inventory;
        this.is_weapon = data.is_weapon;
        this.job = data.job;
        this.level_limit = data.level_limit;
        this.strong = data.strong;
        this.dexility = data.dexility;
        this.intellect = data.intellect;
        this.luck = data.luck;
    }

    public int id;     //4: 4
    public int character_id;   //4: 8
    public String name;    //20 *2: 40
    public int index_in_inventory; //4: 44
    public bool is_weapon;   //1: 45
    public int job;
    public int level_limit;    //4: 49
    public int strong;     //4: 53
    public int dexility;   //4: 57
    public int intellect;  //4: 61
    public int luck;       //4: 65
}