using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using YouYouServer.Common;
using YouYouServer.Common.DBData;
using YouYouServer.Core.Common;
using YouYouServer.Core.Utils;
using YouYouServer.Model;
using YouYouServer.Model.IHandler;

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
            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            RetValue ret = new RetValue();

            //时间戳
            long t = dic["t"].ToLong();
            string deviceIdentifier = dic["deviceIdentifier"].ToString();
            string deviceModel = dic["deviceModel"].ToString();
            string sign = dic["sign"].ToString();

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

            int type = dic["Type"].ToInt();
            string userName = dic["UserName"].ToString();
            string pwd = dic["Password"].ToString();
            short channelId = dic["ChannelId"].ToShort();

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