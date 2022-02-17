using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomPortal : MonoBehaviour
{
    public int nextRoomIndex;
    public int enterDirection;

    // Update is called once per frame
    void Update()
    {
        if (isWarping == false)
        {
            Entity entity = User.instance.entity;
            if (entity)
            {
                float distance = Vector3.Distance(entity.transform.position, transform.position);

                if (distance < 3)
                {
                    isWarping = true;
                    //�̰� ���ϸ� ���� ������ �濡�� ����� �۵� ����
                    Room.instance.userDatas.Clear();
                    Room.instance.room_id_to_move = nextRoomIndex;

                    Room.instance.isGettingInput = false;
                    ClientInput.instance.isReady = false;

                    //�������� ����ȭ �����͸� ���� �ʰڴٰ� ������.
                    byte[] outBuffer1 = new byte[2];
                    outBuffer1[0] = 8;
                    outBuffer1[1] = 0;
                    ClientSocket.instance.SendRequest(outBuffer1);

                    //�������� �� ���� ��û
                    byte[] outBuffer2 = new byte[1 + 4 + 1];
                    outBuffer2[0] = 9;

                    byte[] intBytes = BitConverter.GetBytes(nextRoomIndex);
                    Buffer.BlockCopy(intBytes, 0, outBuffer2, 1, 4);
                    

                    outBuffer2[5] = (byte)enterDirection;
                    ClientSocket.instance.SendRequest(outBuffer2);
                }
            }
        }
    }

    private  bool isWarping = false;
}
