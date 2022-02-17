using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Text;

public class Utility : MonoBehaviour
{   
    public static Utility instance;

    //handler
    public void OnCharacterCreateSuccess() {
        ScrollCharacterContent sc = FindObjectOfType<ScrollCharacterContent>();
        if(sc) {
            sc.OrganizeChild();
        }

        GameObject.Find("CreateCharacterButton").GetComponent<Button>().interactable = true;
        GameObject.Find("CancleCreateCharacterButton").GetComponent<Button>().interactable = true;

        GameObject.Find("NicknameInput").GetComponent<InputField>().text = "";

        OnCancleCreateCharacterButtonClicked();
    }
    public void OnCharacterCreateDuplicate() {
        Text text = GameObject.Find("CreateCharacterMessage").GetComponent<Text>();
        text.text = "이미 존재하는 닉네임입니다." ;
    }

    //Login Sence
    public void OnLoginButtonClicked() {
        InputField emailInput = GameObject.Find("LoginEmailInput").GetComponent<InputField>();
        if(emailInput == null) {
            return;
        }
        String email = emailInput.text;

        InputField passwordInput = GameObject.Find("LoginPasswordInput").GetComponent<InputField>();
        if(passwordInput == null) {
            return;
        }
        String password = passwordInput.text;

        Text loginResultText = GameObject.Find("LoginResultText").GetComponent<Text>();

        //TODO: chekc validation
        if(email.Equals("")) {
            if(loginResultText != null) {
                loginResultText.text = "이메일을 입력하세요. ";
            }
            return;
        }
        else if(password.Equals("")) {
            if(loginResultText) {
                loginResultText.text = "비밀번호를 입력하세요. ";
            }
            return;
        }


        byte[] outBuffer = new byte[1 + 35 * 2 + 20 * 2];
        outBuffer[0] = 1;

        byte[] emailByte = Encoding.Default.GetBytes(email);
        byte[] passwordByte = Encoding.Default.GetBytes(password);

        Buffer.BlockCopy(emailByte, 0, outBuffer, 1, emailByte.Length);
        Buffer.BlockCopy(passwordByte, 0, outBuffer, 71, passwordByte.Length);

        ClientSocket.instance.SendRequest(outBuffer);

        User.instance.email = email;

        if(loginResultText != null) {
                loginResultText.text = "로그인 중.. ";
        }
    }
    public void OnRegisterButtonClicked() {
        InputField registerEmailInput = GameObject.Find("RegisterEmailInput").GetComponent<InputField>();
        if(registerEmailInput == null) {
            return;
        }
        String email = registerEmailInput.text;

        InputField registerPasswordInput = GameObject.Find("RegisterPasswordInput").GetComponent<InputField>();
        if(registerPasswordInput == null) {
            return;
        }
        String password = registerPasswordInput.text;

        InputField registerPasswordInput2 = GameObject.Find("RegisterPasswordInput2").GetComponent<InputField>();
        if(registerPasswordInput2 == null) {
            return;
        }
        String password2 = registerPasswordInput2.text;

        Text registerResultText = GameObject.Find("RegisterResultText").GetComponent<Text>();

        //validation
        if(email.Equals("")) {
            if(registerResultText != null) {
                registerResultText.text = "이메일을 입력하세요. ";
            }
            return;
        } 
        else if(password.Equals("")) {
            if(registerResultText != null) {
                registerResultText.text = "비밀번호를 입력하세요. ";
            }
            return;
        }
        else if(!password.Equals(password2)) {
            if(registerResultText != null) {
                registerResultText.text = "비밀번호가 일치하지 않습니다. ";
            }
            return;
        }

        byte[] outBuffer = new byte[1 + 35 * 2 + 20 * 2];

        outBuffer[0] = 2;
        byte[] emailByte = Encoding.Default.GetBytes(email);
        byte[] passwordByte = Encoding.Default.GetBytes(password);
        Buffer.BlockCopy(emailByte, 0, outBuffer, 1, emailByte.Length);
        Buffer.BlockCopy(passwordByte, 0, outBuffer, 71, passwordByte.Length);

        ClientSocket.instance.SendRequest(outBuffer);

        if(registerResultText != null) {
            registerResultText.text = "가입 중.. ";
        }
    }
    public void ExitGame() {
        Application.Quit(0);
    }
    
