package root.Dungeon;

//섹션은 던전의 일부 구역이다.
//유저가 섹션에 들어가면 트리거처럼 몬스터를 스폰한다거나,
//해당 섹션을 완료하기 전 까지는 섹션 밖으로 나가지 못한다거나 하는 기능을 한다. 

public class Section {
    //섹션이 시작됬을 때 호출된다. 적을 스폰하는 등 기능을 넣을 수 있다. 
    public void OnStart() {

    }

    //이 섹션이 활성화 됬을 때 적이 죽으면 호출된다. 
    public void OnEnemyKilled() {
        killedEnemyCount++;
    }

    public float x;
    public float y;
    public float width;
    public float height;

    public Dungeon dungeon;    //해당 섹션이 속해있는 던전
    public int killedEnemyCount;
    public boolean isBossSection;
    public boolean isBlocking;  //해당 섹션에 들어온 유저가 섹션 밖으로 나갈 수 없는가?
}
