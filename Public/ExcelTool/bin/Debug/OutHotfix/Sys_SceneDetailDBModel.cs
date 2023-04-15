
//===================================================
//作    者：边涯  http://www.u3dol.com
//备    注：此代码为工具生成 请勿手工修改
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hotfix
{
    /// <summary>
    /// Sys_SceneDetail数据管理
    /// </summary>
    public partial class Sys_SceneDetailDBModel : DataTableDBModelBase<Sys_SceneDetailDBModel, Sys_SceneDetailEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_SceneDetail"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_SceneDetailEntity entity = new Sys_SceneDetailEntity();
                entity.Id = ms.ReadInt();
                entity.SceneName = ms.ReadUTF8String();
                entity.ScenePath = ms.ReadUTF8String();
                entity.SceneGrade = ms.ReadInt();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}