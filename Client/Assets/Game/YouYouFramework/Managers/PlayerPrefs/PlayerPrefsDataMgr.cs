using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;


public class PlayerPrefsDataMgr : Observable<PlayerPrefsDataMgr, PlayerPrefsDataMgr.EventName>
{
    public enum EventName : uint
    {
        //主音量
        MasterVolume,
        //背景音乐音量
        BGMVolume,
        //音效音量
        AudioVolume,
        //游戏暂停
        GamePause,
        //最大帧率
        FrameRate,
        //屏幕分辨率
        Screen,
        //画质等级
        QualityLevel,

        //测试事件
        TestEvent,
    }

    public void Init()
    {
        dicInt = GetObject<Dictionary<EventName, int>>("dicInt");
        dicFloat = GetObject<Dictionary<EventName, float>>("dicFloat");
        dicString = GetObject<Dictionary<EventName, string>>("dicString");

        GameEntry.PlayerPrefs.SetFloatHas(EventName.MasterVolume, 1);
        GameEntry.PlayerPrefs.SetFloatHas(EventName.AudioVolume, 1);
        GameEntry.PlayerPrefs.SetFloatHas(EventName.BGMVolume, 1);
        GameEntry.PlayerPrefs.SetIntHas(EventName.FrameRate, 2);
    }
    public void SaveDataAll()
    {
        SetObject("dicInt", dicInt);
        SetObject("dicFloat", dicFloat);
        SetObject("dicString", dicString);

        PlayerPrefs.Save();
    }
    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }


    private Dictionary<EventName, int> dicInt = new Dictionary<EventName, int>();
    public int GetInt(EventName key, int defaultValue = 0)
    {
        if (dicInt.TryGetValue(key, out int retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetInt(EventName key, int value, object param = null)
    {
        dicInt[key] = value;
        Dispatch(key, param);
    }
    public void SetIntAdd(EventName key, int value)
    {
        SetInt(key, GetInt(key) + value);
    }
    public void SetIntHas(EventName key, int value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetInt(key, value);
    }
    public bool GetBool(EventName key, bool defaultValue)
    {
        return GetInt(key, defaultValue ? 1 : 0) == 1;
    }
    public bool GetBool(EventName key)
    {
        return GetInt(key) == 1;
    }
    public void SetBool(EventName key, bool value, object param = null)
    {
        SetInt(key, value ? 1 : 0);
        Dispatch(key, param);
    }
    public void SetBoolHas(EventName key, bool value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetBool(key, value);
    }


    private Dictionary<EventName, float> dicFloat = new Dictionary<EventName, float>();
    public float GetFloat(EventName key, float defaultValue = 0)
    {
        if (dicFloat.TryGetValue(key, out float retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetFloat(EventName key, float value, object param = null)
    {
        dicFloat[key] = value;
        Dispatch(key, param);
    }
    public void SetFloatAdd(EventName key, float value)
    {
        SetFloat(key, GetFloat(key) + value);
    }
    public void SetFloatHas(EventName key, float value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetFloat(key, value);
    }

    private Dictionary<EventName, string> dicString = new Dictionary<EventName, string>();
    public string GetString(EventName key, string defaultValue = null)
    {
        if (dicString.TryGetValue(key, out string retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetString(EventName key, string value, object param = null)
    {
        dicString[key] = value;
        Dispatch(key, param);
    }
    public void SetStringHas(EventName key, string value)
    {
        if (dicString.ContainsKey(key)) return;
        SetString(key, value);
    }


    public T GetObject<T>(string key) where T : new()
    {
        string value = PlayerPrefs.GetString(key);
        if (!string.IsNullOrEmpty(value))
        {
            return value.ToObject<T>();
        }
        else
        {
            return new T();
        }
    }
    public void SetObject<T>(string key, T data)
    {
        PlayerPrefs.SetString(key, data.ToJson());
    }
}