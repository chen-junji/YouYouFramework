using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using YouYou;


public class PlayerPrefsDataMgr
{
    public void Init()
    {
        dicInt = GetObject<Dictionary<string, int>>("dicInt");
        dicFloat = GetObject<Dictionary<string, float>>("dicFloat");
        dicString = GetObject<Dictionary<string, string>>("dicString");
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


    private Dictionary<string, int> dicInt = new Dictionary<string, int>();
    public int GetInt(string key, int defaultValue = 0)
    {
        if (dicInt.TryGetValue(key, out int retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetInt(string key, int value)
    {
        dicInt[key] = value;
    }
    public void SetIntAdd(string key, int value)
    {
        SetInt(key, GetInt(key) + value);
    }
    public void SetIntHas(string key, int value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetInt(key, value);
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
    }
    public void SetBoolHas(string key, bool value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetBool(key, value);
    }


    private Dictionary<string, float> dicFloat = new Dictionary<string, float>();
    public float GetFloat(string key, float defaultValue = 0)
    {
        if (dicFloat.TryGetValue(key, out float retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetFloat(string key, float value, object param = null)
    {
        dicFloat[key] = value;
    }
    public void SetFloatAdd(string key, float value)
    {
        SetFloat(key, GetFloat(key) + value);
    }
    public void SetFloatHas(string key, float value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetFloat(key, value);
    }

    private Dictionary<string, string> dicString = new Dictionary<string, string>();
    public string GetString(string key, string defaultValue = null)
    {
        if (dicString.TryGetValue(key, out string retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetString(string key, string value, object param = null)
    {
        dicString[key] = value;
    }
    public void SetStringHas(string key, string value)
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