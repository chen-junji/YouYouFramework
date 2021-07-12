using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 数据组件
    /// </summary>
    public class DataManager : IDisposable
    {
        public SysDataManager SysDataManager { get; private set; }

        public PlayerPrefsManager PlayerPrefs { get; private set; }

        internal DataManager()
        {
            SysDataManager = new SysDataManager();
            PlayerPrefs = new PlayerPrefsManager();

            PlayerPrefs.Init();
        }
        public void OnUpdate()
        {
        }
        public void Dispose()
        {
            SysDataManager.Dispose();
            PlayerPrefs.Dispose();
        }
    }
}