using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace YouYouServer.Core.YFMongoDB
{
    /// <summary>
    /// 唯一ID管理基类
    /// </summary>
    public abstract class YFUniqueIDBase
    {
        private FindOneAndUpdateOptions<YFUniqueEntity> options = new FindOneAndUpdateOptions<YFUniqueEntity> { IsUpsert = true };

        #region 子类需要实现的属性
        /// <summary>
        /// MongoClient
        /// </summary>
        protected abstract MongoClient Client
        {
            get;
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
        protected abstract string DatabaseName
        {
            get;
        }

        /// <summary>
        /// 集合名称
        /// </summary>
        protected abstract string CollectionName
        {
            get;
        }
        #endregion

        #region 获取文档集合 GetCollection
        private IMongoCollection<YFUniqueEntity> m_Collection = null;

        /// <summary>
        /// 获取文档集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<YFUniqueEntity> GetCollection()
        {
            if (m_Collection == null)
            {
                IMongoDatabase database = Client.GetDatabase(DatabaseName);
                m_Collection = database.GetCollection<YFUniqueEntity>(CollectionName);
            }
            return m_Collection;
        }
        #endregion

        /// <summary>
        /// 获取唯一ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="seq">自增值</param>
        /// <returns></returns>
        public long GetUniqueID(int type, int seq = 1)
        {
            var collection = GetCollection();
            YFUniqueEntity entity = m_Collection.FindOneAndUpdate(
                Builders<YFUniqueEntity>.Filter.Eq(t => t.Type, type),
                Builders<YFUniqueEntity>.Update.Inc(t => t.CurrId, seq),
                options
                );

            return entity == null ? seq : entity.CurrId + seq;
        }

        /// <summary>
        /// 异步获取唯一ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="seq">自增值</param>
        /// <returns></returns>
        public async Task<long> GetUniqueIDAsync(int type, int seq = 1)
        {
            var collection = GetCollection();
            YFUniqueEntity entity = await m_Collection.FindOneAndUpdateAsync(
                Builders<YFUniqueEntity>.Filter.Eq(t => t.Type, type),
                Builders<YFUniqueEntity>.Update.Inc(t => t.CurrId, seq),
                options
                );

            return entity == null ? seq : entity.CurrId + seq;
        }
    }

    /// <summary>
    /// 唯一实体
    /// </summary>
    public class YFUniqueEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        public ObjectId Id;

        public ushort Type;
        public long CurrId;
    }
}