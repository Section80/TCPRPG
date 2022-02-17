using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoMenu : MonoBehaviour
{
    public static CharacterInfoMenu instance;
    public Text nicknameText;
    public Button friendRequestButton;

    public int selected_character_id;

    private void Awake()
    {
        instance = GetComponent<CharacterInfoMenu>();
    }
}
