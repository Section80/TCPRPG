using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChattingInput : MonoBehaviour
{
    //reference
    public Dropdown dropdown;
    public InputField inputField;

    public string lastTarget = "";     // 마지막으로 해당 유저에게 채팅을 보낸 유저의 닉네임이다. 

    //status
    //현재 inputField가 포커스 됬는지를 나타낸다. 
    public static bool isFocused = false;

    public enum eStatus
    {
        None,
        All,    //전체 채팅 입력중
        Normal, //일반 채팅 입력중
        DirectTarget,   //귓속말 대상 입력중
        DirectContent   //귓속말 내용 입력중
    };

    [SerializeField]
    public eStatus status = eStatus.Normal;

    //dropdown의 값이 변경되었을 때 호출되는 함수
    //dropdown의 값은 두가지 방법에 의해 변경될 수 있다.
    //1. 마우스로 직접 클릭해서 값을 선택한 경우
    //2. 코드에 의해
    //두 경우 각각에 대해 처리되어야 하는 방식이 다르다. 
    //1번의 경우, 귓말을 선택했을 때 DirectTarget으로 가야한다.
    //하지만 2번에 경우 DirectContent로 가야한다. 
    private void dropdownChanged()
    {
        if (byMouse == true)
        {
            if (dropdown.options[dropdown.value].text == "전체")
            {
                status = eStatus.All;
            }
            else if (dropdown.options[dropdown.value].text == "일반")
            {
                status = eStatus.Normal;
            }
            else if (dropdown.options[dropdown.value].text == "귓말")
            {
                status = eStatus.DirectTarget;
            }

            OnStatusChanged();
        }
    }

    public void OnStatusChanged()
    {
        if(status == eStatus.All)
        {
            setLockedString("");
            inputField.textComponent.color = new Color(0.9529f, 0.6078f, 0.0f, 1.0f);
            //directTarget = "";
            dropdown.value = dropdown.options.FindIndex(option => option.text == "전체");
        }
        else if (status == eStatus.Normal)
        {
            setLockedString("");
            inputField.textComponent.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            //directTarget = "";
            dropdown.value = dropdown.options.FindIndex(option => option.text == "일반");
        }
        else if (status == eStatus.DirectTarget)
        {
            setLockedString("대화 상대: ");
            inputField.textComponent.color = new Color(0.8431f, 0.5764f, 0.9372f, 1.0f);
            //directTarget = "";
            dropdown.value = dropdown.options.FindIndex(option => option.text == "귓말");
        }
        else if (status == eStatus.DirectContent)
        {
            setLockedString(">> " + directTarget + ": ");
            inputField.textComponent.color = new Color(0.8431f, 0.5764f, 0.9372f, 1.0f);
            dropdown.value = dropdown.options.FindIndex(option => option.text == "귓말");

        }

        //상태가 바뀌면 커서를 맨 끝으로 옮긴다. 
        StartCoroutine(SetPosition(true));

        //inputField의 focus를 가져온다. 
        inputField.ActivateInputField();
        inputField.Select();

        byMouse = true;
    }

    //유저가 inputField에 텍스트를 입력해 값이 바뀌었을 때 호출되는 함수다.
    //입력한 값에 따라 어떤 상태로 전이해야할지 결정하고 결과를 핸들링한다. 
    private void inputFieldChanged()
    {
        //lockedString을 수정할 수 없게 하기
        if ((lockedString.Length > 0) && (inputField.text.IndexOf(lockedString) != 0))
        {
            inputField.text = lockedString;
            inputField.MoveTextEnd(false);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            bool isCommand = false;
            eStatus newStatus = eStatus.None;
            bool isRFail = false;


            string text = getInputString();
            string[] words = text.Split(' ');

            if(words.Length > 1)
            {
                string word = words[0];

                if(word == "")
                {
                    return;
                }

                //명령어를 입력한 경우
                if(word[0] == '/')
                {
                    isCommand = true;

                    if(word.Length == 1)
                    {
                        isCommand = false;
                        return;
                    }

                    //전체 채팅 명령어의 경우
                    if(word[1] == 'a' || word[1] == 'ㅁ')
                    {
                        newStatus = eStatus.All;
                    }
                    //일반 채팅 명령어의 경우
                    else if(word[1] == 's' || word[1] == 'ㄴ')
                    {
                        newStatus = eStatus.Normal;
                    }
                    //귓속말 명령어의 경우
                    else if(word[1] == 'w' || word[1] == 'ㅈ')
                    {
                        newStatus = eStatus.DirectContent;
                        if (words.Length < 3)
                        {
                            isCommand = false;
                        }else
                        {
                            directTarget = words[1];
                        }
                    }
                    //귓속말 답장의 경우
                    else if(word[1] == 'r' || word[1] == 'ㄱ')
                    {
                        Debug.Log("/r: " + lastTarget);

                        newStatus = eStatus.DirectContent;
                        if (lastTarget != "") {
                            Debug.Log("2/r: " + lastTarget);
                            directTarget = lastTarget;
                        }
                        else if(directTarget == "")
                        {
                            newStatus = eStatus.None;
                            isRFail = true;
                        }
                    }
                    //존재하지 않는 명령어의 경우
                    else
                    {
                        newStatus = eStatus.None;
                    }
                }
            }

            //명령을 입력한 경우
            if (isCommand)
            {
                //존재하는 명령인 경우
                if (newStatus != eStatus.None)
                {
                    if (status == eStatus.All)
                    {
                        if (newStatus == eStatus.All)
                        {
                            status = eStatus.All;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.Normal)
                        {
                            status = eStatus.Normal;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.DirectContent)
                        {
                            //directTarget = words[1];
                            status = eStatus.DirectContent;
                            byMouse = false;
                            OnStatusChanged();
                        }
                    }
                    else if (status == eStatus.Normal)
                    {
                        if (newStatus == eStatus.All)
                        {
                            status = eStatus.All;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.Normal)
                        {
                            status = eStatus.Normal;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.DirectContent)
                        {
                            //directTarget = words[1];
                            status = eStatus.DirectContent;
                            byMouse = false;
                            OnStatusChanged();
                        }
                    }
                    else if (status == eStatus.DirectTarget)
                    {
                        if (newStatus == eStatus.All)
                        {
                            status = eStatus.All;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.Normal)
                        {
                            status = eStatus.Normal;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.DirectContent)
                        {
                            //directTarget = words[1];
                            status = eStatus.DirectContent;
                            byMouse = false;
                            OnStatusChanged();
                        }
                    }
                    else if (status == eStatus.DirectContent)
                    {
                        if (newStatus == eStatus.All)
                        {
                            status = eStatus.All;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.Normal)
                        {
                            status = eStatus.Normal;
                            byMouse = false;
                            OnStatusChanged();
                        }
                        else if (newStatus == eStatus.DirectContent)
                        {
                            status = eStatus.DirectContent;
                            byMouse = false;
                            OnStatusChanged();
                        }
                    }
                }
                else
                {
                    if (isRFail == true)
                    {
                        ChatData _data = new ChatData();
                        _data.sender = "SYSTEM";
                        _data.type = ChatData.eType.System;
                        _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                        _data.content = "답신할 대상이 없습니다. ";

                        ChattingMenu.instance.AddSystemMessage(_data);

                    }
                    else
                    {
                        ChatData _data = new ChatData();
                        _data.sender = "SYSTEM";
                        _data.type = ChatData.eType.System;
                        _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                        _data.content = "존재하지 않는 명령입니다.";

                        ChattingMenu.instance.AddSystemMessage(_data);
                    }

                    StartCoroutine(SetPosition(true));
                }
            }
            else
            {
                if (status == eStatus.DirectTarget)
                {
                    Debug.Log("target: " + words.Length);
                    if (words.Length > 0)
                    {
                        status = eStatus.DirectContent;
                        directTarget = words[0];
                        byMouse = false;
                        OnStatusChanged();
                    }
                }
            }
        }
    }

    private void onEnterPressed()
    {
        bool isCommand = false;
        eStatus newStatus = eStatus.None;
        bool isRFail = false;

        string text = getInputString();
        string[] words = text.Split(' ');

        if (words.Length > 0)
        {
            string word = words[0];

            if(word.Length == 0)
            {
                return;
            }

            //명령어를 입력한 경우
            if (word[0] == '/')
            {
                isCommand = true;

                if (word.Length == 1)
                {
                    isCommand = false;
                    return;
                }

                //전체 채팅 명령어의 경우
                if (word[1] == 'a' || word[1] == 'ㅁ')
                {
                    newStatus = eStatus.All;
                }
                //일반 채팅 명령어의 경우
                else if (word[1] == 's' || word[1] == 'ㄴ')
                {
                    newStatus = eStatus.Normal;
                }
                //귓속말 명령어의 경우
                else if (word[1] == 'w' || word[1] == 'ㅈ')
                {
                    newStatus = eStatus.DirectContent;
                    if (words.Length < 2)
                    {
                        isCommand = false;
                    }
                    else
                    {
                        directTarget = words[1];
                    }
                }
                //귓속말 답장의 경우
                else if (word[1] == 'r' || word[1] == 'ㄱ')
                {
                    newStatus = eStatus.DirectContent;
                    if (lastTarget != "")
                    {
                        directTarget = lastTarget;
                    }
                    else if (directTarget == "")
                    {
                        newStatus = eStatus.None;
                        isRFail = true;
                    }
                }
                //존재하지 않는 명령어의 경우
                else
                {
                    newStatus = eStatus.None;
                }
            }
        }

        //명령을 입력한 경우
        if (isCommand)
        {
            //존재하는 명령인 경우
            if (newStatus != eStatus.None)
            {
                if (status == eStatus.All)
                {
                    if (newStatus == eStatus.All)
                    {
                        status = eStatus.All;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.Normal)
                    {
                        status = eStatus.Normal;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.DirectContent)
                    {
                        //directTarget = words[1];
                        status = eStatus.DirectContent;
                        byMouse = false;
                        OnStatusChanged();
                    }
                }
                else if (status == eStatus.Normal)
                {
                    if (newStatus == eStatus.All)
                    {

                        status = eStatus.All;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.Normal)
                    {
                        status = eStatus.Normal;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.DirectContent)
                    {
                        status = eStatus.DirectContent;
                        byMouse = false;
                        OnStatusChanged();
                    }
                }
                else if (status == eStatus.DirectTarget)
                {
                    if (newStatus == eStatus.All)
                    {
                        status = eStatus.All;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.Normal)
                    {
                        status = eStatus.Normal;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.DirectContent)
                    {
                        //directTarget = words[1];
                        status = eStatus.DirectContent;
                        byMouse = false;
                        OnStatusChanged();
                    }
                }
                else if (status == eStatus.DirectContent)
                {
                    if (newStatus == eStatus.All)
                    {
                        status = eStatus.All;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.Normal)
                    {
                        status = eStatus.Normal;
                        byMouse = false;
                        OnStatusChanged();
                    }
                    else if (newStatus == eStatus.DirectContent)
                    {
                        //directTarget = words[1];
                        status = eStatus.DirectContent;
                        byMouse = false;
                        OnStatusChanged();
                    }
                }
            }
            else
            {
                if (isRFail == true)
                {
                    ChatData _data = new ChatData();
                    _data.sender = "SYSTEM";
                    _data.type = ChatData.eType.System;
                    _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                    _data.content = "답신할 대상이 없습니다. ";

                    ChattingMenu.instance.AddSystemMessage(_data);
                }
                else
                {
                    ChatData _data = new ChatData();
                    _data.sender = "SYSTEM";
                    _data.type = ChatData.eType.System;
                    _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                    _data.content = "존재하지 않는 명령어";

                    ChattingMenu.instance.AddSystemMessage(_data);
                }

                StartCoroutine(SetPosition(true));
            }
        }
        else
        {
            if(status == eStatus.DirectTarget)
            {
                status = eStatus.DirectContent;
                directTarget = words[0];
                byMouse = false;
                OnStatusChanged();
                return;
            }

            //chatData를 만든다. 
            ChatData chatData = new ChatData();
            if(status == eStatus.All)
            {
                chatData.type = ChatData.eType.All;
            }
            else if(status == eStatus.Normal)
            {
                chatData.type = ChatData.eType.Normal;
            }
            else if(status == eStatus.DirectContent)
            {
                chatData.type = ChatData.eType.Direct;
                chatData.receiver = directTarget;
            }

            chatData.sender = User.instance.nickname;
            chatData.senderID = User.instance.character_id;
            chatData.content = getInputString();
            chatData.time = DateTime.Now.ToString("HH:mm");

            ChattingMenu.instance.SendChat(chatData);

            StartCoroutine(SetPosition(true));
        }
    }

    private void Update()
    {
        //엔터를 눌렀을 때 isFocused가 false였다면 focuse를 가져온다. 
        //엔터를 눌렀을 때 isFoucuse가 true였다면 onEnterPressed를 호출한다. 
        if (inputField.isFocused != isFocused)
        {
            if (inputField.isFocused == false)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    isFocused = true;
                }
                else
                {
                    isFocused = false;
                }
            }
            else
            {
                isFocused = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isFocused)
            {
                onEnterPressed();
            }
            else
            {
                inputField.ActivateInputField();
                inputField.Select();
                StartCoroutine(SetPosition(false));
            }
        }
    }

    //inputText의 text를 lockedString으로 초기화하고 커서를 맨 끝으로 옮긴다. 
    private IEnumerator SetPosition(bool clear)
    {
        int width = inputField.caretWidth;
        inputField.caretWidth = 0;
        yield return new WaitForEndOfFrame();
        if(clear)
        {
            inputField.text = lockedString;
        }
        else
        {
            inputField.text = lockedString + getInputString();
        }
        inputField.caretWidth = width;
        inputField.caretPosition = inputField.text.Length;
    }

    private void Awake()
    {
        dropdown.onValueChanged.AddListener(delegate { dropdownChanged(); });
        inputField.onValueChanged.AddListener(delegate { inputFieldChanged(); });
    }

    private void setLockedString(string lockedString)
    {
        this.lockedString = lockedString;
        inputField.text = lockedString;

        inputField.characterLimit = lockedString.Length + 50;
    }

    // lockedString은 유저가 직접 입력한 것이 아니다. inputField.text에서 lockedString을 빼고 리턴해준다. 
    private string getInputString()
    {
        string text = inputField.text.Substring(lockedString.Length, inputField.text.Length - lockedString.Length);

        return text;
    }

    private string lockedString = "";   // inputField.text의 앞에 붙는, 유저가 수정할 수 없는 string값이다. 
    private string directTarget = "";   // 현재 설정되어있는 유저가 귓속말을 보낼 유저의 닉네임이다. 

    private bool byMouse = true;
}
