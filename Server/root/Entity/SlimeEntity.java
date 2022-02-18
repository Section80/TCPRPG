package root.Entity;

public class SlimeEntity extends Enemy {
    public SlimeEntity(float x, float y) {
        super(x, y);
        this.type = 3;
        this.max_hp = 20;
        this.hp = 20;
        this.exp = 10;
        this.gold = 5;
    }

    @Override
    public void update(int tick) { 
        UserEntity closestEntity = null;
        float distance = Float.MAX_VALUE;
        float dx = 0;
        float dy = 0;

        for (Entity iEntity : room.entities) {
            if(iEntity instanceof UserEntity) {
                UserEntity ue = (UserEntity)iEntity;

                float _dx = ue.x - this.x;
                float _dy = ue.y - this.y;

                float _distance = (float)Math.sqrt(_dx * _dx + _dy * _dy);

                if(_distance < distance && ue.hp > 0) {
                    distance = _distance;
                    closestEntity = ue;
                    dx = _dx;
                    dy = _dy;

                    float angle = (float)((Math.atan2(_dy, _dx) / (Math.PI * 2.0f)) * 360.0f);
                    this.direction = 90 -angle;
                }

                if(_distance <= 1.5f) {
                    if(ue.leftInvincibilityTick == 0) {
                        ue.leftInvincibilityTick = ue.invincibilityTick;
                        applyDamage(20, ue);
                    }
                }
            }
        }

        if(closestEntity != null && distance < 10.0f) {
            if(distance > 0.5) {
                speedX = (dx / distance) * 0.4f;
                speedY = (dy / distance) * 0.4f;
            }
            else {
                speedX = 0.0f;
                speedY = 0.0f;
            }
        } else {
            speedX = 0.0f;
            speedY = 0.0f;
        }

        super.update(tick);
    }
}
