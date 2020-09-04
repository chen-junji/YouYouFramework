using System;
using YouYou.DataTable;

namespace YouYouServer.Common.Managers
{
    /// <summary>
    /// 数据表管理器
    /// </summary>
    public sealed class DataTableManager
    {
        public static ChapterList ChapterList;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            //每个表都加载数据
            ChapterList.LoadData();
            Console.WriteLine("LoadDataTable Complete");
        }
    }
}