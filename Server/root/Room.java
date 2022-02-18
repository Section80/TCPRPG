package root;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.Vector;

import root.Entity.*;

public class Room {
    public Room() {
        this.id= count++;
        entities = new Vector<>();
        users = new Vector<>();
        wallRects = new Vector<>();
    }

    public void update(int tick) {
        for (int i = 0; i < entities.size(); i++) {
            Entity e = entities.get(i);
            if(e.shouldDestroy) {
                e.onDestroy();
                OnDestroy(e);
                entities.remove(e);
            }
        }

        for(int i = 0; i < entities.size(); i++) {
            Entity e = entities.get(i);
            e.index_in_room = i;
            e.update(tick);
        }
    }

    public void OnDestroy(Entity entity) {

    }

    public void addEntity(Entity entity) {
        entities.add(entity);
        entity.index_in_room = entities.size() - 1;
        entity.id_in_room = last_entity_id++;
        entity.room = this;
    }

    public void addUserEntity(User user, float x, float y) {
        UserEntity entity = new UserEntity(user, x, y);
        addEntity(entity);
        users.add(user);
        user.id_in_room = entity.id_in_room;
        user.room = this;

        System.out.println("유저 객체 룸에 추가, entity id:" + entity.id_in_room);

        entity.job = user.character.job;
        if(entity.job == 0) {
            entity.type = 0;
        }
        else if(entity.job == 1) {
            entity.type = 1;
        }

        user.inputStatus.left = false;
        user.inputStatus.right = false;
        user.inputStatus.up = false;
        user.inputStatus.down = false;
        user.inputStatus.z = false;
        user.inputStatus.x = false;
        user.inputStatus.a = false;
        user.inputStatus.s = false;
    }

    public void addUserEntity(User user, int direction) {
        UserEntity entity = new UserEntity(user, 0, 0);
        addEntity(entity);
        users.add(user);
        user.id_in_room = entity.id_in_room;
        user.room = this;

        System.out.println("유저 객체 룸에 추가, entity id:" + entity.id_in_room);

        entity.job = user.character.job;
        if(entity.job == 0) {
            entity.type = 0;
        }
        else if(entity.job == 1) {
            entity.type = 1;
        }

        user.inputStatus.left = false;
        user.inputStatus.right = false;
        user.inputStatus.up = false;
        user.inputStatus.down = false;
        user.inputStatus.z = false;
        user.inputStatus.x = false;
        user.inputStatus.a = false;
        user.inputStatus.s = false;
    }

    public void exitUser(User user) {
        user.room = null;
        user.roomID = -1;

        for (Entity entity : entities) {
            if(entity.id_in_room == user.id_in_room) {
                entity.shouldDestroy = true;
            }
        }

        users.remove(user);
    }

