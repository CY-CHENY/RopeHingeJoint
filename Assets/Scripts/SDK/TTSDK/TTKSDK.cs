using System;
using System.Collections;
using System.Collections.Generic;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;

public class TTKSDK : ISDKBase
{

    private TTAdSDK m_ttAdSDK;

    private TTInAdSDK m_ttInSDK;

    private TTBannerSDK m_ttBannerSDK;
    //花园小管家
    // private string m_appId = "ttd20ba4b64d01ec4e07";
    //触点花艺 HJFdyopen@163.com
    // private string m_appId = "tt788a0e37e7bfd01907";
    //花境管家 HCLdyopen@163.com
    private string m_appId = "tt9b3eed83040b320307";
    private string m_openId = "";

    private string m_clickId = "";

    private string m_code = "";

    private string m_anonymousCode = "";

    public string appId { get { return m_appId; } }
    public string openId { get { return m_openId; } }
    public string clickId { get { return m_clickId; } }

    private bool isUped = false;

    public TTKSDK()
    {
        SDKMgr.InStance().appId = m_appId;
        TTInitSDK();
    }

    private void TTInitSDK()
    {
        TT.InitSDK((code, env) =>
        {
            Debug.Log("Unity message init sdk callback");
            Debug.Log("Unity message code: " + code);
            Debug.Log("Unity message HostEnum: " + env.m_HostEnum);
            Login();
            m_ttAdSDK = new TTAdSDK(this);
            m_ttInSDK = new TTInAdSDK();
            Read_LaunchOption();
            SDKMgr.InStance().UploadJuLiangEvent("active");
            CoroutineRunner.Instance.RunCoroutine(GetBlackList());
        });
    }

    private IEnumerator GetBlackList()
    {
        while (true)
        {
            SDKMgr.InStance().getBlackList();
            yield return new WaitForSeconds(10f);
        }
    }

