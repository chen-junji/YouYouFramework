
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Sys_Code数据管理
/// </summary>
public partial class Sys_CodeDBModel : DataTableDBModelBase<Sys_CodeDBModel, Sys_CodeEntity>
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public override string DataTableName { get { return "Sys_Code"; } }

    /// <summary>
    /// 加载列表
    /// </summary>
    protected override void LoadList(MMO_MemoryStream ms)
    {
        int rows = ms.ReadInt();
        int columns = ms.ReadInt();

        for (int i = 0; i < rows; i++)
        {
            Sys_CodeEntity entity = new Sys_CodeEntity();
            entity.Id = ms.ReadInt();
            entity.Desc = ms.ReadUTF8String();
            entity.Name = ms.ReadUTF8String();

            m_List.Add(entity);
            m_Dic[entity.Id] = entity;
        }
    }
}