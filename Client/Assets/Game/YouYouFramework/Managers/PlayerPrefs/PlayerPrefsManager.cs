using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;


public class PlayerPrefsManager
{
    public PlayerPrefsData Data { get; private set; }

    public PlayerPrefsManager()
    {
    }
    public void Dispose()
    {
        SaveDataAll();
    }
    public void Init()
    {
        Data = GetObject<PlayerPrefsData>("PlayerPrefsData");
		
		GameEntry.PlayerPrefs.SetFloatHas(CommonEventId.PlayerAudioVolume, 0.5f);
        GameEntry.PlayerPrefs.SetFloatHas(CommonEventId.PlayerBGMVolume, 0.5f);
    }
    public void SaveDataAll()
    {
        SetObject("PlayerPrefsData", Data);
        PlayerPrefs.Save();
    }
    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        Data = new PlayerPrefsData();
    }

    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
    public bool DeleteKey(string key)
    {
        bool has = PlayerPrefs.HasKey(key);
        PlayerPrefs.DeleteKey(key);
        return has;
    }

    public bool GetBool(string key, bool defaultValue)
    {
        return GetInt(key, defaultValue ? 1 : 0) == 1;
    }
    public bool GetBool(string key)
    {
        return GetInt(key) == 1;
    }
    public void SetBool(string key, bool value, object param = null)
    {
        SetInt(key, value ? 1 : 0);
        GameEntry.Event.Common.Dispatch(key, param);
    }
    public void SetBoolHas(string key, bool value)
    {
        if (HasKey(key)) return;
        SetBool(key, value);
    }

    public int GetInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    public int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }
    public void SetInt(string key, int value, object param = null)
    {
        PlayerPrefs.SetInt(key, value);
        GameEntry.Event.Common.Dispatch(key, param);
    }
    public void SetIntAdd(string key, int value)
    {
        SetInt(key, GetInt(key) + value);
    }
    public void SetIntHas(string key, int value)
    {
        if (HasKey(key)) return;
        SetInt(key, value);
    }

    public float GetFloat(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }
    public float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }
    public void SetFloat(string key, float value, object param = null)
    {
        PlayerPrefs.SetFloat(key, value);
        GameEntry.Event.Common.Dispatch(key, param);
    }
    public void SetFloatAdd(string key, float value)
    {
        SetFloat(key, GetFloat(key) + value);
    }
    public void SetFloatHas(string key, float value)
    {
        if (HasKey(key)) return;
        SetFloat(key, value);
    }

    public string GetString(string key, string defaultValue)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }
    public string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }
    public void SetString(string key, string value, object param = null)
    {
        PlayerPrefs.SetString(key, value);
        GameEntry.Event.Common.Dispatch(key, param);
    }
    public void SetStringHas(string key, string value)
    {
        if (HasKey(key)) return;
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