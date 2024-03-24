using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using Object = UnityEngine.Object;

namespace YouYou
{
    public class UIManager
    {
        /// <summary>
        /// 已经打开的UI窗口链表
        /// </summary>
        private LinkedList<UIFormBase> m_OpenUIFormList;
        /// <summary>
        /// 反切链表
        /// </summary>
        private LinkedList<string> m_ReverseChangeUIList;

        private Dictionary<byte, UIGroup> m_UIGroupDic;

        private UILayer UILayer;

        private UIPool UIPool;

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
            m_OpenUIFormList = new LinkedList<UIFormBase>();
            m_ReverseChangeUIList = new LinkedList<string>();

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

            ChangeCanvasRanderMode(RenderMode.ScreenSpaceOverlay);
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
        public T OpenUIForm<T>() where T : UIFormBase
        {
            return OpenUIForm(typeof(T).Name) as T;
        }
        public UIFormBase OpenUIForm(string uiFormName)
        {
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName);
            if (sys_UIForm == null)
            {
                return null;
            }
            if (sys_UIForm.CanMulit == 0 && IsExists(sys_UIForm.Id))
            {
                GameEntry.LogError(LogCategory.Framework, "不重复打开同一个UI窗口==" + sys_UIForm.Id + "  " + sys_UIForm.AssetFullPath);
                return null;
            }
            UIFormBase uiFormBase = OpenUIForm(sys_UIForm);
            CheckReverseClose(uiFormBase);
            return uiFormBase;
        }
        private UIFormBase OpenUIForm(Sys_UIFormEntity sys_UIForm)
        {
            //从对象池里面取
            UIFormBase formBase = UIPool.Dequeue(sys_UIForm.Id);
            if (formBase != null)
            {
                m_OpenUIFormList.AddLast(formBase);
                SetSortingOrder(formBase, true);
                return formBase;
            }

            //对象池没有, 克隆新的
            GameObject uiObj = GameUtil.LoadPrefabClone(sys_UIForm.AssetFullPath, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group);

            //初始化UI
            formBase = uiObj.GetComponent<UIFormBase>();
            if (formBase == null)
            {
                GameEntry.LogError(LogCategory.Framework, "该UI界面没有挂载UIBase脚本==" + uiObj);
                formBase = uiObj.AddComponent<UIFormBase>();
            }
            formBase.Init(sys_UIForm);

            m_OpenUIFormList.AddLast(formBase);
            SetSortingOrder(formBase, true);
            return formBase;
        }
        #endregion

