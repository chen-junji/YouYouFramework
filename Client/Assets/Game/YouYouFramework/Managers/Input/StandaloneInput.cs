using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    public class StandaloneInput : VirtualInput
    {
        public struct KeyCodeItem
        {
            public KeyCode KeyCode;
            public InputKey Name;
            public KeyCodeItem(KeyCode keyCode, InputKey name)
            {
                KeyCode = keyCode;
                Name = name;
            }
        }
        public List<KeyCodeItem> KeyCodeItemList = new List<KeyCodeItem>();
        internal override void OnInit()
        {
            base.OnInit();
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.Mouse0, ConstInput.Fire));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.Mouse1, ConstInput.Aim));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.Space, ConstInput.Jump));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.R, ConstInput.Reload));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.LeftControl, ConstInput.Crouch));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.Escape, ConstInput.Pause));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.G, ConstInput.SkipDelay));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.H, ConstInput.AddCurrency));
            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.E, ConstInput.SellTower));

            //KeyCodeItemList.Add(new KeyCodeItem(KeyCode.T, ConstInput.Interact));
        }
        internal override void OnEnter()
        {
            base.OnEnter();
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        internal override void OnLeave()
        {
            base.OnLeave();

        }
        internal override void OnUpdate()
        {
            base.OnUpdate();
            for (int i = 0; i < KeyCodeItemList.Count; i++)
            {
                KeyCodeItem keyCodeItem = KeyCodeItemList[i];
                if (Input.GetKeyDown(keyCodeItem.KeyCode))
                {
                    GameEntry.Input.SetButtonDown(keyCodeItem.Name);
                }
                if (Input.GetKeyUp(keyCodeItem.KeyCode))
                {
                    GameEntry.Input.SetButtonUp(keyCodeItem.Name);
                }
            }
        }


        public override float GetAxis(string name, bool raw)
        {
            return raw ? Input.GetAxisRaw(name.ToString()) : Input.GetAxis(name.ToString());
        }

        public override void SetAxisPositive(string name)
        {
        }
        public override void SetAxisNegative(string name)
        {
        }
        public override void SetAxisZero(string name)
        {
        }
        public override void SetAxis(string name, float value)
        {
        }
    }
}