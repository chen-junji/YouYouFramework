using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace YouYouServer.Common.Managers
{
    /// <summary>
    /// 可以热更新的配置文件
    /// </summary>
    public sealed class HotFixConfig
    {
        private static Dictionary<string, string> m_ParamsDic;

        #region GetParams
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetParams(string key)
        {
            string value = null;
            m_ParamsDic.TryGetValue(key, out value);
            return value;
        }
        #endregion

        #region Load 加载热更参数
        /// <summary>
        /// 加载热更参数
        /// </summary>
        public static void Load()
        {
            if (m_ParamsDic == null)
            {
                m_ParamsDic = new Dictionary<string, string>();
            }
            else
            {
                m_ParamsDic.Clear();
            }

            string path = AppDomain.CurrentDomain.BaseDirectory + "Configs\\HotFixConfig.xml";

            XDocument doc = XDocument.Load(path);
            IEnumerable<XElement> enumerable = doc.Root.Elements("Item");
            foreach (var item in enumerable)
            {
                string key = item.Attribute("Key").Value;
                string value = item.Attribute("Value").Value;
                m_ParamsDic[key] = value;
            }
            Console.WriteLine("HotFixConfig Load Complete");
        }
        #endregion
    }
}