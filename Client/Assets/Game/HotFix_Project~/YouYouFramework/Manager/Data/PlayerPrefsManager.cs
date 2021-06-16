using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class PlayerPrefsManager
{
    private Dictionary<string, object> LoggerDic;
    public object GetLoggerDic(string loggerType)
    {
        object value = null;
        LoggerDic.TryGetValue(loggerType, out value);
        return value;
    }
    public void SetLoggerDic(string loggerType, object userData, object param = null)
    {
        LoggerDic[loggerType] = userData;
        
        GameEntry.Event.CommonEvent.Dispatch(loggerType, param);
    }
    public void Dispose()
    {
        SaveDataAll();
    }
    public void Init()
    {
        LoggerDic = PlayerPrefs.GetString("LoggerDic").ToObject<Dictionary<string, object>>();
        if (LoggerDic == null) LoggerDic = new Dictionary<string, object>();
    }
    public void SaveDataAll()
    {
        PlayerPrefs.SetString("LoggerDic", LoggerDic.ToJson());
    }
    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();

        Init();
    }
}
