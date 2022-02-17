using System; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;
using Photon.Chat;

public class ClientSocket : MonoBehaviour
{
    private string serverIP = "3.36.102.164";
    public static ClientSocket instance;    //다른 코드에서 참조하기 위한 static instance
    public static string phpIP = "15.165.235.216";

    public void SendRequest(byte[] data) {
        outputDatas.Push(data);
    }

    //private
    private void Awake() {
        DontDestroyOnLoad(gameObject);
        instance = GetComponent<ClientSocket>();
    }

    private class AsyncObject { 
        public byte[] Buffer; 
        public Socket WorkingSocket; 
        public AsyncObject(int bufferSize) { 
            this.Buffer = new byte[bufferSize]; 
        } 
    }

    private void OnReceive(IAsyncResult ar) { 
        AsyncObject ao = (AsyncObject)ar.AsyncState; 
        int inputDataSize = ao.WorkingSocket.EndReceive(ar); 
        if(inputDataSize > 0) {
            if (ao.Buffer != null)
            {
                inputDatas.Push(new InputData(ao.Buffer, inputDataSize));
            }
        } 
        socket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, receiveHandler, ao); 
    }

    private void onEndSend(IAsyncResult ar) {
        socket.EndSend(ar);
        isSending = false;
    }

    private bool ConnectToServer() {
        try { 
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
            socket.Connect(IPAddress.Parse(serverIP), serverPort); 
            socket.Blocking = true;
            AsyncObject ao = new AsyncObject(1024); 
            ao.WorkingSocket = socket; 
            receiveHandler = new AsyncCallback(OnReceive); 
            sendHandler = new AsyncCallback(onEndSend);
            socket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, receiveHandler, ao); 
            return true;
        } 
        catch(SocketException e) { 
            Debug.Log(e.Message); 
            return false;
        } 
    }

    private void Start()
    {
        inputDatas = new ConcurrentStack<InputData>();
        outputDatas = new ConcurrentStack<byte[]>();

        if(ConnectToServer()) {
            Debug.Log("서버 접속에 성공했습니다.");
        }
        else {
            Debug.Log("서버 접속에 실패했습니다.");
        }
    }

    private void HandleReceive(byte[] buffer, int size) {
        int pos = 0;

        while(pos < size)
        {
            if(pos != 0)
            {
                //Debug.Log("pos: " + pos + " size: " + size);
            }

            byte type = buffer[pos];
            pos += 1;

            //login result
            if (type == 1)
            {
                byte result = buffer[pos];
                pos += 1;

                int account_id = 0;

                if (result == 0)
                {
                    account_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                }

                User.instance.OnLoginResult(result, account_id);
            }
            //register result
            else if (type == 2)
            {
                byte result = buffer[pos];
                pos += 1;

                User.instance.OnRegisterResult(result);
            }
            //characterList
            else if (type == 3)
            {
                byte result = buffer[pos];
                pos += 1;

                if (result == 0)
                {
                    User.instance.characterDatas.Clear();

                    int characterNum = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    //Debug.Log("캐릭터 개수: " + characterNum);

                    for (int i = 0; i < characterNum; i++)
                    {
                        CharacterData cd = new CharacterData();

                        cd.id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        byte[] nicknameByte = new byte[40];
                        Buffer.BlockCopy(buffer, pos, nicknameByte, 0, 40);
                        cd.nickname = Encoding.Default.GetString(nicknameByte);

                        for (int j = 0; j < cd.nickname.Length; j++)
                        {
                            byte val = (byte)cd.nickname[j];
                            if (val == 0)
                            {
                                cd.nickname = cd.nickname.Substring(0, j);
                                break;
                            }
                        }

                        pos += 40;

                        cd.job = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.level = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.weapon_id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.armor_id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.status_point = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.gold = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.exp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.strong = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.dexility = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.intellect = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        cd.luck = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        User.instance.AddCharacterData(cd);
                    }

                    SceneManager.LoadScene("CharacterListScene");
                }
                else if (result == 1)
                {
                    //TODO: mysql 캐릭터 목록 가져오기 실패 시 처리
                }
            }
            //캐릭터 생성 결과
            else if (type == 4)
            {
                int result = buffer[pos];
                pos += 1;

                int character_id = BitConverter.ToInt32(buffer, pos);
                pos += 4;

                User.instance.OnCreateCharacterResult(result, character_id);
            }
            //캐릭터 삭제 결과
            else if (type == 5)
            {
                int result = buffer[pos];
                pos += 1;

                if (result == 0)
                {
                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    User.instance.OnDeleteCharacterResult(character_id);
                }
                else if (result == 1)
                {
                    //TODO: handle case (there is no character)
                }
                else if (result == 2)
                {
                    //TODO: handle case SQL FAIL
                }
            }
            //게임 시작 결과1: 캐릭터가 가진 아이템 목록
            else if (type == 6)
            {
                int result = buffer[pos];
                pos += 1;

                //성공
                if (result == 0)
                {
                    isItemListArrived = true;

                    int item_num = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    Debug.Log("아이템 개수: " + item_num);

                    Inventory.instance.Clear();

                    for (int i = 0; i < item_num; i++)
                    {
                        ItemData item = new ItemData();

                        item.id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        item.character_id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        byte[] nameByte = new byte[40];
                        Buffer.BlockCopy(buffer, pos, nameByte, 0, 40);
                        item.name = Encoding.Default.GetString(nameByte);
                        pos += 40;

                        item.index_in_inventory = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        byte is_weapon_byte = buffer[pos];
                        if (is_weapon_byte == 0)
                        {
                            item.is_weapon = false;
                        }
                        else if (is_weapon_byte == 1)
                        {
                            item.is_weapon = true;
                        }
                        pos += 1;

                        item.job = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        item.level_limit = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        item.strong = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        item.dexility = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        item.intellect = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        item.luck = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        Inventory.instance.AddItemData(item);
                    }

                    if (isItemListArrived && Room.instance.isUserListArrived)
                    {
                        Room.instance.isUserListArrived = false;
                        Room.instance.hudTransform.gameObject.SetActive(true);

                        if (Room.instance.room_id_to_move == 0)
                        {
                            Room.instance.room_id_to_move = -1;
                            SceneManager.LoadScene("TownScene");
                        }
                    }
                }
                //실패
                else if (result == 1)
                {
                    Debug.Log("아이템 목록 가져오기 실패");
                }
            }
            //게임 시작 결과2: 방에 있는 유저 목록
            else if (type == 7)
            {
                int roomID = BitConverter.ToInt32(buffer, pos);
                int oldID = Room.instance.id;
                Room.instance.id = roomID;
                pos += 4;

                int userNum = BitConverter.ToInt32(buffer, 5);
                pos += 4;

                Debug.Log("유저 목록 도착 룸 id:" + Room.instance.id + " num: " + userNum);

                Room.instance.isUserListArrived = true;

                List<UserData> newUserDatas = new List<UserData>();

                for (int i = 0; i < userNum; i++)
                {
                    UserData ud = new UserData();
                    ud.character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    ud.entity_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    ud.job = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    byte[] nicknameByte = new byte[40];
                    Buffer.BlockCopy(buffer, pos, nicknameByte, 0, 40);
                    ud.nickname = Encoding.Default.GetString(nicknameByte);
                    pos += 40;

                    Debug.Log(ud.nickname);

                    newUserDatas.Add(ud);
                }

                if (isItemListArrived && Room.instance.isUserListArrived)
                {
                    if (Room.instance.room_id_to_move == roomID)
                    {
                        //새로운 룸으로 가기 위해 모든 Entity를 지운다.
                        while (Room.instance.entityHolder.transform.childCount > 0)
                        {
                            DestroyImmediate(Room.instance.entityHolder.transform.GetChild(0).transform.gameObject);
                        }

                        int childCount = Room.instance.entityHolder.transform.childCount;
                    }

                }

                if (Room.instance.room_id_to_move == roomID)
                {
                    Room.instance.entityHolder.gameObject.SetActive(false);
                }
                Room.instance.UpdateUserDatas(newUserDatas);
                User.instance.entity = Room.instance.GetUserEntity();
                User.instance.entity.nicknameText.color = new Color(0.0f, 1.0f, 0.0f);
                User.instance.entity.GetComponent<BoxCollider>().enabled = false;

                if (FollowingCamera.instance)
                {
                    FollowingCamera.instance.CheckEntity();
                }
                Room.instance.isGettingInput = true;

                if (isItemListArrived && Room.instance.isUserListArrived)
                {
                    Room.instance.isUserListArrived = false;

                    if (Room.instance.room_id_to_move == 0)
                    {
                        //일반 채팅방 나가기
                        ChattingMenu.chatClient.Unsubscribe(new string[] { oldID.ToString() });

                        //새로운 방의 일반 채팅방 들어가기
                        ChattingMenu.chatClient.Subscribe(0.ToString(), creationOptions: new ChannelCreationOptions { PublishSubscribers = true });
                        Room.instance.id = 0;
                        Room.instance.room_id_to_move = -1;
                        SceneManager.LoadScene("TownScene");
                    }
                    else if(Room.instance.room_id_to_move == roomID)
                    {
                        if (DungeonMenu.instance != null)
                        {
                            //일반 채팅방 나가기
                            ChattingMenu.chatClient.Unsubscribe(new string[] { oldID.ToString() });

                            //새로운 방의 일반 채팅방 들어가기
                            ChattingMenu.chatClient.Subscribe(roomID.ToString(), creationOptions: new ChannelCreationOptions { PublishSubscribers = true });

                            if (DungeonMenu.instance.selectedDungeon == DungeonMenu.eDungeonName.SlimeDungeon)
                            {
                                Room.instance.id = Room.instance.room_id_to_move;
                                Room.instance.room_id_to_move = -1;
                                DungeonMenu.instance.selectedDungeon = DungeonMenu.eDungeonName.None;
                                SceneManager.LoadScene("SlimeDungeonScene");
                            }
                            else if (DungeonMenu.instance.selectedDungeon == DungeonMenu.eDungeonName.OakDungeon)
                            {
                                Room.instance.id = Room.instance.room_id_to_move;
                                Room.instance.room_id_to_move = -1;
                                DungeonMenu.instance.selectedDungeon = DungeonMenu.eDungeonName.None;
                                SceneManager.LoadScene("OakDungeonScene");
                                Debug.Log("오크던전 입장");
                            }
                        }
                    }
                }
            }
            //게임룸 동기화
            else if (type == 8)
            {
                //중요!!!
                //TODO: 아래 if문 주석 달고 친구 캐릭터 로그아웃 해보기:
                //값 이상하게 받아옴
                if(pos != 1)
                {
                    return;
                }

                if (Room.instance.isGettingInput == false)
                {
                    return;
                }

                syncCounter += 1;
                int entityNum = BitConverter.ToInt32(buffer, 1);
                pos += 4;

                Room.instance.StartSync();

                for (int i = 0; i < entityNum; i++)
                {
                    int entity_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    int entity_type = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    //Entity entity = Room.instance.FindEntityById(entity_id);
                    Entity entity = Room.instance.CreateEntityIfNotExist(entity_id, entity_type);


                    entity.isSyncDataArrived = true;

                    float entity_x = BitConverter.ToSingle(buffer, pos);
                    pos += 4;
                    float entity_y = BitConverter.ToSingle(buffer, pos);
                    pos += 4;
                    entity.target = new Vector3(entity_x, 0.0f, entity_y);

                    float direction = BitConverter.ToSingle(buffer, pos);
                    entity.direction = direction;
                    pos += 4;

                    if (entity_type == 0 || entity_type == 1)
                    {
                        UserEntity ue = entity.GetUserEntity();
                        ue.max_hp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        ue.hp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        ue.max_mp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        ue.mp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        int anim = BitConverter.ToInt32(buffer, pos);
                        entity.animation_id = anim;
                        pos += 4;
                    }
                    else if (entity_type == 3)
                    {
                        SlimeEntity se = entity.GetSlimeEntity();
                        se.max_hp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        se.hp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        int anim = BitConverter.ToInt32(buffer, pos);
                        entity.animation_id = anim;
                        pos += 4;
                    }
                    else if(entity_type == 4)
                    {
                        DroppedItemEntity de = entity.GetDroppedItemEntity();

                        //character_id   
                        de.character_id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        //is_weapon
                        int is_weapon = buffer[pos];
                        if(is_weapon == 1)
                        {
                            de.isWeapon = true;
                        }
                        else
                        {
                            de.isWeapon = false;
                        }
                        pos += 1;

                        //job
                        de.job = buffer[pos];
                        pos += 1;

                        de.Init();
                        
                    }
                    else if(entity_type == 5)
                    {
                        OakBossEntity se = entity.GetOakBossEntity();
                        se.max_hp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        se.hp = BitConverter.ToInt32(buffer, pos);
                        pos += 4;
                        int anim = BitConverter.ToInt32(buffer, pos);
                        entity.animation_id = anim;
                        pos += 4;
                    }

                    if (ClientInput.instance)
                    {
                        ClientInput.instance.x = entity_x;
                        ClientInput.instance.y = entity_y;
                    }
                }

                Room.instance.EndSync();

            }
            //친구
            else if (type == 9)
            {
                //친구 목록의 경우
                if (buffer[pos] == 0)
                {
                    pos += 1;

                    User.instance.friendDatas.Clear();

                    int friendNum = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                   //Debug.Log("친구 목록 도착, 친구 수: " + friendNum);

                    for (int i = 0; i < friendNum; i++)
                    {
                        int character_id = BitConverter.ToInt32(buffer, pos);
                        pos += 4;

                        String nickname = Encoding.Default.GetString(buffer, pos, 40);
                        pos += 40;

                        for (int j = 0; j < nickname.Length; j++)
                        {
                            byte val = (byte)nickname[j];
                            if (val == 0)
                            {
                                nickname = nickname.Substring(0, j);
                                break;
                            }
                        }

                        bool isOnline = false;
                        if (buffer[pos] == 1)
                        {
                            isOnline = true;
                        }
                        pos += 1;

                        FriendData data = new FriendData(nickname, character_id, isOnline);
                        User.instance.friendDatas.Add(data);

                        bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;
                        HudUI.instance.onFriendMenuClicked();
                        ScrollFriendContent.instance.OrganizeChild();
                        if (!isMenuOpend)
                        {
                            HudUI.instance.onCloseFriendMenuClicked();
                        }
                    }
                }
                //친구가 온라인 상태가 된 경우
                else if (buffer[pos] == 1)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    User.instance.OnFriendCharacterOnline(character_id);

                    foreach (FriendData data in User.instance.friendDatas)
                    {
                        if (data.character_id == character_id)
                        {
                            ChattingMenu.instance.InfoMessage("친구 " + data.nickname + "님이 접속했습니다. ");
                        }
                    }
                }
                //친구가 오프라인 상태가 된 경우
                else if (buffer[pos] == 2)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    User.instance.OnFriendCharacterOffline(character_id);
                    Debug.Log("친구 접속 해제: " + character_id);

                    foreach (FriendData data in User.instance.friendDatas)
                    {
                        if (data.character_id == character_id)
                        {
                            ChattingMenu.instance.InfoMessage("친구 " + data.nickname + "님이 접속해제했습니다. ");
                        }
                    }
                }
                //다른 유저가 친구 요청을 한 경우
                else if (buffer[pos] == 3)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    String nickname = Encoding.Default.GetString(buffer, pos, 40);
                    pos += 40;

                    Debug.Log("친구 요청 도착 닉네임: " + nickname);

                    bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;
                    HudUI.instance.onFriendMenuClicked();
                    ScrollFriendContent.instance.addRequest(nickname, character_id);
                    if (!isMenuOpend)
                    {
                        HudUI.instance.onCloseFriendMenuClicked();
                    }
                }
                //요청했던 상대가 요청을 취소한 경우
                else if (buffer[pos] == 4)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;
                    HudUI.instance.onFriendMenuClicked();

                    ScrollFriendContent.instance.handleCancleRequest(character_id);

                    if (!isMenuOpend)
                    {
                        HudUI.instance.onCloseFriendMenuClicked();
                    }
                }
                //요청을 보낸 상대가 수락한 경우
                else if (buffer[pos] == 5)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;
                    HudUI.instance.onFriendMenuClicked();

                    ScrollFriendContent.instance.handleAcceptRequest(character_id);

                    if (!isMenuOpend)
                    {
                        HudUI.instance.onCloseFriendMenuClicked();
                    }
                }
                //요청을 보낸 상대가 거절한 경우
                else if (buffer[pos] == 6)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;
                    HudUI.instance.onFriendMenuClicked();

                    ScrollFriendContent.instance.handleRejectRequest(character_id);

                    if (!isMenuOpend)
                    {
                        HudUI.instance.onCloseFriendMenuClicked();
                    }
                }
                //상대가 친구를 삭제한 경우
                else if (buffer[pos] == 7)
                {
                    pos += 1;

                    int character_id = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    bool isMenuOpend = HudUI.instance.friendMenu.activeInHierarchy;
                    HudUI.instance.onFriendMenuClicked();

                    ScrollFriendContent.instance.handleDeleteFriend(character_id);

                    if (!isMenuOpend)
                    {
                        HudUI.instance.onCloseFriendMenuClicked();
                    }
                }
            }
            //던전
            else if(type == 10)
            {
                byte _type = buffer[pos];
                pos += 1;

                //던전 ID
                if(_type == 0)
                {
                    int dungeonID = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    Debug.Log("던전ID: " + dungeonID);

                    Room.instance.userDatas.Clear();    //현재 룸에 있는 유저 목록 초기화하기
                    Room.instance.room_id_to_move = dungeonID; //이동할 룸의 id 설정
                    Room.instance.isGettingInput = false;    //동기화 데이터 받지 않게 설정하기
                    ClientInput.instance.isReady = false;    //다른 룸으로 이동 완료 전까지 키보드 데이터를 보내지 않음

                    //서버에게 동기화 데이터 받지 않겠다고 보내기
                    byte[] outBuffer = new byte[2];
                    outBuffer[0] = 8;
                    outBuffer[1] = 0;
                    ClientSocket.instance.SendRequest(outBuffer);

                    //서버에게 룸 입장 요청
                    byte[] outBuffer2 = new byte[1 + 4 + 1];
                    outBuffer2[0] = 9;

                    byte[] intBytes = BitConverter.GetBytes(dungeonID);
                    Buffer.BlockCopy(intBytes, 0, outBuffer2, 1, 4);


                    outBuffer2[5] = (byte)0;
                    ClientSocket.instance.SendRequest(outBuffer2);
                }
                //매치 찾기 성공
                else if(_type == 1)
                {
                    Debug.Log("매치 찾기 성공");
                    DungeonMenu.instance.OnMatchFound();
                }
                //매치 시작 실패
                else if(_type == 2)
                {
                    Debug.Log("매치 시작 실패");
                    DungeonMenu.instance.OnMatchStartFailed();
                }
                //경험치
                else if(_type == 3)
                {
                    Debug.Log("경험치");
                    byte levelDelta = buffer[pos];
                    pos += 1;
                    byte newExp = buffer[pos];
                    pos += 1;
                    byte statusPointDelta = buffer[pos];
                    pos += 1;

                    int goldDelta = BitConverter.ToInt32(buffer, pos);
                    pos += 4;

                    User.instance.level += levelDelta;
                    User.instance.exp = newExp;
                    User.instance.status_point += statusPointDelta;
                    User.instance.gold += goldDelta;
                    InventoryMenu.UpdateGold();
                }
                //목표 업데이트
                else if(_type == 4)
                {
                    Debug.Log("목표 업데이트");
                    byte dungeonType = buffer[pos];
                    pos += 1;

                    byte objectiveID = buffer[pos];
                    pos += 1;

                    byte val = buffer[pos];
                    pos += 1;

                    //슬라임 던전
                    if(dungeonType == 0) {
                        SlimeDungeonHudUI.instance.UpdateObjective(objectiveID, val);
                    }
                    //오크 던전
                    else if(dungeonType == 1)
                    {
                        OakDungeonHudUI.instance.UpdateObjective(objectiveID, val);
                    }
                }
                //게임 오버
                else if(_type == 5)
                {
                    HudUI.instance.backToTownUI.gameObject.SetActive(true);
                }
                //아이템 획득 알림
                else if(_type == 6)
                {
                    //인벤토리를 업데이트한다. 
                    HudUI.instance.GetItemList();
                }
            }
            //던전 초대
            else if(type == 11)
            {
                byte _type = buffer[pos++];

                int characterID = BitConverter.ToInt32(buffer, pos);
                pos += 4;

                //초대 받음 알림
                if(_type == 0)
                {
                    byte dungeonTypeByte = buffer[pos++];
                    DungeonMenu.eDungeonName dungeonName = DungeonMenu.eDungeonName.None;

                    if(dungeonTypeByte == 0)
                    {
                        dungeonName = DungeonMenu.eDungeonName.SlimeDungeon;
                    }
                    else if(dungeonTypeByte == 1)
                    {
                        dungeonName = DungeonMenu.eDungeonName.OakDungeon;
                    }

                    HudUI.instance.onInvitedMenu.gameObject.SetActive(true);
                    HudUI.instance.onInvitedMenu.OnInvited(characterID, dungeonName);
                }
                //초대 거절 알림
                else if(_type == 1)
                {
                    InviteFriendMenu.instance.OnRejectInvite();
                }
                //초대 취소 알림
                else if (_type == 2)
                {
                    HudUI.instance.onInvitedMenu.gameObject.SetActive(true);
                    HudUI.instance.onInvitedMenu.OnCancleInvite(characterID);
                }
                //초대 실패 알림
                else if (_type == 3)
                {
                    InviteFriendMenu.instance.OnInviteFail();
                }
            }
            else
            {
                Debug.Log("?????: " + type);
            }
        }
    }

    private void Update()
    {
        while(inputDatas.Count > 0) {
            InputData data;
            if (inputDatas.TryPop(out data))
            {
                HandleReceive(data.data, data.size);
            }
        }

        oneSecTimer += Time.deltaTime;
        if(oneSecTimer >= 1.0f) {
            oneSecTimer -= 1.0f;
            syncCounter = 0;
        }

        if(isSending == false) {
            if((outputDatas.Count > 0)) {
                byte[] outData;
                if (outputDatas.TryPop(out outData))
                {
                    isSending = true;
                    socket.BeginSend(outData, 0, outData.Length, 0, sendHandler, null);
                }
            }
        }
    }

    private void OnDestroy() {
        socket.Disconnect(false);
        Debug.Log("게임 종료");
    }

    //c# 소켓
    private Socket socket;

    //datas
    private ConcurrentStack<InputData> inputDatas;

    private class InputData
    {
        public InputData(byte[] data, int size)
        {
            this.data = data;
            this.size = size;
        }

        public byte[] data;
        public int size;
    }

    public static  ConcurrentStack<byte[]> outputDatas;    //보낼 데이터들
    
    //status
    private bool isSending = false;
    private bool isItemListArrived = false;

    //settings
    private int serverPort = 666;

    //callback objects
    private AsyncCallback receiveHandler;
    private AsyncCallback sendHandler;

    //for statistics
    private int syncCounter = 9;
    private float oneSecTimer = 0.0f;
}