    //CharacterList Scene
    public void OnLogoutButtonClicked() {
        User.instance.Logout();

        ClientSocket cs = FindObjectOfType<ClientSocket>();

        if(cs) {
            Destroy(cs.gameObject);
        }

        SceneManager.LoadScene("LoginScene");
    }
    public void OnToCreateCharacterButtonClicked() {
        GameObject.Find("CanvasHolder").transform.GetChild(1).gameObject.SetActive(true);
        
        GameObject.Find("CreateCharacterButton").GetComponent<Button>().interactable = true;
        GameObject.Find("CancleCreateCharacterButton").GetComponent<Button>().interactable = true;

        OnCancleDeleteCharacterButtonClicked();
    }
    public void OnToDeleteCharacterButtonClicked() {
        OnCancleCreateCharacterButtonClicked();

        int index = ScrollCharacterContent.selectedCharacterIndex;

        if(ScrollCharacterContent.selectedCharacterIndex == -1) {
            return;    
        }

        GameObject.Find("CanvasHolder").transform.GetChild(3).gameObject.SetActive(true);
        GameObject.Find("DeleteInput").GetComponent<InputField>().text = "";

        GameObject.Find("DeleteCharacterButton").GetComponent<Button>().interactable = true;
        GameObject.Find("DeleteCharacterCancleButton").GetComponent<Button>().interactable = true;
    }
    public void OnStartClicked() {
        if(ScrollCharacterContent.selectedCharacterIndex == -1) {
            return;
        }

        GameObject.Find("StartButton").GetComponent<Button>().interactable = false;

        int character_id = User.instance.characterDatas[ScrollCharacterContent.selectedCharacterIndex].id;

        byte[] buffer = new byte[5];
        buffer[0] = (byte)6;
        byte[] characterIDBytes = BitConverter.GetBytes(character_id);
        Buffer.BlockCopy(characterIDBytes, 0, buffer, 1, 4);

        ClientSocket.instance.SendRequest(buffer);
    }
    public void OnCreateCharacterButtonClicked() {
        string nickname = GameObject.Find("NicknameInput").GetComponent<InputField>().text;
        
        Text text = GameObject.Find("CreateCharacterMessage").GetComponent<Text>();

        if(nickname.Equals("")) {
            text.text = "닉네임을 입력하세요." ;
            return;
        }

        int job = GameObject.Find("Dropdown").GetComponent<Dropdown>().value;

        User.instance.SendCreateCharacterRequest(nickname, job);

        text.text = "서버 응답 기다리는 중." ;

        OnCancleDeleteCharacterButtonClicked();
    }
    public void OnCancleCreateCharacterButtonClicked() {
        GameObject textObject = GameObject.Find("CreateCharacterMessage");
        if(textObject != null) {
            textObject.GetComponent<Text>().text = "";
        }
        GameObject.Find("CanvasHolder").transform.GetChild(1).gameObject.SetActive(false);
    }
    public void OnDeleteCharacterButtonClicked() {
        InputField inputField = GameObject.Find("DeleteInput").GetComponent<InputField>();
        if(inputField.text.Equals("캐릭터 삭제")) {
            int index = ScrollCharacterContent.selectedCharacterIndex;
            int characterID = User.instance.characterDatas[index].id;

            GameObject.Find("DeleteCharacterButton").GetComponent<Button>().interactable = false;
            GameObject.Find("DeleteCharacterCancleButton").GetComponent<Button>().interactable = false;

            User.instance.SendDeleteCharacterRequest(characterID);
        }
    }
    public void OnCancleDeleteCharacterButtonClicked() {
        GameObject button = GameObject.Find("DeleteCharacterButton");
        if(button != null) {
            button.GetComponent<Button>().interactable = true;
        }

        GameObject.Find("CanvasHolder").transform.GetChild(3).gameObject.SetActive(false);
    }

    //private
    private void Awake() {
        instance = GetComponent<Utility>();
    }
}
