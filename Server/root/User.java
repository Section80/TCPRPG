package root;

import java.nio.channels.SocketChannel;
import java.util.Vector;

public class User {
    public User(SocketChannel socket) {
        this.socket = socket;

        this.inputStatus = new InputStatus();
        this.friends = new Vector<>();

        this.weapon = new Item();
        this.armor = new Item();
    }

    public int id = -1;          //계정 ID
    public String nickname = "";    //접속한 캐릭터 닉네임
    public int roomID = -1;                //유저가 현재 들어가있는 방의 ID
    public SocketChannel socket = null;    //유저 클라이언트 소켓
    public CharacterData character;

    boolean isReadyInput = false;

    Vector<CharacterData> characters = null;
    public int characterID = -1;
    int characterIndex = -1; //유저가 게임 시작한 캐릭터의 index in characters vector
    int id_in_room = -1;
    public Vector<Item> items = null;

    Vector<FriendData> friends;

    Item weapon;
    Item armor;

    public Room room;  //user가 들어가있는 룸

    public InputStatus inputStatus;

    public void SetCharacter(int characterID) {
        this.characterID = characterID;

        for(int i = 0; i < characters.size(); i++) {
            if(characters.get(i).id == characterID) {
                characterIndex = i;
                break;
            }
        }
    }

    public int getStrong() {
        int out = character.strong;
        if(weapon != null) {
            out += weapon.strong;
        }

        if(armor != null) {
            out += armor.strong;
        }

        return out;
    }

    public int getDexility() {
        int out = character.dexility;
        if(weapon != null) {
            out += weapon.dexility;
        }

        if(armor != null) {
            out += armor.dexility;
        }

        return out;
    }

    public int getIntellect() {
        int out = character.intellect;
        if(weapon != null) {
            out += weapon.intellect;
        }

        if(armor != null) {
            out += armor.intellect;
        }

        return out;
    }

    public int getLuck() {
        int out = character.luck;
        if(weapon != null) {
            out += weapon.luck;
        }

        if(armor != null) {
            out += armor.luck;
        }

        return out;
    }
}