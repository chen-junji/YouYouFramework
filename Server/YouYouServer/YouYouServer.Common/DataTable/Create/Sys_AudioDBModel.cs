
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Sys_Audio数据管理
/// </summary>
public partial class Sys_AudioDBModel : DataTableDBModelBase<Sys_AudioDBModel, Sys_AudioEntity>
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public override string DataTableName { get { return "Sys_Audio"; } }

    /// <summary>
    /// 加载列表
    /// </summary>
    protected override void LoadList(MMO_MemoryStream ms)
    {
        int rows = ms.ReadInt();
        int columns = ms.ReadInt();

        for (int i = 0; i < rows; i++)
        {
            Sys_AudioEntity entity = new Sys_AudioEntity();
            entity.Id = ms.ReadInt();
            entity.Desc = ms.ReadUTF8String();
            entity.AssetPath = ms.ReadUTF8String();
            entity.Suffix = ms.ReadUTF8String();
            entity.Volume = ms.ReadFloat();
            entity.IsLoop = (byte)ms.ReadByte();
            entity.IsFadeIn = (byte)ms.ReadByte();
            entity.IsFadeOut = (byte)ms.ReadByte();
            entity.Priority = (byte)ms.ReadByte();

            m_List.Add(entity);
            m_Dic[entity.Id] = entity;
        }
    }
}