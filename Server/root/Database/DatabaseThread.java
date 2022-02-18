package root.Database;

import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Queue;

//thread-safe
import java.util.Vector;
import java.util.concurrent.ConcurrentLinkedQueue;

import root.User;
import root.Item;
import root.CharacterData;
import root.FriendData;
import root.Entity.DroppedItem;

public class DatabaseThread  extends Thread {
    static String dbName = "game";
    static String sqlID = "server";
    static String sqlPW = "detox1999";

    public Queue<Request> requestQueue;
    public Queue<Result> resultQueue;

    public DatabaseThread() {
        init();

        requestQueue = new ConcurrentLinkedQueue<>();
        resultQueue = new ConcurrentLinkedQueue<>();
    }

    public void init() {
        //db 연결
        try {
            connection = DriverManager.getConnection(
                "jdbc:mysql://localhost/" + dbName,
                sqlID,
                sqlPW
             );

        } catch (SQLException e) {
            System.out.println("SQLException: " + e.getMessage());
            System.out.println("SQLState: " + e.getSQLState());
            System.out.println("VendorError: " + e.getErrorCode());
        } finally {
            if(enablePrint) {
                System.out.println("DB 연결 성공");
            }
        }
    }

    @Override
    public void run() {
        Request request = null;
        while(true) {
            while((request = requestQueue.poll()) != null) {
                request.execute();
                resultQueue.add(request.result);
            }
        }
    }

    public void  RequestLogin(User user, String email, String password) {
        requestQueue.add(new LoginRequest(user, email, password));
    }

    public void RequestRegister(User user, String email, String password) {
        requestQueue.add(new RegisterRequest(user, email, password));
    }

    public void RequestCharacterList(User user, int account_id) {
        requestQueue.add(new CharacterListRequest(user, account_id));
    }

    public void RequestCreateCharacter(User user, String nickname, int job, int account_id) {
        requestQueue.add(new CreateCharacterRequest(user, nickname, job, account_id));
    }

    public void RequestDeleteCharacter(User user, int character_id) {
        requestQueue.add(new DeleteCharacterRequest(user, character_id));
    }

    public void RequestItemList(User user, int character_id) {
        requestQueue.add(new ItemListRequest(user, character_id));
    }

    public void RequestFriendList(User user, int character_id) {
        requestQueue.add(new FriendListRequest(user, character_id));
    }

    public void AddFriend(User user, int character1_id, int character2_id) {
        requestQueue.add(new AddFriendRequest(user, character1_id, character2_id));
    }

    public void DeleteFriend(User user, int character1_id, int character2_id) {
        requestQueue.add(new DeleteFriendRequest(user, character1_id, character2_id));
    }

    public void EquipItem(User user, int character_id, boolean isWeapon, int item_id) {
        requestQueue.add(new EquipItemRequest(user, character_id, isWeapon, item_id));
    }

    public void UseStatusPoint(User user, int character_id, StatusType type) {
        requestQueue.add(new UseStatusPointRequest(user, character_id, type));
    }

    public void AddItem(User user, int character_id, DroppedItem droppedItem) {
        requestQueue.add(new AddItemRequest(user, character_id, droppedItem));
    }

    public void GetEquipedItem(User user, int nextRoomID, int characterID, CharacterData character, int direction) {
        requestQueue.add(new GetEquipedItemRequest(user, characterID, character, nextRoomID, direction));
    }

    //던전에 들어간 유저에게 exp를 증가시키기 위한 request.
    //level += levelDelta
    //exp = newExp
    //statusPoint += statusPointDelta
    public void DungeonExpRequest(User user, int levelDelta, int newExp, int statusPointDelta, int goldDelta) {
        requestQueue.add(new DungeonExpRequest(user, levelDelta, newExp, statusPointDelta, goldDelta));
    }

    public class Request {
        public Request(User user) {
            this.user = user;
        }
        public User user;

        public void execute() {};

        public Result result = null;
    }

    public class GetEquipedItemRequest extends Request {
        public GetEquipedItemRequest(User user, int characterID, CharacterData character, int nextRoomID, int direction) {
            super(user);
            this.characterID = characterID;
            this.character = character;
            this.nextRoomID = nextRoomID;
            this.direction = direction;
        }

