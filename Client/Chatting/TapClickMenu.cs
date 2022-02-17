using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapClickMenu : MonoBehaviour
{
    public ChattingTap selectedTap;
    public Text tapNameText;
    public Button deleteTapButton;

    //�� �ݱ� ��ư�� ������ �� ȣ��Ǵ� �Լ�
    public void OnClickCloseTap()
    {
        ChattingMenu.instance.OnDeleteTapClicked(selectedTap.index);
        gameObject.SetActive(false);
    }

    //ä�� �� ���� ��ư�� ������ �� ȣ��Ǵ� �Լ�
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
