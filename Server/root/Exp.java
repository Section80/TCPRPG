package root;

public class Exp {
    public Exp(User user) {
        this.user = user;
        this.amount = user.character.exp;
        System.out.println("new exp: " + user.character.id);
    }

    public void AddExp(int amount) {
        this.amount += amount;

        int maxEXP = user.character.level * 50;

        while(this.amount >= maxEXP) {
            this.amount = 0;
            this.levelDelta += 1;
            user.character.level += 1;
            maxEXP = user.character.level * 50;
            this.statusPointDelta += 5;
        }
    }

    public User user;
    

    public int levelDelta;
    public int statusPointDelta;
    public int amount;
    public int goldDelta;
}
