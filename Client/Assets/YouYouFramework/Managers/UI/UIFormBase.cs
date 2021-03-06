using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
	[RequireComponent(typeof(Canvas))]//脚本依赖
	[RequireComponent(typeof(GraphicRaycaster))]//脚本依赖
	public class UIFormBase : MonoBehaviour
	{
		/// <summary>
		/// 是否活跃
		/// </summary>
		protected internal bool IsActive;

		public Sys_UIFormEntity SysUIForm { get; private set; }

		/// <summary>
		/// 当前画布
		/// </summary>
		public Canvas CurrCanvas
		{
			get;
			private set;
		}

		/// <summary>
		/// 关闭时间
		/// </summary>
		public float CloseTime
		{
			get;
			private set;
		}

		/// <summary>
		/// 用户数据
		/// </summary>
		public object UserData
		{
			get;
			private set;
		}

		private BaseAction m_InitComplate;

		void Awake()
		{
			if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
			CurrCanvas = GetComponent<Canvas>();
		}

		internal void Init(Sys_UIFormEntity sysUIForm, object userData, BaseAction initComplate)
		{
			SysUIForm = sysUIForm;
			UserData = userData;
			m_InitComplate = initComplate;
		}

		void Start()
		{
			OnInit(UserData);
			m_InitComplate?.Invoke();
			Open(UserData, true);
		}

		internal void Open(object userData, bool isFormInit = false)
		{
			//GameEntry.Audio.PlayAudio(YFConstDefine.Audio_UIOpen);
			if (!isFormInit)
			{
				UserData = userData;
			}


			if (SysUIForm != null && SysUIForm.DisableUILayer != 1)
			{
				//进行层级管理 增加层级
				GameEntry.UI.SetSortingOrder(this, true);
			}
			OnOpen(UserData);
		}

		public void Close()
		{
			GameEntry.UI.CloseUIForm(this);
		}


		internal void ToClose()
		{
			//GameEntry.Audio.PlayAudio(YFConstDefine.Audio_UIClose);
			if (SysUIForm != null && SysUIForm.DisableUILayer != 1)
			{
				//进行层级管理 减少层级
				GameEntry.UI.SetSortingOrder(this, false);
			}

			OnClose();

			CloseTime = Time.time;
			GameEntry.UI.EnQueue(this);
		}

		void OnDestroy()
		{
			OnBeforDestroy();
		}

		protected virtual void OnInit(object userData) { }
		protected virtual void OnOpen(object userData) { }
		protected virtual void OnClose() { }
		protected virtual void OnBeforDestroy() { }

	}
}