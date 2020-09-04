using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_SceneListExt
    {
        private static Dictionary<int, Sys_Scene?> m_Dic = new Dictionary<int, Sys_Scene?>();
        private static List<Sys_Scene> m_List = new List<Sys_Scene>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_SceneList sys_sceneList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_Scene.bytes", true);
            Init(Sys_SceneList.GetRootAsSys_SceneList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_SceneList sys_sceneList)
        {
            int len = sys_sceneList.SysScenesLength;
            for (int j = 0; j < len; j++)
            {
                Sys_Scene ? sys_scene = sys_sceneList.SysScenes(j);
                if (sys_scene != null)
                {
                    m_List.Add(sys_scene.Value);
                    m_Dic[sys_scene.Value.Id] = sys_scene;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Scene? GetEntity(this Sys_SceneList sys_sceneList, int id)
        {
            Sys_Scene ? sys_scene;
            m_Dic.TryGetValue(id, out sys_scene);
            return sys_scene;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Scene GetEntityValue(this Sys_SceneList sys_sceneList, int id)
        {
            Sys_Scene ? sys_scene = sys_sceneList.GetEntity(id);
            if (sys_scene != null)
            {
                return sys_scene.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_Scene> GetList(this Sys_SceneList sys_sceneList)
        {
            return m_List;
        }
    }
}