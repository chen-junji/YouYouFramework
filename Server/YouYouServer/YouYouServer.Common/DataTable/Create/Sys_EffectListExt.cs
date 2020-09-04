using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// 这类代码工具只生成一次
/// </summary>
    public static class Sys_EffectListExt
    {
        private static Dictionary<int, Sys_Effect?> m_Dic = new Dictionary<int, Sys_Effect?>();
        private static List<Sys_Effect> m_List = new List<Sys_Effect>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this Sys_EffectList sys_effectList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Sys_Effect.bytes", true);
            Init(Sys_EffectList.GetRootAsSys_EffectList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(Sys_EffectList sys_effectList)
        {
            int len = sys_effectList.SysEffectsLength;
            for (int j = 0; j < len; j++)
            {
                Sys_Effect ? sys_effect = sys_effectList.SysEffects(j);
                if (sys_effect != null)
                {
                    m_List.Add(sys_effect.Value);
                    m_Dic[sys_effect.Value.Id] = sys_effect;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Effect? GetEntity(this Sys_EffectList sys_effectList, int id)
        {
            Sys_Effect ? sys_effect;
            m_Dic.TryGetValue(id, out sys_effect);
            return sys_effect;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Sys_Effect GetEntityValue(this Sys_EffectList sys_effectList, int id)
        {
            Sys_Effect ? sys_effect = sys_effectList.GetEntity(id);
            if (sys_effect != null)
            {
                return sys_effect.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Sys_Effect> GetList(this Sys_EffectList sys_effectList)
        {
            return m_List;
        }
    }
}