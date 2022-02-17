using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlimeEntity : Entity
{
    new protected void Update()
    {
        base.Update();
        animator.SetInteger("animation_id", animation_id);
        hpBar.rectTransform.sizeDelta = new Vector2((float)hp / (float)max_hp * 80, 10);
    }

    //game status
    public int max_hp;
    public int hp;

    //reference
    public Animator animator;
    public Image hpBar;
}
