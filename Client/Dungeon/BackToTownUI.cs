using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BackToTownUI : MonoBehaviour
{
    public void OnClickBackButton()
    {
        Room.instance.userDatas.Clear();    //현재 룸에 있는 유저 목록 초기화하기
        Room.instance.room_id_to_move = 0; //이동할 룸의 id 설정
        Room.instance.isGettingInput = false;    //동기화 데이터 받지 않게 설정하기
        ClientInput.instance.isReady = false;    //다른 룸으로 이동 완료 전까지 키보드 데이터를 보내지 않음

        byte[] outBuffer = new byte[2];
        outBuffer[0] = 8;
        outBuffer[1] = 0;
        ClientSocket.instance.SendRequest(outBuffer);

        byte[] outBuffer2 = new byte[1 + 4 + 1];
        outBuffer2[0] = 9;
        byte[] intBytes = BitConverter.GetBytes(0);
        Buffer.BlockCopy(intBytes, 0, outBuffer2, 1, 4);
        outBuffer2[5] = (byte)0;
        ClientSocket.instance.SendRequest(outBuffer2);

        this.gameObject.SetActive(false);
    }
}
