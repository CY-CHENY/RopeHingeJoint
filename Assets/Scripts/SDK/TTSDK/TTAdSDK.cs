using System;
using TTSDK;
using UnityEngine;


/// <summary>
/// 奖励视频广告SDK
/// </summary>
public class TTAdSDK
{
    private TTRewardedVideoAd m_videoAd;
    //花园小管家
    // private string mRewardedVideoAdID = "1n9b4m7mm17n22f0if";
    //触点花艺
    //  private string mRewardedVideoAdID = "251jdkfnah7a1b57ga";
    //花境管家
     private string mRewardedVideoAdID = "asnv4cpgmdjfg08a2i";
    private Action<bool> m_callBack;

    private TTKSDK m_ttkSDK;

    private bool isPlaying = false;

    public TTAdSDK(TTKSDK tTKSDK)
    {
        m_ttkSDK = tTKSDK;
        CreateRewardAd();
        m_videoAd.Load();
    }

    private void CreateRewardAd()
    {
        var param = new CreateRewardedVideoAdParam { AdUnitId = mRewardedVideoAdID };
        m_videoAd = TT.CreateRewardedVideoAd(param);
        m_videoAd.OnClose += (ended, count) =>
        {
            m_videoAd.Load();
            isPlaying = false;
            Debug.Log($"TTSDK Ad OnClose: {ended}, count: {count}");
            if (ended)
            {
                //中台 关键行为回传
                string openId = SDKMgr.InStance().openId != null ? SDKMgr.InStance().openId : PlayerPrefs.GetString("openid");
                SDKMgr.InStance().JuLiangUpLoad(openId, SDKMgr.InStance().appId, m_ttkSDK.clickId);
                //巨量ROI
                SDKMgr.InStance().ReportJuliang("lt_roi");

                m_ttkSDK.Read_LaunchOption();
            }
            m_callBack?.Invoke(ended);
        };
        m_videoAd.OnError += (errorCode, errorMessage) => Debug.Log($"TTSDK Ad OnError: {errorCode}");
    }

    // private void CreateRewardAd(string adID)
    // {
    //     if(m_videoAd != null)
    //     {
    //         m_videoAd.Destroy();
    //     }
    //     m_videoAd = null;

    //     var param = new CreateRewardedVideoAdParam { AdUnitId = adID };
    //     m_videoAd = TT.CreateRewardedVideoAd(param);
    //     m_videoAd.OnLoad += ()=>{
    //         m_videoAd.Show();
    //     };

    //     m_videoAd.OnClose += (ended, count) =>
    //     {
    //         Debug.Log($"TTSDK Ad OnClose: {ended}, count: {count}");
    //         if (ended)
    //         {
    //             string openId = SDKMgr.InStance().openId != null ? SDKMgr.InStance().openId : PlayerPrefs.GetString("openid");
    //             SDKMgr.InStance().JuLiangUpLoad(openId, SDKMgr.InStance().appId, m_ttkSDK.clickId);
    //             m_ttkSDK.Read_LaunchOption();
    //         }
    //         m_callBack?.Invoke(ended);
    //     };
    //     m_videoAd.OnError += (errorCode, errorMessage) => Debug.Log($"TTSDK Ad OnError: {errorCode}");
    // }

    public void showAd(Action<bool> callBack)
    {
        m_callBack = callBack;
        //CreateRewardAd(adID);
        m_videoAd.Show();
        isPlaying = true;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    
}