    public void Read_LaunchOption()
    {
        Debug.Log("LaunchOption: ");
        if (TT.s_ContainerEnv != null)
        {
            LaunchOption launchOption = TT.GetLaunchOptionsSync();
            Debug.Log("启动参数: ");
            Debug.Log("path :" + launchOption.Path);
            Debug.Log("scene :" + launchOption.Scene);
            Debug.Log("subScene :" + launchOption.SubScene);
            Debug.Log("group_id :" + launchOption.GroupId);
            Debug.Log("shareTicket :" + launchOption.ShareTicket);
            Debug.Log("is_sticky :" + launchOption.IsSticky);
            Debug.Log("query :");
            if (launchOption.Query != null)
            {
                foreach (KeyValuePair<string, string> kv in launchOption.Query)
                {
                    if (kv.Value != null)
                    {
                        Debug.Log(kv.Key + ": " + kv.Value);
                        if (kv.Value != null)
                        {
                            if (kv.Key == "clickid")
                            {
                                m_clickId = kv.Value;
                                //DYManager.GetInstance().clickid = kv.Value;
                                SDKMgr.InStance().clickid = kv.Value;
                                Debug.Log(kv.Key + ": " + kv.Value);
                            }
                            Debug.Log(kv.Key + ": " + kv.Value);
                        }
                        else
                        {
                            Debug.Log(kv.Key + ": " + "null ");
                        }
                    }
                }

                Debug.Log("refererInfo :");
                if (launchOption.RefererInfo != null)
                {
                    foreach (KeyValuePair<string, string> kv in launchOption.RefererInfo)
                        if (kv.Value != null)
                            Debug.Log(kv.Key + ": " + kv.Value);
                        else
                            Debug.Log(kv.Key + ": " + "null ");
                }

                Debug.Log("extra :");
                if (launchOption.Extra != null)
                {
                    foreach (KeyValuePair<string, string> kv in launchOption.Extra)
                        if (kv.Value != null)
                            Debug.Log(kv.Key + ": " + kv.Value);
                        else
                            Debug.Log(kv.Key + ": " + "null ");
                }
            }
        }
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="isForce"></param>
    public void Login(bool isForce = true)
    {
        TT.Login((code, anonymousCode, isLogin) =>
        {
            Debug.Log("tt login success");
            Debug.Log($"TestLogin: force:{isForce},code:{code},anonymousCode:{anonymousCode},isLogin:{isLogin}");
            m_code = code;
            m_anonymousCode = anonymousCode;
            SDKMgr.InStance().GetOpenId(code, anonymousCode, m_appId);
            TT.GetUserInfo(GetInfoSuc, GetInfoFail);
        },
        (msg) =>
        {
            Debug.Log($"TestLogin: force:{isForce},{msg}");
        }, isForce);

       // TT.GetUserInfo(GetInfoSuc, GetInfoFail);
    }

    private void GetInfoFail(string errmsg)
    {
        Debug.Log("------GetInfoFail---- "+errmsg);
    }

    private void GetInfoSuc(ref TTUserInfo scuserinfo)
    {
        SDKMgr.InStance().userInfo = scuserinfo;
        Debug.Log("-----GetInfoSuc----- "+scuserinfo.nickName);
    }

    public bool IsRewardPlaying()
    {
        return m_ttAdSDK.IsPlaying();
    }

    /// <summary>
    /// 显示视频激励广告
    /// </summary>
    public void ShowAd(Action<bool> callBack)
    {
        if (!SDKMgr.InStance().isNetWork)
        {
            m_ttAdSDK.showAd(callBack);
        }
        else
        {
            m_ttAdSDK.showAd(callBack);
            // SDKMgr.InStance().pbSDK.IsShowAd(openId, appId, (val) =>
            // {
            //     JsonData responseData = JsonMapper.ToObject(val);
            //     if (responseData["code"].ToString() == "0")
            //     {
            //         m_ttAdSDK.showAd(callBack);
            //     }
            //     else
            //     {
            //         callBack?.Invoke(false);
            //     }
            // });
        }
    }

    /// <summary>
    ///  显示Banner广告
    /// </summary>
    public void ShowBannerAd()
    {
        m_ttBannerSDK.ShowBannerAd();
    }

    public void HideBannerAd()
    {
        m_ttBannerSDK.HideBannerAd();
    }

    /// <summary>
    /// 显示插屏广告
    /// </summary>
    public void ShowInterstitialAd()
    {
        m_ttInSDK.ShowInterstitialAd();
    }

    public void SideBar()
    {
        var data = new TTSDK.UNBridgeLib.LitJson.JsonData
        {
            ["scene"] = "sidebar",
        };
        TT.NavigateToScene(data, () =>
        {
            Debug.Log("成功！！！！！！！");
        }, () =>
        {
            Debug.Log("完成！！！！！！！");

        }, (v, v1) =>
        {
            Debug.Log("失败：" + v + "||" + v1);
        });
    }


    public void GetDeviceInfo()
    {
        if (CanIUse.GetSystemInfo)
        {
            string openId = SDKMgr.InStance().openId != "" ? SDKMgr.InStance().openId : PlayerPrefs.GetString("openid");
            var systemInfo = TT.GetSystemInfo();
            SDKMgr.InStance().ActiveApp(openId, m_appId, m_clickId, JsonUtility.ToJson(systemInfo).ToString());
        }
        else
        {
            Debug.Log("获取设备信息失败");
        }
    }

    public TTSystemInfo PrintSystemInfo()
    {
        if (TTSDK.CanIUse.GetSystemInfo)
        {
            var systemInfo = TT.GetSystemInfo();
            Debug.Log(JsonUtility.ToJson(systemInfo));
            return systemInfo;
        }
        else
        {
            Debug.Log("接口不兼容");
            return null;
        }
    }

    public void DeleteCache(string url)
    {
        if (TT.GetFileSystemManager().IsUrlCached(url))
        {
            var path = TT.GetFileSystemManager().GetLocalCachedPathForUrl(url);
            TT.GetFileSystemManager().UnlinkSync(path);
        }
    }

    public void SetRankData(JsonData jsonData, Action<bool, string> callback)
    {
        TT.SetImRankData(jsonData, callback);
    }

    public void GetRankData(JsonData jsonData, TTRank.OnGetRankDataSuccessCallback success, TTRank.OnGetRankDataFailCallback fail)
    {
        TT.GetImRankData(jsonData, success, fail);
    }
}
