
//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：2020-09-04 02:35:23
//备    注：此代码为工具生成 请勿手工修改
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Job数据管理
/// </summary>
public partial class JobDBModel : DataTableDBModelBase<JobDBModel, JobEntity>
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public override string DataTableName { get { return "Job"; } }

    /// <summary>
    /// 加载列表
    /// </summary>
    protected override void LoadList(MMO_MemoryStream ms)
    {
        int rows = ms.ReadInt();
        int columns = ms.ReadInt();

        for (int i = 0; i < rows; i++)
        {
            JobEntity entity = new JobEntity();
            entity.Id = ms.ReadInt();
            entity.SkinId = ms.ReadInt();

            m_List.Add(entity);
            m_Dic[entity.Id] = entity;
        }
    }
}