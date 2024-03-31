using YouYouMain;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    public class DataTableManager
    {
        internal void Init()
        {
        }


        public LocalizationDBModel LocalizationDBModel { get; private set; }
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
        public Sys_PrefabDBModel Sys_PrefabDBModel { get; private set; }
        public Sys_BGMDBModel Sys_BGMDBModel { get; private set; }
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }


        /// <summary>
        /// 加载表格
        /// </summary>
        private void LoadDataTable()
        {
            //LocalizationDBModel不需要在这里LoadData, 而是在LocalizationManager里面LoadData
            LocalizationDBModel = new LocalizationDBModel();

            Sys_UIFormDBModel = new Sys_UIFormDBModel();
            Sys_UIFormDBModel.LoadData();

            Sys_PrefabDBModel = new Sys_PrefabDBModel();
            Sys_PrefabDBModel.LoadData();

            Sys_AudioDBModel = new Sys_AudioDBModel();
            Sys_AudioDBModel.LoadData();

            Sys_BGMDBModel = new Sys_BGMDBModel();
            Sys_BGMDBModel.LoadData();

            Sys_SceneDBModel = new Sys_SceneDBModel();
            Sys_SceneDBModel.LoadData();
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
            if (MainEntry.IsAssetBundleMode)
            {
                m_DataTableBundle = GameEntry.Loader.LoadAssetBundle(YFConstDefine.DataTableAssetBundlePath);
            }
            LoadDataTable();
        }

        /// <summary>
        /// 获取表格的字节数组
        /// </summary>
        public byte[] GetDataTableBuffer(string dataTableName)
        {
            if (MainEntry.IsAssetBundleMode)
            {
                AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadAsset(GameUtil.GetLastPathName(dataTableName), m_DataTableBundle);
                if (referenceEntity == null) return null;
                TextAsset asset = referenceEntity.Target as TextAsset;
                return asset.bytes;
            }
            else
            {
                return IOUtil.GetFileBuffer(string.Format("{0}/Game/Download/DataTable/{1}.bytes", Application.dataPath, dataTableName));
            }
        }
    }
}