        @Override
        public void execute() {
            result = new GetEquipedItemResult(this.nextRoomID, this.direction);
            GetEquipedItemResult _result = (GetEquipedItemResult)result;
            _result.user = this.user;
            _result.characterData = this.character;

            try {
                //무기 query
                String sql1 = "SELECT b.strong, b.dexility, b.intellect, b.luck FROM `item` b WHERE b.id = (SELECT a.weapon_id FROM `character` a WHERE a.id = ?)";
                PreparedStatement stmt = connection.prepareStatement(sql1);
                stmt.setInt(1, characterID);

                ResultSet resultSet1 = stmt.executeQuery();
                while(resultSet1.next()) {
                    _result.weaponSTR = resultSet1.getInt(1);
                    _result.weaponDEX = resultSet1.getInt(2);
                    _result.weaponINT = resultSet1.getInt(3);
                    _result.weaponLUCK = resultSet1.getInt(4);
                }

                //방어구 query
                String sql2 = "SELECT b.strong, b.dexility, b.intellect, b.luck FROM `item` b WHERE b.id = (SELECT a.armor_id FROM `character` a WHERE a.id = ?)";
                PreparedStatement stmt2 = connection.prepareStatement(sql2);
                stmt2.setInt(1, characterID);

                ResultSet resultSet2 = stmt2.executeQuery();
                while(resultSet2.next()) {
                    _result.armorSTR = resultSet2.getInt(1);
                    _result.armorDEX = resultSet2.getInt(2);
                    _result.armorINT = resultSet2.getInt(3);
                    _result.armorLUCK = resultSet2.getInt(4);
                }
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
            }
        }

        public int characterID;
        public CharacterData character;
        public int nextRoomID;
        public int direction;
    }

    //해당 캐릭터에게 새로운 아이템을 준다. 
    public class AddItemRequest extends Request {
        public AddItemRequest(User user, int character_id, DroppedItem itemEntity) {
            super(user);
            this.character_id = character_id;
            this.droppedItem = itemEntity;
        }

        @Override
        public void execute() {
            result = new AddItemResult();
            AddItemResult _result = (AddItemResult)result;
            _result.user = user;
            _result.droppedItem = this.droppedItem;

            String sqlString1 = "SELECT a.`index_in_inventory` FROM item a WHERE a.`character_id` = ?";
            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString1);
                stmt.setInt(1, character_id);

                ResultSet resultSet = stmt.executeQuery();
                ArrayList<Integer> indice = new ArrayList<>();
                
                while(resultSet.next()) {
                    indice.add(resultSet.getInt(1));
                }

                int index = 999;
                for(int i = 0; i < 16; i++) {
                    boolean success = true;
                    for (Integer integer : indice) {
                        if(integer == i) {
                            success = false;
                            break;
                        }
                    }

                    if(success == true) {
                        if(i < index) {
                            index = i;
                        }
                    }
                }

