package root.Invitation;

import root.User;
import root.Dungeon.Dungeon.eDungeonName;;

public class Invitation {
    public Invitation(User inviter, User invited, eDungeonName dungeonName) {
        this.inviter = inviter;
        this.invited = invited;
        this.dungeonName = dungeonName;
    }


    public User inviter;
    public User invited;
    eDungeonName dungeonName;
}
