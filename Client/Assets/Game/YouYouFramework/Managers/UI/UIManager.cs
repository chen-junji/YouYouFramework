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
        public T OpenUIForm<T>(object userData = null) where T : UIBase
        {
            return OpenUIForm<T>(typeof(T).Name, userData);
        }
        public T OpenUIForm<T>(string uiFormName, object userData = null) where T : UIBase
        {
            //1,读表
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName);
            if (sys_UIForm == null) return null;
            if (sys_UIForm.CanMulit == 0 && IsExists(sys_UIForm.Id))
            {
                GameEntry.LogError(LogCategory.Framework, "不重复打开同一个UI窗口==" + sys_UIForm.Id + "  " + sys_UIForm.AssetFullName);
                return null;
            }

            //从对象池里面取
            UIBase formBase = UIPool.Dequeue(sys_UIForm.Id);
            if (formBase != null)
            {
                m_OpenUIFormList.AddLast(formBase);
                CheckReverseChange(sys_UIForm, formBase, true);

                formBase.ToOpen();
                return formBase as T;
            }

            //对象池没有, 克隆新的
            AssetReferenceEntity referenceEntity = GameEntry.Resource.LoadMainAsset(sys_UIForm.AssetFullName);
            GameObject uiObj = Object.Instantiate((GameObject)referenceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group);
            GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), referenceEntity);

            //初始化UI
            formBase = uiObj.GetComponent<UIBase>();
            if (formBase == null)
            {
                GameEntry.LogError(LogCategory.Framework, "该UI界面没有挂载UIBase脚本==" + uiObj);
                formBase = uiObj.AddComponent<UIBase>();
            }
            formBase.CurrCanvas.overrideSorting = true;
            m_OpenUIFormList.AddLast(formBase);

            CheckReverseChange(sys_UIForm, formBase, true);

            formBase.Init(sys_UIForm);
            formBase.ToOpen();

            return formBase as T;
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
                        if (topForms.OnBack != null)
                        {
                            Action onBack = topForms.OnBack;
                            topForms.OnBack = null;
                            onBack();
                        }
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
            if (!formBase.IsActive) return;
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
        public void CloseUIForm<T>() where T : UIBase
        {
            CloseUIForm(typeof(T).Name);
        }
        public void CloseUIForm(string uiFormName)
        {
            CloseUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName).Id);
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
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName).Id;
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
            uiFormBase.gameObject.SetActive(true);
        }
        /// <summary>
        /// 隐藏/冻结一个UI
        /// </summary>
        public void HideUI(UIBase uiFormBase)
        {
            //GameEntry.Log("HideUI==" + uiFormBase);
            uiFormBase.IsActive = false;
            uiFormBase.gameObject.SetActive(false);
        }
        #endregion

        public T GetUIForm<T>(string uiFormName) where T : UIBase
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName).Id;
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

    }
}