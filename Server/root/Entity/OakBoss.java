package root.Entity;

import java.util.ArrayList;

import root.Item;

public class OakBoss extends Enemy {
    public OakBoss(float x, float y) {
        super(x, y);

        this.type = 5;
        this.max_hp = 50;
        this.hp = this.max_hp;
        this.exp = 200;
        this.gold = 50;
        this.status = eStatus.Sleeping;
        this.damage = 10;

        this.direction = 180.0f;
        this.isOneDead = false;
    }

    public enum eStatus {
        Sleeping,   //공격하기 전 가만히 있는 상태
        Awaked, //적을 발견하고 추격하는 상태
        Waiting //적을 공격하고 난 뒤 잠시 쉬는 상태. 
    }

    @Override
    public void update(int tick) {
        //status와 상관없이 항상 일어나는 일
        for(Entity iEntity : room.entities) {
            if(iEntity instanceof UserEntity) {
                UserEntity ue = (UserEntity)iEntity;
                if(ue.hp <= 0) {
                    continue;
                }

                float _dx = ue.x - this.x;
                float _dy = ue.y - this.y;
                float _distance = (float)Math.sqrt(_dx * _dx + _dy * _dy);

                if(_distance <= 1.5f) {
                    if(ue.leftInvincibilityTick == 0) {
                        ue.leftInvincibilityTick = ue.invincibilityTick;
                        applyDamage(damage, ue);

                        if(ue.hp <= 0) {
                            isOneDead = true;
                            this.status = eStatus.Sleeping;
                        }
                    }
                }
            }
        }

        if(status == eStatus.Sleeping) {
            animation = 0;
            speedX = 0.0f;
            speedY = 0.0f;

            UserEntity closestEntity = null;
            float distance = Float.MAX_VALUE;

            for(Entity iEntity : room.entities) {
                if(iEntity instanceof UserEntity) {
                    UserEntity ue = (UserEntity)iEntity;
                    if(ue.hp <= 0) {
                        continue;
                    }

                    float _dx = ue.x - this.x;
                    float _dy = ue.y - this.y;
                    float _distance = (float)Math.sqrt(_dx * _dx + _dy * _dy);

                    if(_distance < distance) {
                        distance = _distance;
                        closestEntity = ue;
                    }
                }
            }

            if(distance < 10) {
                status = eStatus.Awaked;
                target = closestEntity;
            }
        }
        else if(status == eStatus.Awaked) {
            if(target == null) {
                status = eStatus.Sleeping;
            }
            else {
                float dx = target.x - this.x;
                float dy = target.y - this.y;
                float distance = (float)Math.sqrt(dx * dx + dy * dy);

                if(distance > 1.5) {
                    animation = 1;
                    speedX = (dx / distance) * 0.6f;
                    speedY = (dy / distance) * 0.6f;

                    float angle = (float)((Math.atan2(dy, dx) / (Math.PI * 2.0f)) * 360.0f);
                    this.direction = 90 -angle;
                }
                else {
                    speedX = 0.0f;
                    speedY = 0.0f;
                    status = eStatus.Waiting;
                    waitingTick = 20;
                    animation = 2;
                }
            }
        }
        else if(status == eStatus.Waiting) {
            animation = 0;
            waitingTick -= tick;

            if(waitingTick <= 0) {
                UserEntity oldTarget = target;
                target = null;
                status = eStatus.Awaked;

                for (Entity iEntity : room.entities) {
                    if(!(iEntity instanceof UserEntity)) {
                        continue;
                    }
                    UserEntity ue = (UserEntity)iEntity;

                    if(room.users.size() == 2) {
                        if(isOneDead == true) {
                            target = oldTarget;
                            break;
                        }
                        else if(ue != oldTarget) {
                            target = ue;
                            break;
                        }
                    }
                    else if(room.users.size() == 1) {
                        if(oldTarget.shouldDestroy == true || ue != oldTarget) {
                            target = ue;
                            break;
                        }
                        target = oldTarget;
                        break;
                    }
                    
                }
            }
        }
        
        super.update(tick);
    }

    @Override
    public void onDestroy() {
        ArrayList<DroppedItem> droppedItems = new ArrayList<>();


        for (Entity iEntity : room.entities) {
            if(!(iEntity instanceof UserEntity)) {
                continue;
            }

            UserEntity ue = (UserEntity)iEntity;

            //유저1 아이템 생성
            double val = Math.random();
            Item item = new Item();
            item.character_id = ue.user.character.id;
            item.level_limit = 5;

            //방어구
            if(val < 0.5) {
                item.is_weapon = false;
                item.job = 0;
                item.name = "오크의 방어구";

                item.strong = 5 + (int)(Math.random() * 5);
                item.dexility = 5 + (int)(Math.random() * 5);
                item.intellect = 3;
                item.luck = 3;
            }
            //워리어 무기
            else if(0.5 <= val && val < 0.75) {
                item.is_weapon = true;
                item.job = 0;
                item.name = "오크의 무기";

                item.strong = 5 + (int)(Math.random() * 5);
                item.dexility = 5 + (int)(Math.random() * 5);
                item.intellect = 0;
                item.luck = 3;
            }
            //매지션 무기
            else if(0.75 <= val && val <= 1.0) {
                item.is_weapon = true;
                item.job = 1;
                item.name = "오크의 스태프";

                item.strong = 5;
                item.dexility = 5 + (int)(Math.random() * 5);
                item.intellect = 7 + (int)(Math.random() * 7);;
                item.luck = 3;
            }

            DroppedItem droppedItem = new DroppedItem(x, y, item);
            droppedItems.add(droppedItem);
        }

        for (DroppedItem iDroppedItem : droppedItems) {
            room.addEntity(iDroppedItem);
        }
    }

    public eStatus status;
    public UserEntity target = null;

    public int waitingTick = 0;

    public boolean isOneDead = false;

    private int damage;
}
