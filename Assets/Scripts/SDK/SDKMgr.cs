using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.Networking;


enum EAppPlatform
{
    TT,
}

public class SDKMgr
{
    private static SDKMgr instance = null;

    private bool mInit = false;

    private EAppPlatform mPlatform = EAppPlatform.TT;

    private string mOpenId = null;

    private ISDKBase m_curSDK;

    private PBSDK m_pbSDK;

    private string m_appId = null;

    private string[] m_blackList = null;

    private bool m_isNetWork = true;

    public TTUserInfo userInfo;

    public string[] blackList { get { return m_blackList; } set { m_blackList = value; } }
    public string openId { get { return mOpenId; } set { mOpenId = value; } }

    //public PBSDK pbSDK { get { return m_pbSDK; } }

    public string appId { get { return m_appId; } set { m_appId = value; } }
    public bool isNetWork { get { return m_isNetWork; } set { m_isNetWork = value; } }

    public string clickid = "";

    public static SDKMgr InStance()
    {
        if (instance == null)
        {
            instance = new SDKMgr();
        }
        return instance;
    }

    public void Init()
    {
        if (mInit) return;
        mInit = true;
        m_pbSDK = new PBSDK();
        switch (mPlatform)
        {
            case EAppPlatform.TT:
                m_curSDK = new TTKSDK();
                break;
        }
        Debug.Log("SDK init success");
    }

    public void ShowAd(Action<bool> callBack)
    {
        if (!m_isNetWork)
        {
            m_curSDK.ShowAd(callBack);
        }
        else
        {
            if (m_blackList == null || m_blackList.Length <= 0)
            {
                m_curSDK.ShowAd(callBack);
            }
            else
            {
                // Debug.Log(m_blackList.Length);
                // foreach (var item in m_blackList)
                // {
                //     Debug.Log($"{item}---->{openId}");
                // }
                if (m_blackList.Contains(openId))
                {
                    //Debug.Log($"{openId} 在黑名单内 无法看广告");
                    callBack?.Invoke(false);
                }
                else
                {
                    m_curSDK.ShowAd(callBack);
                }
            }
        }
    }

    public void ShowBannerAd()
    {
        m_curSDK.ShowBannerAd();
    }

    public void HideBannerAd()
    {
        m_curSDK.HideBannerAd();
    }

    public void ShowInterstitialAd()
    {
        m_curSDK.ShowInterstitialAd();
    }

    public void GetDeviceInfo()
    {
        m_curSDK.GetDeviceInfo();
    }

    /// <summary>
    /// 侧边栏
    /// </summary>
    public void Sidebar()
    {

        m_curSDK.SideBar();
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    public TTSDK.TTSystemInfo PrintSystemInfo()
    {
        return m_curSDK.PrintSystemInfo();
    }

    public void SetRankData(JsonData jsonData, Action<bool, string> callback = null)
    {
        m_curSDK?.SetRankData(jsonData, callback);
    }

    public void GetRankData(JsonData jsonData, TTRank.OnGetRankDataSuccessCallback success = null, TTRank.OnGetRankDataFailCallback fail = null)
    {
        m_curSDK?.GetRankData(jsonData, success, fail);
    }


    public IEnumerator PostRequest(string uri, string data, string contentType)
    {
        Uri baseUri = new Uri("https://analytics.oceanengine.com");
        Uri fullUri = new Uri(baseUri, uri);

        Debug.Log("调用巨量 ");

        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(fullUri, data))
        {
            webRequest.SetRequestHeader("Content-Type", contentType);
            webRequest.timeout = 10000;  // 设置超时时间为10秒

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log($"请求错误: {webRequest.error}");
                Debug.Log($"HTTP 状态码: {webRequest.responseCode}");
                Debug.Log($"请求数据: {data}");
                Debug.Log($"服务器响应: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"请求成功: {webRequest.downloadHandler.text}");
                Debug.Log($"HTTP 状态码: {webRequest.responseCode}");

                try
                {
                    // 解析服务器返回的 JSON 数据
                    string responseJson = webRequest.downloadHandler.text;
                    Debug.Log($"服务器响应: {responseJson}");

                    JsonData responseData = JsonMapper.ToObject(responseJson);
                    if (responseData.ContainsKey("code") && responseData.ContainsKey("message"))
                    {
                        int code = (int)responseData["code"];
                        string message = (string)responseData["message"];
                        if (code != 0)
                        {
                            Debug.Log($"服务器返回错误: {message}");
                        }
                        else
                        {
                            Debug.Log($"code: {code}     mesage{message}");
                        }
                    }
                    else
                    {
                        Debug.Log("服务器响应格式不正确");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"解析服务器响应失败: {ex.Message}");
                    //Debug.LogError($"服务器响应: {responseJson}");
                }
            }
        }
    }


