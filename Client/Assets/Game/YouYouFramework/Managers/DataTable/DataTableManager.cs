using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class DataTableManager 
    {
        internal Action OnLoadDataTableComplete;
        internal void Init()
        {
        }


        public LocalizationDBModel LocalizationDBModel { get; private set; }
        public Sys_PrefabDBModel Sys_PrefabDBModel { get; private set; }
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }


        /// <summary>
        /// 加载表格
        /// </summary>
        private void LoadDataTable()
        {
            TaskGroup m_TaskGroup = GameEntry.Task.CreateTaskGroup();
            LocalizationDBModel = new LocalizationDBModel();
            LocalizationDBModel.LoadData(m_TaskGroup);
            Sys_PrefabDBModel = new Sys_PrefabDBModel();
            Sys_PrefabDBModel.LoadData(m_TaskGroup);
            Sys_UIFormDBModel = new Sys_UIFormDBModel();
            Sys_UIFormDBModel.LoadData(m_TaskGroup);
            Sys_SceneDBModel = new Sys_SceneDBModel();
            Sys_SceneDBModel.LoadData(m_TaskGroup);
            Sys_AudioDBModel = new Sys_AudioDBModel();
            Sys_AudioDBModel.LoadData(m_TaskGroup);

            m_TaskGroup.OnComplete = OnLoadDataTableComplete;
            m_TaskGroup.Run(true);
        }

        /// <summary>
        /// 表格资源包
        /// </summary>
        private AssetBundle m_DataTableBundle;

        /// <summary>
        /// 加载表格
        /// </summary>
        internal void LoadDataAllTable(Action onComplete = null)
        {
            OnLoadDataTableComplete = onComplete;
#if ASSETBUNDLE
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundleAsync(YFConstDefine.DataTableAssetBundlePath, onComplete: (AssetBundle bundle) =>
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
        public void GetDataTableBuffer(string dataTableName, Action<byte[]> onComplete)
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
            GameEntry.Resource.ResourceLoaderManager.LoadAssetAsync(GameEntry.Resource.GetLastPathName(dataTableName), m_DataTableBundle, onComplete: (UnityEngine.Object obj, bool isNew) =>
            {
                if (obj == null) return;
                TextAsset asset = obj as TextAsset;
                if (onComplete != null) onComplete(asset.bytes);
            });
#endif
        }
    }
}