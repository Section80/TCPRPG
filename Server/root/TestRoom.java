package root;

import root.Entity.SlimeEntity;

public class TestRoom extends Room {
    public TestRoom() {
        addEntity(new SlimeEntity(6, 0));
        addEntity(new SlimeEntity(4, 2));
        addEntity(new SlimeEntity(2, 4));
        addEntity(new SlimeEntity(0, 6));
    }

    @Override
    public void addUserEntity(User user, int direction) {
        addUserEntity(user, 0.0f, -15.0f);
    }
}
