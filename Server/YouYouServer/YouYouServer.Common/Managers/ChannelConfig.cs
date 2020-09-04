using System;
using System.Collections.Generic;
using System.Xml.Linq;
using YouYouServer.Common.DBData;

namespace YouYouServer.Common.Managers
{
    public sealed class ChannelConfig
    {
        public static Dictionary<string, ChannelEntity> m_ChannelDic;

        public static void Init()
        {
            m_ChannelDic = new Dictionary<string, ChannelEntity>();
            string path = AppDomain.CurrentDomain.BaseDirectory + "Configs\\ChannelConfig.xml";

            XDocument doc = XDocument.Load(path);
            IEnumerable<XElement> enumerable = doc.Root.Elements("Channel");
            foreach (var item in enumerable)
            {
                ChannelEntity channelEntity = new ChannelEntity();
                int.TryParse(item.Attribute("ChannelId").Value, out channelEntity.ChannelId);
                int.TryParse(item.Attribute("InnerVersion").Value, out channelEntity.InnerVersion);
                channelEntity.SourceVersion = item.Attribute("SourceVersion").Value;
                channelEntity.SourceUrl = item.Attribute("SourceUrl").Value;
                channelEntity.RechargeUrl = item.Attribute("RechargeUrl").Value;
                int.TryParse(item.Attribute("PayServerNo").Value, out channelEntity.PayServerNo);
                channelEntity.TDAppId = item.Attribute("TDAppId").Value;
                bool.TryParse(item.Attribute("IsOpenTD").Value, out channelEntity.IsOpenTD);

                m_ChannelDic[channelEntity.ChannelId + "_" + channelEntity.InnerVersion] = channelEntity;
            }

            Console.WriteLine("ChannelConfig Init Complete");
        }

        /// <summary>
        /// 根据渠道号和内部版本号获取渠道信息
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="innerVersion"></param>
        /// <returns></returns>
        public static ChannelEntity Get(string channelId, string innerVersion)
        {
            ChannelEntity channelEntity = null;
            m_ChannelDic.TryGetValue(channelId + "_" + innerVersion, out channelEntity);
            return channelEntity;
        }
    }
}