    //이 룸에 접속한 유저들에게 이 룸에 접속한 유저들의 목록 정보를 보낸다. 
    public void sendRoomUserListToUsers() {
        //byte buffer 만들기
        ByteBuffer outputBuffer = ByteBuffer.allocate(1 + 4 + 4 + users.size() * (4 + 4 + 4 + 40));
        outputBuffer.order(ByteOrder.LITTLE_ENDIAN);
        outputBuffer.position(0);
        outputBuffer.put(0, (byte)7);
        outputBuffer.putInt(1, id);
        outputBuffer.putInt(5, users.size());

        int pos = 9;
        outputBuffer.position(pos);
        for(int i = 0; i < users.size(); i++) {
            User _user = users.get(i);

            outputBuffer.putInt(pos, _user.characterID);
            pos += 4;
            outputBuffer.putInt(pos, _user.id_in_room);
            pos += 4;
            outputBuffer.putInt(pos, _user.character.job);
            pos += 4;

            byte[] nicknameByte = _user.character.nickname.getBytes();

            outputBuffer.position(pos);
            outputBuffer.put(nicknameByte, 0, nicknameByte.length);
            pos += 40;
        }

        //데이터 보내기
        for (User user : users) {
                outputBuffer.position(0);
            try {
                user.socket.write(outputBuffer);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    //해당 룸에 접속한 사람들에게 동기화 정보를 보낸다. 
    public void SendSyncData() {
        int byteSize = 5;
        for (Entity entity : entities) {
            byteSize += 4 * 5;

            if(entity.type == 0 || entity.type == 1) {  //0: warrior, 1: magicin
                byteSize += 4 * 5;
            }
            else if(entity.type == 2) {
            }
            else if(entity.type == 3 || entity.type == 5) {
                byteSize += 4 * 3;
            }
            else if(entity.type == 4) {
                byteSize += 4 + 2;
                //int: characterID
                //byte: isWeapon;
                //byte: job
            }
        }

        ByteBuffer outputBuffer = ByteBuffer.allocate(byteSize);
        outputBuffer.order(ByteOrder.LITTLE_ENDIAN);
        outputBuffer.position(0);
        outputBuffer.put(0, (byte)8);
        
        outputBuffer.putInt(1, entities.size());

        int pos = 5;
        outputBuffer.position(pos);
        for(int i = 0; i < entities.size(); i++) {
            Entity e = entities.get(i);

            if(e.type > 5) {
                System.out.println("error Entity: " + e.id_in_room + " | " + e.type);
                continue;
            }

            outputBuffer.putInt(pos, e.id_in_room);
            pos += 4;
            outputBuffer.putInt(pos, e.type);
            pos += 4;
            outputBuffer.putFloat(pos, e.x);
            pos += 4;
            outputBuffer.putFloat(pos, e.y);
            pos += 4;
            outputBuffer.putFloat(pos, e.direction);
            pos += 4;

            if(e.type == 0 || e.type == 1) {
                UserEntity ue = (UserEntity)e;
                outputBuffer.putInt(pos, ue.max_hp);
                pos += 4;
                outputBuffer.putInt(pos, ue.hp);
                pos += 4;
                outputBuffer.putInt(pos, ue.max_mp);
                pos += 4;
                outputBuffer.putInt(pos, ue.mp);
                pos += 4;
                outputBuffer.putInt(pos, ue.animation);
                pos += 4;
            }
            else if(e.type == 3) {
                SlimeEntity se = (SlimeEntity)e;
                outputBuffer.putInt(pos, se.max_hp);
                pos += 4;
                outputBuffer.putInt(pos, se.hp);
                pos += 4;
                outputBuffer.putInt(pos, se.animation);
                pos += 4;
            }
            else if(e.type == 4) {
                DroppedItem ie = (DroppedItem)e;
                outputBuffer.putInt(pos, ie.item.character_id);
                pos += 4;
                byte is_weapon = 0;
                if(ie.item.is_weapon) {
                    is_weapon = 1;
                }
                outputBuffer.put(pos, (byte)is_weapon);
                pos += 1;
                outputBuffer.put(pos, (byte)ie.item.job);
                pos += 1;
            }
            else if(e.type == 5) {
                OakBoss se = (OakBoss)e;
                outputBuffer.putInt(pos, se.max_hp);
                pos += 4;
                outputBuffer.putInt(pos, se.hp);
                pos += 4;
                outputBuffer.putInt(pos, se.animation);
                pos += 4;
            }
        }

        for(int i = 0; i < users.size(); i++) {
            if(users.get(i).isReadyInput) {
                outputBuffer.position(0);
                try {
                    users.get(i).socket.write(outputBuffer);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    public UserEntity GetUserEntityByUser(User user) {
        for (Entity iEntity : entities) {
            if(!(iEntity instanceof UserEntity)) {
                continue;
            }

            UserEntity ue = (UserEntity)iEntity;

            if(ue.user == user) {
                return ue;
            }
        }

        return null;
    }

    public int last_entity_id = 0;
    public int id;

    public Vector<WallRect> wallRects;
    public Vector<Entity> entities;
    public Vector<User> users;

    private static int count = 0;
}