        #region CloseUIForm 关闭UI窗口
        /// <summary>
        /// 关闭UI窗口
        /// </summary>
        public void CloseUIForm<T>() where T : UIFormBase
        {
            CloseUIForm(typeof(T).Name);
        }
        public void CloseUIForm(string uiFormName)
        {
            CloseUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName));
        }
        public void CloseUIForm(Sys_UIFormEntity sys_UIForm)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == sys_UIForm.Id)
                {
                    CloseUIForm(curr.Value);
                    break;
                }
            }
        }
        public void CloseUIForm(UIFormBase formBase)
        {
            if (!formBase.IsActive)
            {
                //YouYou.GameEntry.LogError(formBase + "==已经是关闭状态了");
                return;
            }
            if (m_OpenUIFormList.Remove(formBase))
            {
                SetSortingOrder(formBase, false);
                UIPool.EnQueue(formBase);
            }
            CheckReverseOpen(formBase);
        }

        /// <summary>
        /// 关闭所有"Default"组的UI窗口
        /// </summary>
        public void CloseAllDefaultUIForm()
        {
            m_ReverseChangeUIList.Clear();

            List<UIFormBase> lst = new List<UIFormBase>();
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null; curr = curr.Previous)
            {
                lst.Add(curr.Value);
            }
            for (int i = 0; i < lst.Count; i++)
            {
                UIFormBase formBase = lst[i];
                if (formBase.SysUIForm.UIGroupId != 2) continue;
                CloseUIForm(formBase);
            }
        }
        #endregion

        #region 强制清除界面
        /// <summary>
        /// 强制清除界面
        /// </summary>
        public void Release(string uiFormName)
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName).Id;
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    Release(curr.Value);
                    return;
                }
            }
            UIPool.Release(uiFormName);
        }
        public void Release(UIFormBase uIBase)
        {
            GameEntry.UI.CloseUIForm(uIBase);
            UIPool.Release(uIBase);
        }
        /// <summary>
        /// 强制清除全部界面
        /// </summary>
        public void ReleaseAll()
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null;)
            {
                LinkedListNode<UIFormBase> next = curr.Next;
                m_OpenUIFormList.Remove(curr.Value);
                if (curr.Value != null) Object.Destroy(curr.Value.gameObject);
                curr = next;
            }
            UIPool.ReleaseAll();
            m_ReverseChangeUIList.Clear();
        }
        #endregion

        public T GetUIForm<T>(string uiFormName) where T : UIFormBase
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName).Id;
            //先看看已打开的窗口有没有
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
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
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId) return true;
            }
            return false;
        }

        private void SetSortingOrder(UIFormBase formBase, bool isAdd)
        {
            LinkedListNode<UIFormBase> findNode = m_OpenUIFormList.FindLast(formBase);
            if (findNode != null)
            {
                UILayer.SetSortingOrder(findNode.Value, isAdd);
                if (isAdd)
                {
                    findNode.Value.SetSortingOrder(UILayer.GetCurrSortingOrder(findNode.Value));
                }
                else
                {
                    for (LinkedListNode<UIFormBase> curr = findNode; curr != null; curr = curr.Next)
                    {
                        //如果当前界面不是最后打开的界面， 那么则需要把更晚打开的界面的sortingOrder全都刷新一下
                        if (curr.Value.SysUIForm.UIGroupId != formBase.SysUIForm.UIGroupId)
                        {
                            continue;
                        }
                        findNode.Value.SetSortingOrder(findNode.Value.sortingOrder - 10);
                    }
                }
            }
        }

        public void ChangeCanvasRanderMode(RenderMode renderMode)
        {
            GameEntry.Instance.UIRootCanvas.renderMode = renderMode;
            GameEntry.Instance.UICamera.enabled = renderMode != RenderMode.ScreenSpaceOverlay;
        }

        /// <summary>
        /// 检查反切，关闭上一个界面
        /// </summary>
        internal void CheckReverseClose(UIFormBase formBase)
        {
            UIFormShowMode uIFormShowMode = (UIFormShowMode)formBase.SysUIForm.ShowMode;
            if (uIFormShowMode != UIFormShowMode.ReverseChange)
            {
                return;
            }

            //如果之前里面有UI
            if (m_ReverseChangeUIList.Count > 0)
            {
                Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(m_ReverseChangeUIList.Last.Value);
                CloseUIForm(sys_UIForm);
            }
            //GameEntry.Log(LogCategory.UI, "窗口入栈==" + formBase);
            m_ReverseChangeUIList.AddLast(formBase.SysUIForm.UIFromName);
        }
        /// <summary>
        /// 检查反切, 打开上一个界面
        /// </summary>
        internal void CheckReverseOpen(UIFormBase formBase)
        {
            UIFormShowMode uIFormShowMode = (UIFormShowMode)formBase.SysUIForm.ShowMode;
            if (uIFormShowMode != UIFormShowMode.ReverseChange)
            {
                return;
            }

            m_ReverseChangeUIList.Remove(formBase.SysUIForm.UIFromName);
            if (m_ReverseChangeUIList.Count > 0)
            {
                Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(m_ReverseChangeUIList.Last.Value);
                UIFormBase topForms = OpenUIForm(sys_UIForm);
                if (topForms.OnBack != null)
                {
                    Action onBack = topForms.OnBack;
                    topForms.OnBack = null;
                    onBack();
                }
            }
        }

    }
}