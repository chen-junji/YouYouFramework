using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// 日志分类
	/// </summary>
	public enum LogCategory
	{
		/// <summary>
		/// 普通日志
		/// </summary>
		Normal,
		/// <summary>
		/// 流程日志
		/// </summary>
		Procedure,
		/// <summary>
		/// 资源管理日志
		/// </summary>
		Resource,
		/// <summary>
		/// 协议日志
		/// </summary>
		Proto,
        /// <summary>
        /// 新手引导日志
        /// </summary>
        Hollow,
        UI,
        Audio
    }

	public enum LoadingType
	{
		/// <summary>
		/// 切换场景
		/// </summary>
		ChangeScene,
		/// <summary>
		/// 检查更新
		/// </summary>
		CheckVersion
	}

    /// <summary>
    /// 提示窗口,按钮显示方式
    /// </summary>
    public enum DialogFormType
    {
        /// <summary>
        /// 确定按钮
        /// </summary>
        Affirm,
        /// <summary>
        /// 确定,取消按钮
        /// </summary>
        Noraml
    }

    /// <summary>
    /// UI窗口的显示类型
    /// </summary>
    public enum UIFormShowMode
    {
        Normal = 0,
        /// <summary>
        /// 反切
        /// </summary>
        ReverseChange = 1,
    }

    /// <summary>
    /// 支付平台
    /// </summary>
    public enum PayPlatform
    {
        IOS,
        Ali_WX,
        Goggle,
    }

}