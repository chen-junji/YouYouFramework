using System;
using System.Collections.Generic;


namespace YouYouFramework
{
    public partial class Sys_TipDBModel
    {
        public Dictionary<string, Sys_TipEntity> keyDic = new();

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();

            foreach (var entity in m_List)
            {
                if (keyDic.ContainsKey(entity.Key))
                {
                    GameEntry.LogError("列表中有重复的Key==" + entity.Key);
                    return;
                }
                keyDic[entity.Key] = entity;
            }

        }
    }

}