                if(index != 999) {
                    String sqlString2 = "INSERT INTO item(`character_id`, `index_in_inventory`, `name`, `is_weapon`, `job`, `level_limit`, `strong`, `dexility`, `intellect`, `luck`) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    PreparedStatement stmt2 = connection.prepareStatement(sqlString2, Statement.RETURN_GENERATED_KEYS);
                    Item item = droppedItem.item;
                    stmt2.setInt(1, character_id);
                    stmt2.setInt(2, index);
                    stmt2.setString(3, item.name);
                    if(item.is_weapon) {
                        stmt2.setInt(4, 1);
                    }
                    else {
                        stmt2.setInt(4, 0);
                    }
                    stmt2.setInt(5, item.job);
                    stmt2.setInt(6, item.level_limit);
                    stmt2.setInt(7, item.strong);
                    stmt2.setInt(8, item.dexility);
                    stmt2.setInt(9, item.intellect);
                    stmt2.setInt(10, item.luck);

                    stmt2.executeUpdate();
                    ResultSet rs = stmt2.getGeneratedKeys();

                    if(rs.next()) {
                        int id = rs.getInt(1);
                        item.id = id;
                    }

                    result.result = DatabaseResult.SUCCESS;
                }
                else {
                    _result.result = DatabaseResult.NO_EMPTY_PLACE;
                }
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                result.result = DatabaseResult.FAIL;
            }
        }

        public int character_id;
        public DroppedItem droppedItem;
    }

    public class DungeonExpRequest extends Request {
        public DungeonExpRequest(User user, int levelDelta, int newExp, int statusPointDelta, int goldDelta){
            super(user);
            this.levelDelta = levelDelta;
            this.newExp = newExp;
            this.statusPointDelta = statusPointDelta;
            this.goldDelta = goldDelta;
        }

        @Override
        public void execute() {
            result = new Result();

            String sqlString = "UPDATE `character` SET `level` = `level` + ?, `exp` = ?, `status_point` = `status_point` + ?, `gold` = `gold` + ? WHERE `id` = ?";
            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, levelDelta);
                stmt.setInt(2, newExp);
                stmt.setInt(3, statusPointDelta);
                stmt.setInt(4, goldDelta);
                stmt.setInt(5, user.characterID);

                stmt.executeUpdate();
                
                result.result = DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                result.result = DatabaseResult.FAIL;
            }
        }
 
        public int newExp;
        public int statusPointDelta;
        public int levelDelta;
        public int goldDelta;
    }

    public class DeleteFriendRequest extends Request {
        public DeleteFriendRequest(User user, int character1_id, int character2_id) {
            super(user);
            this.character1_id = character1_id;
            this.character2_id = character2_id;
        }

        @Override
        public void execute() {
            result = new DeleteFriendResult();
            String sqlString = "DELETE FROM friend WHERE (`character1_id` = ? AND `character2_id` = ?) OR (`character1_id` = ? AND `character2_id` = ?)";

            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);

                stmt.setInt(1, character1_id);
                stmt.setInt(2, character2_id);
                stmt.setInt(3, character2_id);
                stmt.setInt(4, character1_id);

                stmt.executeUpdate();

                result.result = DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                result.result = DatabaseResult.FAIL;
            }
        }

        public int character1_id;
        public int character2_id;
    }

    public class AddFriendRequest extends Request {
        public AddFriendRequest(User user, int character1_id, int character2_id) {
            super(user);
            this.character1_id = character1_id;
            this.character2_id = character2_id;
        }
        
        public void execute() {
            result = new AddFriendResult();
            AddFriendResult _result = (AddFriendResult)result;

            String sqlString = 
                "INSERT INTO friend(`character1_id`, `character2_id`) VALUE(?, ?)";
            
            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, character1_id);
                stmt.setInt(2, character2_id);

                stmt.executeUpdate();
                _result.result = DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
            }
        }

        int character1_id;
        int character2_id;
    }

    public class FriendListRequest extends Request {
        public FriendListRequest(User user, int character_id) {
            super(user);
            this.character_id = character_id;
        }

        public void execute() {
            result = new FriendListResult();
            FriendListResult _result = (FriendListResult)result;
            _result.friends = new Vector<>();
            _result.user = user;
            _result.character_id = character_id;

            String sqlString = 
                "SELECT a.`character1_id`, b.`nickname`, a.`character2_id`, c.`nickname` " +
                "FROM `friend` a " +
                "LEFT JOIN `character` b " +
                "ON a.`character1_id` = b.id " +
                "LEFT JOIN `character` c " +
                "ON a.`character2_id` = c.id " +
                "WHERE a.`character1_id` = ? OR a.`character2_id` = ?";

            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, character_id);
                stmt.setInt(2, character_id);

                ResultSet resultSet = stmt.executeQuery();

                while(resultSet.next()) {
                    int character1_id = resultSet.getInt(1);
                    String nickname1 = resultSet.getString(2);
                    int character2_id = resultSet.getInt(3);
                    String nickname2 = resultSet.getString(4);

                    int _character_id = 0;
                    String nickname;
                    if(character_id == character1_id) {
                        _character_id = character2_id;
                        nickname = nickname2;
                    } else {
                        _character_id = character1_id;
                        nickname = nickname1;
                    }

                    FriendData data = new FriendData(nickname, _character_id);
                    _result.friends.add(data);
                }
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
            }
        }

        public int character_id;
    }

    public class LoginRequest extends Request {
        public LoginRequest(User user, String email, String password) {
            super(user);
            this.email = email;
            this.password = password;
        }

        public String email;
        public String password;

        public void execute() {
            result = new LoginResult();
            LoginResult _result = (LoginResult)result;

            String sqlString = "SELECT a.`id`, a.`password` AS num FROM account a WHERE `email` = ?";

            try {
                //create statement and bind param
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setString(1, email);

                //execute and get result
                ResultSet resultSet = stmt.executeQuery();
                int id = -1;
                String _password = "";
                while(resultSet.next()) {
                    id = resultSet.getInt(1);
                    _password = resultSet.getString(2);
                }

                //계정이 존재하지 않는 경우
                if(id == -1) {
                    _result.result = DatabaseResult.WRONG_EMAIL;
                } 
                //계정이 존재하는 경우
                else {
                    if(password.equals(_password)) {
                        _result.account_id = id;
                        _result.result = DatabaseResult.SUCCESS;
                    } else {
                        _result.result = DatabaseResult.WRONG_PASSWORD;
                    }
                }

            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
                
                if(enablePrint) {
                    System.out.println("유저 로그인 실패: SQL Exception");
                }
            }

            _result.user = user;
            _result.email = email;
            _result.password = password;
        };
    }

    public class RegisterRequest extends Request {
        public RegisterRequest(User user, String email, String password) {
            super(user);
            this.email = email;
            this.password = password;
        }

        public String email;
        public String password;

        public void execute() {
            result = new RegisterResult();
            RegisterResult _result = (RegisterResult)result;

            String sqlString = "SELECT COUNT(*) AS num FROM account WHERE `email` = ?";
        
            try {
                //create statement and bind param
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setString(1, email);

                //execute and get result
                ResultSet resultSet = stmt.executeQuery();
                int accountNum = -1;
                while(resultSet.next()) {
                    accountNum = resultSet.getInt("num");
                }

                //중복된 이메일이 없는 경우: 새로운 계정을 추가(업데이트)한다. 
                if(accountNum == 0) {
                    sqlString = "INSERT INTO account(`email`, `password`, `CREATED`) VALUES(?, ?, NOW())";

                    //create statement and bind param
                    stmt = connection.prepareStatement(sqlString);
                    stmt.setString(1, email);
                    stmt.setString(2, password);

                    stmt.executeUpdate();

                    if(enablePrint) {
                        System.out.println("유저 가입 성공");
                    }
                    
                    _result.result = DatabaseResult.SUCCESS;
                }
                //중복된 이메일이 있는 경우: 실패를 리턴한다. 
                else if(accountNum == 1) {
                    if(enablePrint) {
                        System.out.println("유저 가입 실패: 이메일 중복");
                    }

                    _result.result = DatabaseResult.DUPLICATE_EMAIL;
                }
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
                
                if(enablePrint) {
                    System.out.println("유저 가입 실패: SQL Exception");
                }
            }

            _result.user = user;
            _result.email = email;
            _result.password = password;
        }
    }

    public class CharacterListRequest extends Request {
        public CharacterListRequest(User user, int account_id) {
            super(user);
            this.account_id = account_id;
        }

        public int account_id;

        public void execute() {
            result = new CharacterListResult();
            CharacterListResult _result = (CharacterListResult)result;
            _result.characters = new Vector<>();

            String sqlString = 
            "SELECT " +
            "a.id, a.nickname, a.job, a.level, a.weapon_id, a.armor_id, a.status_point, a.gold, a.exp, a.strong, a.dexility, a.intellect, a.luck " +
            "FROM `character` a " +
            "LEFT JOIN `account` b " +
            "ON a.`account_id` = b.`id` " +
            "WHERE b.`id` = ? AND a.`available` = 1";

            try {
                //create statement and bind param
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, account_id);
    
                //execute and get result
                ResultSet resultSet = stmt.executeQuery();
                while(resultSet.next()) {
                    CharacterData data = new CharacterData();
                    data.id = resultSet.getInt(1);
                    data.nickname = resultSet.getString(2).split("\0")[0];
                    data.job = resultSet.getInt(3);
                    data.level = resultSet.getInt(4);
                    data.weapon_id = resultSet.getInt(5);
                    data.armor_id = resultSet.getInt(6);
                    data.status_point = resultSet.getInt(7);
                    data.gold = resultSet.getInt(8);
                    data.exp = resultSet.getInt(9);
                    data.strong = resultSet.getInt(10);
                    data.dexility = resultSet.getInt(11);
                    data.intellect = resultSet.getInt(12);
                    data.luck = resultSet.getInt(13);
                    
                    _result.characters.add(data);
                }

                _result.result = DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
                
                if(enablePrint) {
                    System.out.println("캐릭터 목록 가져오기 실패: SQL Exception");
                }
            }
        
            _result.user = user;
            _result.account_id = account_id;
        }
    }

    public class CreateCharacterRequest extends Request {
        public CreateCharacterRequest(User user, String nickname, int job, int account_id) {
            super(user);
            this.nickname = nickname;
            this.job = job;
            this.account_id = account_id;
        }

        public String nickname;
        public int job;
        public int account_id;

        public void execute() {
            result = new CreateCharacterResult();
            CreateCharacterResult _result = (CreateCharacterResult)result;

            String sqlString = "SELECT COUNT(*) AS num FROM `character` WHERE `nickname` = ?";
        
        try {
            PreparedStatement stmt = connection.prepareStatement(sqlString);
            stmt.setString(1, nickname);

            ResultSet resultSet = stmt.executeQuery();

            int num = -1;
            while(resultSet.next()) {
                num = resultSet.getInt("num");
            }

            //중복된 닉네임이 없는 경우: db에 캐릭터 생성
            if(num == 0) {
                sqlString = 
                    "INSERT INTO `character`(`account_id`, `nickname`, `job`, `strong`, `dexility`, `intellect`, `luck`, `CREATED`) " +
                    //"OUTPUT INSERTED.`id` " +
                    "VALUES(?, ?, ?, ?, ?, ?, ?, NOW())";

                stmt = connection.prepareStatement(sqlString, Statement.RETURN_GENERATED_KEYS);
                stmt.setInt(1, account_id);
                stmt.setString(2, nickname);
                stmt.setInt(3, job); //job
                stmt.setInt(4, 10); //str
                stmt.setInt(5, 10); //dex
                stmt.setInt(6, 10); //int
                stmt.setInt(7, 10); //luck

                stmt.executeUpdate();

                try(ResultSet generatedKeys = stmt.getGeneratedKeys()) {
                    if(generatedKeys.next()) {
                        _result.character_id = generatedKeys.getInt(1);

                        if(enablePrint) {
                            System.out.println("캐릭터 생성 ID: " + _result.character_id);
                        }
                    }
                }

                _result.result = DatabaseResult.SUCCESS;
            }
            //중복된 닉테임이 있는 경우: 닉네임 중복
            else if(num == 1) {
                _result.result = DatabaseResult.DUPLICATE_NICKNAME;
            } else if(num == -1) {
                _result.result = DatabaseResult.FAIL;
            }

            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
            }

            _result.user = user;
            _result.nickname = nickname;
            _result.job = job;
            _result.account_id = account_id;
        }
    }

    public class DeleteCharacterRequest extends Request {
        public DeleteCharacterRequest(User user, int character_id) {
            super(user);
            this.character_id = character_id;
        }

        public int character_id;

        public void execute() {
            result = new DeleteCharacterResult();
            DeleteCharacterResult _result = (DeleteCharacterResult)result;

            String sqlString = "UPDATE `character` SET `available` = 0 WHERE `id` = ?";

            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, character_id);

                int rowNum = stmt.executeUpdate();

                if(rowNum == 1) {
                    _result.result = DatabaseResult.SUCCESS;
                } 
                else if(rowNum == 0) {
                    _result.result = DatabaseResult.NO_CHARACTER;
                }

            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result =  DatabaseResult.FAIL;
            }

            _result.user = user;
            _result.character_id = character_id;
        }
    }

    public class EquipItemRequest extends Request {
        public EquipItemRequest(User user, int character_id, boolean isWeapon, int item_id) {
            super(user);
            this.character_id = character_id;
            this.isWeapon = isWeapon;
            this.item_id = item_id;
        }
        
        @Override
        public void execute() {
            result = new EquipItemResult();
            
            try {
                String sqlString = null;

                if(isWeapon) {
                    sqlString = "UPDATE `character` SET `weapon_id`=? WHERE `id`=?";
                } else {
                    sqlString = "UPDATE `character` SET `armor_id`=? WHERE `id`=?";
                }

                PreparedStatement stmt = connection.prepareStatement(sqlString);

                stmt.setInt(1, item_id);
                stmt.setInt(2, character_id);

                stmt.executeUpdate();
                result.result = DatabaseResult.SUCCESS;
            }
            catch(SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                result.result =  DatabaseResult.FAIL;
            }
        }

        int character_id;
        boolean isWeapon;
        int item_id;
    }

    public class ItemListRequest extends Request {
        public ItemListRequest(User user, int character_id) {
            super(user);
            this.character_id = character_id;
        }

        public int character_id;

        public void execute() {
            result = new ItemListResult();
            ItemListResult _result = (ItemListResult)result;
            _result.items = new Vector<>();

            String sqlString = 
            "SELECT " +
            "a.id, a.character_id, a.name, a.index_in_inventory, a.is_weapon, a.job, a.level_limit, a.strong, a.dexility, a.intellect, a.luck " +
            "FROM `item` a " +
            "LEFT JOIN `character` b " +
            "ON a.`character_id` = b.`id` " +
            "WHERE b.`id` = ? AND a.`index_in_inventory` >= 0";

            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, character_id);

                ResultSet resultSet = stmt.executeQuery();

                while(resultSet.next()) {
                    Item item = new Item();
                    item.id = resultSet.getInt(1);
                    item.character_id = resultSet.getInt(2);
                    item.name = resultSet.getString(3);
                    item.index_in_inventory = resultSet.getInt(4);
                    int isWeapon = resultSet.getInt(5);
                    item.job = resultSet.getInt(6);
                    if(isWeapon == 1) {
                        item.is_weapon = true;
                    } else {
                        item.is_weapon = false;
                    }
                    item.level_limit = resultSet.getInt(7);
                    item.strong = resultSet.getInt(8);
                    item.dexility = resultSet.getInt(9);
                    item.intellect = resultSet.getInt(10);
                    item.luck = resultSet.getInt(11);

                    _result.items.add(item);
                }

                _result.result = DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                _result.result = DatabaseResult.FAIL;
            }
            
            _result.user = user;
            _result.character_id = character_id;
            }
    }

    public enum StatusType {
        NONE, 
        STR,
        DEX, 
        INT, 
        LUCK
    }

    public class UseStatusPointRequest extends Request {
        public UseStatusPointRequest(User user, int character_id, StatusType type) {
            super(user);
            this.character_id = character_id;
            this.type = type;
        }

        @Override
        public void execute() {
            result = new Result();
            result.user = user;

            String sqlString = null;

            switch (type) {
                case STR:
                    sqlString = "UPDATE `character` SET `strong` = `strong` + 1, `status_point` = `status_point` - 1 WHERE `id` = ?";
                    break;
                case DEX:
                    sqlString = "UPDATE `character` SET `dexility` = `dexility` + 1, `status_point` = `status_point` - 1 WHERE `id` = ?";
                    break;
                case INT:
                    sqlString = "UPDATE `character` SET `intellect` = `intellect` + 1, `status_point` = `status_point` - 1 WHERE `id` = ?";
                    break;
                case LUCK:
                    sqlString = "UPDATE `character` SET `luck` = `luck` + 1, `status_point` = `status_point` - 1 WHERE `id` = ?";
                    break;
                case NONE:
                    break;
            }

            try {
                PreparedStatement stmt = connection.prepareStatement(sqlString);

                stmt.setInt(1, character_id);

                stmt.executeUpdate();
                result.result = DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                result.result = DatabaseResult.FAIL;
            }

            
        }

        public int character_id;
        public StatusType type;
    }

    public class Result {
        public User user;
        public DatabaseResult result;
    }

    public class EquipItemResult extends Result {

    }

    public class LoginResult extends Result {
        public int account_id;
        public String email;
        public String password;
    }

    public class RegisterResult extends Result {
        public String email;
        public String password;
    }

    public class CharacterListResult extends Result {
        public Vector<CharacterData> characters;
        public int account_id;
    }

    public class FriendListResult extends Result {
        public Vector<FriendData> friends;
        public int character_id;
    }

    public class AddItemResult extends Result {
        public DroppedItem droppedItem;
    }

    public class CreateCharacterResult extends Result {
        public int character_id;

        public String nickname;
        public int job;
        public int account_id;
    }

    public class DeleteCharacterResult extends Result {
        public int character_id;
    }

    public class ItemListResult extends Result {
        public Vector<Item> items;
        public int character_id;
    }

    public class AddFriendResult extends Result {
        public int character1_id;
        public int character2_id;
    }

    public class DeleteFriendResult extends Result {
        public int character1_id;
        public int character2_id;
    }

    public class GetEquipedItemResult extends Result {
        public GetEquipedItemResult(int nextRoomID, int direction) {
            this.nextRoomID = nextRoomID;
            this.direction = direction;
        }

        //armor
        public int armorSTR = 0;
        public int armorDEX = 0;
        public int armorINT = 0;
        public int armorLUCK = 0;

        //weapon
        public int weaponSTR = 0;
        public int weaponDEX = 0;
        public int weaponINT = 0;
        public int weaponLUCK = 0;

        public int nextRoomID = -1;
        public int direction;

        public CharacterData characterData;
    }

    Connection connection = null;

    //세팅
    public boolean enablePrint = true;
}