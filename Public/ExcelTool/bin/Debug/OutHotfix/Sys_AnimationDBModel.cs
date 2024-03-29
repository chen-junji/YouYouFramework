
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hotfix
{
    /// <summary>
    /// Sys_Animation数据管理
    /// </summary>
    public partial class Sys_AnimationDBModel : DataTableDBModelBase<Sys_AnimationDBModel, Sys_AnimationEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Animation"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_AnimationEntity entity = new Sys_AnimationEntity();
                entity.Id = ms.ReadInt();
                entity.Desc = ms.ReadUTF8String();
                entity.GroupId = ms.ReadInt();
                entity.AnimPath = ms.ReadUTF8String();
                entity.InitLoad = (byte)ms.ReadByte();
                entity.Expire = ms.ReadInt();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}