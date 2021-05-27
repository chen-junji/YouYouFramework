using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// 数据组件
	/// </summary>
	public class DataManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// 系统相关数据
		/// </summary>
		public SysDataManager SysDataManager { get; private set; }

		/// <summary>
		/// 用户数据
		/// </summary>
		public UserDataManager UserDataManager { get; private set; }

		public RoleDataManager RoleDataManager { get; private set; }
		private float m_NextRunTime = 0f;


		internal DataManager()
		{
			SysDataManager = new SysDataManager();
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
			SysDataManager.Dispose();
			UserDataManager.Dispose();
			RoleDataManager.Dispose();
		}

		internal override void Init()
		{
		}
	}
}