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
        /// UI窗口的显示类型
        /// </summary>
        public enum UIFormShowMode
        {
            Normal = 0,
            /// <summary>
            /// 反切
            /// </summary>
            ReverseChange = 1,
        }
        public class ReverseEntity
        {
            public string Name;

            /// <summary>
            /// 记录这个界面在打开时的渲染Order, 用于反切时设置正确的Order
            /// </summary>
            public int Order;
        }

        /// <summary>
        /// 已经打开的UI窗口链表 主要用于界面池内存释放 除非配置了允许多实例 否则不允许有重复的界面加入进来
        /// </summary>
        private LinkedList<UIFormBase> m_OpenUIFormList = new();

        /// <summary>
        /// 反切链表, 记录反切界面的打开顺序, 允许有重复的界面加入进来
        /// </summary>
        private LinkedList<ReverseEntity> m_ReverseChangeUIList = new();

        private Dictionary<byte, UIGroup> m_UIGroupDic = new();

        private UILayer UILayer = new();

        private UIPool UIPool = new();

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
            StandardScreen = GameEntry.Instance.UIRootCanvasScaler.referenceResolution.x / GameEntry.Instance.UIRootCanvasScaler.referenceResolution.y;
            for (int i = 0; i < GameEntry.Instance.UIGroups.Length; i++)
            {
                m_UIGroupDic[GameEntry.Instance.UIGroups[i].Id] = GameEntry.Instance.UIGroups[i];
            }

            //设置默认渲染模式
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
        private UIFormBase OpenUIForm(string uiFormName)
        {
            return ShowUIForm(uiFormName, true);
        }
        private UIFormBase ShowUIForm(string uiFormName, bool checkReverseChange)
        {
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName);
            if (sys_UIForm == null)
            {
                return null;
            }
            if (sys_UIForm.CanMulit == 0 && IsExists(sys_UIForm.Id))
            {
                CloseUIForm(uiFormName);
                OpenUIForm(uiFormName);
                GameEntry.Log(LogCategory.Framework, "重复打开同一个UI窗口==" + sys_UIForm.Id + "  " + sys_UIForm.AssetFullPath);
                return null;
            }

            if (checkReverseChange)
            {
                //Debug.LogError("Show==" + m_ReverseChangeUIList.ToJson());
                //检查反切
                if ((UIFormShowMode)sys_UIForm.ShowMode == UIFormShowMode.ReverseChange && m_ReverseChangeUIList.Count > 0)
                {
                    //在打开下一个界面前，关闭当前界面
                    for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null; curr = curr.Previous)
                    {
                        if (curr.Value.SysUIForm.UIFromName == m_ReverseChangeUIList.Last.Value.Name)
                        {
                            HideUIForm(curr.Value, false);
                            break;
                        }
                    }
                }
                else
                {
                    UILayer.SetSortingOrder(sys_UIForm, true);
                }
                if ((UIFormShowMode)sys_UIForm.ShowMode == UIFormShowMode.ReverseChange)
                {
                    //把界面加入反切链表中
                    m_ReverseChangeUIList.AddLast(new ReverseEntity() { Name = uiFormName, Order = UILayer.GetCurrSortingOrder(sys_UIForm) });
                }
            }

            //从对象池里面取
            UIFormBase formBase = UIPool.Dequeue(sys_UIForm.Id);
            if (formBase == null)
            {
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
            }
            m_OpenUIFormList.AddLast(formBase);
            formBase.SetSortingOrder(UILayer.GetCurrSortingOrder(sys_UIForm));

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
        private void CloseUIForm(string uiFormName)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null; curr = curr.Previous)
            {
                if (curr.Value.SysUIForm.UIFromName == uiFormName)
                {
                    CloseUIForm(curr.Value);
                    break;
                }
            }
        }
        public void CloseUIForm(UIFormBase formBase)
        {
            HideUIForm(formBase, true);
        }

        private void HideUIForm(UIFormBase formBase, bool checkReverseChange)
        {
            if (!formBase.IsActive)
            {
                GameEntry.LogError(formBase + "==已经是关闭状态了");
                return;
            }
            LinkedListNode<UIFormBase> findLastNode = m_OpenUIFormList.FindLast(formBase);
            if (findLastNode == null)
            {
                GameEntry.LogError(formBase + "==在m_OpenUIFormList中找不到该界面");
                return;
            }

            if (checkReverseChange)
            {
                //检查反切
                if ((UIFormShowMode)formBase.SysUIForm.ShowMode == UIFormShowMode.ReverseChange)
                {
                    //把当前界面从反切链表中移除
                    m_ReverseChangeUIList.RemoveLast();
                }
                if ((UIFormShowMode)formBase.SysUIForm.ShowMode == UIFormShowMode.ReverseChange && m_ReverseChangeUIList.Count > 0)
                {
                    //在关闭当前界面后，打开上一个界面
                    UIFormBase topForms = ShowUIForm(m_ReverseChangeUIList.Last.Value.Name, false);
                    topForms.SetSortingOrder(m_ReverseChangeUIList.Last.Value.Order);
                    if (topForms.OnBack != null)
                    {
                        Action onBack = topForms.OnBack;
                        topForms.OnBack = null;
                        onBack();
                    }
                }
                else
                {
                    if (findLastNode.Next != null && formBase.SysUIForm.DisableUILayer == 0)
                    {
                        for (LinkedListNode<UIFormBase> curr = findLastNode.Next; curr != null; curr = curr.Next)
                        {
                            //假如我现在打开了界面1-2-3-4, 然后关闭了界面2, 那么就需要把界面3和界面4的Order给刷新一下(界面1就不用刷新了)
                            if (curr.Value.SysUIForm.UIGroupId != formBase.SysUIForm.UIGroupId)
                            {
                                continue;
                            }
                            if (curr.Value.SysUIForm.DisableUILayer == 1)
                            {
                                continue;
                            }
                            curr.Value.SetSortingOrder(curr.Value.sortingOrder - 10);
                        }
                    }
                    UILayer.SetSortingOrder(formBase.SysUIForm, false);
                }
                //Debug.LogError("Hide==" + m_ReverseChangeUIList.ToJson());
            }

            //要先SetSortingOrder再Remove
            m_OpenUIFormList.Remove(formBase);
            UIPool.EnQueue(formBase);
        }

        /// <summary>
        /// 关闭所有"Default"组的UI窗口
        /// </summary>
        public void CloseAllDefaultUIForm()
        {
            List<UIFormBase> lst = new();
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
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null; curr = curr.Previous)
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
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null;)
            {
                LinkedListNode<UIFormBase> next = curr.Next;
                m_OpenUIFormList.Remove(curr.Value);
                if (curr.Value != null) Object.Destroy(curr.Value.gameObject);
                curr = next;
            }
            UIPool.ReleaseAll();
        }
        #endregion

        public T GetUIForm<T>() where T : UIFormBase
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(typeof(T).Name).Id;
            //先看看已打开的窗口有没有
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null; curr = curr.Previous)
            {
                if (curr.Value.SysUIForm.Id == uiFormId) return curr.Value as T;
            }

            //再看看对象池内有没有
            return UIPool.GetUIForm(uiFormId) as T;
        }

        /// <summary>
        /// 切换UI渲染模式
        /// </summary>
        public void ChangeCanvasRanderMode(RenderMode renderMode)
        {
            GameEntry.Instance.UIRootCanvas.renderMode = renderMode;
            GameEntry.Instance.UICamera.enabled = renderMode != RenderMode.ScreenSpaceOverlay;
        }

        /// <summary>
        /// 检查UI是否已经打开
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <returns></returns>
        private bool IsExists(int uiFormId)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.Last; curr != null; curr = curr.Previous)
            {
                if (curr.Value.SysUIForm.Id == uiFormId) return true;
            }
            return false;
        }

    }
}