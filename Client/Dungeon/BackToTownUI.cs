using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BackToTownUI : MonoBehaviour
{
    public void OnClickBackButton()
    {
        Room.instance.userDatas.Clear();    //���� �뿡 �ִ� ���� ��� �ʱ�ȭ�ϱ�
        Room.instance.room_id_to_move = 0; //�̵��� ���� id ����
        Room.instance.isGettingInput = false;    //����ȭ ������ ���� �ʰ� �����ϱ�
        ClientInput.instance.isReady = false;    //�ٸ� ������ �̵� �Ϸ� ������ Ű���� �����͸� ������ ����

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
