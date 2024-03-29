using System.Collections;

namespace Hotfix
{
    /// <summary>
      /// Sys_Animation实体
    /// </summary>
    public partial class Sys_AnimationEntity : DataTableEntityBase
    {
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc;

        /// <summary>
        /// 动画组ID
        /// </summary>
        public int GroupId;

        /// <summary>
        /// 动画路径
        /// </summary>
        public string AnimPath;

        /// <summary>
        /// 初始加载
        /// </summary>
        public byte InitLoad;

        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public int Expire;

    }
}
