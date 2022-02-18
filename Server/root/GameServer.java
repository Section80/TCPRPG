package root;

import java.io.IOException;
import java.util.Set;
import java.util.Vector;

import root.Database.*;
import root.Database.DatabaseThread.StatusType;
import root.Dungeon.Dungeon;
import root.Dungeon.MatchManager;
import root.Dungeon.Dungeon.eDungeonName;
import root.Invitation.Invitation;
import root.Invitation.InvitationManager;

import java.util.HashSet;
import java.util.Iterator;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.net.InetSocketAddress; //ip address + port number로 이루어진 객체
import java.nio.channels.SelectionKey; //Channel을 Selector에 등록하는 것을 나타내는 객체
import java.nio.channels.Selector; //Channel을 등록할 수 있는 객체, 등록된 Channel을 관리하는 객체
import java.nio.channels.ServerSocketChannel;  //서버 소켓
import java.nio.channels.SocketChannel; //클라이언트 소켓

public class GameServer {
    public GameServer(int port) {
        try {
            //DB 접속
            databaseThread = new DatabaseThread();
            databaseThread.start();
            matchManager = new MatchManager();
            invitationManager = new InvitationManager();

            //서버 소켓을 만든다. 
            serverSocket = ServerSocketChannel.open();
            //포트 설정
            serverSocket.bind(new InetSocketAddress(port));
            //논블로킹 모드 설정
            serverSocket.configureBlocking(false);

            //서버에 연결된 클라이언트를 관리할 Selector 객체
            selector = Selector.open();
            //selecotor에 서버 소켓을 OP_ACCEPT로 등록한다.
            //OP_ACCEPT: 클라이언트가 서버에 접속 요청하는 이번트를 감지한다.
            serverSocket.register(selector, SelectionKey.OP_ACCEPT);

            //클라이언트가 보낸 데이터를 저장할 버퍼
            inputBuffer = ByteBuffer.allocate(1024);
            inputBuffer.order(ByteOrder.LITTLE_ENDIAN);

            //접속한 클라이언트 소켓을 모아놓은 Set
            users = new HashSet<>();

            //outputBuffer 준비
            outputBuffer2 = ByteBuffer.allocate(2);
            outputBuffer2.order(ByteOrder.LITTLE_ENDIAN);
            outputBuffer3 = ByteBuffer.allocate(3);
            outputBuffer3.order(ByteOrder.LITTLE_ENDIAN);
            outputBuffer6 = ByteBuffer.allocate(6);
            outputBuffer6.order(ByteOrder.LITTLE_ENDIAN);
            outputBuffer10 = ByteBuffer.allocate(10);
            outputBuffer10.order(ByteOrder.LITTLE_ENDIAN);

            outBuffer5 = ByteBuffer.allocate(5);
            outBuffer5.order(ByteOrder.LITTLE_ENDIAN);
            outBuffer9 = ByteBuffer.allocate(9);
            outBuffer9.order(ByteOrder.LITTLE_ENDIAN);

            rooms = new Vector<>();
            //마을 생성
            Room townRoom = new Room();
            townRoom.wallRects.add(new WallRect(-20.0f, -20.0f, 1.0f, 40.0f));
            townRoom.wallRects.add(new WallRect(-20.0f, -20.0f, 40.0f, 1.0f));
            townRoom.wallRects.add(new WallRect(-20.0f, 20.0f, 40.0f, 1.0f));
            townRoom.wallRects.add(new WallRect(20.0f, -20.0f, 1.0f, 40.0f));
            rooms.add(townRoom);

            //테스트 룸 생성
            //rooms.add(new TestRoom());

            friendRequests = new Vector<>();

            System.out.println("서버 준비 완료");

        } catch(IOException e) {
            e.printStackTrace();
        }
    }

