using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 可以把所有Model都装到modelMap里面, 在退出登录时调用Clear清除所有缓存.
    /// 避免重新登录的第二个账号带着第一个账号的脏数据
    /// </summary>
    public class ModelManager
    {
        private Dictionary<string, object> modelMap = new Dictionary<string, object>();

        public T GetModel<T>() where T : new()
        {
            var typeId = typeof(T).FullName;
            if (modelMap.TryGetValue(typeId, out object model))
            {
                return (T)model;
            }
            else
            {
                T t = new T();
                modelMap.Add(typeId, t);
                return t;
            }
        }

        public void Clear()
        {
            modelMap.Clear();
        }
    }
}