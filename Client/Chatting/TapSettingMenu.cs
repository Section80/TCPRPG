using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapSettingMenu : MonoBehaviour
{
    public GameObject tapsObject;
    public GameObject changeNameObject;
    public Dropdown tapsDropdown;
    public InputField tapNameInputField;
    public Toggle allToggle;
    public Toggle normalToggle;
    public Toggle directToggle;
    public Toggle systemToggle;
    public Toggle alertToggle;
    public Button leftButton;
    public Button rightButton;
    public enum eStatus { Taps, ChangeName, NewTap };
    public eStatus status;
    public string selectedTapName;

    public void SetStatusToTaps(string tapName)
    {
        status = eStatus.Taps;
        selectedTapName = tapName;
        tapsObject.SetActive(true);
        changeNameObject.SetActive(false);

        tapsDropdown.value = tapsDropdown.options.FindIndex(option => option.text == tapName);

        ChattingTap tap = getChattingTapByName(tapName);
        if(tap.bShowAll)
        {
            allToggle.isOn = true;
        }
        else
        {
            allToggle.isOn = false;
        }

        if(tap.bShowNormal)
        {
            normalToggle.isOn = true;
        }
        else
        {
            normalToggle.isOn = false;
        }

        if (tap.bShowDirect)
        {
            directToggle.isOn = true;
        }
        else
        {
            directToggle.isOn = false;
        }

        if (tap.bShowSystem)
        {
            systemToggle.isOn = true;
        }
        else
        {
            systemToggle.isOn = false;
        }

        if (tap.bOffAlert)
        {
            alertToggle.isOn = true;
        }
        else
        {
            alertToggle.isOn = false;
        }

        leftButton.GetComponentInChildren<Text>().text = "적용";
        rightButton.GetComponentInChildren<Text>().text = "확인";
    }

    public void SetStatusToChangeName()
    {
        status = eStatus.ChangeName;
        changeNameObject.SetActive(true);
        tapsObject.SetActive(false);

        tapNameInputField.text = selectedTapName;

        tapNameInputField.Select();
        tapNameInputField.ActivateInputField();
    }

    public void SetStatusToNewTap()
    {
        allToggle.isOn = false;
        normalToggle.isOn = false;
        directToggle.isOn = false;
        systemToggle.isOn = false;
        allToggle.isOn = false;

        status = eStatus.NewTap;
        changeNameObject.SetActive(true);
        tapsObject.SetActive(false);

        tapNameInputField.text = "";

        leftButton.GetComponentInChildren<Text>().text = "확인";
        rightButton.GetComponentInChildren<Text>().text = "취소";
    }

    public void OnClickLeftButton()
    {
        if(status == eStatus.Taps)  //적용 버튼
        {
            ChattingTap tap = getChattingTapByName(selectedTapName);

            if(allToggle.isOn)
            {
                tap.bShowAll = true;
            }
            else
            {
                tap.bShowAll = false;
            }

            if (normalToggle.isOn)
            {
                tap.bShowNormal = true;
            }
            else
            {
                tap.bShowNormal = false;
            }

            if (directToggle.isOn)
            {
                tap.bShowDirect = true;
            }
            else
            {
                tap.bShowDirect = false;
            }

            if (systemToggle.isOn)
            {
                tap.bShowSystem = true;
            }
            else
            {
                tap.bShowSystem = false;
            }

            if (alertToggle.isOn)
            {
                tap.bOffAlert = true;
            }
            else
            {
                tap.bOffAlert = false;
            }
        }
        else if(status == eStatus.ChangeName)   //적용 버튼
        {
            ChattingTap tap = getChattingTapByName(selectedTapName);
            tap.tapName = tapNameInputField.text;

            if (allToggle.isOn)
            {
                tap.bShowAll = true;
            }
            else
            {
                tap.bShowAll = false;
            }

            if (normalToggle.isOn)
            {
                tap.bShowNormal = true;
            }
            else
            {
                tap.bShowNormal = false;
            }

            if (directToggle.isOn)
            {
                tap.bShowDirect = true;
            }
            else
            {
                tap.bShowDirect = false;
            }

            if (systemToggle.isOn)
            {
                tap.bShowSystem = true;
            }
            else
            {
                tap.bShowSystem = false;
            }

            if (alertToggle.isOn)
            {
                tap.bOffAlert = true;
            }
            else
            {
                tap.bOffAlert = false;
            }

            status = eStatus.Taps;

            ChattingMenu.instance.UpdateButtonUI();

            changeNameObject.gameObject.SetActive(false);
            tapsObject.gameObject.SetActive(true);

            Dropdown.OptionData data = tapsDropdown.options.Find(option => option.text == selectedTapName);
            int index = tapsDropdown.options.IndexOf(data);
            tapsDropdown.options.Remove(data);

            Dropdown.OptionData newData = new Dropdown.OptionData(tapNameInputField.text);
            tapsDropdown.options.Insert(index, newData);
            tapsDropdown.value = index;
            selectedTapName = tapNameInputField.text;

            tapsDropdown.captionText.text = tapNameInputField.text;
        }
        else if(status == eStatus.NewTap)   //확인 버튼
        {
            if(tapNameInputField.text == "")
            {
                return;
            }

            bool showAll = allToggle.isOn;
            bool showNormal = normalToggle.isOn;
            bool showDirect = directToggle.isOn;
            bool showSystem = systemToggle.isOn;
            bool offAlert = alertToggle.isOn;

            ChattingMenu.instance.AddTap(tapNameInputField.text, showAll, showNormal, showDirect, showSystem, offAlert);
            this.gameObject.SetActive(false);
        }
    }

    public void OnClickRightButton()
    {
        if (status == eStatus.Taps)  //확인 버튼
        {
            ChattingTap tap = getChattingTapByName(selectedTapName);

            if (allToggle.isOn)
            {
                tap.bShowAll = true;
            }
            else
            {
                tap.bShowAll = false;
            }

            if (normalToggle.isOn)
            {
                tap.bShowNormal = true;
            }
            else
            {
                tap.bShowNormal = false;
            }

            if (directToggle.isOn)
            {
                tap.bShowDirect = true;
            }
            else
            {
                tap.bShowDirect = false;
            }

            if (systemToggle.isOn)
            {
                tap.bShowSystem = true;
            }
            else
            {
                tap.bShowSystem = false;
            }

            if (alertToggle.isOn)
            {
                tap.bOffAlert = true;
            }
            else
            {
                tap.bOffAlert = false;
            }

            this.gameObject.SetActive(false);
        }
        else if (status == eStatus.ChangeName)   //확인 버튼
        {
            ChattingTap tap = getChattingTapByName(selectedTapName);
            tap.tapName = tapNameInputField.text;

            if (allToggle.isOn)
            {
                tap.bShowAll = true;
            }
            else
            {
                tap.bShowAll = false;
            }

            if (normalToggle.isOn)
            {
                tap.bShowNormal = true;
            }
            else
            {
                tap.bShowNormal = false;
            }

            if (directToggle.isOn)
            {
                tap.bShowDirect = true;
            }
            else
            {
                tap.bShowDirect = false;
            }

            if (systemToggle.isOn)
            {
                tap.bShowSystem = true;
            }
            else
            {
                tap.bShowSystem = false;
            }

            if (alertToggle.isOn)
            {
                tap.bOffAlert = true;
            }
            else
            {
                tap.bOffAlert = false;
            }

            Dropdown.OptionData data = tapsDropdown.options.Find(option => option.text == selectedTapName);
            int index = tapsDropdown.options.IndexOf(data);
            tapsDropdown.options.Remove(data);

            Dropdown.OptionData newData = new Dropdown.OptionData(tapNameInputField.text);
            tapsDropdown.options.Insert(index, newData);
            tapsDropdown.itemText.text = tapNameInputField.text;
            selectedTapName = tapNameInputField.text;

            tapsDropdown.captionText.text = tapNameInputField.text;

            ChattingMenu.instance.UpdateButtonUI();

            this.gameObject.SetActive(false);
        }
        else if (status == eStatus.NewTap)   //취소 버튼
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }
    
    private ChattingTap getChattingTapByName(string name)
    {
        foreach(ChattingTap tap in ChattingMenu.instance.taps)
        {
            if(tap != null)
            {
                if(tap.index != -1 && tap.tapName == name) {
                    return tap;
                }
            }
        }

        return null;
    }

    private void dropdownChanged()
    {
        SetStatusToTaps(tapsDropdown.options[tapsDropdown.value].text);
    }

    private void Awake()
    {
        tapsDropdown.onValueChanged.AddListener(delegate { 
            dropdownChanged(); 
        });
    }
}
