using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnInviteMenu : MonoBehaviour
{
    public static OnInviteMenu instance;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = GetComponent<OnInviteMenu>();
        }
    }

    public void OnClickCancle()
    {
        //친구 초대 취소 패킷을 보낸다. 
        byte[] outBuffer = new byte[6];
        outBuffer[0] = 14;
        outBuffer[1] = 3;
        byte[] intBuffer = BitConverter.GetBytes(InvitableFriendElement.invitedCharacterID);
        Buffer.BlockCopy(intBuffer, 0, outBuffer, 2, intBuffer.Length);

        ClientSocket.instance.SendRequest(outBuffer);
        gameObject.SetActive(false);
    }
}
