using MongoDB.Driver;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    public class UniqueIDGameServer : YFUniqueIDBase
    {
		public enum CollectionType
		{
			None = 0,
			Role = 1,
		}
        protected override MongoClient Client => MongoDBClient.CurrClient;

        protected override string DatabaseName => ServerConfig.GameServerDBName;

        protected override string CollectionName => "UniqueIDGameServer";
    }
}
