package root.Dungeon;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;
import root.Dungeon.Dungeon.eDungeonName;
import root.Dungeon.Dungeon.eStatus;
import root.User;
import root.Room;

public class MatchManager {
    public static MatchManager instance;

    public MatchManager() {
        requests = new ArrayList<>();
        dungeons = new ArrayList<>();

        instance = this;
    }

    public void Update(int tick) {
        for (Dungeon dungeon : dungeons) {
            dungeon.update(tick);
        }

        for (Dungeon dungeon : dungeons) {
            dungeon.SendSyncData();
        }
    }

    //유저의 혼자하기 요청
    public void OnUserSoloPlayRequest(User user, eDungeonName dungeonName) {
        System.out.println("유저 혼자하기 요청");
        if(dungeonName == eDungeonName.SlimeDungeon) {
            System.out.println("캐릭터 ID: " + user.character.id + " 던전: 슬라임 던전");
        }
        else if(dungeonName == eDungeonName.OakDungeon) {
            System.out.println("캐릭터 ID: " + user.character.id + " 던전: 오크 던전");
        }

        //유저가 요청한 던전을 새로 만든다. 
        Dungeon newDungeon = null;
        if(dungeonName == eDungeonName.SlimeDungeon) {
            newDungeon = new SlimeDungeon();
            dungeons.add(newDungeon);
        } 
        else if(dungeonName == eDungeonName.OakDungeon) {
            newDungeon = new OakDungeon();
            dungeons.add(newDungeon);
        }

        //접속할 유저가 정해져 있으니 바로 Playing 상태로 만들어도 된다. 
        newDungeon.status = eStatus.Playing;

        //유저에게 새로 만든 던전의 ID를 보낸다. 
        ByteBuffer outBuffer = ByteBuffer.allocate(6);
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);    //endian 설정하기: BIG_ENDIAN or LITTLE_ENDIAN
        outBuffer.put(0, (byte)10);
        outBuffer.put(1, (byte)0);
        outBuffer.putInt(2, newDungeon.id);

