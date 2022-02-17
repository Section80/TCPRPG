using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserEntity : Entity
{
    public void SetNickname(string nickname)
    {
        this.nickname = nickname;
        if (nicknameText)
        {
            nicknameText.text = this.nickname;
        }
    }

    new protected void Update()
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
        animator.SetInteger("animation_id", animation_id);
    }

    //game status
    public int max_hp;
    public int hp;
    public int max_mp;
    public int mp;

    //info
    public int character_id;
    public string nickname;

    //reference
    public Animator animator;
    public Text nicknameText;



    private void OnMouseDown()
    {
        HudUI.instance.showCharacterInfoMenu(nickname, character_id);
    }
}
