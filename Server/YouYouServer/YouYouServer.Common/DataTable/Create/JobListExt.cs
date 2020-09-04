using FlatBuffers;
using System.Collections.Generic;
using YouYouServer.Core.Utils;
using YouYouServer.Common.Managers;

namespace YouYou.DataTable
{
/// <summary>
/// Create By 悠游课堂 http://www.u3dol.com 陈俊基 13288578058
/// </summary>
    public static class JobListExt
    {
        private static Dictionary<int, Job?> m_Dic = new Dictionary<int, Job?>();
        private static List<Job> m_List = new List<Job>();

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        public static void LoadData(this JobList jobList)
        {
            byte[] buffer = YFIOUtil.GetBuffer(ServerConfig.DataTablePath + "/Job.bytes", true);
            Init(JobList.GetRootAsJobList(new ByteBuffer(buffer)));
        }
        #endregion

        /// <summary>
        /// 初始化到字典
        /// </summary>
        public static void Init(JobList jobList)
        {
            int len = jobList.JobsLength;
            for (int j = 0; j < len; j++)
            {
                Job ? job = jobList.Jobs(j);
                if (job != null)
                {
                    m_List.Add(job.Value);
                    m_Dic[job.Value.Id] = job;
                }
            }
        }

        /// <summary>
        /// 获取数据实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Job? GetEntity(this JobList jobList, int id)
        {
            Job ? job;
            m_Dic.TryGetValue(id, out job);
            return job;
        }

        /// <summary>
        /// 获取数据实体值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Job GetEntityValue(this JobList jobList, int id)
        {
            Job ? job = jobList.GetEntity(id);
            if (job != null)
            {
                return job.Value;
            }
            return default;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<Job> GetList(this JobList jobList)
        {
            return m_List;
        }
    }
}