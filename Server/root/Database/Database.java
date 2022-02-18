package root.Database;

import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.Vector;
import java.util.concurrent.atomic.AtomicInteger;

import root.User;
import root.CharacterData;
import root.Item;

// WARNING: db io할 때 blocking 발생해서 쿼리 오래걸리면 서버 업데이트 멈출 수 있음
// 그래서 DatabaseThread 클래스 만들었으니까 그거 사용 권장
// GameServer는 현제 DatabaseThread로 작동하고 있음

public class Database {
    static String dbName = "game";
    static String sqlID = "server";
    static String sqlPW = "detox1999";

    public Database() {
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

    public class Request {
        public User user;
    }

    public class LoginRequest extends Request {
        public String email;
        public String password;
    }

    public class RegisterRequest extends Request {
        public String email;
        public String password;
    }

    public class GetCharacterRequest extends Request {
        public int account_id;
    }

    public class CreateCharacterRequest extends Request {
        public String nickname;
        public int job;
        public int account_id;
    }

    public class DeleteCharacterRequest extends Request {
        public int characterID;
    }

    public class GetItemListRequest extends Request {
        public int character_id;
    }

    public class Result {
        public User user;
    }
    
    public DatabaseResult login(String email, String password, AtomicInteger out_account_id) {
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
                return DatabaseResult.WRONG_EMAIL;
            } 
            //계정이 존재하는 경우
            else {
                if(password.equals(_password)) {
                    out_account_id.set(id);
                    return DatabaseResult.SUCCESS;
                } else {
                    return DatabaseResult.WRONG_PASSWORD;
                }
            }
        } catch (SQLException e) {
            System.out.println("SQLException: " + e.getMessage());
            System.out.println("SQLState: " + e.getSQLState());
            System.out.println("VendorError: " + e.getErrorCode());
            
            if(enablePrint) {
                System.out.println("유저 로그인 실패: SQL Exception");
            }
        }

