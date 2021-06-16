//===================================================
//作    者：边涯  http://www.u3dol.com
//备    注：此代码为工具生成 请勿手工修改
//===================================================
using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_Prefab实体
    /// </summary>
    public partial class Sys_PrefabEntity : DataTableEntityBase
    {
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc;

        /// <summary>
        /// Name
        /// </summary>
        public string Name;

        /// <summary>
        /// 资源分类
        /// </summary>
        public int AssetCategory;

        /// <summary>
        /// 路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 后缀
        /// </summary>
        public string Suffixes;

        /// <summary>
        /// 对象池编号
        /// </summary>
        public byte PoolId;

        /// <summary>
        /// 是否开启缓存池自动清理模式
        /// </summary>
        public byte CullDespawned;

        /// <summary>
        /// 缓存池自动清理但是始终保留几个对象不清理
        /// </summary>
        public int CullAbove;

        /// <summary>
        /// 多长时间清理一次单位是秒
        /// </summary>
        public int CullDelay;

        /// <summary>
        /// 每次清理几个
        /// </summary>
        public int CullMaxPerPass;

    }
}
