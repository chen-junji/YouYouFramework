
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hotfix
{
    /// <summary>
    /// Sys_Prefab数据管理
    /// </summary>
    public partial class Sys_PrefabDBModel : DataTableDBModelBase<Sys_PrefabDBModel, Sys_PrefabEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Prefab"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_PrefabEntity entity = new Sys_PrefabEntity();
                entity.Id = ms.ReadInt();
                entity.AssetPath = ms.ReadUTF8String();
                entity.PoolId = (byte)ms.ReadByte();
                entity.CullDespawned = (byte)ms.ReadByte();
                entity.CullAbove = ms.ReadInt();
                entity.CullDelay = ms.ReadInt();
                entity.CullMaxPerPass = ms.ReadInt();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}