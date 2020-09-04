using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace YouYouServer.Common.Managers
{
    /// <summary>
    /// 游戏服配置
    /// </summary>
    public sealed class ServerConfig
    {
        /// <summary>
        /// 区服编号(这个编号指的是整个区的编号)
        /// </summary>
        public static int AreaServerId;

        /// <summary>
        /// 当前服务器类型
        /// </summary>
        public static ConstDefine.ServerType CurrServerType;

        /// <summary>
        /// 当前的服务器编号
        /// </summary>
        public static int CurrServerId = 0;

        /// <summary>
        /// Mongo连接字符串
        /// </summary>
        public static string MongoConnectionString;

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public static string RedisConnectionString;

        /// <summary>
        /// 数据表路径
        /// </summary>
        public static string DataTablePath;

        /// <summary>
        /// 区服内服务器列表
        /// </summary>
        private static List<Server> ServeList;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Configs\\ServerConfig.xml";

            XDocument doc = XDocument.Load(path);

            AreaServerId = doc.Root.Element("AreaServerId").Value.ToInt();
            CurrServerType = (ConstDefine.ServerType)doc.Root.Element("CurrServerType").Value.ToInt();
            CurrServerId = doc.Root.Element("CurrServerId").Value.ToInt();

            MongoConnectionString = doc.Root.Element("MongoConnectionString").Value;
            RedisConnectionString = doc.Root.Element("RedisConnectionString").Value;
            DataTablePath = doc.Root.Element("DataTablePath").Value;

            ServeList = new List<Server>();

            IEnumerable<XElement> lst = doc.Root.Element("Servers").Elements("Item");
            foreach (XElement item in lst)
            {
                ServeList.Add(new Server()
                {
                    CurrServerType = (ConstDefine.ServerType)item.Attribute("ServerType").Value.ToInt(),
                    ServerId = item.Attribute("ServerId").Value.ToInt(),
                    Ip = item.Attribute("Ip").Value,
                    Port = item.Attribute("Port").Value.ToInt()
                });
            }
            Console.WriteLine("ServerConfig Init Complete");
        }

        /// <summary>
        /// 账号数据库DBName
        /// </summary>
        public const string AccountDBName = "DBAccount";

        #region GameServerDBName 游戏服DBName
        private static string m_GameServerDBName = null;

        /// <summary>
        /// 游戏服DBName
        /// </summary>
        public static string GameServerDBName
        {
            get
            {
                if (string.IsNullOrEmpty(m_GameServerDBName))
                {
                    m_GameServerDBName = string.Format("GameServer_{0}", AreaServerId);
                }
                return m_GameServerDBName;
            }
        }
        #endregion

        #region RoleHashKey 角色哈希Key
        private static string m_RoleHashKey = null;

        /// <summary>
        /// 角色哈希Key
        /// </summary>
        public static string RoleHashKey
        {
            get
            {
                if (string.IsNullOrEmpty(m_RoleHashKey))
                {
                    m_RoleHashKey = string.Format("{0}_RoleHash", AreaServerId);
                }
                return m_RoleHashKey;
            }
        }
        #endregion

        #region RoleNickNameKey 角色昵称Key
        private static string m_RoleNickNameKey = null;

        /// <summary>
        /// 角色昵称Key
        /// </summary>
        public static string RoleNickNameKey
        {
            get
            {
                if (string.IsNullOrEmpty(m_RoleNickNameKey))
                {
                    m_RoleNickNameKey = string.Format("{0}_NickName", AreaServerId);
                }
                return m_RoleNickNameKey;
            }
        }
        #endregion

        #region Server 单台服务器
        /// <summary>
        /// 单台服务器
        /// </summary>
        public class Server
        {
            /// <summary>
            /// 服务器类型
            /// </summary>
            public ConstDefine.ServerType CurrServerType;

            /// <summary>
            /// 服务器编号
            /// </summary>
            public int ServerId;

            /// <summary>
            /// 服务器IP
            /// </summary>
            public string Ip;

            /// <summary>
            /// 服务器端口
            /// </summary>
            public int Port;
        }
        #endregion

        #region GetServer 根据服务器类型和编号获取服务器
        /// <summary>
        /// 根据服务器类型和编号获取服务器
        /// </summary>
        /// <param name="serverType"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static Server GetServer(ConstDefine.ServerType serverType, int serverId)
        {
            int len = ServeList.Count;
            for (int i = 0; i < len; i++)
            {
                Server server = ServeList[i];
                if (server.CurrServerType == serverType && server.ServerId == serverId)
                {
                    return server;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据服务器类型获取服务器列表
        /// </summary>
        /// <param name="serverType">服务器类型</param>
        /// <returns></returns>
        public static List<Server> GetServerByType(ConstDefine.ServerType serverType)
        {
            List<Server> lst = new List<Server>();
            int len = ServeList.Count;
            for (int i = 0; i < len; i++)
            {
                Server server = ServeList[i];
                if (server.CurrServerType == serverType)
                {
                    lst.Add(server);
                }
            }
            return lst;
        }
        #endregion

        #region GetCurrServer 获取当前服务器
        /// <summary>
        /// 获取当前服务器
        /// </summary>
        /// <returns></returns>
        public static Server GetCurrServer()
        {
            return GetServer(CurrServerType, CurrServerId);
        }
        #endregion
    }
}
