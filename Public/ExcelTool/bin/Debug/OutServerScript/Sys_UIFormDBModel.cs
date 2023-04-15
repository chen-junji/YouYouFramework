
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Sys_UIForm数据管理
/// </summary>
public partial class Sys_UIFormDBModel : DataTableDBModelBase<Sys_UIFormDBModel, Sys_UIFormEntity>
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public override string DataTableName { get { return "Sys_UIForm"; } }

    /// <summary>
    /// 加载列表
    /// </summary>
    protected override void LoadList(MMO_MemoryStream ms)
    {
        int rows = ms.ReadInt();
        int columns = ms.ReadInt();

        for (int i = 0; i < rows; i++)
        {
            Sys_UIFormEntity entity = new Sys_UIFormEntity();
            entity.Id = ms.ReadInt();
            entity.UIGroupId = (byte)ms.ReadByte();
            entity.AssetPath_Chinese = ms.ReadUTF8String();
            entity.AssetPath_English = ms.ReadUTF8String();
            entity.DisableUILayer = ms.ReadInt();
            entity.IsLock = ms.ReadInt();
            entity.CanMulit = ms.ReadInt();
            entity.ShowMode = (byte)ms.ReadByte();

            m_List.Add(entity);
            m_Dic[entity.Id] = entity;
        }
    }
}