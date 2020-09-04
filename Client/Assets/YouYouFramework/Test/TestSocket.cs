//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using Google.Protobuf;
using UnityEngine;
using YouYou;
using YouYou.Proto;

public class TestSocket : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
			//连接Socket
            GameEntry.Socket.ConnectToMainSocket("192.168.0.109", 1304);
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
			//发送消息
            C2WS_CreateRole proto = new C2WS_CreateRole();
            proto.JobId = 1;
            proto.Sex = 1;
            proto.NickName = "protoYouyou你好01";

            GameEntry.Socket.SendMainMsg(proto);

            //序列化
            //C2GWS_RegClient proto = new C2GWS_RegClient();
            //proto.AccountId = 10;

            //byte[] buffer = proto.ToByteArray();

            //GameEntry.Socket.SendMainMsg(proto);

            //反序列化
            //C2GWS_RegClient proto2 = (C2GWS_RegClient)C2GWS_RegClient.Parser.ParseFrom(buffer);

            //Debug.Log("proto2="+ proto2.AccountId);
        }
    }

    void TestA()
    {

    }

    void TestB()
    {

    }
}