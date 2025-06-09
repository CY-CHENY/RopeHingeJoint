using System;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;

interface ISDKBase
{
    string appId { get; }

    string openId { get; }

    void ShowAd(Action<bool> callBack);

    bool IsRewardPlaying();

    void ShowBannerAd();

    void HideBannerAd();

    void ShowInterstitialAd();

    void SideBar();

    void GetDeviceInfo();

    void DeleteCache(string url);

    TTSystemInfo PrintSystemInfo();

    void SetRankData(JsonData jsonData, Action<bool, string> callback);

    void GetRankData(JsonData jsonData, TTRank.OnGetRankDataSuccessCallback success, TTRank.OnGetRankDataFailCallback fail);
}