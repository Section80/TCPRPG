using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapClickMenu : MonoBehaviour
{
    public ChattingTap selectedTap;
    public Text tapNameText;
    public Button deleteTapButton;

    //탭 닫기 버튼을 눌렀을 때 호출되는 함수
    public void OnClickCloseTap()
    {
        ChattingMenu.instance.OnDeleteTapClicked(selectedTap.index);
        gameObject.SetActive(false);
    }

    //채팅 탭 설정 버튼을 눌렀을 대 호출되는 함수
    public void OnClickTapSetting()
    {
        ChattingMenu.instance.tapSettingMenu.gameObject.SetActive(true);
        ChattingMenu.instance.tapSettingMenu.SetStatusToTaps(selectedTap.tapName);
        gameObject.SetActive(false);
    }

    public void OnClickCancle()
    {
        gameObject.SetActive(false);
    }
}
