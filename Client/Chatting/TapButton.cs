using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class TapButton : MonoBehaviour, IPointerClickHandler
{
    public ChattingTap tap;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ChattingMenu.instance.currentTap.gameObject.SetActive(false);
            ChattingMenu.instance.currentTap = this.tap;
            ChattingMenu.instance.currentTap.gameObject.SetActive(true);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnRightClick()
    {
        ChattingMenu.instance.tapClickMenu.gameObject.SetActive(true);
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        ChattingMenu.instance.tapClickMenu.transform.position = mousePos;
        ChattingMenu.instance.tapClickMenu.selectedTap = tap;

        ChattingMenu.instance.tapClickMenu.tapNameText.text = tap.tapName;

        if (tap.index == 0 || tap.index == 1)
        {
            ChattingMenu.instance.tapClickMenu.deleteTapButton.interactable = false;
        }
        else
        {
            ChattingMenu.instance.tapClickMenu.deleteTapButton.interactable = true;
        }
    }


}
