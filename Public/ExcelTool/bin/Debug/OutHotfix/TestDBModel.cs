
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hotfix
{
    /// <summary>
    /// Test数据管理
    /// </summary>
    public partial class TestDBModel : DataTableDBModelBase<TestDBModel, TestEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Test"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                TestEntity entity = new TestEntity();
                entity.Id = ms.ReadInt();
                entity.Desc = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}