using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonNPC : MonoBehaviour
{
    public GameObject dungeonMenu;

    private void Awake()
    {
        dungeonMenu.SetActive(false);
    }

    private void OnMouseDown()
    {
        HudUI.instance.onCloseStatusMenuClicked();
        HudUI.instance.onCloseInventoryMenuClicked();
        HudUI.instance.onCloseFriendMenuClicked();

        dungeonMenu.SetActive(true);
    }
}
