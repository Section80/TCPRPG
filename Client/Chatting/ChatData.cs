public class ChatData
{
    public enum eType { All, Normal, Direct, System };
    public eType type;

    public enum eSystemMessageType { Alert, Info} ;
    public eSystemMessageType systemMessageType;

    public string sender;   // ä���� ���� ������ �г���
    public int senderID;    //ä���� ���� ������ ĳ���� ID
    public string receiver; //�ӼӸ��� ���, ä���� ���� ������ �г���
    public string time;    // ��/�� ��)1216
    public string content;  // ä�� �޼��� ����
}
