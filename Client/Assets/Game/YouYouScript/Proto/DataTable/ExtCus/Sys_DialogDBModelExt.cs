using System;
using System.Collections.Generic;


namespace YouYouFramework
{
    public partial class Sys_DialogDBModel
    {
        public Dictionary<string, Sys_DialogEntity> keyDic = new();

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

        public Sys_DialogEntity GetKey(string key) 
        {
            return keyDic[key];
        }
    }

}
