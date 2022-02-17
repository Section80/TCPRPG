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

    public string lastTarget = "";     // ���������� �ش� �������� ä���� ���� ������ �г����̴�. 

    //status
    //���� inputField�� ��Ŀ�� ������� ��Ÿ����. 
    public static bool isFocused = false;

    public enum eStatus
    {
        None,
        All,    //��ü ä�� �Է���
        Normal, //�Ϲ� ä�� �Է���
        DirectTarget,   //�ӼӸ� ��� �Է���
        DirectContent   //�ӼӸ� ���� �Է���
    };

    [SerializeField]
    public eStatus status = eStatus.Normal;

    //dropdown�� ���� ����Ǿ��� �� ȣ��Ǵ� �Լ�
    //dropdown�� ���� �ΰ��� ����� ���� ����� �� �ִ�.
    //1. ���콺�� ���� Ŭ���ؼ� ���� ������ ���
    //2. �ڵ忡 ����
    //�� ��� ������ ���� ó���Ǿ�� �ϴ� ����� �ٸ���. 
    //1���� ���, �Ӹ��� �������� �� DirectTarget���� �����Ѵ�.
    //������ 2���� ��� DirectContent�� �����Ѵ�. 
    private void dropdownChanged()
    {
        if (byMouse == true)
        {
            if (dropdown.options[dropdown.value].text == "��ü")
            {
                status = eStatus.All;
            }
            else if (dropdown.options[dropdown.value].text == "�Ϲ�")
            {
                status = eStatus.Normal;
            }
            else if (dropdown.options[dropdown.value].text == "�Ӹ�")
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
            dropdown.value = dropdown.options.FindIndex(option => option.text == "��ü");
        }
        else if (status == eStatus.Normal)
        {
            setLockedString("");
            inputField.textComponent.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            //directTarget = "";
            dropdown.value = dropdown.options.FindIndex(option => option.text == "�Ϲ�");
        }
        else if (status == eStatus.DirectTarget)
        {
            setLockedString("��ȭ ���: ");
            inputField.textComponent.color = new Color(0.8431f, 0.5764f, 0.9372f, 1.0f);
            //directTarget = "";
            dropdown.value = dropdown.options.FindIndex(option => option.text == "�Ӹ�");
        }
        else if (status == eStatus.DirectContent)
        {
            setLockedString(">> " + directTarget + ": ");
            inputField.textComponent.color = new Color(0.8431f, 0.5764f, 0.9372f, 1.0f);
            dropdown.value = dropdown.options.FindIndex(option => option.text == "�Ӹ�");

        }

        //���°� �ٲ�� Ŀ���� �� ������ �ű��. 
        StartCoroutine(SetPosition(true));

        //inputField�� focus�� �����´�. 
        inputField.ActivateInputField();
        inputField.Select();

        byMouse = true;
    }

    //������ inputField�� �ؽ�Ʈ�� �Է��� ���� �ٲ���� �� ȣ��Ǵ� �Լ���.
    //�Է��� ���� ���� � ���·� �����ؾ����� �����ϰ� ����� �ڵ鸵�Ѵ�. 
    private void inputFieldChanged()
    {
        //lockedString�� ������ �� ���� �ϱ�
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

                //��ɾ �Է��� ���
                if(word[0] == '/')
                {
                    isCommand = true;

                    if(word.Length == 1)
                    {
                        isCommand = false;
                        return;
                    }

                    //��ü ä�� ��ɾ��� ���
                    if(word[1] == 'a' || word[1] == '��')
                    {
                        newStatus = eStatus.All;
                    }
                    //�Ϲ� ä�� ��ɾ��� ���
                    else if(word[1] == 's' || word[1] == '��')
                    {
                        newStatus = eStatus.Normal;
                    }
                    //�ӼӸ� ��ɾ��� ���
                    else if(word[1] == 'w' || word[1] == '��')
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
                    //�ӼӸ� ������ ���
                    else if(word[1] == 'r' || word[1] == '��')
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
                    //�������� �ʴ� ��ɾ��� ���
                    else
                    {
                        newStatus = eStatus.None;
                    }
                }
            }

            //����� �Է��� ���
            if (isCommand)
            {
                //�����ϴ� ����� ���
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
                        _data.content = "����� ����� �����ϴ�. ";

                        ChattingMenu.instance.AddSystemMessage(_data);

                    }
                    else
                    {
                        ChatData _data = new ChatData();
                        _data.sender = "SYSTEM";
                        _data.type = ChatData.eType.System;
                        _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                        _data.content = "�������� �ʴ� ����Դϴ�.";

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

            //��ɾ �Է��� ���
            if (word[0] == '/')
            {
                isCommand = true;

                if (word.Length == 1)
                {
                    isCommand = false;
                    return;
                }

                //��ü ä�� ��ɾ��� ���
                if (word[1] == 'a' || word[1] == '��')
                {
                    newStatus = eStatus.All;
                }
                //�Ϲ� ä�� ��ɾ��� ���
                else if (word[1] == 's' || word[1] == '��')
                {
                    newStatus = eStatus.Normal;
                }
                //�ӼӸ� ��ɾ��� ���
                else if (word[1] == 'w' || word[1] == '��')
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
                //�ӼӸ� ������ ���
                else if (word[1] == 'r' || word[1] == '��')
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
                //�������� �ʴ� ��ɾ��� ���
                else
                {
                    newStatus = eStatus.None;
                }
            }
        }

        //����� �Է��� ���
        if (isCommand)
        {
            //�����ϴ� ����� ���
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
                    _data.content = "����� ����� �����ϴ�. ";

                    ChattingMenu.instance.AddSystemMessage(_data);
                }
                else
                {
                    ChatData _data = new ChatData();
                    _data.sender = "SYSTEM";
                    _data.type = ChatData.eType.System;
                    _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                    _data.content = "�������� �ʴ� ��ɾ�";

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

            //chatData�� �����. 
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
        //���͸� ������ �� isFocused�� false���ٸ� focuse�� �����´�. 
        //���͸� ������ �� isFoucuse�� true���ٸ� onEnterPressed�� ȣ���Ѵ�. 
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

    //inputText�� text�� lockedString���� �ʱ�ȭ�ϰ� Ŀ���� �� ������ �ű��. 
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

    // lockedString�� ������ ���� �Է��� ���� �ƴϴ�. inputField.text���� lockedString�� ���� �������ش�. 
    private string getInputString()
    {
        string text = inputField.text.Substring(lockedString.Length, inputField.text.Length - lockedString.Length);

        return text;
    }

    private string lockedString = "";   // inputField.text�� �տ� �ٴ�, ������ ������ �� ���� string���̴�. 
    private string directTarget = "";   // ���� �����Ǿ��ִ� ������ �ӼӸ��� ���� ������ �г����̴�. 

    private bool byMouse = true;
}
