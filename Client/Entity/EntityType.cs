using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityType : MonoBehaviour
{
    public enum eType
    {
        Warrior,
        Magician,
        Attack,
        Slime,
        DroppedItem,
        OakBoss
    }

    public Entity GetEntity()
    {
        switch(type)
        {
            case eType.Warrior:
                return GetComponent<WarriorEntity>();
            case eType.Magician:
                return GetComponent<MagicianEntity>();
            case eType.Attack:
                return GetComponent<Entity>();
            case eType.Slime:
                return GetComponent<SlimeEntity>();
            case eType.DroppedItem:
                return GetComponent<DroppedItemEntity>();
            case eType.OakBoss:
                return GetComponent<OakBossEntity>();
            default:
                return null;
        }
    }

    public WarriorEntity GetWarriorEntity()
    {
        return GetComponent<WarriorEntity>();
    }

    public MagicianEntity GetMagicianEntity()
    {
        return GetComponent<MagicianEntity>();
    }

    public SlimeEntity GetSlimeEntity()
    {
        return GetComponent<SlimeEntity>();
    }

    public OakBossEntity GetOakBossEntity()
    {
        return GetComponent<OakBossEntity>();
    }

    public UserEntity GetUserEntity()
    {
        if(type == eType.Warrior)
        {
            return GetComponent<WarriorEntity>();
        }
        else if(type == eType.Magician)
        {
            return GetComponent<MagicianEntity>();
        }

        return null;
    }

    public DroppedItemEntity GetDroppedItemEntity()
    {
        return GetComponent<DroppedItemEntity>();
    }

    //describe
    public eType type;
}
