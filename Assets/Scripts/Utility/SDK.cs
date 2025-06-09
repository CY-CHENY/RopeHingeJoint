using System;
using System.Collections;
using QFramework;
using TTSDK;
#if !UNITY_EDITOR && UNITY_WEBGL
using TTSDK.UNBridgeLib.LitJson;
#endif
using Utils;

public class SDKUtility : IUtility
{
    private bool addEnable = true;
    public void Init()
    {
        //   SDKMgr.InStance().Init();
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        SDKMgr.InStance().Init();
#endif
    }

    public void ShowAd(Action<bool> callback)
    {
        if (!addEnable)
            return;
        addEnable = false;
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        SDKMgr.InStance().ShowAd(callback);
#else
        callback?.Invoke(true);
#endif

        Util.DelayExecuteWithSecond(8f, () =>
        {
            addEnable = true;
        });
    }

    public void ShowInterstitialAd()
    {
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        SDKMgr.InStance().ShowInterstitialAd();
#endif
    }

    //     public void ReportTest(string type)
    //     {
    // #if !UNITY_EDITOR && UNITY_WEBGL
    //         var pam = new JsonData();
    //         pam["event_type"] = type;
    //         pam["context"] = new JsonData
    //         {
    //             // ["ad"] = new JsonData { ["callback"] = DYManager.GetInstance().clickid }
    //             ["ad"] = new JsonData { ["callback"] = SDKMgr.InStance().clickid }
    //         };
    //         pam["timestamp"] = DateTime.Now.Ticks;

    //         UnityEngine.Debug.Log("pam:" + pam);

    //         //StartCoroutine(DYManager.GetInstance().PostRequest("/api/v2/conversion", pam.ToJson(), "application/json"));
    //         CoroutineRunner.Instance.StartCoroutine(SDKMgr.InStance().PostRequest("/api/v2/conversion", pam.ToJson(), "application/json"));
    // #endif
    //     }

    public void SetRankData(int level)
    {
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        var pam = new JsonData();
        pam["dataType"] = 0;
        pam["value"] = level.ToString();
        pam["priority"] = 0;
        pam["zoneId"] = "default"; //"default";

        SDKMgr.InStance().SetRankData(pam, (bool success, string errMsg) =>
        {
            UnityEngine.Debug.Log($"SetRankData:{success} : {errMsg}");
        });
#endif
    }

    public void GetRankData(string relationType, int pageNum, int pageSize, TTRank.OnGetRankDataSuccessCallback success, TTRank.OnGetRankDataFailCallback fail)
    {
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        var pam = new JsonData();
        pam["dataType"] = 0;
        pam["relationType"] = relationType;//"all"; //"friend"
        pam["pageSize"] = pageSize;//10;
        pam["pageNum"] = pageNum;//从1开始;
        pam["zoneId"] = "default";
        pam["rankType"] = "all"; //day、week、month、all
        SDKMgr.InStance().GetRankData(pam, success, fail);

        // SDKMgr.InStance().GetRankData(pam, (ref RankData rankData) =>
        // {
        //     UnityEngine.Debug.Log($"GetRankData Success: {rankData}");
        // },
        // (errMsg) =>
        // {
        //     UnityEngine.Debug.Log($"GetRankData Fail: {errMsg}");
        // });
#endif

    }

    public void VibrateLong()
    {
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        TT.VibrateLong(null);
#endif
    }

    public void VibrateShort()
    {
#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
        TT.VibrateShort(null);
#endif
    }

#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
    public TTSDK.TTSystemInfo PrintSystemInfo()
    {
        return SDKMgr.InStance().PrintSystemInfo();
    }
#endif
}