using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouYouServer.Common;
using YouYouServer.Core;
using YouYouServer.Model;

namespace YouYouServer.HotFix
{
    [Handler(ConstDefine.AccountControllerHandler)]
    public class AccountControllerHandler : IAccountControllerHandler
    {
        #region RegisterAsync 异步注册
        /// <summary>
        /// 异步注册
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public async Task<AccountEntity> RegisterAsync(string userName, string pwd, short channelId, string deviceIdentifier, string deviceModel)
        {
            //1.把UserName写入UserName集合
            long result = await YFRedisHelper.SAddAsync("Register_UserName", userName);

            //写入失败
            if (result == 0)
            {
                return null;
            }

            //2.去Mongodb 查询是否存在
            AccountEntity accountEntity = await DBModelMgr.AccountDBModel.GetEntityAsync(Builders<AccountEntity>.Filter.Eq(a => a.UserName, userName));

            //如果DB存在 不能注册
            if (accountEntity != null)
            {
                return null;
            }

            //3.写入MongoDB
            accountEntity = new AccountEntity();
            accountEntity.YFId = await DBModelMgr.UniqueIDAccount.GetUniqueIDAsync(0);

            accountEntity.ChannelId = channelId;
            accountEntity.DeviceIdentifier = deviceIdentifier;
            accountEntity.DeviceModel = deviceModel;

            accountEntity.UserName = userName;
            accountEntity.Password = pwd;
            accountEntity.CreateTime = DateTime.UtcNow;
            accountEntity.UpdateTime = DateTime.UtcNow;

            await DBModelMgr.AccountDBModel.AddAsync(accountEntity);
            return accountEntity;
        }
        #endregion

        #region LoginAsync 异步登录
        /// <summary>
        /// 异步登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="channelId"></param>
        /// <param name="deviceIdentifier"></param>
        /// <param name="deviceModel"></param>
        /// <returns></returns>
        public async Task<AccountEntity> LoginAsync(string userName, string pwd, short channelId, string deviceIdentifier, string deviceModel)
        {
            AccountEntity accountEntity = await YFRedisHelper.YFCacheShellAsync("Login_User", string.Format("{0}^{1}", userName, pwd), GetAccountAsync);

            //如果账号存在 并且换了设备 更新设备信息
            if (accountEntity != null && (accountEntity.ChannelId != channelId || !accountEntity.DeviceIdentifier.EndsWith(deviceIdentifier, StringComparison.CurrentCultureIgnoreCase) || !accountEntity.DeviceModel.EndsWith(deviceModel, StringComparison.CurrentCultureIgnoreCase)))
            {
                accountEntity.ChannelId = channelId;
                accountEntity.DeviceIdentifier = deviceIdentifier;
                accountEntity.DeviceModel = deviceModel;

                await DBModelMgr.AccountDBModel.UpdateAsync(accountEntity);
            }

            return accountEntity;
        }

        /// <summary>
        /// 异步查询账户实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<AccountEntity> GetAccountAsync(string key)
        {
            string[] arr = key.Split("^");
            if (arr.Length == 2)
            {
                return await DBModelMgr.AccountDBModel.GetEntityAsync(Builders<AccountEntity>.Filter.And(
                                Builders<AccountEntity>.Filter.Eq(a => a.UserName, arr[0]),
                                Builders<AccountEntity>.Filter.Eq(a => a.Password, arr[1])
                              ));
            }

            return null;
        }
        #endregion
    }
}