//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace YouYou
{
    /// <summary>
    /// Lua组件类型
    /// </summary>
    public enum LuaComType
    {
        GameObject = 0,
        Transform = 1,
        Button = 2,
        Image = 3,
        YouYouImage = 4,
        Text = 5,
        YouYouText = 6,
        RawImage = 7,
        InputField = 8,
        Scrollbar = 9,
        ScrollView = 10,
        MulityScroller = 11
    }

    /// <summary>
    /// Lua窗口Form
    /// </summary>
    [LuaCallCSharp]
    public class LuaForm : UIFormBase
    {
        [CSharpCallLua]
        public delegate void OnInitHandler(Transform transform, object userData);
        OnInitHandler onInit;

        [CSharpCallLua]
        public delegate void OnOpenHandler(object userData);
        OnOpenHandler onOpen;

        [CSharpCallLua]
        public delegate void OnCloseHandler();
        OnCloseHandler onClose;

        [CSharpCallLua]
        public delegate void OnBeforDestroyHandler();
        OnBeforDestroyHandler onBeforDestroy;

        public TextAsset luaScript;

        [Header("Lua组件分组")]
        [SerializeField]
        private LuaComGroup[] m_LuaComGroups;

        /// <summary>
        /// 设置UI组件
        /// </summary>
        /// <returns></returns>
        public void SetLuaComs()
        {
            int len = m_LuaComGroups.Length;
            for (int i = 0; i < len; i++)
            {
                LuaComGroup group = m_LuaComGroups[i];
                int lenCom = group.LuaComs.Length;
                for (int j = 0; j < lenCom; j++)
                {
                    LuaCom com = group.LuaComs[j];
                    object obj = null;
                    switch (com.Type)
                    {
                        default:
                        case LuaComType.GameObject:
                            obj = com.Trans.gameObject;
                            break;
                        case LuaComType.Transform:
                            obj = com.Trans;
                            break;
                        case LuaComType.Button:
                            obj = com.Trans.GetComponent<Button>();
                            break;
                        case LuaComType.Image:
                            obj = com.Trans.GetComponent<Image>();
                            break;
                        case LuaComType.YouYouImage:
                            obj = com.Trans.GetComponent<YouYouImage>();
                            break;
                        case LuaComType.Text:
                            obj = com.Trans.GetComponent<Text>();
                            break;
                        case LuaComType.YouYouText:
                            obj = com.Trans.GetComponent<YouYouText>();
                            break;
                        case LuaComType.RawImage:
                            obj = com.Trans.GetComponent<RawImage>();
                            break;
                        case LuaComType.InputField:
                            obj = com.Trans.GetComponent<InputField>();
                            break;
                        case LuaComType.Scrollbar:
                            obj = com.Trans.GetComponent<Scrollbar>();
                            break;
                        case LuaComType.ScrollView:
                            obj = com.Trans.GetComponent<ScrollRect>();
                            break;
                        case LuaComType.MulityScroller:
                            obj = com.Trans.GetComponent<UIMultiScroller>();
                            break;
                    }

                    scriptEnv.Set(com.Name, obj);
                }
            }
        }

        private LuaTable scriptEnv;


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (luaScript == null) return;

            if (LuaManager.luaEnv == null) return;

            scriptEnv = LuaManager.luaEnv.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = LuaManager.luaEnv.NewTable();
            meta.Set("__index", LuaManager.luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);
            SetLuaComs();

            LuaManager.luaEnv.DoString(luaScript.text, "LuaBehaviour", scriptEnv);
            onInit = scriptEnv.Get<OnInitHandler>("OnInit");
            onOpen = scriptEnv.Get<OnOpenHandler>("OnOpen");
            onClose = scriptEnv.Get<OnCloseHandler>("OnClose");
            onBeforDestroy = scriptEnv.Get<OnBeforDestroyHandler>("OnBeforDestroy");

            if (onInit != null)
            {
                onInit(transform, userData);
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (onOpen != null)
            {
                onOpen(userData);
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (onClose != null)
            {
                onClose();
            }
        }

        protected override void OnBeforDestroy()
        {
            base.OnBeforDestroy();
            if (onBeforDestroy != null)
            {
                onBeforDestroy();
            }

            onInit = null;
            onOpen = null;
            onClose = null;
            onBeforDestroy = null;
            luaScript = null;
            if (scriptEnv != null)
            {
                scriptEnv.Dispose();
                scriptEnv = null;
            }
            int len = m_LuaComGroups.Length;
            for (int i = 0; i < len; i++)
            {
                LuaComGroup group = m_LuaComGroups[i];
                int lenCom = group.LuaComs.Length;
                for (int j = 0; j < lenCom; j++)
                {
                    group.LuaComs[j] = null;
                }
                group = null;
            }
        }
    }

    [Serializable]
    /// <summary>
    /// Lua组件分组
    /// </summary>
    public class LuaComGroup
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// Lua组件数组
        /// </summary>
        public LuaCom[] LuaComs;
    }

    [Serializable]
    /// <summary>
    /// Lua组件
    /// </summary>
    public class LuaCom
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 类型
        /// </summary>
        public LuaComType Type;

        [OnValueChanged("SetName")]
        /// <summary>
        /// 变换
        /// </summary>
        public Transform Trans;

        #region 设置变化的名称 SetName
        /// <summary>
        /// 设置变化的名称
        /// </summary>
        private void SetName()
        {
            if (Trans != null)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    Name = Trans.gameObject.name;
                }

                #region 设置类型
                if (Trans.GetComponent<Button>() != null)
                {
                    Type = LuaComType.Button;
                }
                else if (Trans.GetComponent<Image>() != null)
                {
                    Type = LuaComType.Image;
                }
                else if (Trans.GetComponent<YouYouImage>() != null)
                {
                    Type = LuaComType.YouYouImage;
                }
                else if (Trans.GetComponent<Text>() != null)
                {
                    Type = LuaComType.Text;
                }
                else if (Trans.GetComponent<YouYouText>() != null)
                {
                    Type = LuaComType.YouYouText;
                }
                else if (Trans.GetComponent<RawImage>() != null)
                {
                    Type = LuaComType.RawImage;
                }
                else if (Trans.GetComponent<InputField>() != null)
                {
                    Type = LuaComType.InputField;
                }
                else if (Trans.GetComponent<Scrollbar>() != null)
                {
                    Type = LuaComType.Scrollbar;
                }
                else if (Trans.GetComponent<ScrollRect>() != null)
                {
                    Type = LuaComType.ScrollView;
                }
                else if (Trans.GetComponent<UIMultiScroller>() != null)
                {
                    Type = LuaComType.MulityScroller;
                }
                #endregion
            }
            else
            {
                Name = "";
            }
        }
        #endregion
    }
}