    // //发送HTTP请求
    // public IEnumerator GetRequest(string uri, Dictionary<string, string> parameters, string contentType, Action<int> callback)
    // {

    //     string finalurl = BuildUrlWithParameters(uri, parameters);

    //     using (UnityWebRequest webRequest = UnityWebRequest.Get(finalurl))
    //     {
    //         webRequest.SetRequestHeader("Content-Type", contentType);
    //         webRequest.timeout = 10000;  // 设置超时时间为10秒


    //         yield return webRequest.SendWebRequest();


    //         // 检查请求结果
    //         if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
    //             webRequest.result == UnityWebRequest.Result.ProtocolError)
    //         {
    //             // 处理错误
    //             Debug.Log("Error: " + webRequest.error);
    //             callback?.Invoke(-1);
    //         }
    //         else
    //         {
    //             // 处理成功响应
    //             string responseText = webRequest.downloadHandler.text;
    //             Debug.Log("Response: " + responseText);

    //             JsonData responseData = JsonMapper.ToObject(responseText);

    //             Debug.Log(responseData);

    //             if (responseData.ContainsKey("openid"))
    //             {
    //                 PlayerPrefs.SetString("openid", (string)responseData["openid"]);
    //             }
    //             if (responseData.ContainsKey("code"))
    //             {
    //                 callback?.Invoke((int)responseData["code"]);
    //             }

    //         }


    //     }
    // }

    public void ReportJuliang(string type)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        var pam = new JsonData();
        pam["event_type"] = type;
        pam["context"] = new JsonData
        {
            // ["ad"] = new JsonData { ["callback"] = DYManager.GetInstance().clickid }
            ["ad"] = new JsonData { ["callback"] = SDKMgr.InStance().clickid }
        };
        pam["timestamp"] = DateTime.Now.Ticks;

        UnityEngine.Debug.Log("pam:" + pam);

        CoroutineRunner.Instance.StartCoroutine(PostRequest("/api/v2/conversion", pam.ToJson(), "application/json"));
#endif
    }
    /*
    public IEnumerator PostNew(string uri, string data, string contentType, Action<int> callback)
    {
        Uri baseUri = new Uri(uri);

        Debug.Log("调用巨量 ");

        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(baseUri, data))
        {
            webRequest.SetRequestHeader("Content-Type", contentType);
            webRequest.timeout = 10000;  // 设置超时时间为10秒

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log($"请求错误: {webRequest.error}");
                Debug.Log($"HTTP 状态码: {webRequest.responseCode}");
                Debug.Log($"请求数据: {data}");
                Debug.Log($"服务器响应: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"请求成功 PostNew : {webRequest.downloadHandler.text}");
                Debug.Log($"HTTP 状态码: {webRequest.responseCode}");

                try
                {
                    // 解析服务器返回的 JSON 数据
                    string responseJson = webRequest.downloadHandler.text;
                    Debug.Log($"服务器响应: {responseJson}");

                    JsonData responseData = JsonMapper.ToObject(responseJson);

                    Debug.Log("code:" + responseData.ContainsKey("code"));
                    if (responseData.ContainsKey("code"))
                    {
                        callback?.Invoke((int)responseData["code"]);
                    }

                }
                catch (Exception ex)
                {
                    Debug.Log($"解析服务器响应失败: {ex.Message}");
                    //Debug.LogError($"服务器响应: {responseJson}");
                }
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
    */
    public void DeleteCache(string url)
    {
        m_curSDK?.DeleteCache(url);
    }

    public bool IsRewardPlaying()
    {
        return m_curSDK == null ? false : m_curSDK.IsRewardPlaying();
    }

    #region PBSDK
    public void JuLiangUpLoad(string openId, string appId, string clickId, Action<int> callback = null)
    {
        m_pbSDK?.JuLiangUpLoad(openId, appId, clickId, callback);
    }

    public void getBlackList()
    {
        m_pbSDK?.getBlackList();
    }

    public void UploadJuLiangEvent(string type)
    {
        m_pbSDK?.UploadJuLiangEvent(type);
    }

    public void GetOpenId(string code, string anonymousCode, string appid)
    {
        m_pbSDK?.GetOpenId(code, anonymousCode, appid);
    }

    public void ActiveApp(string openId, string appId, string clickId, string sysinfo)
    {
        m_pbSDK?.ActiveApp(openId, appId, clickId, sysinfo);
    }

    public void GetTTUserInfo()
    {
        
    }
    #endregion

}


