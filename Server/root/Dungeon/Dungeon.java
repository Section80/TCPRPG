package root.Dungeon;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

import root.Exp;
import root.GameServer;
import root.Room;
import root.User;
import root.CharacterData;

public class Dungeon extends Room {
    public Dungeon() {
        this.requests = new ArrayList<>();
        this.exps = new ArrayList<>();
        this.sections = new ArrayList<>();
        this.status = eStatus.WaitingAccept;
    }

    public enum eStatus {
        WaitingAccept,
        Playing,
        Cleared
    }

    public enum eDungeonName {
        None,
        SlimeDungeon,
        OakDungeon
    }

    @Override
    public void update(int tick) {
        if(status == eStatus.Playing) {
            super.update(tick);
        }
        else if(status == eStatus.Cleared) {
            super.update(tick);
        }
    }

    public static eDungeonName ByteToEDungeonName(Byte val) {
        if(val == 0) {
            return eDungeonName.SlimeDungeon;
        }
        else if(val == 1) {
            return eDungeonName.OakDungeon;
        }

        return eDungeonName.None;
    }

    public FindMatchRequest GetFindMatchRequestByUser(User user) {
        for (FindMatchRequest iRequest : requests) {
            if(user == iRequest.user) {
                return iRequest;
            }
        }
        return null;
    }

    public boolean CheckIfAllAccepted() {
        if(requests.size() == 1) {
            return false;
        }

        for (FindMatchRequest iRequest : requests) {
            
            if(iRequest.status == FindMatchRequest.eStatus.Accepted) {
                System.out.println("accepted");
            }

            if(iRequest.status != FindMatchRequest.eStatus.Accepted) {
                System.out.println("not accepted");
                return false;
            }
        }

        return true;
    }

    public void SendGetExp(int gold) {
        GameServer.outBuffer9.put(0, (byte)10);
        GameServer.outBuffer9.put(1, (byte)3);
        GameServer.outBuffer9.putInt(5, gold);

        for (User iUser : users) {
            Exp exp = getExp(iUser);

            GameServer.outBuffer9.put(2, (byte)exp.levelDelta);
            GameServer.outBuffer9.put(3, (byte)exp.amount);
            GameServer.outBuffer9.put(4, (byte)exp.statusPointDelta);

            GameServer.outBuffer9.position(0);
            try {
                iUser.socket.write(GameServer.outBuffer9);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    @Override
    public void addUserEntity(User user, int direction) {
        exps.add(new Exp(user));
        super.addUserEntity(user, direction);
    }

    @Override
    public void addUserEntity(User user, float x, float y) {
        exps.add(new Exp(user));
        super.addUserEntity(user, x, y);
    }

    @Override
    public void exitUser(User user) {
        CharacterData character = user.character;
        Exp exp = null;

        for (Exp iExp : exps) {
            if(iExp.user.character.id == character.id) {
                exp = iExp;
            }
        }

        character.level += exp.levelDelta;
        character.exp = exp.amount;
        character.status_point += exp.statusPointDelta;
        character.gold += exp.goldDelta;

        System.out.println("exitUser " + exp.levelDelta + " " + exp.amount + " " + exp.statusPointDelta);
        GameServer.databaseThread.DungeonExpRequest(user, exp.levelDelta, exp.amount, exp.statusPointDelta, exp.goldDelta);
        super.exitUser(user);

        if(users.size() == 0) {
            MatchManager.instance.DeleteDungeon(this);
        }
    }

    private Exp getExp(User user) {
        for (Exp iExp : exps) {
            if(user.character.id == iExp.user.character.id) {
                return iExp;
            }
        }
        return null;
    }

    public ArrayList<FindMatchRequest> requests;
    public eDungeonName dungeonName = eDungeonName.None;
    public eStatus status = eStatus.WaitingAccept;
    public ArrayList<Exp> exps;
    public ArrayList<Section> sections;

    public Section currentSection;
}
