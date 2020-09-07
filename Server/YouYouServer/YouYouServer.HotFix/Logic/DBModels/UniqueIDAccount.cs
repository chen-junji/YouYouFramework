using MongoDB.Driver;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    public class UniqueIDAccount : YFUniqueIDBase
    {
		public enum CollectionType
		{
			None = 0,
			Account = 1,
			GameServer = 2,
		}
        protected override MongoClient Client => MongoDBClient.CurrClient;

        protected override string DatabaseName => ServerConfig.AccountDBName;

        protected override string CollectionName => "UniqueIDAccount";
    }
}