package root.Entity;

import root.Exp;
import root.Dungeon.Dungeon;

public class Enemy extends Entity {
    public Enemy(float x, float y) {
        super(x, y);
    }

    @Override
    public void update(int tick) {
        if(hp <= 0) {
            this.shouldDestroy = true;
        }
        else {
            super.update(tick);
        }
    }

    @Override
    public void onDestroy() {
        //만약 던전이라면 던전에 참가한 유저들에게 경험치를 준다. 
        if(room instanceof Dungeon) {
            Dungeon dungeon = (Dungeon)room;

            for (Exp iExp : dungeon.exps) {
                UserEntity ue = dungeon.GetUserEntityByUser(iExp.user);
                
                if(ue == null)
                {
                    continue;
                }

                if(ue.hp >= 0) {
                    iExp.AddExp(exp);
                    iExp.goldDelta += gold;
                }
            }

            //경험치가 올랐음을 알린다. 
            dungeon.SendGetExp(gold);
        }
    }

    public int hp;
    public int max_hp;
    public int animation = 0;

    public int exp = 0;
    public int gold = 0;

    protected void applyDamage(int damage, UserEntity userEntity) {
        damage -= userEntity.user.getDexility() / 4;
        if(damage > 0) {
            userEntity.hp -= damage;
        }
    }
}
