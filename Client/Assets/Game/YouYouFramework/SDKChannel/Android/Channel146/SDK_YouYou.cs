using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using YouYou;
using System.Configuration;

public class SDK_YouYou : IAndroidSDK
{
	public void DoAction(string actionName, string param, Action<EventArgs> callBack)
	{
		switch (actionName.ToLower())
		{
			case "logon":
				WeixinLogOn(param, callBack);
				break;
			case "pay":
				WeixinPay(param);
				break;
			case "sendcode":
				SendCode(param);
				break;
			case "payv2":
				PayV2(param);
				break;
		}
	}

	#region 支付宝支付
	private void PayV2(string param)
	{
		Dictionary<string, string> dic = new Dictionary<string, string>();
		//dic["userNum"] = GameEntry.Data.UserDataManager.UserNum;
		dic["itemCode"] = param;
		GameEntry.Http.Post(GameEntry.Http.RealWebAccountUrl + "PayInfo/AliPayInfo/AliPay", dic.ToJson(), true, (string retValue) =>
		 {
			AndroidInterface.Instance.DoAndroidAction("payv2", retValue);
		 });
	}
	#endregion

	#region 微信登录
	private string m_state;//随机数
	private Action<EventArgs> m_onWeixinLogOn;
	private void WeixinLogOn(string param, Action<EventArgs> callBack)
	{
		m_onWeixinLogOn = callBack;
		m_state = UnityEngine.Random.Range(0, 10000).ToString();
		AndroidInterface.Instance.DoAndroidAction("login", m_state);
	}
	private void SendCode(string param)
	{
		Debug.LogError("SendCode==" + param);
		string[] arr = param.Split('\t');

		string state = arr[0];
		string code = arr[1];

		if (m_state.Equals(state))
		{
			//给账户服务器发送请求
			Dictionary<string, object> data = new Dictionary<string, object>();
			data["ChennelId"] = GameEntry.Data.SysData.CurrChannelConfig.ChannelId;
			data["Code"] = code;

			Dictionary<string, object> dic = new Dictionary<string, object>();
			dic["SDKName"] = "SDK_Weixin";
			dic["ActionName"] = "LogOn";
			dic["UserData"] = data.ToJson();
			Debug.LogError(dic.ToJson());
		}
	}
	#endregion

	#region 微信支付
	private void WeixinPay(string param)
	{
		Dictionary<string, string> dic = new Dictionary<string, string>();
		//dic["userNum"] = GameEntry.Data.UserDataManager.UserNum;
		dic["itemCode"] = param;
		GameEntry.Http.Post(GameEntry.Http.RealWebAccountUrl + "PayInfo/WeChatPay/StartPay", dic.ToJson(), true, (string retValue) =>
		{

			string partnerId = retValue.JsonCutApart("partnerId");
			string prepayId = retValue.JsonCutApart("prepayId");
			string nonceStr = retValue.JsonCutApart("nonceStr");
			string timeStamp = retValue.JsonCutApart("timeStamp");
			string package = retValue.JsonCutApart("package");
			string sign = retValue.JsonCutApart("sign");

				string str = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", partnerId, prepayId, nonceStr, timeStamp, package, sign);
				AndroidInterface.Instance.DoAndroidAction("pay", str);
		});
	}
	#endregion
}
