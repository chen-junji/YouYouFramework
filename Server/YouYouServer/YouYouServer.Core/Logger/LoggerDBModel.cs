using MongoDB.Driver;
using System;
using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Core.Logger
{
    public class LoggerDBModel : YFMongoDBModelBase<LoggerEntity>
    {
        protected override MongoClient Client => LoggerMgr.CurrClient;

        protected override string DatabaseName => "Logger";

        protected override string CollectionName => string.Format("Log_{0}", DateTime.UtcNow.ToString("yyyy-MM-dd"));

        protected override bool CanLogError => false;
    }
}
