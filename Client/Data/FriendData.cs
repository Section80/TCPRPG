using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendData
{
    public FriendData(string nickname, int character_id, bool isOnline)
    {
        this.nickname = nickname;
        this.character_id = character_id;
        this.isOnline = isOnline;
    }

    public string nickname;
    public int character_id;
    public bool isOnline = false;
}
