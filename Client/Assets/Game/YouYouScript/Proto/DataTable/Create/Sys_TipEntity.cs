using System.Collections;

namespace YouYouFramework
{
    /// <summary>
      /// Sys_Tip实体
    /// </summary>
    public partial class Sys_TipEntity : DataTableEntityBase
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Key;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content;

        /// <summary>
        /// 持续时间
        /// </summary>
        public float Duration;

    }
}
