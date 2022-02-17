using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public static Room instance;
    public int id = 0;

    public List<UserData> userDatas;

    public bool isUserListArrived = false;

    public GameObject entityHolder;

    //prefaps
    public GameObject warriorPrefap;
    public GameObject magicianPrefap;
    public GameObject defaultAttackPrefap;
    public GameObject slimePrefap;
    public GameObject droppedItemPrefap;
    public GameObject oakBossPrefap;

    public Transform hudTransform;

    public bool isGettingInput = false;

    public int room_id_to_move = 0;

    // Start is called before the first frame update
    void Start()
    {
        userDatas = new List<UserData>();
        DontDestroyOnLoad(gameObject);
        instance = GetComponent<Room>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Entity FindEntityById(int entity_id)
    {
        int entityNum = entityHolder.transform.childCount;

        for (int i = 0; i < entityNum; i++)
        {
            GameObject gameObject = entityHolder.transform.GetChild(i).gameObject;
            Entity entity = gameObject.GetComponent<EntityType>().GetEntity();

            if (entity.entity_id == entity_id)
            {
                return entity;
            }
        }

        return null;
    }

    public void UpdateEntityTarget(int entity_id, Vector3 target) {
        int entityNum = entityHolder.transform.childCount;

        for(int i = 0; i < entityNum; i++) {
            GameObject gameObject = entityHolder.transform.GetChild(i).gameObject;
            Entity entity = gameObject.GetComponent<EntityType>().GetEntity();

            if (entity.entity_id == entity_id) {
                entity.target = target;
                break;
            }
        }
    }

    public void UpdateEntityAnimationID(int entity_id, int animation_id) {
        int entityNum = entityHolder.transform.childCount;

        for(int i = 0; i < entityNum; i++) {
            GameObject gameObject = entityHolder.transform.GetChild(i).gameObject;
            Entity entity = gameObject.GetComponent<EntityType>().GetEntity();

            if (entity.entity_id == entity_id) {
                entity.animation_id = animation_id;
                break;
            }
        }
    }

    public void UpdateEntityHP(int entity_id, int hp) {
        int entityNum = entityHolder.transform.childCount;

        for(int i = 0; i < entityNum; i++) {
            GameObject gameObject = entityHolder.transform.GetChild(i).gameObject;
            EntityType type = gameObject.GetComponent<EntityType>();

            if(type.type == EntityType.eType.Warrior)
            {
                type.GetWarriorEntity().hp = hp;
                break;
            }
            else if(type.type == EntityType.eType.Magician)
            {
                type.GetMagicianEntity().hp = hp;
                break;
            }
            else if(type.type == EntityType.eType.Slime)
            {
                type.GetSlimeEntity().hp = hp;
                break;
            }
            else if(type.type == EntityType.eType.OakBoss)
            {
                type.GetOakBossEntity().hp = hp;
                break;
            }
        }
    }

    public Entity CreateEntityIfNotExist(int entity_id, int entity_type) {
        int entityNum = entityHolder.transform.childCount;
        bool isExist = false;
        Entity entity = null;

        /*
        if(entity_type > 5)
        {
            Debug.Log("NULL ENTITY ID: " + entity_id);
            Debug.Log("NULL ENTITY TYPE: " + entity_type);

            return null;
        }
        */

        //check if exist
        for(int i = 0; i < entityNum; i++) {
            GameObject _gameObject = entityHolder.transform.GetChild(i).gameObject;
            entity = _gameObject.GetComponent<EntityType>().GetEntity();

            if(entity.entity_id == entity_id) {
                isExist = true;

                return entity;
            }
        }

        GameObject gameObject = null;
        if(!isExist) {
            if (entity_type == 0)
            {
                Debug.Log("BUG");
                gameObject = Instantiate(warriorPrefap, entityHolder.transform);
                UserEntity userEntity = gameObject.GetComponent<EntityType>().GetUserEntity();

                foreach(UserData iData in userDatas)
                {
                    if(iData.character_id == userEntity.character_id)
                    {
                        userEntity.SetNickname(iData.nickname);
                        break;
                    }
                }

            } else if(entity_type == 1) 
            {
                Debug.Log("BUG");
                gameObject = Instantiate(magicianPrefap, entityHolder.transform);
                UserEntity userEntity = gameObject.GetComponent<EntityType>().GetUserEntity();
                
                foreach (UserData iData in userDatas)
                {
                    if (iData.character_id == userEntity.character_id)
                    {
                        userEntity.SetNickname(iData.nickname);
                        break;
                    }
                }
            } else if(entity_type == 2)
            {
                gameObject = Instantiate(defaultAttackPrefap, entityHolder.transform);
            } else if(entity_type == 3)
            {
                gameObject = Instantiate(slimePrefap, entityHolder.transform);
            } else if(entity_type == 4)
            {
                gameObject = Instantiate(droppedItemPrefap, entityHolder.transform);
            } else if(entity_type == 5)
            {
                gameObject = Instantiate(oakBossPrefap, entityHolder.transform);
            }
            else
            {
                Debug.Log("NULL ENTITY ID: " + entity_id);
                Debug.Log("NULL ENTITY TYPE: " + entity_type);
            }
            
            int count = gameObject.transform.childCount;
            for(int i = 0; i < count; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
            
            entity = gameObject.GetComponent<EntityType>().GetEntity();

            entity.entity_id = entity_id;
        }
        
        return entity;
    }

    //유저 목록을 업데이트한다.
    public void UpdateUserDatas(List<UserData> newUserDatas) {
        //기존 목록에 있는 유저가 여전히 있는지 확인해야 한다.
        for(int i = 0; i < userDatas.Count; i++) {
            UserData user = userDatas[i];



            bool success = false;
            foreach(UserData newUser in newUserDatas) {
                if(user.character_id == newUser.character_id) {
                    success = true;

                    user.entity_id = newUser.entity_id;

                    for(int j = 0; j < entityHolder.transform.childCount; j++)
                    {
                        EntityType type = entityHolder.transform.GetChild(i).GetComponent<EntityType>();
                        

                        if(type.type == EntityType.eType.Warrior || type.type == EntityType.eType.Magician)
                        {
                            UserEntity ue = type.GetUserEntity();
                            
                            if (ue.character_id == user.character_id)
                            {
                                ue.entity_id = newUser.entity_id;
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            if(success == true) {
                Debug.Log("유저 접속 유지: " + user.nickname);
            }
            else if(success == false) {
                Debug.Log("유저 접속 끊김: " + user.nickname);
            }
            
            //기존 목록에 있었던 유저가 새로운 목록에서 없어진 경우
            if(!success) {

                //entity holder에서 해당 객체를 삭제해야 한다.
                int entityNum = entityHolder.transform.childCount;

                for(int j = 0; j < entityNum; j++) {
                    UserEntity ue = entityHolder.transform.GetChild(i).GetComponent<EntityType>().GetUserEntity();
                    if(ue.character_id == user.character_id) {
                        Destroy(ue.gameObject);
                    }
                }

                userDatas.Remove(user);
            }
        }

        //새로운 유저가 생성되었는지 확인해야 한다. 
        foreach (UserData newUser in newUserDatas) 
        {
            bool isRealNewUser = true;

            //기존 데이터에 new User가 있는지 확인한다.
            foreach(UserData user in userDatas) {
                if(newUser.entity_id == user.entity_id) {
                    isRealNewUser = false;
                    break;
                }
            }

            if(isRealNewUser) {
                userDatas.Add(newUser);
            }

            //새로운 유저가 맞든 아니든, 객체가 존재하지 않았다면 생성해줘야 한다. 
            createUserEntity(newUser);
        }
    }

    
    private void createUserEntity(UserData user)
    {
        //이미 유저 객체가 존재하는지 확인한다.
        for(int i = 0; i < entityHolder.transform.childCount; i++)
        {
            GameObject go = entityHolder.transform.GetChild(i).gameObject;
            EntityType et = go.GetComponent<EntityType>();

            if (et.type == EntityType.eType.Warrior || et.type == EntityType.eType.Magician)
            {
                UserEntity _ue = et.GetUserEntity();

                if (_ue.character_id == user.character_id)
                {
                    return;
                }
            }
        }

        GameObject gameObject = null;

        if (user.job == 0)
        {
            gameObject = Instantiate(warriorPrefap, entityHolder.transform);
        }
        else if (user.job == 1)
        {
            gameObject = Instantiate(magicianPrefap, entityHolder.transform);
        }


        int count = gameObject.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            gameObject.transform.GetChild(i).gameObject.SetActive(false);
        }

        Entity entity = gameObject.GetComponent<EntityType>().GetEntity();

        entity.entity_id = user.entity_id;

        EntityType type = entity.GetComponent<EntityType>();
        UserEntity ue = entity.GetUserEntity();

        ue.SetNickname(user.nickname);
        ue.character_id = user.character_id;
    }

    public UserEntity GetUserEntity()
    {
        int entityNum = entityHolder.transform.childCount;
        for(int i = 0; i < entityNum; i++)
        {
            GameObject gameObject = entityHolder.transform.GetChild(i).gameObject;
            EntityType type = gameObject.GetComponent<EntityType>();

            if(type.type == EntityType.eType.Warrior || type.type == EntityType.eType.Magician)
            {
                UserEntity ue = gameObject.GetComponent<EntityType>().GetUserEntity();

                if (ue != null)
                {
                    if (ue.character_id == User.instance.character_id)
                    {
                        return ue;
                    }
                }
            }
            
        }

        return null;
    }

    public void StartSync()
    {
        int count = Room.instance.entityHolder.transform.childCount;

        for(int i = 0; i < count; i++)
        {
            Entity entity = entityHolder.transform.GetChild(i).GetComponent<EntityType>().GetEntity();

            entity.isSyncDataArrived = false;
        }
    }

    public void EndSync()
    {
        foreach(Transform transform in Room.instance.entityHolder.transform)
        {
            Entity entity = transform.GetComponent<EntityType>().GetEntity();
            if(entity.isSyncDataArrived == false)
            {
                Destroy(entity.gameObject);
            }
        }
    }
}
