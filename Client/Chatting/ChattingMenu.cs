using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Chat;
using ExitGames.Client.Photon;
using Newtonsoft.Json.Linq;
using System;


public class ChattingMenu : MonoBehaviour, IChatClientListener
{
    public static ChattingMenu instance;
    public static ChatClient chatClient;
    public GameObject chatElementPrefap;
    public ChattingInput chattingInput;
    public TapSettingMenu tapSettingMenu;
    public TapClickMenu tapClickMenu;

    public Button addTapButton;

    public ChattingTap nullTap;

    public Transform tapsTransform;
    public Transform tapButtonsTransform;

    public ChattingTap currentTap;
    public ChattingTap[] taps;

    public int tapNum = 2;

    public void SendChat(ChatData data)
    {
        if (data.type == ChatData.eType.All)
        {
            chatClient.PublishMessage("All", getJson(data));
        }
        else if(data.type == ChatData.eType.Normal)
        {
            chatClient.PublishMessage(Room.instance.id.ToString(), getJson(data));
        }
        else if(data.type == ChatData.eType.Direct)
        {
            bool success = false;
            if (chatClient.TryGetChannel("All", out allChatChannel))
            {
                foreach (string nickname in allChatChannel.Subscribers)
                {
                    if (nickname == data.receiver)
                    {
                        success = true;
                    }
                }

                if (success)
                {
                    chatClient.SendPrivateMessage(data.receiver, getJson(data));
                }
                else
                {
                    ChatData _data = new ChatData();
                    _data.sender = "SYSTEM";
                    _data.type = ChatData.eType.System;
                    _data.systemMessageType = ChatData.eSystemMessageType.Alert;
                    _data.content = "귓속말 대상을 찾을 수 없습니다. ";

                    //chattingInput.status = ChattingInput.eStatus.DirectTarget;
                    //chattingInput.OnStatusChanged();

                    AddSystemMessage(_data);
                    return;
                }
            }
            else
            {
                Debug.Log("전체 채널 찾기 실패");
            }
        }

        foreach (ChattingTap tap in taps)
        {
            if (tap != null)
            {
                if (tap.index != -1)
                {
                    tap.OnSendChat(data);
                }
            }
        }
    }

    public void AddSystemMessage(ChatData data)
    {
        foreach (ChattingTap tap in taps)
        {
            if (tap != null)
            {
                tap.OnSystemMessage(data);
            }
        }
    }

    public void AlertMessage(string message)
    {
        ChatData data = new ChatData();
        data.content = message;
        data.type = ChatData.eType.System;
        data.systemMessageType = ChatData.eSystemMessageType.Alert;
        data.sender = "System";
        AddSystemMessage(data);
    }

    public void InfoMessage(string message)
    {
        ChatData data = new ChatData();
        data.content = message;
        data.type = ChatData.eType.System;
        data.systemMessageType = ChatData.eSystemMessageType.Info;
        data.sender = "System";
        AddSystemMessage(data);
    }

    public void OnChattingTapButtonClicked(int index)
    {
        foreach(ChattingTap tap in taps)
        {
            if(tap != null)
            {
                if(tap.index == index)
                {
                    tap.gameObject.SetActive(true);
                    currentTap = tap;
                }
                else
                {
                    tap.gameObject.SetActive(false);
                }
            }
        }
    }

    public void AddTap(string tapName, bool checkAll, bool checkNormal, bool checkDirect, bool checkSystem, bool checkAlertOff)
    {
        tapNum += 1;

        ChattingTap tap = null;
        for (int i = 0; i < tapsTransform.childCount; i++)
        {
            ChattingTap iTap = tapsTransform.GetChild(i).GetComponent<ChattingTap>();

            if (iTap.index == -1)
            {
                tap = iTap;
                break;
            }
        }

        int index = -1;

        for (int i = 0; i < 6; i++)
        {
            if (taps[i] != null)
            {
                if (taps[i].index != -1)
                {
                    continue;
                }
            }

            index = i;

            taps[i] = tap;
            tap.index = i;
            tap.tapName = tapName;
            tap.bShowAll = checkAll;
            tap.bShowNormal = checkNormal;
            tap.bShowDirect = checkDirect;
            tap.bShowSystem = checkSystem;
            tap.bOffAlert = checkAlertOff;

            break;
        }

        UpdateButtonUI();

        if(index == 5)
        {
            addTapButton.gameObject.SetActive(false);
        }

        List<string> options = new List<string>();
        options.Add(tapName);
        FindObjectOfType<TapSettingMenu>().tapsDropdown.AddOptions(options);

        currentTap.gameObject.SetActive(false);
        currentTap = tap;
        currentTap.gameObject.SetActive(true);
    }

