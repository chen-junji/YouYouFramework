using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_SceneDetailListExt
    {
        private static Dictionary<int, Sys_SceneDetail?> m_Dic = new Dictionary<int, Sys_SceneDetail?>();
        private static List<Sys_SceneDetail> m_List = new List<Sys_SceneDetail>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_SceneDetailList sys_scenedetailList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_SceneDetail.bytes", true);
            Init(Sys_SceneDetailList.GetRootAsSys_SceneDetailList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_SceneDetailList sys_scenedetailList)
        {
            int len = sys_scenedetailList.SysSceneDetailsLength;
            for (int j = 0; j < len; j++)
            {
                Sys_SceneDetail ? sys_scenedetail = sys_scenedetailList.SysSceneDetails(j);
                if (sys_scenedetail != null)
                {
                    m_List.Add(sys_scenedetail.Value);
                    m_Dic[sys_scenedetail.Value.Id] = sys_scenedetail;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_SceneDetail? GetEntity(this Sys_SceneDetailList sys_scenedetailList, int id)
        {
            Sys_SceneDetail ? sys_scenedetail;
            m_Dic.TryGetValue(id, out sys_scenedetail);
            return sys_scenedetail;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_SceneDetail GetEntityValue(this Sys_SceneDetailList sys_scenedetailList, int id)
        {
            Sys_SceneDetail ? sys_scenedetail = sys_scenedetailList.GetEntity(id);
            if (sys_scenedetail != null)
            {
                return sys_scenedetail.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_SceneDetail> GetList(this Sys_SceneDetailList sys_scenedetailList)
        {
            return m_List;
        }
    }
}