    public void update(int tick) {
        try {
            //이벤트 감지
            //참고
            //selector.selct() 함수를 사용하면 blocking 이 발생한다. 
            selector.selectNow();
            //발생한 이벤트들을 iterator에 넣는다. 
            iterator = selector.selectedKeys().iterator();

            while(iterator.hasNext()) {
                //현재 처리할 이벤트
                SelectionKey key = iterator.next();
                //해당 이벤트를 iterator에서 지워준다. 
                iterator.remove();

                //해당 이벤트가 접속 요청인 경우
                if(key.isAcceptable()) {
                    //접속 요청을 한 클라이언트를 가져온다. 
                    SocketChannel clientSocket = ((ServerSocketChannel)key.channel()).accept();

                    if(enablePrint){
                        System.out.println("유저 접속");
                        System.out.println("address: " + clientSocket.getRemoteAddress().toString());
                    }

                    //서버와 마찬가지로 논 블로킹 모드로 설정해준다. 
                    clientSocket.configureBlocking(false);

                    //접속한 user를 set에 추가한다.
                    User user = new User(clientSocket);
                    users.add(user);

                    //해당 클라이언트에게 data를 받을 것임으로 읽기 모드로 Selector에 등록해준다. 
                    clientSocket.register(selector, SelectionKey.OP_READ, user);
                } //end of if(key.isAcceptable())
                //해당 이벤트가 클라이언트 -> 서버 데이터 전송인 경우
                else if(key.isReadable()) {
                    //데이터를 보낸 채널
                    SocketChannel clientSocket = (SocketChannel)key.channel();

                    if(enablePrint){
                        //System.out.println("===== 유저가 데이터 전송 =====");
                        //System.out.println("address: " + clientSocket.getRemoteAddress().toString());
                    }

                    //보낸 데이터 읽기
                    try {
                        inputBuffer.clear();
                        inputDataLength = clientSocket.read(inputBuffer);

                        if(inputDataLength > 0) {
                            handleInput((User)key.attachment());
                        } 
                        //클라이언트가 연결을 끊은 경우 
                        else {
                            onUserDisconnected(key);
                        }
                    }
                    catch(IOException e) {
                        e.printStackTrace();
                    }
                }//end else if(key.isReadable())
            } //end of if(iterator.hasNext())
        } catch(IOException e) {
            e.printStackTrace();
        }

        //database thread result 처리
        DatabaseThread.Result result = null;
        while((result = databaseThread.resultQueue.poll()) != null) {
            if(result.getClass() == DatabaseThread.LoginResult.class) {
                handleLoginResult((DatabaseThread.LoginResult)result);
            }
            else if(result.getClass() == DatabaseThread.RegisterResult.class) {
                handleRegisterResult((DatabaseThread.RegisterResult)result);
            }
            else if(result.getClass() == DatabaseThread.CharacterListResult.class) {
                handleCharacterListResult((DatabaseThread.CharacterListResult)result);
            }
            else if(result.getClass() == DatabaseThread.CreateCharacterResult.class) {
                handleCreateCharacterResult((DatabaseThread.CreateCharacterResult)result);
            }
            else if(result.getClass() == DatabaseThread.DeleteCharacterResult.class) {
                handleDeleteCharacterResult((DatabaseThread.DeleteCharacterResult)result);
            }
            else if(result.getClass() == DatabaseThread.ItemListResult.class) {
                handleItemListResult((DatabaseThread.ItemListResult)result);
            }
            else if(result.getClass() == DatabaseThread.FriendListResult.class) {
                handleFriendListResult((DatabaseThread.FriendListResult)result);
            }
            else if(result.getClass() == DatabaseThread.AddItemResult.class) {
                handleAddItemResult((DatabaseThread.AddItemResult)result);
            }
            else if(result.getClass() == DatabaseThread.GetEquipedItemResult.class) {
                handleGetEquipedItemResult((DatabaseThread.GetEquipedItemResult)result);
            }
        }

        for (Room room : rooms) {
            room.update(tick);
        }

        for (Room room : rooms) {
            room.SendSyncData(); 
        }

        matchManager.Update(tick);
    }