    public void OnDeleteTapClicked(int index)
    {
        Debug.Log("deleteIndex: " + index);

        bool isActive = tapSettingMenu.gameObject.activeInHierarchy;
        tapSettingMenu.gameObject.SetActive(true);
        Dropdown dropdown = tapSettingMenu.tapsDropdown;
        dropdown.options.Remove(dropdown.options.Find(o => string.Equals(o.text, taps[index].tapName)));

        taps[index].gameObject.SetActive(false);
        taps[index].index = -1;

        ChattingTap newTap = null;

        //5번 탭을 삭제한 경우
        if (index == 5)
        {
            Debug.Log("index 5");
            newTap = taps[4];
        }
        //가장 오른쪽에 있는 탭을 삭제한 경우
        else if (taps[index + 1] == null)
        {
            Debug.Log("가장 오른쪽");
            newTap = taps[index - 1];
        }
        else if (taps[index + 1].index == -1)
        {
            Debug.Log("가장 오른쪽");
            newTap = taps[index - 1];
        }
        //중간에 있는 탭을 삭제한 경우
        else
        {
            Debug.Log("중간");

            for (int i = index; i < 5; i++)
            {
                if(i + 1 == tapNum)
                {
                    break;
                }
                else
                {
                    taps[i] = taps[i + 1];
                    taps[i].index = i;
                }
            }
            newTap = taps[index];
        }

        taps[tapNum - 1] = nullTap;
        tapNum -= 1;
        currentTap.gameObject.SetActive(false);
        currentTap = newTap;
        newTap.gameObject.SetActive(true);

        UpdateButtonUI();

        addTapButton.gameObject.SetActive(true);

        Dropdown.OptionData data = dropdown.options.Find(option => option.text == newTap.tapName);

        int dropdownIndex = dropdown.options.IndexOf(data);
        dropdown.value = dropdownIndex;
        dropdown.captionText.text = newTap.tapName;
        tapSettingMenu.gameObject.SetActive(isActive);
    }

    //button callbacks
    public void OnClickAddButton()
    {
        tapSettingMenu.gameObject.SetActive(true);
        tapSettingMenu.SetStatusToNewTap();
    }

    //photon callbacks
    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }

    public void OnDisconnected()
    {
        //throw new System.NotImplementedException();
    }

    public void OnConnected()
    {
        //전체 채팅에 참가한다.
        chatClient.Subscribe("All", creationOptions: new ChannelCreationOptions { PublishSubscribers = true });
        //일반 채팅에 참가한다.
        chatClient.Subscribe("0", creationOptions: new ChannelCreationOptions { PublishSubscribers = true });
    }

    public void OnChatStateChange(ChatState state)
    {
        //throw new System.NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            if(senders[i] == User.instance.nickname)
            {
                continue;
            }

            ChatData chatData = new ChatData();
            chatData.sender = senders[i];
            if (channelName == "All")
            {
                chatData.type = ChatData.eType.All;
            }
            else
            {
                chatData.type = ChatData.eType.Normal;
            }


            Debug.Log(messages[i].ToString());
            JObject decoded = JObject.Parse(messages[i].ToString());
            chatData.time = decoded["time"].ToObject<string>();
            chatData.senderID = decoded["id"].ToObject<int>();
            chatData.content = decoded["content"].ToObject<string>();

            string[] time = chatData.time.Split(':');
            if(int.Parse(DateTime.Now.Hour.ToString()) != int.Parse(time[0]))
            {
                Debug.Log(DateTime.Now.Hour.ToString() + " | " + time[0]);
                return;
            }
            else if(int.Parse(time[1]) - DateTime.Now.Minute < 0)
            {
                Debug.Log("return2");
                return;
            }

            Debug.Log(chatData.time);

            foreach (ChattingTap tap in taps)
            {
                if (tap != null)
                {
                    if (tap.index != -1)
                    {
                        tap.OnGetChat(chatData);
                    }
                }
            }
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if(sender == User.instance.nickname)
        {
            return;
        }

        ChatData chatData = new ChatData();
        chatData.sender = sender;
        chatData.receiver = User.instance.nickname;
        chatData.type = ChatData.eType.Direct;

        chattingInput.lastTarget = sender;

        JObject decoded = JObject.Parse(message.ToString());
        chatData.time = decoded["time"].ToObject<string>();
        chatData.senderID = decoded["id"].ToObject<int>();
        chatData.content = decoded["content"].ToObject<string>();

        foreach (ChattingTap tap in taps)
        {
            if (tap != null)
            {
                tap.OnGetChat(chatData);
            }
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        //throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
       //throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }
    
    public void UpdateButtonUI()
    {
        for (int i = 0; i < 6; i++)
        {
            Button button = tapButtonsTransform.GetChild(i).GetComponent<Button>();
            button.gameObject.SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            if (taps[i] == null)
            {
                continue;
            }

            if (taps[i].index == -1)
            {
                continue;
            }

            Button button = tapButtonsTransform.GetChild(taps[i].index).GetComponent<Button>();
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<Text>().text = taps[i].tapName;
            button.GetComponent<TapButton>().tap = taps[i];
        }
    }

    private void OnEnable()
    {
        instance = GetComponent<ChattingMenu>();
        UpdateButtonUI();

        chatClient = new ChatClient(this);
        chatClient.ChatRegion = "ASIA";
        bool result = chatClient.Connect("60854ca2-b381-4b94-8963-d0cb22c07416", "1.0", new AuthenticationValues(User.instance.nickname));
    }
    
    private void Update()
    {
        chatClient.Service();
    }

    private string getJson(ChatData data)
    {
        return "{" + "\"time\": \"" + data.time + "\", \"id\": " + data.senderID + ", \"content\": \"" + data.content + "\"}";
    }

    private ChatChannel allChatChannel;
}
