public class ChatData
{
    public enum eType { All, Normal, Direct, System };
    public eType type;

    public enum eSystemMessageType { Alert, Info} ;
    public eSystemMessageType systemMessageType;

    public string sender;   // 채팅을 보낸 유저의 닉네임
    public int senderID;    //채팅을 보낸 유저의 캐릭터 ID
    public string receiver; //귓속말의 경우, 채팅을 바은 유저의 닉네임
    public string time;    // 시/분 예)1216
    public string content;  // 채팅 메세지 내용
}
