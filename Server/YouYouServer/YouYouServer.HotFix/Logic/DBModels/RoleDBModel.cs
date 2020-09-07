using MongoDB.Driver;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 角色数据管理器
    /// </summary>
    public class RoleDBModel : YFMongoDBModelBase<RoleEntity>
    {
        protected override MongoClient Client => MongoDBClient.CurrClient;

        protected override string DatabaseName => ServerConfig.GameServerDBName;

        protected override string CollectionName => "Role";
    }
}