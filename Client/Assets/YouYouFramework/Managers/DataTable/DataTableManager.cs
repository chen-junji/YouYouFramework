using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class DataTableManager : ManagerBase, IDisposable
	{
		internal DataTableManager()
		{
			AlreadyLoadTable = new Dictionary<string, ushort>();
		}

		internal override void Init()
		{
			InitDBModel();
		}

		/// <summary>
		/// 已经在c#加载的表格
		/// </summary>
		public Dictionary<string, ushort> AlreadyLoadTable
		{
			get;
			private set;
		}

		/// <summary>
		/// 添加到已加载字典
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="version"></param>
		public void AddToAlreadyLoadTable(string tableName, ushort version)
		{
			AlreadyLoadTable[tableName] = version;
		}

		/// <summary>
		/// 根据表格名称和版本号检查是否已经在c#加载
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public bool CheckAlreadyLoadTable(string tableName, ushort version)
		{
			ushort ver = 0;
			if (AlreadyLoadTable.TryGetValue(tableName, out ver))
			{
				if (ver == version)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 总共需要加载的表格数量
		/// </summary>
		internal int TotalTableCount = 0;

		/// <summary>
		/// 当前加载的表格数量
		/// </summary>
		internal int CurrLoadTableCount = 0;


		public Sys_CodeDBModel Sys_CodeDBModel { get; private set; }
		public LocalizationDBModel LocalizationDBModel { get; private set; }
		public Sys_PrefabDBModel Sys_PrefabDBModel { get; private set; }
		public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
		public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
		public Sys_SceneDetailDBModel Sys_SceneDetailDBModel { get; private set; }
		public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }


		internal void InitDBModel()
		{
			Sys_CodeDBModel = new Sys_CodeDBModel();
			LocalizationDBModel = new LocalizationDBModel();
			Sys_PrefabDBModel = new Sys_PrefabDBModel();
			Sys_UIFormDBModel = new Sys_UIFormDBModel();
			Sys_SceneDBModel = new Sys_SceneDBModel();
			Sys_SceneDetailDBModel = new Sys_SceneDetailDBModel();
			Sys_AudioDBModel = new Sys_AudioDBModel();
		}
		/// <summary>
		/// 加载表格
		/// </summary>
		private void LoadDataTable()
		{
			Sys_CodeDBModel.LoadData();
			LocalizationDBModel.LoadData();
			Sys_PrefabDBModel.LoadData();
			Sys_UIFormDBModel.LoadData();
			Sys_SceneDBModel.LoadData();
			Sys_SceneDetailDBModel.LoadData();
			Sys_AudioDBModel.LoadData();
		}

		/// <summary>
		/// 表格资源包
		/// </summary>
		private AssetBundle m_DataTableBundle;

		/// <summary>
		/// 加载表格
		/// </summary>
		internal void LoadDataAllTable()
		{
#if ASSETBUNDLE
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.DataTableAssetBundlePath, onComplete: (AssetBundle bundle) =>
            {
                m_DataTableBundle = bundle;
                LoadDataTable();
            });
#else
			LoadDataTable();
#endif
		}

		/// <summary>
		/// 获取表格的字节数组
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		internal void GetDataTableBuffer(string dataTableName, Action<byte[]> onComplete)
		{
#if EDITORLOAD
			GameEntry.Time.Yield(() =>
			{
				byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/Download/DataTable/{1}.bytes", GameEntry.Resource.LocalFilePath, dataTableName));
				if (onComplete != null) onComplete(buffer);
			});
#elif RESOURCES
			GameEntry.Time.Yield(() =>
			{
				TextAsset asset = Resources.Load<TextAsset>(string.Format("DataTable/{0}", dataTableName));
				if (onComplete != null) onComplete(asset.bytes);
			});
#else
            GameEntry.Resource.ResourceLoaderManager.LoadAsset(GameEntry.Resource.GetLastPathName(dataTableName), m_DataTableBundle, onComplete: (UnityEngine.Object obj) =>
            {
				if (obj == null) return;
                TextAsset asset = obj as TextAsset;
                if (onComplete != null) onComplete(asset.bytes);
            });
#endif
		}

		public void Dispose()
		{
			Sys_CodeDBModel.Clear();
			LocalizationDBModel.Clear();
			Sys_PrefabDBModel.Clear();
			Sys_UIFormDBModel.Clear();
			Sys_SceneDBModel.Clear();
			Sys_SceneDetailDBModel.Clear();
		}
	}
}