using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_PrefabListExt
    {
        private static Dictionary<int, Sys_Prefab?> m_Dic = new Dictionary<int, Sys_Prefab?>();
        private static List<Sys_Prefab> m_List = new List<Sys_Prefab>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_PrefabList sys_prefabList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_Prefab.bytes", true);
            Init(Sys_PrefabList.GetRootAsSys_PrefabList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_PrefabList sys_prefabList)
        {
            int len = sys_prefabList.SysPrefabsLength;
            for (int j = 0; j < len; j++)
            {
                Sys_Prefab ? sys_prefab = sys_prefabList.SysPrefabs(j);
                if (sys_prefab != null)
                {
                    m_List.Add(sys_prefab.Value);
                    m_Dic[sys_prefab.Value.Id] = sys_prefab;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Prefab? GetEntity(this Sys_PrefabList sys_prefabList, int id)
        {
            Sys_Prefab ? sys_prefab;
            m_Dic.TryGetValue(id, out sys_prefab);
            return sys_prefab;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Prefab GetEntityValue(this Sys_PrefabList sys_prefabList, int id)
        {
            Sys_Prefab ? sys_prefab = sys_prefabList.GetEntity(id);
            if (sys_prefab != null)
            {
                return sys_prefab.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_Prefab> GetList(this Sys_PrefabList sys_prefabList)
        {
            return m_List;
        }
    }
}