using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class User : MonoBehaviour
{
    public static User instance;

    [SerializeField]
    public int account_id = -1; //로그인한 계정의 아이디
    public string email = "";   //로그인한 계정의 이메일
    public string nickname = "";    //유저가 선택한 캐릭터의 닉네임
    public int character_id = -1;    //유저가 선택한 캐릭터의 아이디
    public int job = -1;    //유저가 선택한 캐릭터의 직업

    public List<CharacterData> characterDatas;  //계정이 가지고 있는 캐릭터 data
    public CharacterData selectedCharacter; //유저가 선택한 캐릭터의 data
    public List<FriendData> friendDatas;    //캐릭터의 친구 정보들
    public Inventory inventory;

    //캐릭터 정보
    public int level;
    public int weapon_id;
    public int armor_id;
    public int status_point;
    public int gold;
    public int exp;
    public int strong;
    public int dexility;
    public int intellect;
    public int luck;

    public UserEntity entity;

    private void Awake() {
        instance = GetComponent<User>();
        characterDatas = new List<CharacterData>();
        friendDatas = new List<FriendData>();
        inventory = new Inventory();
    }

    public void OnLoginResult(int result, int account_id) {
        Text loginResultText = GameObject.Find("LoginResultText").GetComponent<Text>();

        //로그인 성공
        if(result == 0) {
            if(loginResultText != null) {
                loginResultText.text = "로그인 성공! 캐릭터 불러오는 중...";
            }

            User.instance.account_id = account_id;

            //서버에 캐릭터 목록 요청하기
            byte[] outBuffer = new byte[1 + 4];

            outBuffer[0] = 3;
            byte[] intBuffer = BitConverter.GetBytes(account_id);
            Buffer.BlockCopy(intBuffer, 0, outBuffer, 1, 4);

            ClientSocket.instance.SendRequest(outBuffer);
        }
        //등록되지 않은 이메일
        else if(result == 1) {
            if(loginResultText != null) {
                loginResultText.text = "등록되지 않은 이메일입니다.";
            }
        }
        //비밀번호 불일지
        else if(result == 2) {
            if(loginResultText != null) {
                loginResultText.text = "비밀번호가 일치하지 않습니다.";
            }
        }
        //서버 에러
        else if(result == 3) {
            if(loginResultText != null) {
                loginResultText.text = "서버 에러. 개발자에게 문의해 주세요.";
            }
        }

    }

    public void OnRegisterResult(int result) {
        Text registerResultText = GameObject.Find("RegisterResultText").GetComponent<Text>();

        //성공한 경우
        if(result == 0) {
            GameObject registerCanvas = GameObject.Find("CanvasHolder").transform.Find("RegisterCanvas").gameObject;
            if(registerCanvas != null) {
                registerCanvas.SetActive(false);
            }

            GameObject loginCanvas = GameObject.Find("CanvasHolder").transform.Find("LoginCanvas").gameObject;
            if(loginCanvas != null) {
                loginCanvas.SetActive(true);
            }

            Text loginResultText = GameObject.Find("LoginResultText").GetComponent<Text>();
            if(loginResultText != null) {
                loginResultText.text = "가입 성공. 로그인해주세요. ";
            }
        } 
        //이메일 중복
        else if(result == 1) {
            registerResultText.text = "이미 존재하는 이메일입니다.";
        } 
        //서버 에러
        else if(result == 2) {
            registerResultText.text = "서버 에러. 개발자에게 문의해 주세요.";
        }
    }

    public void OnDeleteCharacterResult(int character_id) {
        foreach(CharacterData data in characterDatas) {
                    if(data.id == character_id) {
                        characterDatas.Remove(data);
                        break;
                    }
                }

        ScrollCharacterContent sc = FindObjectOfType<ScrollCharacterContent>();
                if(sc != null) {
                    sc.OrganizeChild();
                }

                Utility.instance.OnCancleDeleteCharacterButtonClicked();
    }

    public void OnCreateCharacterResult(int result, int character_id) {
        //캐릭터 생성 성공시
        if(result == 0) {
            CharacterData cd = new CharacterData();
            cd.id = character_id;
            cd.nickname = character_create_nickname;
            cd.job = character_create_job;
            User.instance.AddCharacterData(cd);

            character_create_nickname = "";
            character_create_job = -1;

            Utility.instance.OnCharacterCreateSuccess();
        }
        //닉네임 중복
        else if(result == 1) {
            Debug.Log("캐릭터 생성 실패: 닉네임 중복");
            Utility.instance.OnCharacterCreateDuplicate();
        }
        //실패
        else if(result == 2) {

        }
    }
    public void AddCharacterData(CharacterData data) {
        characterDatas.Add(data);
    }

    public void OnFriendCharacterOnline(int character_id)
    {
        //친구 목록에서 해당 아이디를 가진 캐릭터를 찾는다.
        foreach (FriendData data in friendDatas)
        {
            if(data.character_id == character_id)
            {
                Debug.Log("친구 접속1: " + data.character_id);
                data.isOnline = true;
            }
        }

        if(ScrollFriendContent.instance)
        {
            ScrollFriendContent.instance.onFriendOnline(character_id);
        }
    }

    public void OnFriendCharacterOffline(int character_id)
    {
        //친구 목록에서 해당 아이디를 가진 캐릭터를 찾는다.
        foreach (FriendData data in friendDatas)
        {
            if (data.character_id == character_id)
            {
                data.isOnline = false;
            }
        }

        if (ScrollFriendContent.instance)
        {
            ScrollFriendContent.instance.onFriendOffline(character_id);
        }
    }

    public void SendDeleteCharacterRequest(int character_id) {
        byte[] outBuffer = new byte[5];
        outBuffer[0] = (byte)5;
        byte[] characterIDBytes = BitConverter.GetBytes(character_id);
        Buffer.BlockCopy(characterIDBytes, 0, outBuffer, 1, 4);

        ClientSocket.instance.SendRequest(outBuffer);
    }

    public void SendCreateCharacterRequest(string nickname, int job) {
        byte[] buffer = new byte[1 + 1 + 40];

        buffer[0] = 4;
        buffer[1] = (byte)job;

        byte[] nicknameBytes = Encoding.Default.GetBytes(nickname);
        Buffer.BlockCopy(nicknameBytes, 0, buffer, 2, nicknameBytes.Length);

        character_create_nickname = nickname;
        character_create_job = job;

        ClientSocket.instance.SendRequest(buffer);
    }

    public void Logout() {
        account_id = -1;
        email = "";
        nickname = "";
        characterDatas.Clear();
        inventory.Clear();
    }

    private static string character_create_nickname = "";   //마지막으로 생성한 캐릭터의 이름
    private static int character_create_job = -1;   //마지막으로 생성한 캐릭터의 직업
}
