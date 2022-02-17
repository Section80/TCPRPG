using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindingMatchMenu : MonoBehaviour
{
    public Text text;
       
    public void OnClickCancle()
    {
        //매치 찾기 취소 패킷을 보낸다. 
        byte[] outBuffer = new byte[2];
        outBuffer[0] = 13;
        outBuffer[1] = 3;

        ClientSocket.instance.SendRequest(outBuffer);
        gameObject.SetActive(false);

        DungeonMenu.instance.slimeDungeonButton.image.color = new Color(1.0f, 1.0f, 1.0f);
        DungeonMenu.instance.oakDungeonButton.image.color = new Color(1.0f, 1.0f, 1.0f);
        DungeonMenu.instance.selectedDungeon = DungeonMenu.eDungeonName.None;
    }
}
