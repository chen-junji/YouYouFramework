using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace YouYou
{
    /// <summary>
    /// Lua管理器
    /// </summary>
    public class LuaManager : IDisposable
    {
        /// <summary>
        /// 全局的xLua引擎
        /// </summary>
        public static LuaEnv luaEnv;

        /// <summary>
        /// Lua中可释放表数据的生命周期
        /// </summary>
        public int LuaDataTableLife
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否打印日志
        /// </summary>
        public bool DebugLog
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否打印协议日志
        /// </summary>
        public bool DebugLogProto
        {
            get;
            private set;
        }

		public Action OnLoadDataTableComplete;
        public void Dispose()
        {

        }
        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {
#if DEBUG_MODEL
            DebugLog = true;
#endif
#if DEBUG_LOG_PROTO && DEBUG_MODEL
            DebugLogProto = true;
#endif
            LuaDataTableLife = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Lua_DataTableLife, GameEntry.CurrDeviceGrade);

            //1. 实例化 xLua引擎
            luaEnv = new LuaEnv();

            // 重要：初始化LuaCSharpArr
            LuaArrAccessAPI.RegisterPinFunc(luaEnv.L);

#if EDITORLOAD
            //2.设置xLua的脚本路径
            luaEnv.DoString(string.Format("package.path = '{0}/?.bytes'", Application.dataPath + "/Download/xLuaLogic/"));
            DoString("require 'Main'");
#else
			//1.添加自定义Loader
			luaEnv.AddLoader(MyLoader);

			//2.加载Bundle
			LoadLuaAssetBundle();
#endif
        }

        /// <summary>
        /// 当前xLua脚本的资源包
        /// </summary>
        private AssetBundle m_CurrAssetBundle;

        /// <summary>
        /// 加载xlua的AssetBundle
        /// </summary>
        private void LoadLuaAssetBundle()
        {
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.XLuaAssetBundlePath, onComplete: (AssetBundle bundle) =>
            {
                m_CurrAssetBundle = bundle;
                DoString("require 'Main'");
            });
        }

        /// <summary>
        /// 自定义的Loader
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private byte[] MyLoader(ref string filePath)
        {
            string path = GameEntry.Resource.GetLastPathName(filePath);
            TextAsset asset = m_CurrAssetBundle.LoadAsset<TextAsset>(path);
            byte[] buffer = asset.bytes;
            if (buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
            {
                // 处理UTF-8 BOM头
                buffer[0] = buffer[1] = buffer[2] = 32;
            }
            return buffer;
        }

        /// <summary>
        /// 执行Lua脚本
        /// </summary>
        /// <param name="str"></param>
        public void DoString(string str)
        {
            luaEnv.DoString(str);
        }

        /// <summary>
        /// 发送Http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callBack"></param>
        /// <param name="luaTable"></param>
        public void SendHttpData(string url, HttpSendDataCallBack callBack, LuaTable luaTable)
        {
            Dictionary<string, object> dic = GameEntry.Pool.DequeueClassObject<Dictionary<string, object>>();
            dic.Clear();

            IEnumerator enumerator = luaTable.GetKeys().GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = enumerator.Current.ToString();
                dic[key] = luaTable.GetInPath<string>(key);
            }

            GameEntry.Http.PostArgs(url, dic.ToJson(), false, callBack);
        }

        /// <summary>
        /// 获取PB文件字节
        /// </summary>
        /// <param name="pbName">PB文件名</param>
        /// <returns></returns>
        public void GetPBBuffer(string pbName, Action<byte[]> onComplete)
        {
#if EDITORLOAD
            byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/download/xLuaLogic/PB/{1}.bytes", GameEntry.Resource.LocalFilePath, pbName));
            onComplete?.Invoke(buffer);
#else
			GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(string.Format("Assets/Download/xLuaLogic/PB/{0}.bytes", pbName), onComplete: (TextAsset res) =>
			{
				onComplete?.Invoke(res.bytes);
			});
#endif
        }

        public void LoadDataTable(string DataTableName, Action<MMO_MemoryStream> onComplete)
        {
            GameEntry.DataTable.GetDataTableBuffer(DataTableName, (byte[] buffer) =>
            {
                using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
                {
                    onComplete?.Invoke(ms);
                }
            });
        }
    }
}