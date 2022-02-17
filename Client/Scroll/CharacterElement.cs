using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    static float rgb = 65.0f / 255.0f;
    RectTransform rectTransform = null;

    [SerializeField]
    public int characterID = -1;

    public void Set(int characterID, string name, int level, string job) {
        this.characterID = characterID;
        transform.Find("Name").gameObject.GetComponent<Text>().text = name;
        transform.Find("Level").gameObject.GetComponent<Text>().text = "LV." + level.ToString();
        transform.Find("Job").gameObject.GetComponent<Text>().text = job;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        GetComponent<Image>().color = new Color(rgb, rgb, rgb, 2.0f / 4.0f);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        GetComponent<Image>().color = new Color(rgb, rgb, rgb, 1.0f / 4.0f);
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        GetComponent<Image>().color = new Color(rgb, rgb, rgb, 1.0f / 4.0f);
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        GetComponent<Image>().color = new Color(rgb, rgb, rgb, 3.0f / 4.0f);

        ScrollCharacterContent.selectedCharacterIndex = transform.GetSiblingIndex();
        ScrollCharacterContent.SelectCharacter(transform.GetSiblingIndex());

        Utility.instance.OnCancleDeleteCharacterButtonClicked();
    }

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }
}
