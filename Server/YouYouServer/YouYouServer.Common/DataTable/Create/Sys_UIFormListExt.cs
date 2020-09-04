using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_UIFormListExt
    {
        private static Dictionary<int, Sys_UIForm?> m_Dic = new Dictionary<int, Sys_UIForm?>();
        private static List<Sys_UIForm> m_List = new List<Sys_UIForm>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_UIFormList sys_uiformList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_UIForm.bytes", true);
            Init(Sys_UIFormList.GetRootAsSys_UIFormList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_UIFormList sys_uiformList)
        {
            int len = sys_uiformList.SysUIFormsLength;
            for (int j = 0; j < len; j++)
            {
                Sys_UIForm ? sys_uiform = sys_uiformList.SysUIForms(j);
                if (sys_uiform != null)
                {
                    m_List.Add(sys_uiform.Value);
                    m_Dic[sys_uiform.Value.Id] = sys_uiform;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_UIForm? GetEntity(this Sys_UIFormList sys_uiformList, int id)
        {
            Sys_UIForm ? sys_uiform;
            m_Dic.TryGetValue(id, out sys_uiform);
            return sys_uiform;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_UIForm GetEntityValue(this Sys_UIFormList sys_uiformList, int id)
        {
            Sys_UIForm ? sys_uiform = sys_uiformList.GetEntity(id);
            if (sys_uiform != null)
            {
                return sys_uiform.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_UIForm> GetList(this Sys_UIFormList sys_uiformList)
        {
            return m_List;
        }
    }
}