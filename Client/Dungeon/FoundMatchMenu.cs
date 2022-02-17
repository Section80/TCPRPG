using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoundMatchMenu : MonoBehaviour
{
    public GameObject waitingResponse;     //매치를 찾았습니다. 수락/거절
    public GameObject waitingRoomStart;    //매치 시작 대기 중..
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
        //매치 참가 수락 패킷을 보낸다. 
        byte[] outBuffer = new byte[2];
        outBuffer[0] = 13;
        outBuffer[1] = 4;

        ClientSocket.instance.SendRequest(outBuffer);

        SetUI(eStatus.WaitingRoomStart);
    }

    public void OnClickReject()
    {
        //매치 참가 거절 패킷을 보낸다.
        byte[] outBuffer = new byte[2];
        outBuffer[0] = 13;
        outBuffer[1] = 5;

        ClientSocket.instance.SendRequest(outBuffer);

        shouldUpdateProgressBar = false;

        //메뉴를 닫는다. 
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(shouldUpdateProgressBar)
        {
            leftTime -= Time.deltaTime;
            
            //5초 경과
            if(leftTime < 0.0f)
            {
                leftTime = 0.0f;

                //매치 참가 수락 패킷을 보낸다. 
                byte[] outBuffer = new byte[2];
                outBuffer[0] = 13;
                outBuffer[1] = 5;

                ClientSocket.instance.SendRequest(outBuffer);

                shouldUpdateProgressBar = false;
                gameObject.SetActive(false);
            }

            progressBar.rectTransform.sizeDelta = new Vector2((leftTime / timeLimit) * 300.0f, 15.0f);
            leftSecText.text = ((int)leftTime).ToString() + "초";
        }
    }

    private float leftTime;
    private bool shouldUpdateProgressBar = false;

    private float timeLimit = 15.0f;
}
