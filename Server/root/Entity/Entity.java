package root.Entity;

import java.util.ArrayList;
import root.Room;
import root.WallRect;

public class Entity {
    public Entity(float x, float y) {
        this.x = x;
        this.y = y;
        this.radius = 1;
    }

    public void update(int tick) {
        //롬이 가진 WallRect를 고려해 움직인다. 
        float newX = this.x + speedX * tick * 0.5f;
        float newY = this.y + speedY * tick * 0.5f;

        boolean success = true;
        for (WallRect iWallRect : room.wallRects) {
            if(iWallRect.IsInWallRect(newX, newY)) {
                success = false;
            }
        }

        if(success) {
            this.x = newX;
            this.y = newY;
        }
    };

    public void onDestroy() {

    }

    public ArrayList<Entity> GetCollisionEntites() {
        ArrayList<Entity> entities = new ArrayList<>();

        for (Entity iEntity : room.entities) {
            if(checkOverlap(iEntity)) {
                entities.add(iEntity);
            }
        }

        return entities;
    }

    public float x;
    public float y;
    public float speedX;
    public float speedY;
    public float direction;
    public int index_in_room;   //index in room::entities vector
    public int id_in_room;
    public int type = -1;    //0:warrior, 1: magician, 2: attackEntity, 3: slimeEntity, 4: droppedItem 5: oakBoss
    public boolean shouldDestroy = false;

    public float radius;

    public Room room;

    protected boolean checkOverlap(Entity entity) {
        float dx = this.x - entity.x;
        float dy = this.y - entity.y;

        float distance = (float)Math.sqrt(dx * dx + dy * dy);

        if(distance > this.radius + entity.radius) {
            return false;
        }
        else {
            return true;
        }
    }
}