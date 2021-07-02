using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouYou;

namespace Hotfix
{
    public class UIManager
    {
        public async ETTask<T> OpenUIForm<T>(string uiFormName, object userData = null) where T : UIFormBase, new()
        {
            ETTask<T> task = ETTask<T>.Create();
            OpenUIFormAction<T>(uiFormName, userData, task.SetResult);
            return await task;
        }
        public void OpenUIForm(string uiFormName, object userData = null)
        {
            OpenUIFormAction<UIFormBase>(uiFormName, userData);
        }
        public void OpenUIFormAction<T>(string uiFormName, object userData = null, Action<T> onOpen = null) where T : UIFormBase, new()
        {
            GameEntry.UI.OpenUIFormAction(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, (ILRuntimeForm form) =>
            {
                onOpen?.Invoke(form.HotfixObj as T);
            }, (form) =>
            {
                T obj = new T();
                obj.Init(form);
            });
        }
    }
}
