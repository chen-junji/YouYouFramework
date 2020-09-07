using System;

namespace YouYouServer.Common
{
	/// <summary>
	/// 数据表管理器
	/// </summary>
	public sealed class DataTableManager
    {
		public static Sys_CodeDBModel Sys_CodeDBModel { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
			//每个表都加载数据
			Sys_CodeDBModel = new Sys_CodeDBModel();
			Sys_CodeDBModel.LoadData();

			Console.WriteLine("LoadDataTable Complete");
        }
    }
}