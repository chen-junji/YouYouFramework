using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hotfix
{
    class UIDialog : UIFormBase
    {
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            TestEntity entity = GameEntryIL.DataTable.TestDBModel.GetDic(1);
            Debug.Log("TestEntity.Desc==" + entity.Desc);
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GameEntryIL.Event.CommonEvent.AddEventListener(CommonEventId.OnUpdate, OnUpdate);
        }
        protected override void OnClose()
        {
            base.OnClose();
            GameEntryIL.Event.CommonEvent.RemoveEventListener(CommonEventId.OnUpdate, OnUpdate);
        }
        protected override void OnBeforDestroy()
        {
            base.OnBeforDestroy();
        }
        private void OnUpdate(object userData)
        {
            Debug.Log("UIDialog.OnUpdate");
        }
    }
}
