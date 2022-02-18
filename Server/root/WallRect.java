package root;


//플레이어가 뚫을 수 없는 벽
public class WallRect {
    public WallRect(float x, float y, float w, float h) {
        this.x = x;
        this.y = y;
        this.width = w;
        this.height = h;
    }

    //해당 좌표가 벽 안에 있는지 확인한다. 
    public boolean IsInWallRect(float x, float y) {
        if(this.x <= x && x <= this.x + this.width && this.y <= y && y <= this.y + this.height) {
            return true;
        }
        return false;
    }

    public float x;
    public float y;
    public float width;
    public float height;
}
