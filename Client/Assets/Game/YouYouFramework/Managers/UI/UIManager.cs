using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Object = UnityEngine.Object;

namespace YouYou
{
    public class UIManager
    {
        /// <summary>
        /// 已经打开的UI窗口链表
        /// </summary>
        private LinkedList<UIBase> m_OpenUIFormList;
        /// <summary>
        /// 反切链表
        /// </summary>
        private LinkedList<UIBase> m_ReverseChangeUIList;

        internal UILayer UILayer;

        private Dictionary<byte, UIGroup> m_UIGroupDic;

        internal UIPool UIPool;

        /// <summary>
        /// 标准分辨率比值
        /// </summary>
        public float StandardScreen { get; private set; }
        /// <summary>
        /// 当前分辨率比值
        /// </summary>
        public float CurrScreen { get; private set; }

        internal UIManager()
        {
            m_OpenUIFormList = new LinkedList<UIBase>();
            m_ReverseChangeUIList = new LinkedList<UIBase>();

            UILayer = new UILayer();
            m_UIGroupDic = new Dictionary<byte, UIGroup>();
            UIPool = new UIPool();

        }
        internal void Init()
        {
            StandardScreen = GameEntry.Instance.UIRootCanvasScaler.referenceResolution.x / GameEntry.Instance.UIRootCanvasScaler.referenceResolution.y;

            for (int i = 0; i < GameEntry.Instance.UIGroups.Length; i++)
            {
                m_UIGroupDic[GameEntry.Instance.UIGroups[i].Id] = GameEntry.Instance.UIGroups[i];
            }
            UILayer.Init(GameEntry.Instance.UIGroups);
        }
        internal void OnUpdate()
        {
            //分辨率适配
            float value = Screen.width / (float)Screen.height;
            if (Mathf.Abs(CurrScreen - value) > 0.1f)
            {
                CurrScreen = value;
            }
            UIPool.OnUpdate();
        }

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

