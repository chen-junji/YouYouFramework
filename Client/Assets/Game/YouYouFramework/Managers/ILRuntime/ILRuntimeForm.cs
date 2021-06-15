using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class ILRuntimeForm : UIFormBase
    {
        public Action<object> onInit;
        public Action<object> onOpen;
        public Action onClose;
        public Action onBeforDestroy;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            onInit?.Invoke(userData);
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            onOpen?.Invoke(userData);
        }
        protected override void OnClose()
        {
            base.OnClose();
            onClose?.Invoke();
        }
        protected override void OnBeforDestroy()
        {
            base.OnBeforDestroy();
            onBeforDestroy?.Invoke();
        }
    }
}