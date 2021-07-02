using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotfix
{
    public class GameEntryIL
    {
        public static EventManager Event { get; private set; }
        public static DataTableManager DataTable { get; private set; }
        public static DataManager Data { get; private set; }
        public static UIManager UI { get; private set; }

        public GameEntryIL()
        {
            Event = new EventManager();
            DataTable = new DataTableManager();
            Data = new DataManager();
            UI = new UIManager();

            DataTable.Init();

            YouYou.GameEntry.ActionOnUpdate = OnUpdate;
            YouYou.GameEntry.ActionOnApplicationPause = OnApplicationPause;
            YouYou.GameEntry.ActionOnApplicationQuit = OnApplicationQuit;
            YouYou.GameEntry.ActionOnGameEnter = OnGameEnter;
        }
        private void OnUpdate()
        {
            Event.CommonEvent.Dispatch(CommonEventId.OnUpdate);
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause) Data.PlayerPrefsManager.SaveDataAll();
        }
        private void OnApplicationQuit()
        {
            Data.PlayerPrefsManager.SaveDataAll();
        }
        public static void OnGameEnter()
        {
            UI.OpenUIForm(UIFormId.UIDialog);
        }
    }
}
