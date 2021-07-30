using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;


public class PlayerPrefsManager
{
    #region LoggerDic
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
    public void SetLoggerDicAdd(string loggerType, float userData, object param = null)
    {
        SetLoggerDic(loggerType, GetLoggerDic(loggerType).ToFloat() + userData, param);
    }
    #endregion

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

    public T GetData<T>() where T : new()
    {
        if (PlayerPrefs.HasKey(typeof(T).Name))
        {
            return PlayerPrefs.GetString(typeof(T).Name).ToObject<T>();
        }
        else
        {
            return new T();
        }
    }

    public void SaveData<T>(T data) where T : new()
    {
        PlayerPrefs.SetString(typeof(T).Name, data.ToJson());
    }
}