using System;
using QFramework;
using UnityEngine;

public class PlayerPrefsStorage : IUtility
{
    public void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public int LoadInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public void SaveObject<T>(string key, T value)
    {
        string json = JsonUtility.ToJson(value);
        PlayerPrefs.SetString(key, json);
    }

    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public T LoadObject<T>(string key)
    {
        return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key));
    }
}