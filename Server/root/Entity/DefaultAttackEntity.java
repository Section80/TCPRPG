package root.Entity;

import root.User;

public class DefaultAttackEntity extends Entity {
    public DefaultAttackEntity(float x, float y, float direction, float speed, User user) {
        super(x, y);
        this.direction = 90.0f - direction;
        this.speed = speed;
        this.type = 2;

        speedX = (float)Math.cos(this.direction * Math.PI / 180) * speed;
        speedY = (float)Math.sin(this.direction * Math.PI / 180) * speed;

        this.damage = user.getStrong() / 2;
        this.critical = user.getLuck() * 2 / 100.0f;
    }

    @Override
    public void update(int tick) {
        leftLifeTick -= tick;
        if(leftLifeTick <= 0) {
            shouldDestroy = true;
        }

        for (Entity iEntity : GetCollisionEntites()) {
            if(iEntity instanceof Enemy) {
                Enemy enemy = (Enemy)iEntity;

                if(Math.random() < this.critical) {
                    enemy.hp -= this.damage * 2;
                } 
                else {
                    enemy.hp -= this.damage;
                }

                
                this.speedX = 0.0f;
                this.speedY = 0.0f; 

                shouldDestroy = true;
            }
        }
        
        super.update(tick);
    }

    float speed;

    int leftLifeTick = 3;
    public int damage = 5;
    public float critical = 0;
}
