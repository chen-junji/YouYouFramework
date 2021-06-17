using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public partial class Sys_CodeDBModel
    {
        /// <summary>
        /// 根据系统码获取提示内容
        /// </summary>
        public string GetSysCodeContent(int sysCode)
        {
            Sys_CodeEntity sys_Code = GetDic(sysCode);
            if (sys_Code != null) return GameEntry.Localization.GetString(sys_Code.Name);
            return string.Empty;
        }
    }
}