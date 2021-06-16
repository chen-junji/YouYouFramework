//===================================================
//作    者：边涯  http://www.u3dol.com
//备    注：此代码为工具生成 请勿手工修改
//===================================================
using System.Collections;

namespace Hotfix
{
    /// <summary>
      /// Sys_SceneDetail实体
    /// </summary>
    public partial class Sys_SceneDetailEntity : DataTableEntityBase
    {
        /// <summary>
        /// 场景编号
        /// </summary>
        public int SceneId;

        /// <summary>
        /// 场景路径
        /// </summary>
        public string ScenePath;

        /// <summary>
        /// 场景等级(0=必须1=重要2=不重要)
        /// </summary>
        public int SceneGrade;

    }
}
