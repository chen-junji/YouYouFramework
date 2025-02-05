
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYouFramework
{
    /// <summary>
    /// Sys_Tip数据管理
    /// </summary>
    public partial class Sys_TipDBModel : DataTableDBModelBase<Sys_TipDBModel, Sys_TipEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Tip"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_TipEntity entity = new Sys_TipEntity();
                entity.Id = ms.ReadInt();
                entity.Key = ms.ReadUTF8String();
                entity.Content = ms.ReadUTF8String();
                entity.Duration = ms.ReadFloat();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}