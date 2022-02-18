package root.Dungeon;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.io.IOException;

import root.WallRect;
import root.Entity.Entity;
import root.Entity.OakBoss;
import root.User;

public class OakDungeon extends Dungeon {
    public OakDungeon() {
        this.dungeonName = eDungeonName.OakDungeon;

        wallRects.add(new WallRect(-12, -12, 2, 20));
        wallRects.add(new WallRect(-12, -12, 80, 2));
        wallRects.add(new WallRect(-12, 10, 82, 2));
        wallRects.add(new WallRect(70, -12, 2, 24));
        
        addEntity(new OakBoss(10, 5));
    }

    @Override
    public void OnDestroy(Entity entity) {
        if(entity instanceof OakBoss) {
            objective1 += 1;
            
            //목표 1 달성 시
            if(objective1 == 1){
                this.status = eStatus.Cleared;
            }

            //목표 업데이트 패킷 보내기
            ByteBuffer outBuffer = ByteBuffer.allocate(5);
            outBuffer.order(ByteOrder.LITTLE_ENDIAN);

            outBuffer.put(0, (byte)10); //던전
            outBuffer.put(1, (byte)4);  //목표 업데이트
            outBuffer.put(2, (byte)1);  //오크 던전
            outBuffer.put(3, (byte)0);  //목표 ID
            outBuffer.put(4, (byte)this.objective1);    //val

            //목표 업데이트 알림
            for (User iUser : users) {
                outBuffer.position(0);
                try {
                    iUser.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    public int objective1 = 0;
}
