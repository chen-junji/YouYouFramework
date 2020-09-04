using System;
using MongoDB.Driver;
using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Core.Logger
{
    public class UniqueIDLogger : YFUniqueIDBase
    {
        protected override MongoClient Client => LoggerMgr.CurrClient;

        protected override string DatabaseName => "Logger";

        protected override string CollectionName => "UniqueIDLogger";
    }
}