        outBuffer.position(0);
        try {
            user.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    //유저의 매치 찾기 요청
    public void OnUserFindMatchRequest(User user, eDungeonName dungeonName) {
        System.out.println("유저 매치 찾기 요청");
        if(dungeonName == eDungeonName.SlimeDungeon) {
            System.out.println("캐릭터 ID: " + user.character.id + " 던전: 슬라임 던전");
        }
        else if(dungeonName == eDungeonName.OakDungeon) {
            System.out.println("캐릭터 ID: " + user.character.id + " 던전: 오크 던전");
        }
        
        //findMatchRequests에 유저의 요청을 추가한다.
        FindMatchRequest newRequest = new FindMatchRequest(user, dungeonName);
        newRequest.status = FindMatchRequest.eStatus.NotFound;
        requests.add(newRequest);

        onAddRequest(newRequest);
    }

    //유저의 매치 찾기 취소 요청
    public void OnUserCancleFindMatchRequest(User user) {
        System.out.println("유저 매치 찾기 취소 요청");
        System.out.println("캐릭터 ID: " + user.character.id);

        //findMatchRequests에서 유저가 전에 보냈던 요청을 삭제한다. 
        for (FindMatchRequest iRequest : requests) {
            if(iRequest.user == user) {
                requests.remove(iRequest);
                break;
            }
        }
    }

    //유저의 매치 참가 수락
    public void OnUserAcceptMatch(User user) {
        System.out.println("유저 매치 참가 수락");
        System.out.println("캐릭터 ID: " + user.character.id);

        //해당하는 던전 룸의 해당하는 findMatchRequest의 status를 Accepted로 바꾼다.
        Dungeon dungeon = null;
        FindMatchRequest request = null;

        for (Dungeon iDungeon : dungeons) {
            request = iDungeon.GetFindMatchRequestByUser(user);
            if(request != null) {
                dungeon = iDungeon;
                break;
            }
        }

        //예외 처리
        if(dungeon == null) {
            System.out.println("MatchManager.java line 148");
            System.out.println("유저가 입장 수락한 Dungeon을 찾을 수 없습니다. ");
            return;
        }

        if(request == null) {
            System.out.println("MatchManager.java line 153");
            System.out.println("유저의 FindMatchReqeust를 찾을 수 없습니다. ");
            return;
        }

        request.status = FindMatchRequest.eStatus.Accepted;

        //던전 룸의 모든 FindMatchRequest가 accept인지 확인한다.
        if(dungeon.CheckIfAllAccepted()) {
            System.out.println("게임 시작 성공");

            dungeon.status = Dungeon.eStatus.Playing;
            
            //유저들에게 룸 id를 보낸다. 
            ByteBuffer outBuffer = ByteBuffer.allocate(6);
            outBuffer.order(ByteOrder.LITTLE_ENDIAN);    //endian 설정하기: BIG_ENDIAN or LITTLE_ENDIAN
            outBuffer.put(0, (byte)10);
            outBuffer.put(1, (byte)0);
            outBuffer.putInt(2, dungeon.id);

            for (FindMatchRequest iRequest : dungeon.requests) {
                outBuffer.position(0);
                try {
                    iRequest.user.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    public void AddDungeon(Dungeon dungeon) {
        dungeons.add(dungeon);
    }

    //유저의 매치 참가 거절
    public void OnUserRejectMatch(User user) {
        System.out.println("유저 매치 참가 거절");
        System.out.println("캐릭터 ID: " + user.character.id);

        //유저가 매치 입장 거절한 던전, 유저의 FindMatchRequest 찾기
        Dungeon dungeon = null;
        FindMatchRequest request = null;

        for (Dungeon iDungeon : dungeons) {
            request = iDungeon.GetFindMatchRequestByUser(user);
            if(request != null) {
                dungeon = iDungeon;
                break;
            }
        }

        //예외 처리
        if(dungeon == null) {
            System.out.println("MatchManager.java line 148");
            System.out.println("유저가 입장 수락한 Dungeon을 찾을 수 없습니다. ");
            return;
        }

        if(request == null) {
            System.out.println("MatchManager.java line 153");
            System.out.println("유저의 FindMatchReqeust를 찾을 수 없습니다. ");
            return;
        }

        request.status = FindMatchRequest.eStatus.Rejected;

        for (FindMatchRequest iRequest : dungeon.requests) {
            if(iRequest != request){
                //룸에 있던 다른 유저에게 매치 시작 실패 알림 패킷을 보낸다. 
                ByteBuffer outBuffer = ByteBuffer.allocate(2);
                outBuffer.order(ByteOrder.LITTLE_ENDIAN);    //endian 설정하기: BIG_ENDIAN or LITTLE_ENDIAN
                outBuffer.put(0, (byte)10);
                outBuffer.put(1, (byte)2);

                outBuffer.position(0);
                try {
                    iRequest.user.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }

                //accepted 인 경우 NotFound로 바꿔서  GameServer::FindMatchRequests로 옮긴다.
                if(iRequest.status != FindMatchRequest.eStatus.Rejected) {
                    iRequest.status = FindMatchRequest.eStatus.NotFound;
                    requests.add(iRequest);

                    onAddRequest(iRequest);
                }
            }
        }

        //던전 room을 파괴한다.
        dungeons.remove(dungeon);
    }

    //유저의 매치 입장 알림
    public void OnUserEnterDungeonReqeust(User user, int dungeonID) {
        System.out.println("유저 매치 입장 요청");
        System.out.println("캐릭터 ID: " + user.character.id);

        //기존 룸에서 탈퇴
        Room oldRoom = user.room;
        oldRoom.exitUser(user);
        
        //기존 룸에 있넌 유저들에게 해당 유저가 나갔다고 알린다.
        oldRoom.sendRoomUserListToUsers();

        Dungeon dungeonToEnter = getDungeonByID(dungeonID);
        
        if(dungeonToEnter == null) {
            System.out.println("MatchMagager.java line: 200");
            System.out.println("dungeonToEnter is null. ");
            return;
        }

        dungeonToEnter.addUserEntity(user, 0, 0);
        //새로운 방에 있는 유저들에게 새롭게 업데이트된 유저 목록을 보낸다. 
        dungeonToEnter.sendRoomUserListToUsers();
    }

    //유저의 접속이 끊긴 경우
    public void OnUserDisconnected(User user) {
        //유저가 매치 찾기 신청을 한 경우(not found 상태) 삭제한다.
        DeleteFindMatchRequest(user);
        
        //유저의 매치 찾기 신청이 WaitingAccept일 수도 있다. 이 경우에는 거절로 철리해야 한다. 
        //또한, 유저가 이미 매치 입장  수락을 했는데 다른 유저가 모두 수락하기 전에 접속 종료 됬다면
        //이 경우도 매치 거절로 처리해야 한다. 

        //유저가 매치 입장 거절한 던전, 유저의 FindMatchRequest 찾기
        Dungeon dungeon = null;
        FindMatchRequest request = null;

        for (Dungeon iDungeon : dungeons) {
            request = iDungeon.GetFindMatchRequestByUser(user);
            if(request != null) {
                dungeon = iDungeon;
                break;
            }
        }

        //WaitingAccept나 Accepted 상태(모든 참여자가 Accept하지 않아 게임이 시작되지 않은 상태)의 요청이 있다면
        if(request != null) {
            if(request.status == FindMatchRequest.eStatus.WaitingAccept ||
                (request.status == FindMatchRequest.eStatus.Accepted && dungeon.status == Dungeon.eStatus.WaitingAccept)) {
                System.out.println("매치 거절 처리");
                
                //거절로 처리한다. 
                for (FindMatchRequest iRequest : dungeon.requests) {
                    if(iRequest != request){
                        //룸에 있던 다른 유저에게 매치 시작 실패 알림 패킷을 보낸다. 
                        ByteBuffer outBuffer = ByteBuffer.allocate(2);
                        outBuffer.order(ByteOrder.LITTLE_ENDIAN);    //endian 설정하기: BIG_ENDIAN or LITTLE_ENDIAN
                        outBuffer.put(0, (byte)10);
                        outBuffer.put(1, (byte)2);

                        outBuffer.position(0);
                        try {
                            iRequest.user.socket.write(outBuffer);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
        
                        //accepted 인 경우 NotFound로 바꿔서  GameServer::FindMatchRequests로 옮긴다.
                        if(iRequest.status != FindMatchRequest.eStatus.Rejected) {
                            iRequest.status = FindMatchRequest.eStatus.NotFound;
                            requests.add(iRequest);
        
                            onAddRequest(iRequest);
                        }
                    }
                }
        
                //던전 room을 파괴한다.
                dungeons.remove(dungeon);
            }
        }


        //유저가 속한 방에서 제거
        if(user.room != null) {
            Room room = user.room;
            user.room.exitUser(user);
            
            if(room.id != 0) {
                //던전에 아무도 없으면 던전을 삭제한다. 
                if(room.users.size() == 0) {
                    DeleteDungeon((Dungeon)room);
                }
            }

            //유저가 속했던 방에 있는 유저들에게 새로운 유저 목록을 보낸다. 
            room.sendRoomUserListToUsers();
        }
    }

    //해당 유저가 보낸 findMatchRequest를 삭제한다.
    public boolean DeleteFindMatchRequest(User user) {
        for (FindMatchRequest iRequest : requests) {
            if(iRequest.user == user) {
                requests.remove(iRequest);
                return true;
            }
        }

        return false;
    }

    public void DeleteDungeon(Dungeon dungeon) {
        dungeons.remove(dungeon);
    }

    public Room GetRoomByID(int id) {
        for (Dungeon dungeon : dungeons) {
            if(dungeon.id == id) {
                return dungeon;
            }
        }

        return null;
    }

    //findMatchRequests에 새로운 리퀘스트를 추가한 뒤 호출해야 하는 함수
    private void onAddRequest(FindMatchRequest request) {
        //동일한 던전에 대한 요청이 있는지 확인한다. 
        for (FindMatchRequest iRequest : requests) {
            //자기 자신이람 매치를 할 수는 없다.
            if(request == iRequest){
                continue;
            }

            //있는 경우
            if(request.dungeonName == iRequest.dungeonName) {
                System.out.println("매치 찾기 성공");
                System.out.println("id1: " + request.user.character.id + " id2: " + iRequest.user.character.id);

                //findMatchRequest에서 삭제
                requests.remove(request);
                requests.remove(iRequest);

                System.out.println("requests size: " + requests.size());

                request.status = FindMatchRequest.eStatus.WaitingAccept;
                iRequest.status = FindMatchRequest.eStatus.WaitingAccept;
                
                //새로운 매치 만들기
                Dungeon newDungeon = new Dungeon();
                if(request.dungeonName == eDungeonName.SlimeDungeon) {
                    newDungeon = new SlimeDungeon();
                }
                else if(request.dungeonName == eDungeonName.OakDungeon) {
                    newDungeon = new OakDungeon();
                }
                newDungeon.requests.add(request);
                newDungeon.requests.add(iRequest);
                dungeons.add(newDungeon);

                //양측 유저에게 매치 찾기 성공 패킷을 보낸다. 
                //보낼 데이터 만들기
                ByteBuffer outBuffer = ByteBuffer.allocate(2);    //버퍼 사이즈 정하기. 단위: byte
                outBuffer.order(ByteOrder.LITTLE_ENDIAN);    //endian 설정하기: BIG_ENDIAN or LITTLE_ENDIAN

                //버퍼에 데이터 넣기
                outBuffer.put(0, (byte)10); //index 0에  (byte)0을 넣는다.
                outBuffer.put(1, (byte)1);

                //데이터 보내기1
                outBuffer.position(0);  //보내기 전에 position을 0으로 설정해야 데이터가 index 0부터 전송된다. 
                
                try {
                    request.user.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }

                //데이터 보내기2
                outBuffer.position(0);  //보내기 전에 position을 0으로 설정해야 데이터가 index 0부터 전송된다. 
                
                try {
                    iRequest.user.socket.write(outBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }

                break;
            }
        }
    }

    private Dungeon getDungeonByID(int dungeonID) {
        for (Dungeon iDungeon : dungeons) {
            if(iDungeon.id == dungeonID){
                return iDungeon;
            }
        }

        return null;
    }

    private ArrayList<FindMatchRequest> requests;
    private ArrayList<Dungeon> dungeons;
}