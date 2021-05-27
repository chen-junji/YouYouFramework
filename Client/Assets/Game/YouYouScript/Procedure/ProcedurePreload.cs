using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// 预加载流程
	/// </summary>
	public class ProcedurePreload : ProcedureBase
	{
		/// <summary>
		/// 目标进度
		/// </summary>
		private float m_TargetProgress;
		/// <summary>
		/// 当前进度
		/// </summary>
		private float m_CurrProgress;

		/// <summary>
		/// 预加载参数
		/// </summary>
		private BaseParams m_PreloadParams;

		internal override void OnEnter()
		{
			base.OnEnter();
			//监听数据表加载
			GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.LoadOneDataTableComplete, OnLoadOneDataTableComplete);
			GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.LoadDataTableComplete, OnLoadDataTableComplete);
			GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.LoadLuaDataTableComplete, OnLoadLuaDataTableComplete);

			m_PreloadParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
			m_PreloadParams.Reset();
			GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadBegin);

			m_CurrProgress = 0;
			m_TargetProgress = 85;

#if ASSETBUNDLE
			GameEntry.Resource.InitAssetInfo(() =>
			{
				GameEntry.DataTable.LoadDataAllTable();
			});
#else
			GameEntry.DataTable.LoadDataAllTable();
#endif
		}
		internal override void OnUpdate()
		{
			base.OnUpdate();

			if (m_LoadDataTableStatus == 1)
			{
				m_LoadDataTableStatus = 2;
				LoadAudio();
			}

			//加载进度(模拟)
			if (m_CurrProgress < m_TargetProgress)
			{
				m_CurrProgress = Mathf.Min(m_CurrProgress + Time.deltaTime * 100, m_TargetProgress);//根据实际情况调节速度
				m_PreloadParams.FloatParam1 = m_CurrProgress;
				GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadUpdate, m_PreloadParams);
			}

			if (m_TargetProgress == 100)
			{
				m_CurrProgress = 100;
				m_PreloadParams.FloatParam1 = m_CurrProgress;

				GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadUpdate, m_PreloadParams);

				GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadComplete);
				GameEntry.Pool.EnqueueClassObject(m_PreloadParams);

				//进入到业务流程
				GameEntry.Procedure.ChangeState(ProcedureState.Login);
			}
		}
		internal override void OnLeave()
		{
			base.OnLeave();
			GameEntry.Event.CommonEvent.RemoveEventListener(CommonEventId.LoadOneDataTableComplete, OnLoadOneDataTableComplete);
			GameEntry.Event.CommonEvent.RemoveEventListener(CommonEventId.LoadDataTableComplete, OnLoadDataTableComplete);
			GameEntry.Event.CommonEvent.RemoveEventListener(CommonEventId.LoadLuaDataTableComplete, OnLoadLuaDataTableComplete);
		}

		private void OnLoadOneDataTableComplete(object userData)
		{
			//Debug.Log("数据表单个加载完毕, TabName = " + userData);

			GameEntry.DataTable.CurrLoadTableCount++;
			if (GameEntry.DataTable.CurrLoadTableCount == GameEntry.DataTable.TotalTableCount)
			{
				GameEntry.Event.CommonEvent.Dispatch(CommonEventId.LoadDataTableComplete);
			}
		}
		/// <summary>
		/// 加载表格状态0=未加载 1=加载完毕
		/// </summary>
		byte m_LoadDataTableStatus = 0;
		private void OnLoadDataTableComplete(object userData)
		{
			GameEntry.Log(LogCategory.Normal, "加载所有C#表格完毕)");
			m_LoadDataTableStatus = 1;
		}
		private void OnLoadLuaDataTableComplete(object userData)
		{
			GameEntry.Log(LogCategory.Normal, "加载所有lua表格完毕");
			LoadShader();
		}

		/// <summary>
		/// 加载声音
		/// </summary>
		private void LoadAudio()
		{
			GameEntry.Audio.LoadBanks(() =>
			{
#if RESOURCES
				m_TargetProgress = 100;
#else
				//初始化Xlua
				GameEntry.Lua.Init();
#endif

			});
		}

		/// <summary>
		/// 加载自定义Shader
		/// </summary>
		private void LoadShader()
		{
#if ASSETBUNDLE
			GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.CusShadersAssetBundlePath, onComplete: (ResourceEntity bundleEntity) =>
			{
				AssetBundle bundle = bundleEntity.Target as AssetBundle;
				bundle.LoadAllAssets();
				Shader.WarmupAllShaders();
				GameEntry.Log(LogCategory.Normal, "加载资源包中的自定义Shader完毕");
				InitCircle();
			});
#else
			InitCircle();
#endif
		}
		private void InitCircle()
		{
			GameEntry.UI.OpenUIForm(UIFormId.UICircle,onOpen:(UIFormBase uiform)=>
			{
				uiform.Close();
			m_TargetProgress = 100;
			});
		}
	}
}