using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
	public class UIManager : ManagerBase
	{
		/// <summary>
		/// 已经打开的UI窗口链表
		/// </summary>
		private LinkedList<UIFormBase> m_OpenUIFormList;
		/// <summary>
		/// 反切UI栈
		/// </summary>
		private Stack<UIFormBase> m_ReverseChangeUIStack;
		/// <summary>
		/// 正在加载中的UI窗口
		/// </summary>
		private LinkedList<int> m_LoadingUIFormList;

		private UILayer m_UILayer;

		private Dictionary<byte, UIGroup> m_UIGroupDic;

		private UIPool m_UIPool;

		/// <summary>
		/// UI对象池中最大数量
		/// </summary>
		public int UIPoolMaxCount { get; private set; }
		/// <summary>
		/// UI回池后过期时间_秒
		/// </summary>
		public float UIExpire { get; private set; }
		/// <summary>
		/// UI释放间隔_秒
		/// </summary>
		public float ClearInterval { get; private set; }

		/// <summary>
		/// 下次运行时间
		/// </summary>
		private float m_NextRunTime = 0f;

		/// <summary>
		/// 标准分辨率比值
		/// </summary>
		private float m_StandardScreen = 0;
		/// <summary>
		/// 当前分辨率比值
		/// </summary>
		private float m_CurrScreen = 0;

		public UIManager()
		{
			m_OpenUIFormList = new LinkedList<UIFormBase>();
			m_ReverseChangeUIStack = new Stack<UIFormBase>();
			m_LoadingUIFormList = new LinkedList<int>();

			m_UILayer = new UILayer();
			m_UIGroupDic = new Dictionary<byte, UIGroup>();
			m_UIPool = new UIPool();

		}
		internal void Dispose()
		{
		}
		public override void Init()
		{
			UIPoolMaxCount = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_PoolMaxCount, GameEntry.CurrDeviceGrade);
			UIExpire = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_Expire, GameEntry.CurrDeviceGrade);
			ClearInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_ClearInterval, GameEntry.CurrDeviceGrade);

			m_StandardScreen = GameEntry.Instance.StandardWidth / (float)GameEntry.Instance.StandardHeight;
			m_CurrScreen = Screen.width / (float)Screen.height;

			LoadingFormCanvasScaler();

			for (int i = 0; i < GameEntry.Instance.UIGroups.Length; i++)
			{
				m_UIGroupDic[GameEntry.Instance.UIGroups[i].Id] = GameEntry.Instance.UIGroups[i];
			}
			m_UILayer.Init(GameEntry.Instance.UIGroups);
		}
		public void OnUpdate()
		{
			if (Time.time > m_NextRunTime + ClearInterval)
			{
				m_NextRunTime = Time.time;

				//释放UI对象池
				m_UIPool.CheckClear();
			}
		}

		#region UI适配
		/// <summary>
		/// LoadingForm适配缩放
		/// </summary>
		public void LoadingFormCanvasScaler()
		{
			GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = (m_CurrScreen >= m_StandardScreen) ? 1 : 0;
		}
		/// <summary>
		/// FullForm适配缩放
		/// </summary>
		public void FullFormCanvasScaler()
		{
			GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = 1;
		}
		/// <summary>
		/// NormalForm适配缩放
		/// </summary>
		public void NormalFormCanvasScaler()
		{
			if (m_CurrScreen > m_StandardScreen)
			{
				//设置为0
				GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = 0;
			}
			else
			{
				GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = m_StandardScreen - m_CurrScreen;
			}
		}

		#endregion

		#region GetUIGroup 根据UI分组编号获取UI分组
		/// <summary>
		/// 根据UI分组编号获取UI分组
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public UIGroup GetUIGroup(byte id)
		{
			UIGroup group = null;
			m_UIGroupDic.TryGetValue(id, out group);
			return group;
		}
		#endregion

		#region OpenDialogForm 打开提示窗口
		/// <summary>
		/// 打开提示窗口
		/// </summary>
		public void OpenDialogForm(int sysCode, string title = "提示", DialogFormType dialogFormType = DialogFormType.Noraml, BaseAction okAction = null, BaseAction cancelAction = null)
		{
			OpenDialogForm(GameEntry.Data.SysDataManager.GetSysCodeContent(sysCode), title, dialogFormType, okAction, cancelAction);
		}
		/// <summary>
		/// 打开提示窗口
		/// </summary>
		public void OpenDialogForm(string str, string title = "提示", DialogFormType dialogFormType = DialogFormType.Noraml, BaseAction okAction = null, BaseAction cancelAction = null)
		{
			OpenUIForm(UIFormId.UI_Dialog, onOpen: (UIFormBase uiFormBase) =>
			  {
				  UIDialogForm messageForm = uiFormBase as UIDialogForm;
				  messageForm.SetUI(str, title, dialogFormType, okAction, cancelAction);
			  });
		}
		#endregion

		#region OpenUIForm 打开UI窗口
		public void OpenUIForm(string uiFormName, object userData = null, BaseAction<UIFormBase> onOpen = null)
		{
			OpenUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, onOpen);
		}
		public void OpenUIForm(int uiFormId, object userData = null, BaseAction<UIFormBase> onOpen = null)
		{
			//1,读表
			Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(uiFormId);
			if (sys_UIForm == null) Debug.LogError(uiFormId + "对应的UI窗口不存在");

			if (sys_UIForm.CanMulit == 0 && IsExists(uiFormId))
			{
				Debug.LogError("不重复打开同一个UI窗口");
				return;
			}

			UIFormBase formBase = GameEntry.UI.Dequeue(uiFormId);
			if (formBase == null)
			{
				//异步加载UI需要时间 此处需要处理过滤加载中的UI
				if (IsLoading(uiFormId)) return;
				m_LoadingUIFormList.AddLast(uiFormId);

				string assetPath = string.Empty;
				switch (GameEntry.CurrLanguage)
				{
					case YouYouLanguage.Chinese:
						assetPath = sys_UIForm.AssetPath_Chinese;
						break;
					case YouYouLanguage.English:
						assetPath = sys_UIForm.AssetPath_English;
						break;
				}
				//加载UI资源并克隆
				GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(AssetCategory.UIPrefab, string.Format("Assets/Download/UI/UIPrefab/{0}.prefab", assetPath), (ResourceEntity resourceEntity) =>
				{
					GameObject uiObj = Object.Instantiate((Object)resourceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group) as GameObject;

					//把克隆出来的资源 加入实例资源池
					GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), resourceEntity);


					RectTransform rectTransform = uiObj.GetComponent<RectTransform>();
					rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
					rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
					rectTransform.anchorMin = Vector2.zero;
					rectTransform.anchorMax = Vector2.one;

					//初始化UI
					formBase = uiObj.GetComponent<UIFormBase>();
					formBase.CurrCanvas.overrideSorting = true;
					formBase.Init(uiFormId, sys_UIForm, (byte)sys_UIForm.UIGroupId, sys_UIForm.DisableUILayer == 1, sys_UIForm.IsLock == 1, userData, () =>
					{
						OpenUI(sys_UIForm, formBase, onOpen);
					});
					m_OpenUIFormList.AddLast(formBase);
					m_LoadingUIFormList.Remove(uiFormId);

				});
			}
			else
			{
				formBase.Open(userData);
				m_OpenUIFormList.AddLast(formBase);
				GameEntry.UI.ShowUI(formBase);
				OpenUI(sys_UIForm, formBase, onOpen);
			}

			//检查对象池释放
			m_UIPool.CheckByOpenUI();
		}
		private void OpenUI(Sys_UIFormEntity sys_UIFormEntity, UIFormBase formBase, BaseAction<UIFormBase> onOpen)
		{
			//判断反切UI
			UIFormShowMode uIFormShowMode = (UIFormShowMode)sys_UIFormEntity.ShowMode;
			if (uIFormShowMode == UIFormShowMode.ReverseChange)
			{
				//如果之前栈里面有UI
				if (m_ReverseChangeUIStack.Count > 0)
				{
					//从栈顶上 拿到UI
					UIFormBase topUIForm = m_ReverseChangeUIStack.Peek();

					//禁用 冻结
					GameEntry.UI.HideUI(topUIForm);
				}

				//把自己加入栈
				//Debug.LogError("入栈==" + formBase.gameObject.GetInstanceID());
				m_ReverseChangeUIStack.Push(formBase);
			}

			onOpen?.Invoke(formBase);
		}
		#endregion

		#region CloseUIForm 关闭UI窗口
		public void CloseUIForm(int uiFormId)
		{
			//m_UIManager.CloseUIForm(uiFormId);
			for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.CurrUIFormId == uiFormId)
				{
					CloseUIForm(curr.Value);
					break;
				}
			}
		}
		public void CloseUIForm(UIFormBase formBase)
		{
			m_OpenUIFormList.Remove(formBase);
			formBase.ToClose();

			//判断反切UI
			UIFormShowMode uIFormShowMode = (UIFormShowMode)formBase.SysUIForm.ShowMode;
			if (uIFormShowMode == UIFormShowMode.ReverseChange)
			{
				m_ReverseChangeUIStack.Pop();

				if (m_ReverseChangeUIStack.Count > 0)
				{
					UIFormBase topForms = m_ReverseChangeUIStack.Peek();
					GameEntry.UI.ShowUI(topForms);
				}
			}
		}
		/// <summary>
		/// 关闭UI窗口
		/// </summary>
		/// <param name="uiFormName"></param>
		public void CloseUIForm(string uiFormName)
		{
			CloseUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName));
		}
		/// <summary>
		/// 关闭所有"Default"组的UI窗口
		/// </summary>
		public void CloseAllDefaultUIForm()
		{
			UIFormBase[] uIFormBases = m_UIGroupDic[2].Group.GetComponentsInChildren<UIFormBase>();
			for (int i = 0; i < uIFormBases.Length; i++)
			{
				CloseUIForm(uIFormBases[i]);
			}
		}
		/// <summary>
		/// 根据InstanceID关闭UI
		/// </summary>
		/// <param name="instanceID"></param>
		internal void CloseUIFormByInstanceID(int instanceID)
		{
			for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.gameObject.GetInstanceID() == instanceID)
				{
					CloseUIForm(curr.Value);
					break;
				}
			}
		}
		#endregion


		/// <summary>
		/// 显示/激活一个UI
		/// </summary>
		/// <param name="uIFormBase"></param>
		public void ShowUI(UIFormBase uiFormBase)
		{
			if (uiFormBase.SysUIForm.FreezeMode == 0)
			{
				uiFormBase.IsActive = true;
				uiFormBase.CurrCanvas.enabled = true;
				uiFormBase.gameObject.layer = 5;
			}
			else
			{
				uiFormBase.gameObject.SetActive(true);
			}
			//Debug.LogError("显示 " + uIFormBase.gameObject.GetInstanceID());
		}
		/// <summary>
		/// 隐藏/冻结一个UI
		/// </summary>
		/// <param name="uIFormBase"></param>
		public void HideUI(UIFormBase uiFormBase)
		{
			if (uiFormBase.SysUIForm.FreezeMode == 0)
			{
				uiFormBase.IsActive = false;
				uiFormBase.CurrCanvas.enabled = false;
				uiFormBase.gameObject.layer = 0;
			}
			else
			{
				uiFormBase.gameObject.SetActive(false);
			}
			//Debug.LogError("隐藏 " + uIFormBase.gameObject.GetInstanceID());
		}

		/// <summary>
		/// 设置层级
		/// </summary>
		/// <param name="formBase">窗口</param>
		/// <param name="isAdd">true:增加  false:减少</param>
		internal void SetSortingOrder(UIFormBase formBase, bool isAdd)
		{
			m_UILayer.SetSortingOrder(formBase, isAdd);
		}

		/// <summary>
		/// 从池中获取UI窗口
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		internal UIFormBase Dequeue(int uiFormId)
		{
			return m_UIPool.Dequeue(uiFormId);
		}

		/// <summary>
		/// UI窗口回池
		/// </summary>
		/// <param name="form"></param>
		internal void EnQueue(UIFormBase form)
		{
			m_UIPool.EnQueue(form);
		}

		/// <summary>
		/// 检查UI是否已经打开
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		public bool IsExists(int uiFormId)
		{
			for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.CurrUIFormId == uiFormId)
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 检查UI正在加载中
		/// </summary>
		private bool IsLoading(int uiFormId)
		{
			for (LinkedListNode<int> curr = m_LoadingUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value == uiFormId)
				{
					GameEntry.LogError("UI正在加载中, 打开的频率过高");
					return true;
				}
			}
			return false;
		}


	}
}