using System;
using MongoDB.Driver;
using YouYouServer.Core;

namespace YouYouServer.Core
{
    public class UniqueIDLogger : YFUniqueIDBase
    {
        protected override MongoClient Client => LoggerMgr.CurrClient;

        protected override string DatabaseName => "Logger";

        protected override string CollectionName => "UniqueIDLogger";
    }
}