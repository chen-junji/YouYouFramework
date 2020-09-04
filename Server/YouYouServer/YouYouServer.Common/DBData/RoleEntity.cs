using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;
using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Common.DBData
{
    /// <summary>
    /// 角色实体
    /// </summary>
    public class RoleEntity : YFMongoEntityBase
    {
        public RoleEntity()
        {
            TaskList = new List<TaskEntity>();
            SkillDic = new Dictionary<int, SkillEntity>();
        }

        /// <summary>
        /// 账号
        /// </summary>
        public long AccountId;

        /// <summary>
        /// 职业
        /// </summary>
        public byte JobId;

        /// <summary>
        /// 性别
        /// </summary>
        public byte Sex;

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName;

        /// <summary>
        /// 等级
        /// </summary>
        public int Level;

        /// <summary>
        /// 任务列表
        /// </summary>
        public List<TaskEntity> TaskList;

        /// <summary>
        /// 技能字典
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, SkillEntity> SkillDic;
    }
}