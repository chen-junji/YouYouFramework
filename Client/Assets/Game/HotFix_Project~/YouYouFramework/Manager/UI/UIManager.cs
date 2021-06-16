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
        public void OpenUIForm<T>(string uiFormName, object userData = null, Action<YouYou.UIFormBase> onOpen = null) where T : UIFormBase, new()
        {
            GameEntry.UI.OpenUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, onOpen, (uiFormBase) =>
            {
                T obj = new T();
                obj.Init(uiFormBase as ILRuntimeForm);
            });
        }
    }
}
