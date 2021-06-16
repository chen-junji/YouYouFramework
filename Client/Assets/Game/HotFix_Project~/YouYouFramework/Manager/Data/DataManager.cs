using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hotfix
{
    /// <summary>
    /// 数据组件
    /// </summary>
    public class DataManager
    {
        public PlayerPrefsManager PlayerPrefsManager { get; private set; }
        public UserDataManager UserDataManager { get; private set; }
        public RoleDataManager RoleDataManager { get; private set; }
        private float m_NextRunTime = 0f;



        internal DataManager()
        {
            PlayerPrefsManager = new PlayerPrefsManager();
            UserDataManager = new UserDataManager();
            RoleDataManager = new RoleDataManager();
        }
        public void OnUpdate()
        {
            if (Time.time > m_NextRunTime + 30)
            {
                m_NextRunTime = Time.time;
                RoleDataManager.CheckUnloadRoleAnimation();
            }
        }

        public void Dispose()
        {
            PlayerPrefsManager.Dispose();
            RoleDataManager.Dispose();
        }
    }
}