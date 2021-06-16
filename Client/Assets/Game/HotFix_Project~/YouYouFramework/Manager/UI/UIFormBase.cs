using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouYou;

namespace Hotfix
{
    public class UIFormBase
    {
        private ILRuntimeForm ILRuntimeForm;

        public void Init(ILRuntimeForm form)
        {
            ILRuntimeForm = form;
            ILRuntimeForm.onInit = OnInit;
            ILRuntimeForm.onOpen = OnOpen;
            ILRuntimeForm.onClose = OnClose;
            ILRuntimeForm.onBeforDestroy = OnBeforDestroy;
        }

        protected virtual void OnInit(object userData) { }
        protected virtual void OnOpen(object userData) { }
        protected virtual void OnClose() { }
        protected virtual void OnBeforDestroy() { }
    }
}
