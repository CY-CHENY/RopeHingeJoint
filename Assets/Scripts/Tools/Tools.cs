using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Tools
{
    public static void SetActive(this Image image, bool active)
    {
        image.gameObject.SetActive(active);   
    }
    public static void SetActive(this Button btn, bool active)
    {
        btn.gameObject.SetActive(active);   
    }
    
}

public static class Log
{
    public static void Debug(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    public static void Warning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
    }

    public static void Error(string msg)
    {
        UnityEngine.Debug.LogError(msg);
    }
}
