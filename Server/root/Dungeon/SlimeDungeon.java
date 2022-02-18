package root.Dungeon;

import root.Entity.Entity;
import root.Entity.SlimeEntity;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import root.GameServer;
import root.User;
import root.WallRect;

public class SlimeDungeon extends Dungeon {
    public SlimeDungeon() {
        this.dungeonName = eDungeonName.SlimeDungeon;

        wallRects.add(new WallRect(-7, -12, 2, 24));
        wallRects.add(new WallRect(-7, -12, 44, 2));
        wallRects.add(new WallRect(-7, 10, 44, 2));
        wallRects.add(new WallRect(35, -12, 2, 24));

        addEntity(new SlimeEntity(5, 0));
        addEntity(new SlimeEntity(10, 6));
        addEntity(new SlimeEntity(13, -5));
        addEntity(new SlimeEntity(16, 3));
        addEntity(new SlimeEntity(20, 1));
    }

    @Override
    public void OnDestroy(Entity entity) {
        if(entity instanceof SlimeEntity) {
            objective1 += 1;
            
            //목표 1 달성 시
            if(objective1 == 5){
                this.status = eStatus.Cleared;
            }

            //목표 업데이트 패킷 보내기
            GameServer.outBuffer5.put(0, (byte)10); //던전
            GameServer.outBuffer5.put(1, (byte)4);  //목표 업데이트
            GameServer.outBuffer5.put(2, (byte)0);  //슬라임 던전
            GameServer.outBuffer5.put(3, (byte)0);  //목표 ID
            GameServer.outBuffer5.put(4, (byte)this.objective1);    //val

            //목표 업데이트 알림
            for (User iUser : users) {
                GameServer.outBuffer5.position(0);
                try {
                    iUser.socket.write(GameServer.outBuffer5);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    public int objective1 = 0;  //목표1: 슬라임 5마리 잡기
}
