using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemEntity : Entity
{
    public void Init()
    {
        armor.gameObject.SetActive(false);
        warriorWeapon.gameObject.SetActive(false);
        magicianWeapon.gameObject.SetActive(false);

        if (character_id == User.instance.character_id || character_id == 0)
        {
            if(isWeapon)
            {
                if(job == 0)
                {
                    warriorWeapon.gameObject.SetActive(true);
                }
                else if(job == 1)
                {
                    magicianWeapon.gameObject.SetActive(true);
                }
            }
            else
            {
                armor.gameObject.SetActive(true);
            }
        }
        else
        {
            armor.gameObject.SetActive(false);
            warriorWeapon.gameObject.SetActive(false);
            magicianWeapon.gameObject.SetActive(false);
        }
    }

    //reference
    public GameObject armor;
    public GameObject warriorWeapon;
    public GameObject magicianWeapon;

    //info
    public int character_id;
    public bool isWeapon;
    public int job;


}
