using Google.Protobuf;
using UnityEngine;
using YouYou;
using YouYou.Proto;

public class TestSocket : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //连接Socket
            GameEntry.Socket.ConnectToMainSocket("127.0.0.1", 1304);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //序列化, 发送消息
            C2GWS_RegClient proto = new C2GWS_RegClient();
            proto.AccountId = 10;
            GameEntry.Socket.SendMainMsg(proto);
        }
    }
}