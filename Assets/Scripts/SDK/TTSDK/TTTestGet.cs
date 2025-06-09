using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TTSDK.UNBridgeLib.LitJson;

public class TTTestGet : MonoBehaviour
{
    public static IEnumerator sendGet()
    {
        Debug.LogError("HttpGet HttpGet HttpGet");
        var url = "https://hjfzxc.cn/api/douyin/getOpenId?code=111&anonymousCode=222&appid=tt2eb3580eb18671b407";
        var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
        {
            Debug.LogError("DDDDD");
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }
}
