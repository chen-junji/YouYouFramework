using System.Collections;

namespace YouYouFramework
{
    /// <summary>
      /// Sys_BGM实体
    /// </summary>
    public partial class Sys_BGMEntity : DataTableEntityBase
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string AssetFullPath;

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
