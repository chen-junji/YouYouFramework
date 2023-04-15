using System.Collections;

namespace Hotfix
{
    /// <summary>
      /// Sys_UIForm实体
    /// </summary>
    public partial class Sys_UIFormEntity : DataTableEntityBase
    {
        /// <summary>
        /// UI分组编号
        /// </summary>
        public byte UIGroupId;

        /// <summary>
        /// 路径
        /// </summary>
        public string AssetPath_Chinese;

        /// <summary>
        /// 路径
        /// </summary>
        public string AssetPath_English;

        /// <summary>
        /// 禁用层级管理
        /// </summary>
        public int DisableUILayer;

        /// <summary>
        /// 是否锁定
        /// </summary>
        public int IsLock;

        /// <summary>
        /// 允许多实例
        /// </summary>
        public int CanMulit;

        /// <summary>
        /// 显示类型0=普通1=反切
        /// </summary>
        public byte ShowMode;

    }
}
