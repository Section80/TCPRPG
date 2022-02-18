package root.Entity;

import java.util.ArrayList;
import root.Item;
import root.GameServer;



public class DroppedItem  extends Entity {
    public DroppedItem(float x, float y, Item item) {
        super(x, y);
        this.item = item;
        this.character_id = item.character_id;
        this.type = 4;
    }

    @Override
    public void update(int tick) {
        ArrayList<Entity> entities = GetCollisionEntites();

        if(waitDB == true) {
            return;
        }

        for (Entity iEntity : entities) {
            if(iEntity instanceof UserEntity) {
                UserEntity userEntity = (UserEntity)iEntity;

                if(userEntity.hp <= 0) {
                    continue;
                }

                if(userEntity.user.character.id == character_id || character_id == 0) {
                    if(checkOverlap(iEntity)) {
                        if(this.character_id == 0) {
                            this.character_id = userEntity.user.character.id;
                        }

                        //db업데이트
                        GameServer.databaseThread.AddItem(userEntity.user, character_id, this);
                        waitDB = true;
                    }
                }
            }
        }
    }

    public int character_id;
    public Item item;

    public boolean waitDB = false;
}
