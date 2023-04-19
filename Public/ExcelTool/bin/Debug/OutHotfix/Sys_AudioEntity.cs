using System.Collections;

namespace Hotfix
{
    /// <summary>
      /// Sys_Audio实体
    /// </summary>
    public partial class Sys_AudioEntity : DataTableEntityBase
    {
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc;

        /// <summary>
        /// 路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 后缀
        /// </summary>
        public string Suffix;

        /// <summary>
        /// 音量（0-1）
        /// </summary>
        public float Volume;

        /// <summary>
        /// 是否循环
        /// </summary>
        public byte IsLoop;

        /// <summary>
        /// 是否淡入
        /// </summary>
        public byte IsFadeIn;

        /// <summary>
        /// 是否淡出
        /// </summary>
        public byte IsFadeOut;

        /// <summary>
        /// 优先级(默认128)
        /// </summary>
        public byte Priority;

    }
}
