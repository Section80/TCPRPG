package root;

public class FriendData {
    public FriendData(String nickname, int character_id) {
        this.nickname = nickname;
        this.character_id = character_id;
    }

    public FriendData(String nickname, int character_id, boolean isOnline) {
        this.nickname = nickname;
        this.character_id = character_id;
        this.isOnline = isOnline;
    }

    public String nickname;
    public int character_id;
    public boolean isOnline = false;
}
