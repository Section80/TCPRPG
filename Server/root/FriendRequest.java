package root;

public class FriendRequest {
    public FriendRequest(int from, String fromNickname, int to, String toNickname) {
        this.from = from;
        this.to = to;

        this.fromNickname = fromNickname;
        this.toNickname = toNickname;
    }

    public boolean isSame(int id1, int id2) {
        if(from == id1 && to == id2) {
            return true;
        } 
        else if(from == id2 && to == id1) {
            return true;
        } 

        return false;
    }

    int from;
    String fromNickname;
    int to;
    String toNickname;
}