        #region OpenUIForm 打开UI窗口
        public async ETTask<T> OpenUIFormAsync<T>(string uiFormName, object userData = null) where T : UIBase
        {
            ETTask<T> task = ETTask<T>.Create();
            OpenUIFormAction<T>(uiFormName, userData, task.SetResult);
            return await task;
        }
        public void OpenUIFormAction<T>(string uiFormName, object userData = null, Action<T> onComplete = null) where T : UIBase
        {
            OpenUIFormAction(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, onComplete);
        }
        public void OpenUIFormAction<T>(int uiFormId, object userData = null, Action<T> onComplete = null) where T : UIBase
        {
            //1,读表
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(uiFormId);
            if (sys_UIForm == null) return;
            if (sys_UIForm.CanMulit == 0 && IsExists(uiFormId))
            {
                YouYou.GameEntry.LogError("不重复打开同一个UI窗口==" + uiFormId + "  " + sys_UIForm.AssetPath_Chinese);
                onComplete(null);
                return;
            }

            //从对象池里面取
            UIBase formBase = UIPool.Dequeue(uiFormId);
            if (formBase != null)
            {
                m_OpenUIFormList.AddLast(formBase);
                onComplete?.Invoke(formBase as T);
                CheckReverseChange(sys_UIForm, formBase, true);

                if (formBase.IsStart) formBase.Open(userData);
                return;
            }

            //对象池没有, 克隆新的
            GameEntry.Task.AddTaskCommon(async (taskRoutine) =>
            {
                ResourceEntity resourceEntity = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync(sys_UIForm.AssetFullName);
                taskRoutine.Leave();

                GameObject uiObj = Object.Instantiate((GameObject)resourceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group);
                GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), resourceEntity);

                //初始化UI
                formBase = uiObj.GetComponent<UIBase>();
                if (formBase == null)
                {
                    GameEntry.LogError("该UI界面没有挂载UIBase脚本==" + uiObj);
                    formBase = uiObj.AddComponent<UIBase>();
                }
                formBase.CurrCanvas.overrideSorting = true;
                m_OpenUIFormList.AddLast(formBase);

                onComplete?.Invoke(formBase as T);
                CheckReverseChange(sys_UIForm, formBase, true);

                formBase.Init(sys_UIForm, userData);
                if (formBase.IsStart) //必须调用Start后才会触发OnShow, 否则有问题
                    formBase.OnShow();
            }, sys_UIForm.LoadType != 0);
        }

        public void OpenUIForm(string uiFormName, object userData = null)
        {
            OpenUIForm(uiFormName, out UIBase uiForm, userData);
        }
        public void OpenUIForm<T>(string uiFormName, out T uiForm, object userData = null) where T : UIBase
        {
            OpenUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), out uiForm, userData);
        }
        public void OpenUIForm<T>(int uiFormId, out T uiForm, object userData = null) where T : UIBase
        {
            //1,读表
            uiForm = null;
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(uiFormId);
            if (sys_UIForm == null) return;
            if (sys_UIForm.CanMulit == 0 && IsExists(uiFormId))
            {
                GameEntry.LogError("不重复打开同一个UI窗口==" + uiFormId + "  " + sys_UIForm.AssetPath_Chinese);
                return;
            }

            //从对象池里面取
            UIBase formBase = UIPool.Dequeue(uiFormId);
            if (formBase != null)
            {
                m_OpenUIFormList.AddLast(formBase);
                CheckReverseChange(sys_UIForm, formBase, true);

                if (formBase.IsStart) formBase.Open(userData);
                uiForm = formBase as T;
                return;
            }

            //对象池没有, 克隆新的
            ResourceEntity resourceEntity = GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(sys_UIForm.AssetFullName);
            GameObject uiObj = Object.Instantiate((GameObject)resourceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group);
            GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), resourceEntity);

            //初始化UI
            formBase = uiObj.GetComponent<UIBase>();
            if (formBase == null)
            {
                GameEntry.LogError("该UI界面没有挂载UIBase脚本==" + uiObj);
                formBase = uiObj.AddComponent<UIBase>();
            }
            formBase.CurrCanvas.overrideSorting = true;
            m_OpenUIFormList.AddLast(formBase);

            CheckReverseChange(sys_UIForm, formBase, true);

            formBase.Init(sys_UIForm, userData);
            formBase.OnShow();

            uiForm = formBase as T;
        }

        /// <summary>
        /// 检查反切
        /// </summary>
        private void CheckReverseChange(Sys_UIFormEntity sys_UIFormEntity, UIBase formBase, bool OpenOrClose)
        {
            UIFormShowMode uIFormShowMode = (UIFormShowMode)sys_UIFormEntity.ShowMode;
            if (uIFormShowMode == UIFormShowMode.ReverseChange)
            {
                if (OpenOrClose)
                {
                    //如果之前里面有UI
                    if (m_ReverseChangeUIList.Count > 0)
                    {
                        UIBase topUIForm = m_ReverseChangeUIList.First.Value;
                        GameEntry.UI.HideUI(topUIForm);
                    }
                    //GameEntry.Log(LogCategory.UI, "窗口入栈==" + formBase);
                    m_ReverseChangeUIList.AddFirst(formBase);
                }
                else
                {
                    m_ReverseChangeUIList.Remove(formBase);

                    if (m_ReverseChangeUIList.Count > 0)
                    {
                        UIBase topForms = m_ReverseChangeUIList.First.Value;
                        //topForms.OnBack?.Invoke();
                        //topForms.OnBack = null;
                        GameEntry.UI.ShowUI(topForms);
                    }
                }
            }
        }
        #endregion

        #region CloseUIForm 关闭UI窗口
        public void CloseUIForm(int uiFormId)
        {
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    CloseUIForm(curr.Value);
                    break;
                }
            }
        }
        internal void CloseUIFormByInstanceID(int instanceID)
        {
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.gameObject.GetInstanceID() == instanceID)
                {
                    CloseUIForm(curr.Value);
                    break;
                }
            }
        }
        internal void CloseUIForm(UIBase formBase)
        {
            if (!m_OpenUIFormList.Remove(formBase))
            {
                //YouYou.GameEntry.LogError(formBase + "==已经是关闭状态了");
                return;
            }
            formBase.ToClose();

            //判断反切UI
            CheckReverseChange(formBase.SysUIForm, formBase, false);
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
            List<UIBase> lst = new List<UIBase>();
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                UIBase formBase = curr.Value;
                if (formBase.SysUIForm.UIGroupId != 2) continue;
                formBase.ToClose();
                lst.Add(formBase);
            }
            lst.ForEach(x => m_OpenUIFormList.Remove(x));
            m_ReverseChangeUIList.Clear();
        }
        #endregion

        #region 强制清除界面
        /// <summary>
        /// 强制清除界面
        /// </summary>
        public void Release(string uiFormName)
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName);
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    Release(curr.Value);
                    return;
                }
            }
            UIPool.Release(uiFormName);
        }
        public void Release(UIBase uIBase)
        {
            uIBase.Close();
            UIPool.Release(uIBase);
        }
        /// <summary>
        /// 强制清除全部界面
        /// </summary>
        public void ReleaseAll()
        {
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null;)
            {
                LinkedListNode<UIBase> next = curr.Next;
                m_OpenUIFormList.Remove(curr.Value);
                if (curr.Value != null) Object.Destroy(curr.Value.gameObject);
                curr = next;
            }
            UIPool.ReleaseAll();
            m_ReverseChangeUIList.Clear();
        }
        #endregion

        #region 显示和隐藏
        /// <summary>
        /// 显示/激活一个UI
        /// </summary>
        public void ShowUI(UIBase uiFormBase)
        {
            //GameEntry.Log("ShowUI==" + uiFormBase);
            uiFormBase.IsActive = true;
            if (uiFormBase.SysUIForm.FreezeMode == 0)
            {
                uiFormBase.CurrCanvas.enabled = true;
                uiFormBase.gameObject.layer = 5;
                if (uiFormBase.gameObject.activeInHierarchy == false) uiFormBase.gameObject.SetActive(true);
            }
            else
            {
                uiFormBase.gameObject.SetActive(true);
            }
            if (uiFormBase.IsStart) //当已经调用Start方法后才会调用OnShow, 要不然就是在Start调用OnShow
                uiFormBase.OnShow();
        }
        /// <summary>
        /// 隐藏/冻结一个UI
        /// </summary>
        public void HideUI(UIBase uiFormBase)
        {
            //GameEntry.Log("HideUI==" + uiFormBase);
            uiFormBase.IsActive = false;
            if (uiFormBase.SysUIForm.FreezeMode == 0)
            {
                uiFormBase.CurrCanvas.enabled = false;
                uiFormBase.gameObject.layer = 0;
            }
            else
            {
                uiFormBase.gameObject.SetActive(false);
            }
            uiFormBase.OnHide();
        }
        #endregion

        public T GetUIForm<T>(string uiFormName) where T : UIBase
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName);
            //先看看已打开的窗口有没有
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId) return curr.Value as T;
            }

            //再看看对象池内有没有
            return UIPool.GetUIForm(uiFormId) as T;
        }

        /// <summary>
        /// 检查UI是否已经打开
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <returns></returns>
        private bool IsExists(int uiFormId)
        {
            for (LinkedListNode<UIBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId) return true;
            }
            return false;
        }

        public T PreloadLoadAssetUI<T>(string uiFormName) where T : UIBase
        {
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName));
            return PreloadLoadAssetUI(sys_UIForm) as T;
        }
        public UIBase PreloadLoadAssetUI(Sys_UIFormEntity sys_UIForm)
        {
            //1,读表
            if (sys_UIForm == null) return null;
            if (IsExists(sys_UIForm.Id) || UIPool.GetUIForm(sys_UIForm.Id) != null)
            {
                YouYou.GameEntry.LogError("不预加载已存在的UI" + sys_UIForm.Id + "  " + sys_UIForm.AssetPath_Chinese);
                return null;
            }

            //对象池没有, 克隆新的
            ResourceEntity resourceEntity = GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(sys_UIForm.AssetFullName);
            GameObject uiObj = Object.Instantiate((GameObject)resourceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group);
            GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), resourceEntity);

            //初始化UI
            UIBase formBase = uiObj.GetComponent<UIBase>();
            if (formBase == null)
            {
                GameEntry.LogError("该UI界面没有挂载UIBase脚本==" + uiObj);
                formBase = uiObj.AddComponent<UIBase>();
            }
            formBase.CurrCanvas.overrideSorting = true;

            formBase.Init(sys_UIForm, null);

            formBase.gameObject.SetActive(false);//马上隐藏, 避免调用到Start方法
            formBase.IsActive = false;
            UIPool.EnQueue(formBase);
            return formBase;
        }
    }
}