        return DatabaseResult.FAIL;
    }

    public DatabaseResult register(String email, String password) {
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
                
                return DatabaseResult.SUCCESS;
            }
            //중복된 이메일이 있는 경우: 실패를 리턴한다. 
            else if(accountNum == 1) {
                if(enablePrint) {
                    System.out.println("유저 가입 실패: 이메일 중복");
                }

                return DatabaseResult.DUPLICATE_EMAIL;
            }
        } catch (SQLException e) {
            System.out.println("SQLException: " + e.getMessage());
            System.out.println("SQLState: " + e.getSQLState());
            System.out.println("VendorError: " + e.getErrorCode());
            
            if(enablePrint) {
                System.out.println("유저 가입 실패: SQL Exception");
            }
        }

        return DatabaseResult.FAIL;
    }

    public DatabaseResult getCharacterList(int account_id, Vector<CharacterData> datas) {
        String sqlString = 
            "SELECT " +
            "a.id, a.nickname, a.job, a.level, a.weapon_id, a.armor_id, a.status_point, a.gold, a.exp, a.strong, a.dexility, a.intellect, a.luck " +
            "FROM `character` a " +
            "LEFT JOIN `account` b " +
            "ON a.`account_id` = b.`id` " +
            "WHERE b.`id` = ? AND a.`available` = 1";

            try {
                datas.clear();

                //create statement and bind param
                PreparedStatement stmt = connection.prepareStatement(sqlString);
                stmt.setInt(1, account_id);
    
                //execute and get result
                ResultSet resultSet = stmt.executeQuery();
                while(resultSet.next()) {
                    CharacterData data = new CharacterData();
                    data.id = resultSet.getInt(1);
                    data.nickname = resultSet.getString(2).split("\0")[0];
                    System.out.println(data.nickname);
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
                    data.intellect = resultSet.getInt(13);
                    datas.add(data);
                }

                return DatabaseResult.SUCCESS;
            } catch (SQLException e) {
                System.out.println("SQLException: " + e.getMessage());
                System.out.println("SQLState: " + e.getSQLState());
                System.out.println("VendorError: " + e.getErrorCode());
                
                if(enablePrint) {
                    System.out.println("캐릭터 목록 가져오기 실패: SQL Exception");
                }
            }
        
        return DatabaseResult.FAIL;
    }

    public DatabaseResult CreateCharacter(String nickname, int job, int id, AtomicInteger out_character_id) {
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
                stmt.setInt(1, id);
                stmt.setString(2, nickname);
                stmt.setInt(3, job); //job
                stmt.setInt(4, 10); //str
                stmt.setInt(5, 10); //dex
                stmt.setInt(6, 10); //int
                stmt.setInt(7, 10); //luck

                stmt.executeUpdate();

                try(ResultSet generatedKeys = stmt.getGeneratedKeys()) {
                    if(generatedKeys.next()) {
                        out_character_id.set(generatedKeys.getInt(1));

                        if(enablePrint) {
                            System.out.println("캐릭터 생성 ID: " + out_character_id);
                        }
                    }
                }

                return DatabaseResult.SUCCESS;
            }
            //중복된 닉테임이 있는 경우: 닉네임 중복
            else if(num == 1) {
                return DatabaseResult.DUPLICATE_NICKNAME;
            } else if(num == -1) {
                return DatabaseResult.FAIL;
            }

        } catch (SQLException e) {
            System.out.println("SQLException: " + e.getMessage());
            System.out.println("SQLState: " + e.getSQLState());
            System.out.println("VendorError: " + e.getErrorCode());
        }

        return DatabaseResult.FAIL;
    }

    public DatabaseResult DeleteCharacter(int characterID) {
        String sqlString = "UPDATE `character` SET `available` = 0 WHERE `id` = ?";

        try {
            PreparedStatement stmt = connection.prepareStatement(sqlString);
            stmt.setInt(1, characterID);

            int rowNum = stmt.executeUpdate();

            if(rowNum == 1) {
                return DatabaseResult.SUCCESS;
            } 
            else if(rowNum == 0) {
                return DatabaseResult.NO_CHARACTER;
            }

        } catch (SQLException e) {
            System.out.println("SQLException: " + e.getMessage());
            System.out.println("SQLState: " + e.getSQLState());
            System.out.println("VendorError: " + e.getErrorCode());
        }

        return DatabaseResult.FAIL;
    }

    public DatabaseResult getItemList(int character_id, Vector<Item> items) {
        String sqlString = 
            "SELECT " +
            "a.id, a.character_id, a.name, a.index_in_inventory, a.is_weapon, a.level_limit, a.strong, a.dexility, a.intellect, a.luck " +
            "FROM `item` a " +
            "LEFT JOIN `character` b " +
            "ON a.`character_id` = b.`id` " +
            "WHERE b.`id` = ?";

        try {
            PreparedStatement stmt = connection.prepareStatement(sqlString);
            stmt.setInt(1, character_id);

            ResultSet resultSet = stmt.executeQuery();
            items.clear();

            while(resultSet.next()) {
                Item item = new Item();
                item.id = resultSet.getInt(1);
                item.character_id = resultSet.getInt(2);
                item.name = resultSet.getString(3);
                item.index_in_inventory = resultSet.getInt(4);
                int isWeapon = resultSet.getInt(5);
                if(isWeapon == 1) {
                    item.is_weapon = true;
                } else {
                    item.is_weapon = false;
                }
                item.level_limit = resultSet.getInt(6);
                item.strong = resultSet.getInt(7);
                item.dexility = resultSet.getInt(8);
                item.intellect = resultSet.getInt(9);
                item.luck = resultSet.getInt(10);

                items.add(item);
            }

            return DatabaseResult.SUCCESS;
        } catch (SQLException e) {
            System.out.println("SQLException: " + e.getMessage());
            System.out.println("SQLState: " + e.getSQLState());
            System.out.println("VendorError: " + e.getErrorCode());
        }
        
        return DatabaseResult.FAIL;
    }

    Connection connection = null;

    //세팅
    public boolean enablePrint = true;
}