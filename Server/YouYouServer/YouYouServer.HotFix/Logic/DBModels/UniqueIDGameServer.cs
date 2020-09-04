using MongoDB.Driver;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Model.Logic.DBModels
{
    public class UniqueIDGameServer : YFUniqueIDBase
    {
        protected override MongoClient Client => MongoDBClient.CurrClient;

        protected override string DatabaseName => ServerConfig.GameServerDBName;

        protected override string CollectionName => "UniqueIDGameServer";
    }
}
