using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using Object = UnityEngine.Object;

namespace YouYouFramework
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
        private UIFormBase OpenUIForm(string uiFormName, bool checkReverseChange = true)
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

            if (checkReverseChange && (UIFormShowMode)sys_UIForm.ShowMode == UIFormShowMode.ReverseChange)
            {
                //检查反切，在打开下一个界面前，关闭当前界面
                if (m_ReverseChangeUIList.Count > 0)
                {
                    CloseUIForm(m_ReverseChangeUIList.Last.Value, false);
                }

                //把界面加入反切链表中
                m_ReverseChangeUIList.AddLast(sys_UIForm.UIFromName);
            }

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
        private void CloseUIForm(string uiFormName, bool checkReverseChange = true)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.UIFromName == uiFormName)
                {
                    CloseUIForm(curr.Value, checkReverseChange);
                    break;
                }
            }
        }
        public void CloseUIForm(UIFormBase formBase, bool checkReverseChange = true)
        {
            if (!formBase.IsActive)
            {
                //YouYou.GameEntry.LogError(formBase + "==已经是关闭状态了");
                return;
            }
            if (m_OpenUIFormList.Contains(formBase))
            {
                SetSortingOrder(formBase, false);
                m_OpenUIFormList.Remove(formBase);
                UIPool.EnQueue(formBase);
            }

            if (checkReverseChange && (UIFormShowMode)formBase.SysUIForm.ShowMode == UIFormShowMode.ReverseChange)
            {
                //把当前界面从反切链表中移除
                m_ReverseChangeUIList.Remove(formBase.SysUIForm.UIFromName);

                //检查反切，在关闭当前界面后，打开上一个界面
                if (m_ReverseChangeUIList.Count > 0)
                {
                    UIFormBase topForms = OpenUIForm(m_ReverseChangeUIList.Last.Value, false);
                    if (topForms.OnBack != null)
                    {
                        Action onBack = topForms.OnBack;
                        topForms.OnBack = null;
                        onBack();
                    }
                }
            }
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
            UILayer.SetSortingOrder(formBase, isAdd);
            if (isAdd)
            {
                formBase.SetSortingOrder(UILayer.GetCurrSortingOrder(formBase));
            }
            else
            {
                LinkedListNode<UIFormBase> findNode = m_OpenUIFormList.FindLast(formBase);
                if (findNode != null && findNode.Next != null)
                {
                    for (LinkedListNode<UIFormBase> curr = findNode.Next; curr != null; curr = curr.Next)
                    {
                        //假如我现在打开了界面1-2-3-4, 然后关闭了界面2, 那么就需要把界面3和界面4的Order给刷新一下(界面1就不用刷新了)
                        if (curr.Value.SysUIForm.UIGroupId != formBase.SysUIForm.UIGroupId)
                        {
                            continue;
                        }
                        curr.Value.SetSortingOrder(curr.Value.sortingOrder - 10);
                    }
                }
            }
        }

        public void ChangeCanvasRanderMode(RenderMode renderMode)
        {
            GameEntry.Instance.UIRootCanvas.renderMode = renderMode;
            GameEntry.Instance.UICamera.enabled = renderMode != RenderMode.ScreenSpaceOverlay;
        }

    }
}