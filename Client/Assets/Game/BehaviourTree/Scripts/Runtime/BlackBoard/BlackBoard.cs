using System;
using System.Collections.Generic;

/// <summary>
/// 黑板
/// </summary>
[Serializable]
public class BlackBoard
{
    /// <summary>
    /// 参数字典
    /// </summary>
    public Dictionary<string, BBParam> ParamDict = new Dictionary<string, BBParam>();

    /// <summary>
    /// 获取参数
    /// </summary>
    public T GetParam<T>(string key,bool isCrete = false) where T : BBParam,new()
    {
        BBParam param = GetParam(key);
        if (param == null)
        {
            if (isCrete)
            {
                //isCreate为true 则不存在时创建
                param = new T();
                SetParam(key,param);
            }
            else
            {
                return default;
            }
               
        }

        return ((T)param);
    }

    /// <summary>
    /// 获取参数
    /// </summary>
    public BBParam GetParam(string key)
    {
        ParamDict.TryGetValue(key, out var param);
        return param;
    }
        
    /// <summary>
    /// 设置参数
    /// </summary>
    public void SetParam(string key, BBParam param)
    {
        if (param == null)
        {
            return;
        }
            
        param.Key = key;
        ParamDict[key] = param;
    }

    /// <summary>
    /// 移除参数
    /// </summary>
    public void RemoveParam(string key)
    {
        ParamDict.Remove(key);
    }
        
        
}