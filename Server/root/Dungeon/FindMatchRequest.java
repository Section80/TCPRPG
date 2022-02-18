package root.Dungeon;

import root.User;

//유저의 매치 찾기 요청
public class FindMatchRequest {
    public FindMatchRequest(User user, Dungeon.eDungeonName dungeonName) {
        this.user = user;
        this.dungeonName = dungeonName;
        this.status = eStatus.NotFound;
    }

    public FindMatchRequest(User user, Dungeon.eDungeonName dungeonName, eStatus status) {
        this.user = user;
        this.dungeonName = dungeonName;
        this.status = status;
    }

    public enum eStatus {
        NotFound,
        WaitingAccept,
        Accepted,
        Rejected
    }

    public User user;
    public Dungeon.eDungeonName dungeonName;
    public eStatus status = eStatus.NotFound;
}
