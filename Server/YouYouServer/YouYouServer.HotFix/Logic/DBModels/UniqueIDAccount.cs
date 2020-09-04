using MongoDB.Driver;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Model.Logic.DBModels
{
    public class UniqueIDAccount : YFUniqueIDBase
    {
        protected override MongoClient Client => MongoDBClient.CurrClient;

        protected override string DatabaseName => ServerConfig.AccountDBName;

        protected override string CollectionName => "UniqueIDAccount";
    }
}