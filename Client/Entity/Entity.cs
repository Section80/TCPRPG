using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected void Update()
    {
        if (isFirstPos == true)
        {
            if (target.Equals(new Vector3(-666.0f, -666.0f, -666.0f)))
            {
                return;
            }

            transform.position = target;
            directionTransform.eulerAngles = new Vector3(
                0,
                direction,
                0
            );
            isFirstPos = false;

            int count = gameObject.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 10.0f);
            directionTransform.eulerAngles = new Vector3(
                0,
                direction,
                0
            );
        }
    }

    public WarriorEntity GetWarriorEntity()
    {
        return (WarriorEntity)this;
    }

    public MagicianEntity GetMagicianEntity()
    {
        return (MagicianEntity)this;
    }

    public SlimeEntity GetSlimeEntity()
    {
        return (SlimeEntity)this;
    }

    public OakBossEntity GetOakBossEntity()
    {
        return (OakBossEntity)this;
    }

    public UserEntity GetUserEntity()
    {
        return (UserEntity)this;
    }

    public DroppedItemEntity GetDroppedItemEntity()
    {
        return (DroppedItemEntity)this;
    }

    //reference
    public Transform directionTransform;

    //info
    public int entity_id;

    //transform
    public Vector3 target;
    public float direction;

    //status
    public bool isFirstPos = true;
    public bool isSyncDataArrived = false;
    public int animation_id;
}
