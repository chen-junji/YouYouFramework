using YouYouMain;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;


namespace YouYouFramework
{
    public class DataTableManager
    {
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; } = new();
        public Sys_BGMDBModel Sys_BGMDBModel { get; private set; } = new();
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; } = new();
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; } = new();
        public Sys_DialogDBModel Sys_DialogDBModel { get; private set; } = new();
        public Sys_TipDBModel Sys_TipDBModel { get; private set; } = new();


        /// <summary>
        /// 加载表格
        /// </summary>
        internal async UniTask LoadDataAllTable()
        {
            await Sys_UIFormDBModel.LoadData();
            await Sys_AudioDBModel.LoadData();
            await Sys_BGMDBModel.LoadData();
            await Sys_SceneDBModel.LoadData();
            await Sys_DialogDBModel.LoadData();
            await Sys_TipDBModel.LoadData();
        }

        /// <summary>
        /// 获取表格的字节数组
        /// </summary>
        public async UniTask<byte[]> GetDataTableBuffer(string dataTableName)
        {
            var referenceEntity = Addressables.LoadAssetAsync<TextAsset>($"Assets/Game/Download/DataTable/{dataTableName}.bytes");
            TextAsset asset = await referenceEntity.Task;
            return asset.bytes;
        }
    }
}