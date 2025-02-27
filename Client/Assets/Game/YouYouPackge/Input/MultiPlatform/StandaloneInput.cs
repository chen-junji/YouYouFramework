using System;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// Input检测, PC端状态
/// </summary>
public class StandaloneInput : VirtualInput
{
    public struct KeyCodeItem
    {
        public KeyCode KeyCode;
        public InputKeyCode Name;
        public KeyCodeItem(KeyCode keyCode, InputKeyCode name)
        {
            KeyCode = keyCode;
            Name = name;
        }
    }
    public List<KeyCodeItem> KeyCodeItemList = new List<KeyCodeItem>();

    internal override void OnInit(Fsm<InputManager> currFsm, int myStateIndex)
    {
        base.OnInit(currFsm, myStateIndex);
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
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public override void OnUpdate(float elapseSeconds)
    {
        base.OnUpdate(elapseSeconds);
        for (int i = 0; i < KeyCodeItemList.Count; i++)
        {
            KeyCodeItem keyCodeItem = KeyCodeItemList[i];
            if (Input.GetKeyDown(keyCodeItem.KeyCode))
            {
                InputManager.Instance.SetButtonDown(keyCodeItem.Name);
            }
            if (Input.GetKeyUp(keyCodeItem.KeyCode))
            {
                InputManager.Instance.SetButtonUp(keyCodeItem.Name);
            }
        }
    }


    public override float GetAxis(string name, bool raw)
    {
        return raw ? Input.GetAxisRaw(name.ToString()) : Input.GetAxis(name.ToString());
    }
    public override void SetAxis(string name, float value)
    {
    }

}