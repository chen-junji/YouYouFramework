using System.Collections;

namespace YouYouFramework
{
    /// <summary>
      /// Sys_Dialog实体
    /// </summary>
    public partial class Sys_DialogEntity : DataTableEntityBase
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Key;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content;

        /// <summary>
        /// 按钮1文本
        /// </summary>
        public string BtnText1;

        /// <summary>
        /// 按钮2文本
        /// </summary>
        public string BtnText2;

    }
}
