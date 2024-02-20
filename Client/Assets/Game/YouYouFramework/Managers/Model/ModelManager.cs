using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
