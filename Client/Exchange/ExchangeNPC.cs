using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExchangeNPC : MonoBehaviour
{
    public GameObject exchangeMenu;

    private void Awake()
    {
        exchangeMenu.SetActive(false);
    }

    private void OnMouseDown()
    {
        HudUI.instance.onCloseStatusMenuClicked();
        HudUI.instance.onCloseInventoryMenuClicked();
        HudUI.instance.onCloseFriendMenuClicked();

        exchangeMenu.SetActive(true);
        exchangeMenu.GetComponent<ExchangeMenu>().status = ExchangeMenu.EStatus.exchange;
        exchangeMenu.GetComponent<ExchangeMenu>().requestItemData();
    }

    public void onCloseExchangeMenuClicked()
    {
        exchangeMenu.SetActive(false);
    }
}
