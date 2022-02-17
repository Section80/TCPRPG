using System; 

public class CharacterData {
    public int id = -1;      //4
    public String nickname = ""; //20 * 2: 44
    public int job = -1;    //4: 48
    public int level = 1;   //4: 52
    public int weapon_id;   //4: 56
    public int armor_id;    //4: 60
    public int status_point = 0;    //4: 64
    public int gold = 0;    //4: 68
    public int exp = 0;     //4: 72
    public int strong = 10;  //4: 76
    public int dexility = 10;    //4: 80
    public int intellect = 10;   //4: 84
    public int luck = 10;    //4: 88
}