    private void handleAddItemResult(DatabaseThread.AddItemResult result) {
        if(result.result == DatabaseResult.SUCCESS) {
            result.droppedItem.shouldDestroy = true;

            //아이템 획득 알림 보내기
            outputBuffer2.clear();
            outputBuffer2.put(0, (byte)10);
            outputBuffer2.put(1, (byte)6);
            
            outputBuffer2.position(0);
            try {
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        else if(result.result == DatabaseResult.NO_EMPTY_PLACE) {
            result.droppedItem.waitDB = false;
        }
    }

    private void handleGetEquipedItemResult(DatabaseThread.GetEquipedItemResult result) {
        //스텟 적용
        //무기
        User user = result.user;
        user.weapon.strong = result.weaponSTR;
        user.weapon.dexility = result.weaponDEX;
        user.weapon.intellect = result.weaponINT;
        user.weapon.luck = result.weaponLUCK;
        //방어구
        user.armor.strong = result.armorSTR;
        user.armor.dexility = result.armorDEX;
        user.armor.intellect = result.armorINT;
        user.armor.luck = result.armorLUCK;

        System.out.println("@#@#GetEquipedStr:" + user.weapon.strong);

        Room newRoom = null;
        for(int i = 0; i < rooms.size(); i++) {
            Room room = rooms.get(i);
            if(room.id == result.nextRoomID) {
                newRoom = room;
            }
        }

        if(newRoom == null) {
            newRoom = matchManager.GetRoomByID(result.nextRoomID);
        }

        if(newRoom != null) {
            newRoom.addUserEntity(result.user, result.direction);

            //새로운 방에 있는 유저들에게 새롭게 업데이트된 유저 목록을 보낸다.
            newRoom.sendRoomUserListToUsers();
        }
        else {
            System.out.println("GameServer.java OnUserEnterRoomRequest new Room is null");
        }

    }

    private void onUserDisconnected(SelectionKey key) {
        User user = (User)key.attachment();

        //클라이언트가 연결을 끊은 경우 
        if(enablePrint) {
            System.out.println("유저 접속 끊김");
        }


        matchManager.OnUserDisconnected(user);
        invitationManager.OnUserDisconnected(user);

        //해당 유저의 친구들이 온라인인 경우, 해당 유저 캐릭터가
        //접속을 끊었다고 알려줘야 한다.
        if(user.characterID != -1) {
            outputBuffer6.position(0);
            int pos = 0;
            outputBuffer6.put(pos, (byte)9);
            pos += 1;
            outputBuffer6.put(pos, (byte)2);
            pos += 1;
            outputBuffer6.putInt(pos, user.characterID);

            for (FriendData data : user.friends) {
                if(data.isOnline){
                    User _user = findUserByCharacterID(data.character_id);
                    if(_user != null) {
                        outputBuffer6.position(0);
                        try {
                            _user.socket.write(outputBuffer6);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }

                        for(FriendData _data : _user.friends) {
                            if(_data.character_id == user.characterID) {
                                _data.isOnline = false;
                            }
                        }
                    }
                }
            }
        }
        
        //clients set에서 제거
        users.remove(key.attachment());

        //셀렉터에서 제거
        key.cancel();
    }

    //클라이언트가 data를 보내면 
    public void handleInput(User user) {
        int pos = 0;

        //온 데이터를 끝까지 읽는다!
        while(pos < inputDataLength) {
            //로그인 요청
            if(inputBuffer.get(pos) == 1) {
                pos += 1;
                inputBuffer.position(pos);

                byte[] emailByte = new byte[35 * 2];
                byte[] passwordByte = new byte[20 * 2];
                
                inputBuffer.get(emailByte);
                pos += 70;
                inputBuffer.position(pos);

                inputBuffer.get(passwordByte);
                pos += 40;
                inputBuffer.position(pos);
        
                String email = new String(emailByte).split("\0")[0];
                String password = new String(passwordByte).split("\0")[0];

                databaseThread.RequestLogin(user, email, password);
            }
            //가입 요청
            else if(inputBuffer.get(pos) == 2) {
                pos += 1;
                inputBuffer.position(pos);

                byte[] emailByte = new byte[35 * 2];
                byte[] passwordByte = new byte[20 * 2];
                
                inputBuffer.get(emailByte);
                pos += 70;
                inputBuffer.position(pos);

                inputBuffer.get(passwordByte);
                pos += 40;
                inputBuffer.position(pos);

                String email = new String(emailByte).split("\0")[0];
                String password = new String(passwordByte).split("\0")[0];

                databaseThread.RequestRegister(user, email, password);
            }
            //캐릭터 목록 요청
            else if(inputBuffer.get(pos) == 3) {
                pos += 1;
                inputBuffer.position(pos);

                int account_id = inputBuffer.getInt(1);
                pos += 4;
                inputBuffer.position(pos);

                databaseThread.RequestCharacterList(user, account_id);
            }
            //캐릭터 생성 요청
            else if(inputBuffer.get(pos) == 4) {
                pos += 1;
                inputBuffer.position(pos);

                int job = inputBuffer.get(pos);
                pos += 1;
                inputBuffer.position(pos);

                byte[] nicknameByte = new byte[40];
                inputBuffer.get(nicknameByte);
                pos += 40;
                inputBuffer.position(pos);

                String nickname = new String(nicknameByte).split("\0")[0];

                databaseThread.RequestCreateCharacter(user, nickname, job, user.id);
            }
            //캐릭터 삭제 요청
            else if(inputBuffer.get(pos) == 5) {
                pos += 1;
                inputBuffer.position(pos);

                int character_id = inputBuffer.getInt(pos);
                pos += 4;
                inputBuffer.position(pos);

                databaseThread.RequestDeleteCharacter(user, character_id);
            }
            //게임 입장 요청
            else if(inputBuffer.get(pos) == 6) {
                pos += 1;
                inputBuffer.position(pos);

                int character_id = inputBuffer.getInt(pos);
                pos += 4;
                inputBuffer.position(pos);

                //게임 요청을 한 유저는 이미 로그인한 유저다. 캐릭터 목록을 이미 가지고 있다.
                user.characterID = character_id;
                for (CharacterData c : user.characters) {
                    if(c.id == character_id) {
                        user.nickname = c.nickname;
                        user.character = c;
                    }
                }

                //게임 입장을 요청한 캐릭터가 가지고 있는 아이템 목록을 보낸다.
                databaseThread.RequestItemList(user, character_id);
                //친구 목록
                databaseThread.RequestFriendList(user, character_id);

                for(int i = 0; i < friendRequests.size(); i++) {
                    FriendRequest request = friendRequests.get(i);

                    //게임을 종료하기 전에 요청을 받았다면
                    if(request.to == user.characterID) {

                    }
                    //게임을 종료하기 전에 요청을 보냈다면
                    if(request.from == user.characterID) {

                    }
                }
            }
            //input data
            else if(inputBuffer.get(pos) == 7) {
                if(user.isReadyInput == false) {
                    return;
                }

                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.left = false;
                } else {
                    user.inputStatus.left = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.right = false;
                } else {
                    user.inputStatus.right = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.up = false;
                } else {
                    user.inputStatus.up = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.down = false;
                } else {
                    user.inputStatus.down = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.z = false;
                } else {
                    user.inputStatus.z = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.x = false;
                } else {
                    user.inputStatus.x = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.a = false;
                } else {
                    user.inputStatus.a = true;
                }
                pos += 1;
                inputBuffer.position(pos);

                if(inputBuffer.get(pos) == 0) {
                    user.inputStatus.s = false;
                } else {
                    user.inputStatus.s = true;
                }
                pos += 1;
                inputBuffer.position(pos);
            }
            //유저 입력 준비 완료: 입력 준비가 완료된 이후에만 sync data를 보낸다.
            else if(inputBuffer.get(pos) == 8) {
                pos += 1;
                inputBuffer.position(pos);
                byte isReady = inputBuffer.get(pos);
                pos += 1;
                inputBuffer.position(pos);

                if(isReady == 1) {
                    System.out.println("유저 입력 준비: true");
                    user.isReadyInput = true;
                }
                else if(isReady == 0) {
                    System.out.println("유저 입력 준비: false");
                    user.isReadyInput = false;
                }
            }
            //룸 입장 요청
            else if(inputBuffer.get(pos) == 9) {
                pos += 1;
                inputBuffer.position(pos);
                
                int room_id = inputBuffer.getInt(pos);
                pos += 4;
                inputBuffer.position(pos);

                System.out.println("유저 룸 입장 요청 room_id: " + room_id);

                byte direction = inputBuffer.get(pos);
                pos += 1;
                inputBuffer.position(pos);

                OnUserEnterRoomRequest(user, room_id, direction);
            }
            //친구 관련
            else if(inputBuffer.get(pos) == 10) {
                pos += 1;
                inputBuffer.position(pos);

                byte type = inputBuffer.get(pos);
                pos += 1;

                //친구 요청
                if(type == 0) {
                    int from = user.characterID;
                    int to = inputBuffer.getInt(pos);
                    pos += 4;

                    if(enablePrint) {
                        System.out.println("친구 요청");
                        System.out.println("from: " + from + " to: " + to);
                    }

                    //요청받은 유저가 온라인이 아닌 경우 거절 처리된다.
                    User toUser = findUserByCharacterID(to);
                    if(toUser == null) {
                        System.out.println("친구가 온라인이 아님");
                        outputBuffer6.position(0);
                        outputBuffer6.put(0, (byte)10);
                        outputBuffer6.put(1, (byte)2);
                        outputBuffer6.putInt(2, to);

                        outputBuffer6.position(0);
                        try {
                            user.socket.write(outputBuffer6);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
                    }


                    boolean isThereReverse = false;
                    //역 요청 찾기
                    for (FriendRequest rq : friendRequests) {
                        //서로 요청한 경우 
                        if(rq.from == to && rq.to == from) {
                            isThereReverse = true;

                            //from에게 수락 알리기
                            outputBuffer6.position(0);
                            outputBuffer6.put(0, (byte)9);
                            outputBuffer6.put(1, (byte)4);
                            outputBuffer6.putInt(2, to);

                            outputBuffer6.position(0);
                            try {
                                user.socket.write(outputBuffer6);
                            } catch (IOException e) {
                                e.printStackTrace();
                            }

                            //to에게 수락 알리기
                            outputBuffer6.position(0);
                            outputBuffer6.put(0, (byte)9);
                            outputBuffer6.put(1, (byte)4);
                            outputBuffer6.putInt(2, from);

                            outputBuffer6.position(0);
                            try {
                                toUser.socket.write(outputBuffer6);
                            } catch (IOException e) {
                                e.printStackTrace();
                            }

                            //mysql 친구 데이터 추가하기
                            databaseThread.AddFriend(user, from, to);

                            //friendData 객체 추가하기
                            user.friends.add(new FriendData(toUser.nickname, to, true));
                            toUser.friends.add(new FriendData(user.nickname, user.characterID, true));

                            //request에서 삭제하기
                            //request에서 삭제하기
                            for(int i = 0; i < friendRequests.size(); i++) {
                                FriendRequest request = friendRequests.get(i);
                                if(request.isSame(from, to)) {
                                    friendRequests.remove(request);
                                }
                            }
                        }
                    }

                    //역 요청이 없는 경우
                    //친구 요청을 받은 유저에게 알리기
                    if(isThereReverse == false) {
                        ByteBuffer outBuffer = ByteBuffer.allocate(1 + 1 + 4 + 40);
                        outBuffer.order(ByteOrder.LITTLE_ENDIAN);

                        outBuffer.put(0, (byte)9);
                        outBuffer.put(1, (byte)3);
                        outBuffer.putInt(2, from);
                        outBuffer.position(6);
                        outBuffer.put(user.nickname.getBytes(), 0, user.nickname.getBytes().length);

                        outBuffer.position(0);
                        try {
                            toUser.socket.write(outBuffer);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }

                        //request에 추가하기
                        friendRequests.add(new FriendRequest(from, user.nickname, to, toUser.nickname));
                    }
                } 
                //친구 요청 수락
                else if(type == 2) {
                    int requester = inputBuffer.getInt(pos);
                    User requesterUser = findUserByCharacterID(requester);
                    pos += 4;

                    if(enablePrint){
                        System.out.println("친구 요청 수락");
                        System.out.println("요청한 사람:" + requester + " 수락한 사람: " + user.characterID);
                    }

                    //mysql 친구 데이터 추가하기
                    databaseThread.AddFriend(user, requester, user.characterID);

                    //friendData 객체 추가하기
                    user.friends.add(new FriendData(requesterUser.nickname, requester, true));
                    requesterUser.friends.add(new FriendData(user.nickname, user.characterID, true));

                    //request에서 삭제하기
                    for(int i = 0; i < friendRequests.size(); i++) {
                        FriendRequest request = friendRequests.get(i);
                        if(request.isSame(requester, user.characterID)) {
                            friendRequests.remove(request);
                        }
                    }

                    //요청한 유저에게 친구 수락 알리기
                    ByteBuffer outBuffer = ByteBuffer.allocate(1 + 1 + 4);
                    outBuffer.order(ByteOrder.LITTLE_ENDIAN);

                    outBuffer.put(0, (byte)9);
                    outBuffer.put(1, (byte)5);
                    outBuffer.putInt(2, user.characterID);

                    outBuffer.position(0);
                    try {
                        requesterUser.socket.write(outBuffer);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                } 
                //친구 요청 거절
                else if(type == 3) {
                    int requester = inputBuffer.getInt(pos);
                    pos += 4;
                    User requesterUser = findUserByCharacterID(requester);
                    
                    if(enablePrint){
                        System.out.println("친구 요청 거절");
                        System.out.println("요청한 사람:" + requester + " 거절한 사람: " + user.characterID);
                    }

                    //request에서 삭제하기
                    for(int i = 0; i < friendRequests.size(); i++) {
                        FriendRequest request = friendRequests.get(i);
                        if(request.isSame(requester, user.characterID)) {
                            friendRequests.remove(request);
                        }
                    }

                    //요청한 유저에게 친구 거절 알리기
                    ByteBuffer outBuffer = ByteBuffer.allocate(1 + 1 + 4);
                    outBuffer.order(ByteOrder.LITTLE_ENDIAN);

                    outBuffer.put(0, (byte)9);
                    outBuffer.put(1, (byte)6);
                    outBuffer.putInt(2, user.characterID);

                    outBuffer.position(0);
                    try {
                        requesterUser.socket.write(outBuffer);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                } 
                //친구 요청 취소
                else if(type == 1) {
                    int to = inputBuffer.getInt(pos);
                    User toUser = findUserByCharacterID(to);
                    pos += 4;

                    if(enablePrint){
                        System.out.println("친구 요청 취소");
                        System.out.println("요청한 사람:" + to + "취소한 사람: " + user.characterID);
                    }

                    //request에서 삭제하기
                    for(int i = 0; i < friendRequests.size(); i++) {
                        FriendRequest request = friendRequests.get(i);
                        if(request.isSame(to, user.characterID)) {
                            friendRequests.remove(request);
                        }
                    }

                    outputBuffer6.position(0);
                    outputBuffer6.put(0, (byte)9);
                    outputBuffer6.put(1, (byte)4);
                    outputBuffer6.putInt(2, user.characterID);

                    outputBuffer6.position(0);
                    try {
                        toUser.socket.write(outputBuffer6);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
                //친구 삭제
                else if(type == 4) {
                    int from = user.characterID;
                    int to = inputBuffer.getInt(pos);
                    User toUser = findUserByCharacterID(to);
                    pos += 4;

                    if(enablePrint) {
                        System.out.println("친구 삭제");
                        System.out.println("from: " + from + " to: " + to);
                    }
                    
                        if(toUser != null) {
                        outputBuffer6.position(0);
                        outputBuffer6.put(0, (byte)9);
                        outputBuffer6.put(1, (byte)7);
                        outputBuffer6.putInt(2, from);

                        outputBuffer6.position(0);
                        try {
                            toUser.socket.write(outputBuffer6);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
                    }

                    databaseThread.DeleteFriend(user, from, to);
                }
            }
            //아이템 관련
            else if(inputBuffer.get(pos) == 11) {
                pos += 1;
                byte type = inputBuffer.get(pos);
                pos += 1;

                //아이템 장착
                if(type == 0) {
                    byte itemType = inputBuffer.get(pos);
                    pos += 1;

                    int item_id = inputBuffer.getInt(pos);
                    pos += 4;

                    if(enablePrint) {
                        System.out.println("아이템 장착/해제");
                        System.out.println("아이템 id: " + item_id); 
                    }

                    //방어구 착용
                    if(itemType == 0) {
                        //착용 해제
                        if(item_id == 0) {
                            user.armor = null;
                        }
                        else {
                            for (Item item : user.items) {
                                if(item.id == item_id) {
                                    user.armor = item;
                                }
                            }
                        }
                        databaseThread.EquipItem(user, user.characterID, false, item_id);
                    }
                    //무기 착용
                    else if(itemType == 1) {
                        //착용 해제
                        if(item_id == 0) {
                            user.armor = null;
                        }
                        else {
                            for (Item item : user.items) {
                                if(item.id == item_id) {
                                    user.weapon = item;
                                }
                            }
                        }
                        databaseThread.EquipItem(user, user.characterID, true, item_id);
                    }
                }
            }
            //스텟 올리기
            else if(inputBuffer.get(pos) == 12) {
                pos += 1;

                byte status_type = inputBuffer.get(pos);
                pos += 1;

                if(user.character.status_point > 0) {
                    user.character.status_point -= 1;
                    DatabaseThread.StatusType type = StatusType.NONE;

                    if(status_type == 0) {
                        type = StatusType.STR;
                    }
                    else if(status_type == 1) {
                        type = StatusType.DEX;
                    }
                    else if(status_type == 2) {
                        type = StatusType.INT;
                    }
                    else if(status_type == 3) {
                        type = StatusType.LUCK;
                    }

                    databaseThread.UseStatusPoint(user, user.characterID, type);
                }
            }
            //던전 관련
            else if(inputBuffer.get(pos) == 13) {
                pos += 1;
                
                byte type = inputBuffer.get(pos);
                pos += 1;

                //혼자 하기 요청
                if(type == 1) {
                    byte dungeonTypeByte = inputBuffer.get(pos);
                    pos += 1;

                    eDungeonName dungeonName = Dungeon.ByteToEDungeonName(dungeonTypeByte);
                    matchManager.OnUserSoloPlayRequest(user, dungeonName);
                }
                //매치 찾기 요청
                else if(type == 2) {
                    byte dungeonTypeByte = inputBuffer.get(pos);
                    pos += 1;

                    eDungeonName dungeonName = Dungeon.ByteToEDungeonName(dungeonTypeByte);
                    matchManager.OnUserFindMatchRequest(user, dungeonName);
                }
                //매치 찾기 취소 요청
                else if(type == 3) {
                    matchManager.OnUserCancleFindMatchRequest(user);
                }
                //매치 수락
                else if(type == 4) {
                    matchManager.OnUserAcceptMatch(user);
                }
                //매치 거절
                else if(type == 5) {
                    matchManager.OnUserRejectMatch(user);
                }
            }
            //던전 초대 관련
            else if(inputBuffer.get(pos) == 14) {
                pos += 1;

                byte type = inputBuffer.get(pos);
                pos += 1;

                //type에 따라 inviter일 수도 있고 invited일 수도 있다. 
                int characterID = inputBuffer.getInt(pos);
                pos += 4;

                User _user = findUserByCharacterID(characterID);

                //친구 초대
                if(type == 0) {
                    byte dungeonType = inputBuffer.get(pos);
                    pos += 1;

                    System.out.println("던전 초대 inviter: " + user.character.id + " " + "invited: " + characterID);

                    eDungeonName dungeonName = eDungeonName.None;
                    if(dungeonType == 0) {
                        dungeonName = eDungeonName.SlimeDungeon;
                        System.out.println("던전 타입: 슬라임 던전");
                    }
                    else if(dungeonType == 1) {
                        dungeonName = eDungeonName.OakDungeon;
                        System.out.println("던전 타입: 오크 던전");
                    }

                    invitationManager.OnUserInvite(user, _user, dungeonName);
                }
                //친구 초대 거절
                else if(type == 1) {
                    System.out.println("던전 초대 거절 inviter: " + characterID + " " + "invited: " + user.character.id);
                    invitationManager.OnUserRejectInvite(_user, user);
                }
                //친구 초대 수락
                else if(type == 2) {
                    System.out.println("던전 초대 수락 inviter: " + characterID + " " + "invited: " + user.character.id);
                    invitationManager.OnUserAcceptInvite(_user, user);
                }
                //친구 초대 취소
                else if(type == 3) {
                    System.out.println("던전 초대 취소 inviter: " + user.character.id + " " + "invited: " + characterID);
                    if(_user != null) {
                        invitationManager.OnUserCancleInvite(user, _user);
                    }
                }
            }
        }
    }

    int counter = 0;

    private User findUserByCharacterID(int character_id) {
        for (User user : users) {
            if(user.characterID == character_id) {
                return user;
            }
        }
        
        return null;
    }

    //해당 함수는 유저가 로그인 후 캐릭터를 선택해 인게임에 접속하거나, 캐릭터를 바꿔서 접속할 때만 발생한다. 
    public void handleFriendListResult(DatabaseThread.FriendListResult result) {
        if(enablePrint) {
            System.out.println("친구 목록 요청 결과");
            System.out.println("친구 수: " + result.friends.size());
        }

        //해당 유저가 온라인 상태가 되었다는 것을 나타내는 byte bufffer
        ByteBuffer outBuffer = ByteBuffer.allocate(1 + 1 + 4);
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);
        int pos = 0;
        outBuffer.put(pos, (byte)9);
        pos += 1;
        outBuffer.put(pos, (byte)1);
        pos += 1;
        outBuffer.putInt(pos, result.character_id);

        //요청한 user 객체의 친구 목록을 설정한다. 
        result.user.friends = result.friends;

        //각 유저들이 online인지 확인한다.
        for (FriendData data : result.user.friends) {
            //방금 접속한 유저와 친구인 유저
            for (User friendUser : users) {
                if(data.character_id == friendUser.characterID) {
                    data.isOnline = true;

                    //온라인인 유저의 경우 친구가 방금 접속했다고 알려야 한다. 
                    //위에서 만든 버퍼를 보낸다.
                    outBuffer.position(0);
                    try {
                        friendUser.socket.write(outBuffer);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }

                    //해당 유저의 친구인 유저의 친구 목록에서 해당 유저를 온라인 상태로 만들어야 한다. 
                    for (FriendData _data : friendUser.friends) {
                        System.out.println("test4: " + _data.character_id);
                        if(_data.character_id == result.user.characterID) {
                            _data.isOnline = true;
                            System.out.println("test3");
                            break;
                        }
                    }
                    
                    break;
                }
            }
        }

        //자, 해당 유저 클라이언트에게 친구 목록과 온라인 여부를 보내야 한다. 
        sendFriendList(result.user);
    }

    private void sendFriendList(User user) {
        ByteBuffer outBuffer = ByteBuffer.allocate(1 + 1 + 4 + 45 * user.friends.size());
        outBuffer.order(ByteOrder.LITTLE_ENDIAN);

        int pos = 0;
        outBuffer.put(pos, (byte)9);
        pos += 1;

        outBuffer.put(pos, (byte)0);
        pos += 1;

        outBuffer.putInt(pos, user.friends.size());
        pos += 4;

        for(int i = 0; i < user.friends.size(); i++) {
            FriendData data = user.friends.get(i);
            outBuffer.putInt(pos, data.character_id);
            pos += 4;

            byte[] stringBytes = data.nickname.getBytes();
            outBuffer.position(pos);
            outBuffer.put(stringBytes, 0, stringBytes.length);
            pos += 40;

            if(data.isOnline) {
                outBuffer.put(pos, (byte)1);
            } else {
                outBuffer.put(pos, (byte)0);
            }
            pos += 1;
        }

        outBuffer.position(0);
        try {
            user.socket.write(outBuffer);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void handleItemListResult(DatabaseThread.ItemListResult result) {
        if(enablePrint) {
            System.out.println("게임 시작 요청 결과");
            System.out.println("캐릭터 ID: " + result.character_id);
            System.out.println("아이템 개수: " + result.items.size());
        }

        result.user.items = result.items;
        result.user.roomID = 0;
        result.user.SetCharacter(result.character_id);

        //user가 장착하고 있는 아이템 설정
        //방어구
        if(result.user.character.armor_id != 0) {
            for(int i = 0; i < result.user.items.size(); i++) {
                Item item = result.user.items.get(i);
                if(item.id == result.user.character.armor_id) {
                    result.user.armor = item;
                }
            }
        }

        //무기
        if(result.user.character.weapon_id != 0) {
            for(int i = 0; i < result.user.items.size(); i++) {
                Item item = result.user.items.get(i);
                if(item.id == result.user.character.weapon_id) {
                    result.user.weapon = item;
                }
            }
        }

        //마을에 userEntity객체를 추가한다.
        rooms.get(0).addUserEntity(result.user, 0.0f, 0.0f);
        //현재 마을에 접속해있는 유저들의 목록을 보낸다.
        rooms.get(0).sendRoomUserListToUsers();

        if(result.result == DatabaseResult.SUCCESS) {
            ByteBuffer outputBuffer = ByteBuffer.allocate(1 + 1 + 4 + 77 * result.items.size());
            outputBuffer.order(ByteOrder.LITTLE_ENDIAN);
            outputBuffer.put(0, (byte)6);
            outputBuffer.put(1, (byte)0);
            outputBuffer.putInt(2, result.items.size());
            int pos = 6;

            for(int i = 0; i < result.items.size(); i++) {
                Item item = result.items.get(i);
                outputBuffer.putInt(pos, item.id);
                pos += 4;
                outputBuffer.putInt(pos, item.character_id);
                pos += 4;
                
                byte[] stringBytes = item.name.getBytes();
                outputBuffer.position(pos);
                outputBuffer.put(stringBytes, 0, stringBytes.length);
                pos += 40;
                
                outputBuffer.putInt(pos, item.index_in_inventory);
                pos += 4;
                
                byte is_weapon = 0;
                if(item.is_weapon) {
                    is_weapon = 1;
                }
                outputBuffer.put(pos, is_weapon);
                pos += 1;

                outputBuffer.putInt(pos, item.job);
                pos += 4;
                
                outputBuffer.putInt(pos, item.level_limit);
                pos += 4;

                outputBuffer.putInt(pos, item.strong);
                pos += 4;
                outputBuffer.putInt(pos, item.dexility);
                pos += 4;
                outputBuffer.putInt(pos, item.intellect);
                pos += 4;
                outputBuffer.putInt(pos, item.luck);
                pos += 4;
            }

            outputBuffer.position(0);
            try {
                result.user.socket.write(outputBuffer);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        else if(result.result == DatabaseResult.FAIL) {
            outputBuffer2.position(0);
            outputBuffer2.putInt(0, (byte)6);   //type
            outputBuffer2.putInt(1, (byte)1);   //result: fail(1)

            outputBuffer2.position(0);
            try {
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    public void handleDeleteCharacterResult(DatabaseThread.DeleteCharacterResult result) {
        if(enablePrint) {
            System.out.println("캐릭터 삭제 요청 결과");
        }

        //DatabaseResult result = database.DeleteCharacter(characterID);

        if(result.result == DatabaseResult.SUCCESS) {
            outputBuffer6.position(0);
            outputBuffer6.put(0, (byte)5);
            outputBuffer6.put(1, (byte)0);
            outputBuffer6.putInt(2, result.character_id);
            try {
                outputBuffer6.position(0);
                result.user.socket.write(outputBuffer6);
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if(result.result == DatabaseResult.NO_CHARACTER) {
            outputBuffer2.position(0);
            outputBuffer2.put(0, (byte)5);
            outputBuffer2.put(1, (byte)1);
            outputBuffer2.position(0);

            try {
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if(result.result == DatabaseResult.FAIL) {
            outputBuffer2.position(0);
            outputBuffer2.put(0, (byte)5);
            outputBuffer2.put(1, (byte)1);
            outputBuffer2.position(0);

            try {
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    public void handleCreateCharacterResult(DatabaseThread.CreateCharacterResult result) {
        if(enablePrint) {
            System.out.println("캐릭터 생성 요청 결과");
        }

        CharacterData character = new CharacterData();
        character.id = result.character_id;
        character.nickname = result.nickname;
        character.job = result.job;
        character.level = 1;
        character.weapon_id = 0;
        character.armor_id = 0;
        character.status_point = 0;
        character.gold = 1000;
        character.exp = 0;
        character.strong = 10;
        character.dexility = 10;
        character.intellect = 10;
        character.luck = 10;

        result.user.characters.add(character);
        

        if(result.result == DatabaseResult.SUCCESS) {
            outputBuffer6.position(0);
            outputBuffer6.put(0, (byte)4);
            outputBuffer6.put(1, (byte)0);
            outputBuffer6.putInt(2, result.character_id);
            try {
                outputBuffer6.position(0);
                result.user.socket.write(outputBuffer6);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }  else if(result.result == DatabaseResult.DUPLICATE_NICKNAME) {
            outputBuffer2.position(0);
            outputBuffer2.put(0, (byte)4);
            outputBuffer2.put(1, (byte)1);
            try {
                outputBuffer2.position(0);
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        } else if(result.result == DatabaseResult.FAIL) {
            outputBuffer2.position(0);
            outputBuffer2.put(0, (byte)4);
            outputBuffer2.put(1, (byte)2);
            try {
                outputBuffer2.position(0);
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    public void handleLoginResult(DatabaseThread.LoginResult result) {
        if(enablePrint) {
            System.out.println("로그인 요청 결과");

            System.out.println("email: " + result.email);
            System.out.println("password: " + result.password);
        }

        //result
        try {
            outputBuffer6.position(0);
            outputBuffer6.put(0, (byte)1);

            if(result.result == DatabaseResult.SUCCESS) {
                if(enablePrint) {
                    System.out.println("로그인 결과: 성공");
                    System.out.println("account id:" + result.account_id);
                }

                result.user.id = result.account_id;

                outputBuffer6.put(1, (byte)0);
                outputBuffer6.putInt(2, result.account_id);
            } else if(result.result == DatabaseResult.WRONG_EMAIL) {
                if(enablePrint) {
                    System.out.println("로그인 결과: 등록되지 않은 이메일");
                }

                outputBuffer6.put(1, (byte)1);
            } else if(result.result == DatabaseResult.WRONG_PASSWORD) {
                if(enablePrint) {
                    System.out.println("로그인 결과: 비밀번호 불일치");
                }

                outputBuffer6.put(1, (byte)2);
            } else if(result.result == DatabaseResult.FAIL) {
                if(enablePrint) {
                    System.out.println("로그인 결과: SQL FAIl");
                }

                outputBuffer6.put(1, (byte)3);
            }

            outputBuffer6.position(0);
            result.user.socket.write(outputBuffer6);
        } catch(IOException e) {
            e.printStackTrace();
        }
    }

    public void handleRegisterResult(DatabaseThread.RegisterResult result) {
        if(enablePrint) {
            System.out.println("가입 요청 결과");

            System.out.println("email: " + result.email);
            System.out.println("password: " + result.password);
        }

        //result
        try {
            outputBuffer2.put(0, (byte)2);

            if(result.result == DatabaseResult.SUCCESS) {
                if(enablePrint) {
                    System.out.println("가입 결과: 성공");
                }

                outputBuffer2.put(1, (byte)0);
                result.user.socket.write(outputBuffer2);
            } else if(result.result == DatabaseResult.DUPLICATE_EMAIL) {
                if(enablePrint) {
                    System.out.println("가입 결과: 중복된 이메일");
                }

                outputBuffer2.put(1, (byte)1);
                result.user.socket.write(outputBuffer2);
            } else if(result.result == DatabaseResult.FAIL) {
                if(enablePrint) {
                    System.out.println("가입 결과: SQL FAIl");
                }

                outputBuffer2.put(1, (byte)2);
                result.user.socket.write(outputBuffer2);
            }
        } catch(IOException e) {
            e.printStackTrace();
        }
    }

    public void handleCharacterListResult(DatabaseThread.CharacterListResult result) {
        if(enablePrint) {
            System.out.println("캐릭터 목록 요청 결과 ID: " + result.account_id);
        }

        if(result.result == DatabaseResult.SUCCESS) {
            if(enablePrint) {
                System.out.println("캐릭터 개수: " + result.characters.size());
            }
            
            ByteBuffer outputBuffer = ByteBuffer.allocate(1 + 1 + 4 + 88 * result.characters.size());
            outputBuffer.order(ByteOrder.LITTLE_ENDIAN);
            outputBuffer.put(0, (byte)3);
            outputBuffer.put(1, (byte)0);
            outputBuffer.putInt(2, result.characters.size());

            int writePos = 6;
            
            for(int i = 0; i < result.characters.size(); i++) {
                CharacterData d = result.characters.get(i);
                
                //왜그런지 모르겠으나
                //outputBuffer.putInt(d.id); 하면 클라이언트한테 데이터가 안감
                //도데체 외?

                outputBuffer.putInt(writePos, d.id);
                writePos+= 4;

                byte[] stringBytes = result.characters.get(i).nickname.getBytes();

                outputBuffer.position(writePos);
                outputBuffer.put(stringBytes, 0, stringBytes.length);
                writePos += 40;
                outputBuffer.putInt(writePos, d.job);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.level);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.weapon_id);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.armor_id);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.status_point);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.gold);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.exp);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.strong);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.dexility);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.intellect);
                writePos+= 4;
                outputBuffer.putInt(writePos, d.luck);
                writePos+= 4;

                System.out.println(result.characters.get(i).nickname);
            }
            try {
                outputBuffer.position(0);
                result.user.socket.write(outputBuffer);
                result.user.characters = result.characters;
            } catch (IOException e) {
                e.printStackTrace();
            }

        } else if(result.result == DatabaseResult.FAIL) {
            if(enablePrint) {
                System.out.println("캐릭터 목록 가져오기 실패");
            }

            outputBuffer2.put(0, (byte)3);
            outputBuffer2.put(1, (byte)0);
            try {
                result.user.socket.write(outputBuffer2);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    public void OnUserEnterRoomRequest(User user, int room_id, byte direction) {
        //기존 룸에서 탈퇴
        Room oldRoom = user.room;
        oldRoom.exitUser(user);
        //기존 룸에 있는 유저들에게 해당 유저가 나갔다고 알린다. 
        oldRoom.sendRoomUserListToUsers();

        databaseThread.GetEquipedItem(user, room_id, user.character.id, user.character, direction);
    }

    private ServerSocketChannel serverSocket = null;
    //서버에 연결된 클라이언트를 관리할 Selector 객체
    private Selector selector = null;
    //이벤트 발생 시 해당 iterator로 이벤트를 넣는다. 
    private Iterator<SelectionKey> iterator = null;

    //클라이언트가 보낸 데이터를 저장할 버퍼
    public ByteBuffer inputBuffer;

    //클라이언트에게 보낼 버퍼: 보낼 때마다 할당하면 너무 비효율적이기 때문에 미리 크기별로 만들어둔다.
    private ByteBuffer outputBuffer2;
    private ByteBuffer outputBuffer3;
    private ByteBuffer outputBuffer6;
    private ByteBuffer outputBuffer10;

    public static ByteBuffer outBuffer5;
    public static ByteBuffer outBuffer9;

    //접속한 유저 정보를 모아놓은 Set
    public Set<User> users;

    //세팅
    public boolean enablePrint = true;


    //컴포넌트
    public static DatabaseThread databaseThread;
    public MatchManager matchManager;
    public InvitationManager invitationManager;

    //Rooms
    public Vector<Room> rooms;  //어차피 마을 룸 하나밖에 없어서 궂이 벡터 쓸 필요 없긴함.

    //Friend Requests
    public Vector<FriendRequest> friendRequests;

    //input data byte size
    public int inputDataLength = 0;
}