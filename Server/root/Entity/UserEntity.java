package root.Entity;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import root.User;

public class UserEntity extends Entity {
    public UserEntity(User user, float x, float y) {
        super(x, y);
        this.user = user;
        this.max_hp = 100 + user.getStrong() * 10;
        this.hp = this.max_hp;
        this.max_mp = 100 + user.getIntellect() * 10;
        this.mp = this.max_mp;

        this.attackDelayTick = 10.0f / (1.0f + user.getDexility() / 20.0f);
    }

    public void update(int tick) {
        leftAttackDelayTick -= tick;
        leftManaRegienTick -= tick;

        if(hp <= 0) {
            hp = 0;
            animation = 3;

            if(isGameOverpacketSent == false) {
                ByteBuffer outBuffer = ByteBuffer.allocate(2);
                outBuffer.order(ByteOrder.LITTLE_ENDIAN);
                outBuffer.put(0, (byte)10);
                outBuffer.put(1, (byte)5);

                outBuffer.position(0);
                try {
                    user.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }
                
                isGameOverpacketSent = true;
            }

            return;
        }

        if(leftManaRegienTick < 0) {
            this.mp += this.max_mp * (5.0f + user.getIntellect()) / 100.0f;
            if(this.max_mp < this.mp) {
                this.mp = this.max_mp;
            }

            leftManaRegienTick = 30;
        }

        leftInvincibilityTick -= tick;
        if(leftInvincibilityTick < 0) {
            leftInvincibilityTick = 0;
        }

        if(canMove) {
            if(user.inputStatus.left) {
                speedX = -1;
            }
            if(user.inputStatus.right) {
                speedX = 1;
            }
            if(!user.inputStatus.left && !user.inputStatus.right) {
                speedX = 0;
            }

            if(user.inputStatus.up) {
                speedY = 1;
            }
            if(user.inputStatus.down) {
                speedY = -1;
            }
            if(!user.inputStatus.up && !user.inputStatus.down) {
                speedY = 0;
            }

            if(speedX == 0 && speedY > 0) {
                direction = 0;
            }
            else if(speedX > 0 && speedY > 0) {
                direction = 45;
            }
            else if(speedX > 0 && speedY == 0) {
                direction = 90;
            }
            else if(speedX > 0 && speedY < 0) {
                direction = 135;
            }
            else if(speedX == 0 && speedY < 0) {
                direction = 180;
            }
            else if(speedX < 0 && speedY < 0) {
                direction = 225;
            }
            else if(speedX < 0 && speedY == 0) {
                direction = 270;
            }
            else if(speedX < 0 && speedY > 0) {
                direction = 315;
            }

            if(speedX != 0 || speedY != 0) {
                animation = 1;
            } else {
                animation = 0;
            }

            if(user.inputStatus.z) {
                speedX = 0;
                speedY = 0;
                animation = 2;

                if(leftAttackDelayTick <= 0) {
                    leftAttackDelayTick = attackDelayTick;
                    room.addEntity(new DefaultAttackEntity(this.x, this.y, this.direction, 5.0f, user));
                }
            }
        }

        super.update(tick);
    }

    public User user; 
    public int job;

    public int max_hp;
    public int hp;

    public int max_mp;
    public int mp;

    public int animation = 0;

    public boolean canMove = true;

    public float attackDelayTick;
    public float leftAttackDelayTick;

    public int invincibilityTick = 10;
    public int leftInvincibilityTick = 0;

    public int leftManaRegienTick = 30;

    private boolean isGameOverpacketSent = false;
}
