
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYouFramework
{
    /// <summary>
    /// Sys_Dialog数据管理
    /// </summary>
    public partial class Sys_DialogDBModel : DataTableDBModelBase<Sys_DialogDBModel, Sys_DialogEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Dialog"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_DialogEntity entity = new Sys_DialogEntity();
                entity.Id = ms.ReadInt();
                entity.Key = ms.ReadUTF8String();
                entity.Title = ms.ReadUTF8String();
                entity.Content = ms.ReadUTF8String();
                entity.BtnText1 = ms.ReadUTF8String();
                entity.BtnText2 = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}