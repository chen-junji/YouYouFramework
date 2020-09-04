using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_StorySoundListExt
    {
        private static Dictionary<int, Sys_StorySound?> m_Dic = new Dictionary<int, Sys_StorySound?>();
        private static List<Sys_StorySound> m_List = new List<Sys_StorySound>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_StorySoundList sys_storysoundList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_StorySound.bytes", true);
            Init(Sys_StorySoundList.GetRootAsSys_StorySoundList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_StorySoundList sys_storysoundList)
        {
            int len = sys_storysoundList.SysStorySoundsLength;
            for (int j = 0; j < len; j++)
            {
                Sys_StorySound ? sys_storysound = sys_storysoundList.SysStorySounds(j);
                if (sys_storysound != null)
                {
                    m_List.Add(sys_storysound.Value);
                    m_Dic[sys_storysound.Value.Id] = sys_storysound;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_StorySound? GetEntity(this Sys_StorySoundList sys_storysoundList, int id)
        {
            Sys_StorySound ? sys_storysound;
            m_Dic.TryGetValue(id, out sys_storysound);
            return sys_storysound;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_StorySound GetEntityValue(this Sys_StorySoundList sys_storysoundList, int id)
        {
            Sys_StorySound ? sys_storysound = sys_storysoundList.GetEntity(id);
            if (sys_storysound != null)
            {
                return sys_storysound.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_StorySound> GetList(this Sys_StorySoundList sys_storysoundList)
        {
            return m_List;
        }
    }
}