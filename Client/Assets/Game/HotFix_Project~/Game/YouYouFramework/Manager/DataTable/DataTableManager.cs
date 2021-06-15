using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouYou;

namespace Hotfix
{
    public class DataTableManager
    {
        internal void Init()
        {
            InitDBModel();
            m_TaskGroup = GameEntry.Task.CreateTaskGroup();

            LoadDataTable();
        }

        private TaskGroup m_TaskGroup;

        public TestDBModel TestDBModel { get; private set; }
        internal void InitDBModel()
        {
            TestDBModel = new TestDBModel();
        }
        /// <summary>
        /// 加载表格
        /// </summary>
        private void LoadDataTable()
        {
            TestDBModel.LoadData(m_TaskGroup);

            m_TaskGroup.OnComplete = GameEntry.ILRuntime.OnLoadDataTableComplete;
            m_TaskGroup.Run(true);
        }
    }
}
