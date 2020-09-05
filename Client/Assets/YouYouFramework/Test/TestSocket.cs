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
			GameEntry.Socket.ConnectToMainSocket("127.0.0.1", 1304);
		}
		else if (Input.GetKeyUp(KeyCode.B))
		{
			//序列化, 发送消息
			C2GWS_RegClient proto = new C2GWS_RegClient();
			proto.AccountId = 10;
			GameEntry.Socket.SendMainMsg(proto);
		}
		else if (Input.GetKeyUp(KeyCode.C))
		{
			//创建角色消息
			C2WS_CreateRole proto = new C2WS_CreateRole();
			proto.JobId = 1;
			proto.Sex = 1;
			proto.NickName = "protoYouyou你好01";

			GameEntry.Socket.SendMainMsg(proto);
		}
	}
}