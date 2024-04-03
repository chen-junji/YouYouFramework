using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    /// <summary>
    /// 日志分类
    /// </summary>
    public enum LogCategory
    {
        /// <summary>
        /// 框架日志
        /// </summary>
        Framework,
        /// <summary>
        /// 流程
        /// </summary>
        Procedure,
        /// <summary>
        /// 资源管理
        /// </summary>
        Loader,
        /// <summary>
        /// 网络消息
        /// </summary>
        NetWork,
        /// <summary>
        /// UI管理
        /// </summary>
        UI,
        /// <summary>
        /// 音效管理
        /// </summary>
        Audio,
        /// <summary>
        /// 场景管理
        /// </summary>
        Scene,
        /// <summary>
        /// 对象池
        /// </summary>
        Pool,

        /// <summary>
        /// 业务通用日志
        /// </summary>
        Normal,
        /// <summary>
        /// 新手引导
        /// </summary>
        Guide,
        /// <summary>
        /// 技能系统相关日志
        /// </summary>
        Skill
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
        AffirmAndCancel
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

}