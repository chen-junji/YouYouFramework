using YouYouMain;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    public class DataTableManager
    {
        public LocalizationDBModel LocalizationDBModel { get; private set; } = new();
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; } = new();
        public Sys_BGMDBModel Sys_BGMDBModel { get; private set; } = new();
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; } = new();
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; } = new();
        public Sys_DialogDBModel Sys_DialogDBModel { get; private set; } = new();
        public Sys_TipDBModel Sys_TipDBModel { get; private set; } = new();


        /// <summary>
        /// 加载表格
        /// </summary>
        internal void LoadDataAllTable()
        {
            Sys_UIFormDBModel.LoadData();
            Sys_AudioDBModel.LoadData();
            Sys_BGMDBModel.LoadData();
            Sys_SceneDBModel.LoadData();
            Sys_DialogDBModel.LoadData();
            Sys_TipDBModel.LoadData();
        }

        /// <summary>
        /// 获取表格的字节数组
        /// </summary>
        public byte[] GetDataTableBuffer(string dataTableName)
        {
            if (MainEntry.IsAssetBundleMode)
            {
                AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset($"Assets/Game/Download/DataTable/{dataTableName}.bytes");
                if (referenceEntity == null) return null;
                TextAsset asset = referenceEntity.Target as TextAsset;
                return asset.bytes;
            }
            else
            {
                return IOUtil.GetFileBuffer(string.Format("{0}/Game/Download/DataTable/{1}.bytes", MainConstDefine.EditorAssetBundlePath, dataTableName));
            }
        }
    }
}