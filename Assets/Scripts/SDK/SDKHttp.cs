using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.Networking;


public class SDKHttp
{
    public IEnumerator HttpGet(string url, Dictionary<string, string> parameters, Action<string> callback = null, string contentType = "application/json")
    {       
        string finalurl = BuildUrlWithParameters(url, parameters);
        using (UnityWebRequest www = UnityWebRequest.Get(finalurl))
        {
            www.SetRequestHeader("Content-Type", contentType);
            www.timeout = 10000;  // ???ó??????10??
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.error);
                callback?.Invoke("500");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                callback?.Invoke(responseText);
            }
        }
    }


    public IEnumerator HttpPost(string url, Dictionary<string, string> dic, Action<string> callback = null, string contentType = "application/json")
    {        
        var data = new JsonData();
        foreach(var val in dic)
        {
            data[val.Key] = val.Value;
        }
        Debug.Log("请求参数:" + data.ToJson());
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", contentType);
            webRequest.timeout = 10000;  // ???ó??????10??
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data.ToJson());
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"post error: {webRequest.error}");
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                Debug.Log($"post success: {responseJson}");
                callback?.Invoke(responseJson);
            }
        }
    }

    public IEnumerator HttpPost(string url,string dic, Action<string> callback = null, string contentType = "application/json")
    {
        Debug.Log("请求参数:" + dic);
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", contentType);
            webRequest.timeout = 10000; 
            byte[] bodyRaw = Encoding.UTF8.GetBytes(dic);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"post error: {webRequest.error}");
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                Debug.Log($"post success: {responseJson}");
                callback?.Invoke(responseJson);
            }
        }
    }

    public string BuildUrlWithParameters(string baseUrl, Dictionary<string, string> parameters)
    {
        if (parameters.Count == 0)
        {
            return baseUrl;
        }

        string queryString = "";
        int index = 0;
        foreach (KeyValuePair<string, string> param in parameters)
        {
            if (index > 0)
            {
                queryString += "&";
            }
            queryString += param.Key + "=" + UnityWebRequest.EscapeURL(param.Value);
            index++;
        }

        return baseUrl + "?" + queryString;
    }

    public string BuildUrlWithParameters(Dictionary<string, string> parameters)
    {
        string queryString = "";
        int index = 0;
        foreach (KeyValuePair<string, string> param in parameters)
        {
            if (index > 0)
            {
                queryString += "&";
            }
            queryString += param.Key + "=" + UnityWebRequest.EscapeURL(param.Value);
            index++;
        }
        return queryString;
    }
}
