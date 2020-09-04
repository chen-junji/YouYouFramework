using MongoDB.Driver;
using YouYouServer.Common;
using YouYouServer.Common.DBData;
using YouYouServer.Common.Managers;
using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Model.Logic.DBModels
{
    /// <summary>
    /// 玩家账号DBModel
    /// </summary>
    public class AccountDBModel : YFMongoDBModelBase<AccountEntity>
    {
        protected override MongoClient Client => MongoDBClient.CurrClient;

        protected override string DatabaseName => ServerConfig.AccountDBName;

        protected override string CollectionName => "Account";
    }
}
