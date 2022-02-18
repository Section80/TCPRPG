package root.Invitation;

import java.util.ArrayList;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import root.User;
import root.Dungeon.OakDungeon;
import root.Dungeon.SlimeDungeon;
import root.Dungeon.Dungeon;
import root.Dungeon.MatchManager;
import root.Dungeon.Dungeon.eDungeonName;
import root.Dungeon.Dungeon.eStatus;

public class InvitationManager {
    public InvitationManager() {
        invitations = new ArrayList<>();
    }

    //던전 초대 패킷을 받았을 때 호출되는 함수
    public void OnUserInvite(User inviter, User invited, eDungeonName dungeonName) {
        //초대한 캐릭터가 이미 던전에 있는 경우
        if(invited.room.id != 0) {
            //친구 초대 실패 패킷을 보낸다. 
            ByteBuffer outBuffer= ByteBuffer.allocate(2);
            outBuffer.order(ByteOrder.LITTLE_ENDIAN);
            outBuffer.put(0, (byte)11);
            outBuffer.put(1, (byte)3);

            outBuffer.position(0);
            try {
                inviter.socket.write(outBuffer);
            } catch (IOException e) {
                e.printStackTrace();
            }

            return;
        }
        
        invitations.add(new Invitation(inviter, invited, dungeonName));
        
        //invited에게 던전 초대 알림 패킷을 보낸다. 
        ByteBuffer outBuffer= ByteBuffer.allocate(7);
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);
        outBuffer.put(0, (byte)11);
        outBuffer.put(1, (byte)0);
        outBuffer.putInt(2, inviter.character.id);
        if(dungeonName == eDungeonName.SlimeDungeon) {
            outBuffer.put(6, (byte)0);
        }
        else if(dungeonName == eDungeonName.OakDungeon) {
            outBuffer.put(6, (byte)1);
        }

        outBuffer.position(0);
        try {
            invited.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    //친구 초대 수락 패킷을 받았을 때 호출되는 함수
    public void OnUserAcceptInvite(User inviter, User invited) {
        //해당하는 Invitation객체를 찾는다.
        Invitation invitation = getInvitation(inviter, invited);
        Dungeon dungeon = null;;

        //해당하는 던전을 만든다. 
        if(invitation.dungeonName == eDungeonName.SlimeDungeon) {
            dungeon = new SlimeDungeon();
        }
        else if(invitation.dungeonName == eDungeonName.OakDungeon) {
            dungeon = new OakDungeon();
        }

        dungeon.status = eStatus.Playing;
        MatchManager.instance.AddDungeon(dungeon);

        //만들어진 던전 룸ID를 각 유저에게 보낸다. 
        ByteBuffer outBuffer = ByteBuffer.allocate(6);
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);    //endian 설정하기: BIG_ENDIAN or LITTLE_ENDIAN
        outBuffer.put(0, (byte)10);
        outBuffer.put(1, (byte)0);
        outBuffer.putInt(2, dungeon.id);

        outBuffer.position(0);
        try {
            inviter.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }

        outBuffer.position(0);
        try {
            invited.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }

        invitations.remove(invitation);

        //기존에 invited에게 던전 초대를 보냈던 유저들에게 거절 패킷을 보낸다. 
        ArrayList<Invitation> _invitations = new ArrayList<>();

        getInvitationsByInvited(invited, _invitations);
        
        while(_invitations.size() != 0) {
            Invitation iInvitation = _invitations.get(0);

            //inviter에게 친구 초대 거절 알림 패킷을 보낸다.
            ByteBuffer _outBuffer = ByteBuffer.allocate(6);
            _outBuffer.order(ByteOrder.LITTLE_ENDIAN);
            _outBuffer.put(0, (byte)11);
            _outBuffer.put(1, (byte)1);
            _outBuffer.putInt(2, invited.character.id);

            _outBuffer.position(0);
            try {
                iInvitation.inviter.socket.write(_outBuffer);
            } catch (IOException e) {
                e.printStackTrace();
            }

            _invitations.remove(iInvitation);
            invitations.remove(iInvitation);
        }
    }

    //친구 초대 거절 패킷을 받았을 때 호출되는 함수
    public void OnUserRejectInvite(User inviter, User invited) {
        Invitation invitation = getInvitation(inviter, invited);
        
        //inviter에게 친구 초대 거절 알림 패킷을 보낸다.
        ByteBuffer outBuffer = ByteBuffer.allocate(6);
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);
        outBuffer.put(0, (byte)11);
        outBuffer.put(1, (byte)1);
        outBuffer.putInt(2, invited.character.id);

        outBuffer.position(0);
        try {
            inviter.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }

        invitations.remove(invitation);
    }

    //친구 초대 취소 패킷을 받았을 때 호출되는 함수
    public void OnUserCancleInvite(User inviter, User invited) {
        Invitation invitation = getInvitation(inviter, invited);

        //invited에게 친구 초대 취소 알림 패킷을 보낸다.
        ByteBuffer outBuffer= ByteBuffer.allocate(6);
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);
        outBuffer.put(0, (byte)11);
        outBuffer.put(1, (byte)2);
        outBuffer.putInt(2, inviter.character.id);

        outBuffer.position(0);
        try {
            invited.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }

        invitations.remove(invitation);
    }

    //유저가 접속 종료됬을 때 호출되는 함수
    public void OnUserDisconnected(User user) {
        ArrayList<Invitation> shouldRemove = new ArrayList<>();

        for (Invitation iInvitation : invitations) {
            //유저가 보낸 친구 초대를 취소 처리한다.
            if(iInvitation.inviter == user) {
                System.out.println("inviter disconnect");
                //invited에게 친구 초대 취소 알림 패킷을 보낸다.
                ByteBuffer outBuffer= ByteBuffer.allocate(6);
                outBuffer.order(ByteOrder.LITTLE_ENDIAN);
                outBuffer.put(0, (byte)11);
                outBuffer.put(1, (byte)2);
                outBuffer.putInt(2, user.character.id);

                outBuffer.position(0);
                try {
                    iInvitation.invited.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }

                shouldRemove.add(iInvitation);
            }
            //유저가 받은 친구 초대를 거절 처리한다. 
            else if(iInvitation.invited == user) {
                System.out.println("invited disconnect");

                //inviter에게 친구 초대 거절 알림 패킷을 보낸다.
                ByteBuffer outBuffer= ByteBuffer.allocate(6);
                outBuffer.order(ByteOrder.LITTLE_ENDIAN);
                outBuffer.put(0, (byte)11);
                outBuffer.put(1, (byte)1);
                outBuffer.putInt(2, iInvitation.invited.character.id);
        
                outBuffer.position(0);
                try {
                    iInvitation.inviter.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }
        
                shouldRemove.add(iInvitation);
            }
        }

        for (Invitation iInvitation : shouldRemove) {
            invitations.remove(iInvitation);
        }
    }

    public ArrayList<Invitation> invitations;

    private Invitation getInvitation(User inviter, User invited) {
        for (Invitation iInvitation : invitations) {
            if(iInvitation.invited == invited && iInvitation.inviter == inviter) {
                return iInvitation;
            }            
        }

        return null;
    }

    private void getInvitationsByInvited(User invited, ArrayList<Invitation> outInvitations) {
        if(outInvitations == null) {
            outInvitations = new ArrayList<>();
        }

        outInvitations.clear();

        for (Invitation iInvitation : invitations) {
            if(iInvitation.invited == invited) {
                outInvitations.add(iInvitation);
            }
        }
    }
}
