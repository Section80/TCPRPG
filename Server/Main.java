import java.util.Timer;
import java.util.TimerTask;

import root.GameServer;

public class Main {
    public static void main(String args[]) {
        server = new GameServer(666);

        Timer timer = new Timer();
        timer.scheduleAtFixedRate(new Task(), 100, 100);
    }

    public static GameServer server;
    public static boolean isUpdated = true;
    public static int deferedTick = 0;

    private static class Task extends TimerTask {
        @Override
        public void run() {
            if(isUpdated == true) {
                isUpdated = false;
                server.update(1 + deferedTick);
                deferedTick = 0;
                isUpdated = true;
            } else {
                System.out.println("defered");
                deferedTick++;
            }
        }
    }
}
