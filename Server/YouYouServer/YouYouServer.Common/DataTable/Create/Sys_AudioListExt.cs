using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_AudioListExt
    {
        private static Dictionary<int, Sys_Audio?> m_Dic = new Dictionary<int, Sys_Audio?>();
        private static List<Sys_Audio> m_List = new List<Sys_Audio>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_AudioList sys_audioList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_Audio.bytes", true);
            Init(Sys_AudioList.GetRootAsSys_AudioList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_AudioList sys_audioList)
        {
            int len = sys_audioList.SysAudiosLength;
            for (int j = 0; j < len; j++)
            {
                Sys_Audio ? sys_audio = sys_audioList.SysAudios(j);
                if (sys_audio != null)
                {
                    m_List.Add(sys_audio.Value);
                    m_Dic[sys_audio.Value.Id] = sys_audio;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Audio? GetEntity(this Sys_AudioList sys_audioList, int id)
        {
            Sys_Audio ? sys_audio;
            m_Dic.TryGetValue(id, out sys_audio);
            return sys_audio;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Audio GetEntityValue(this Sys_AudioList sys_audioList, int id)
        {
            Sys_Audio ? sys_audio = sys_audioList.GetEntity(id);
            if (sys_audio != null)
            {
                return sys_audio.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_Audio> GetList(this Sys_AudioList sys_audioList)
        {
            return m_List;
        }
    }
}