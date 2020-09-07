using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.WebAccount.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost]
        public async Task<string> Post()
        {
            string json = Request.HttpContext.Request.Form["json"];

            RetValue ret = new RetValue();

			//时间戳
			long t = json.JsonCutApart("t").ToLong();
			string deviceIdentifier = json.JsonCutApart("deviceIdentifier");
			string deviceModel = json.JsonCutApart("deviceModel");
			string sign = json.JsonCutApart("sign");

			//1.判断时间戳 如果大于30秒 直接返回错误
			if (YFDateTimeUtil.GetTimestamp() - t > 30)
            {
                ret.HasError = true;
                ret.ErrorCode = SysCode.Connect_TimeOut;
                return JsonConvert.SerializeObject(ret);
            }

            //2.验证签名
            string signServer = YFEncryptUtil.Md5(string.Format("{0}:{1}", t, deviceIdentifier));
            if (!signServer.Equals(sign, StringComparison.CurrentCultureIgnoreCase))
            {
                ret.HasError = true;
                ret.ErrorCode = SysCode.SignError;
                return JsonConvert.SerializeObject(ret);
            }

			string value = json.JsonCutApart("value");
			int type = value.JsonCutApart("Type").ToInt();
			string userName = value.JsonCutApart("UserName").ToString();
			string pwd = value.JsonCutApart("Pwd").ToString();
			short channelId = value.JsonCutApart("ChannelId").ToShort();

			if (type == 0)
            {
                //注册
                AccountEntity accountEntity = await HotFixMgr.CurrAccountControllerHandler.RegisterAsync(userName, pwd, channelId, deviceIdentifier, deviceModel);
                if (accountEntity == null)
                {
                    ret.HasError = true;
                    ret.ErrorCode = SysCode.Reg_UserNameExists;
                    return JsonConvert.SerializeObject(ret);
                }

                ret.Value = JsonConvert.SerializeObject(accountEntity);
            }
            else if (type == 1)
            {
                //登录
                AccountEntity accountEntity = await HotFixMgr.CurrAccountControllerHandler.LoginAsync(userName, pwd, channelId, deviceIdentifier, deviceModel);
                if (accountEntity == null)
                {
                    ret.HasError = true;
                    ret.ErrorCode = SysCode.Login_UnExists;
                    return JsonConvert.SerializeObject(ret);
                }

                ret.Value = JsonConvert.SerializeObject(accountEntity);
            }

            return JsonConvert.SerializeObject(ret);
        }
    }
}