using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoundMatchMenu : MonoBehaviour
{
    public GameObject waitingResponse;     //��ġ�� ã�ҽ��ϴ�. ����/����
    public GameObject waitingRoomStart;    //��ġ ���� ��� ��..
    public Image progressBar;
    public Text leftSecText;

    public enum eStatus
    {
        WaitingUserResponse, WaitingRoomStart
    };

    public eStatus status = eStatus.WaitingUserResponse;

    public void SetUI(eStatus status)
    {
        if (status == eStatus.WaitingUserResponse)
        {
            waitingResponse.SetActive(true);
            waitingRoomStart.SetActive(false);

            shouldUpdateProgressBar = true;
            leftTime = timeLimit;
        }
        else if (status == eStatus.WaitingRoomStart)
        {
            waitingResponse.SetActive(false);
            waitingRoomStart.SetActive(true);
            
            shouldUpdateProgressBar = false;
        }
    }

    public void OnClickAccept()
    {
        //��ġ ���� ���� ��Ŷ�� ������. 
        byte[] outBuffer = new byte[2];
        outBuffer[0] = 13;
        outBuffer[1] = 4;

        ClientSocket.instance.SendRequest(outBuffer);

        SetUI(eStatus.WaitingRoomStart);
    }

    public void OnClickReject()
    {
        //��ġ ���� ���� ��Ŷ�� ������.
        byte[] outBuffer = new byte[2];
        outBuffer[0] = 13;
        outBuffer[1] = 5;

        ClientSocket.instance.SendRequest(outBuffer);

        shouldUpdateProgressBar = false;

        //�޴��� �ݴ´�. 
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(shouldUpdateProgressBar)
        {
            leftTime -= Time.deltaTime;
            
            //5�� ���
            if(leftTime < 0.0f)
            {
                leftTime = 0.0f;

                //��ġ ���� ���� ��Ŷ�� ������. 
                byte[] outBuffer = new byte[2];
                outBuffer[0] = 13;
                outBuffer[1] = 5;

                ClientSocket.instance.SendRequest(outBuffer);

                shouldUpdateProgressBar = false;
                gameObject.SetActive(false);
            }

            progressBar.rectTransform.sizeDelta = new Vector2((leftTime / timeLimit) * 300.0f, 15.0f);
            leftSecText.text = ((int)leftTime).ToString() + "��";
        }
    }

    private float leftTime;
    private bool shouldUpdateProgressBar = false;

    private float timeLimit = 15.0f;
}
