using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Core.Common
{
    /// <summary>
    /// 处理协议的标记
    /// </summary>
    public class HandlerAttribute : BaseAttribute
    {
        public string TypeName
        {
            get;
        }

        public HandlerAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}
