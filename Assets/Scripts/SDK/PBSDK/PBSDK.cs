using System;
using System.Collections;
using System.Collections.Generic;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
public class PBSDK : MonoBehaviour
{
    private SDKHttp mHttp;

    public PBSDK()
    {
        mHttp = new SDKHttp();
    }

    public void GetOpenId(string code, string anonymousCode, string appid)
    {
        string url = "https://hjfzxc.cn/api/douyin/getOpenId";
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("code", code);
        data.Add("anonymousCode", anonymousCode);
        data.Add("appid", appid);

        //AudioMgr.Instance.StartCoroutine(mHttp.HttpGet(url, data, (val) =>
        CoroutineRunner.Instance.RunCoroutine(mHttp.HttpGet(url, data, (val) =>
        {
            JsonData responseData = JsonMapper.ToObject(val);
            if (responseData.ContainsKey("openid"))
            {
                PlayerPrefs.SetString("openid", (string)responseData["openid"]);
                SDKMgr.InStance().openId = responseData["openid"].ToString();
                Debug.Log("获取openid:" + SDKMgr.InStance().openId);
                SDKMgr.InStance().GetDeviceInfo();

                //激活后首次是60秒调用一次插屏广告 后面就每5分钟调用一次，如果正在弹广告就忽略
                CoroutineRunner.Instance.RunCoroutine(AutoPlayInAd());
            }
            else
            {
                SDKMgr.InStance().isNetWork = false;
            }
        }));
    }
    private static bool flag = true;
    private IEnumerator AutoPlayInAd()
    {
        while (true)
        {
            int seconds = flag ? 60 : 60 * 5;
            Debug.Log($"等待:{seconds}");
            yield return new WaitForSeconds(seconds);
            flag = false;
            Debug.Log($"准备播放插屏广告:{SDKMgr.InStance().IsRewardPlaying()}");
            if(!SDKMgr.InStance().IsRewardPlaying())
                SDKMgr.InStance().ShowInterstitialAd();
        }
    }

    public void ActiveApp(string openId, string appId, string clickId, string sysinfo)
    {
        string url = "https://hjfzxc.cn/api/douyin/getActivaIp";
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("openid", openId);
        data.Add("app_id", appId);
        data.Add("click_id", clickId);
        data.Add("sysinfo", sysinfo);
        CoroutineRunner.Instance.RunCoroutine(mHttp.HttpPost(url, data, (val) =>
        {
            JsonData responseData = JsonMapper.ToObject(val);
            Debug.Log("active app success" + val);
            if (responseData["code"].ToString() == "0")
            {
                Debug.Log("active app success");
            }
            else
            {
                Debug.Log("active app fail" + responseData.ToString());
            }
        }));
    }

    public void JuLiangUpLoad(string openId, string appId, string clickId, Action<int> callback = null)
    {
        if (!SDKMgr.InStance().isNetWork) return;
        string url = "https://hjfzxc.cn/api/douyin/game_addiction";
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("open_id", openId);
        data.Add("app_id", appId);
        data.Add("click_id", clickId);
        CoroutineRunner.Instance.RunCoroutine(mHttp.HttpPost(url, data, (val) =>
        {
            JsonData responseData = JsonMapper.ToObject(val);
            callback?.Invoke(int.Parse(responseData["code"].ToString()));
            if (responseData["code"].ToString() == "0")
            {
                Debug.Log("upload  success");
            }
            else
            {
                Debug.Log("upload  fail" + responseData);
            }
        }));
    }

    public void IsShowAd(string openId, string appId, Action<string> action)
    {
        string url = "https://hjfzxc.cn/api/douyin/isAllowedRequest";
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("open_id", openId);
        data.Add("app_id", appId);
        CoroutineRunner.Instance.RunCoroutine(mHttp.HttpGet(url, data, action));
    }

    public void getBlackList()
    {
        if (!SDKMgr.InStance().isNetWork) return;
        string url = "https://hjfzxc.cn/api/douyin/blackList";
        Dictionary<string, string> data = new Dictionary<string, string>();

        //删除黑名单缓存
        SDKMgr.InStance().DeleteCache(url);
        CoroutineRunner.Instance.RunCoroutine(mHttp.HttpGet(url, data, (string list) =>
        {
            JsonData responseData = JsonMapper.ToObject(list);
            string[] newList = new string[responseData["data"].Count];
            if (responseData.ContainsKey("data"))
            {
                int idx = 0;
                foreach (var item in responseData["data"])
                {
                    newList[idx++] = item.ToString();
                }
                Debug.Log("blackList:" + newList.ToString());
                SDKMgr.InStance().blackList = newList;
            }
        }));
    }

    public void UploadJuLiangEvent(string type)
    {
        var pam = new JsonData();
        pam["event_type"] = type;
        pam["context"] = new JsonData
        {
            // ["ad"] = new JsonData { ["callback"] = DYManager.GetInstance().clickid }
            ["ad"] = new JsonData { ["callback"] = SDKMgr.InStance().clickid }
        };
        pam["timestamp"] = DateTime.Now.Ticks;
        CoroutineRunner.Instance.RunCoroutine(mHttp.HttpPost("https://analytics.oceanengine.com/api/v2/conversion", pam.ToJson()));
    }
}
