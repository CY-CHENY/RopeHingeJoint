using TTSDK;
using UnityEngine;


/// <summary>
/// 插屏sdk
/// </summary>
public class TTInAdSDK
{
    private TTInterstitialAd m_InterAdIns = null;
    //花园小管家
    // private string mInterstitialAdID = "2mfuk0ma7h616ed294";
    //触点花艺
    // private string mInterstitialAdID = "73igm7fb10m11j8gqv";
    //花境管家
    private string mInterstitialAdID = "2u30a389qmk4rr48as";
    public TTInAdSDK()
    {
        CreateInterstitialAd();
    }

    private void CreateInterstitialAd()
    {
        var param = new CreateInterstitialAdParam { InterstitialAdId = mInterstitialAdID };
        m_InterAdIns = TT.CreateInterstitialAd(param);
        m_InterAdIns.OnClose += () => Debug.Log("插屏广告关闭");
        m_InterAdIns.OnLoad += () => Debug.Log("插屏广告加载");
        m_InterAdIns.OnError += (code, message) => Debug.Log($"错误 ： {code}  {message}");
    }

    public void LoadInterstitialAd()
    {
        if (m_InterAdIns != null)
            m_InterAdIns.Load();
        else
        {
            Debug.Log("插屏AD未创建");
        }
    }

    public void ShowInterstitialAd()
    {
        Debug.Log("显示插屏AD");
        if (m_InterAdIns != null)
            m_InterAdIns.Show();
        else
        {
            Debug.Log("插屏AD未创建");
        }
    }

    public void DestroyInterstitialAd()
    {
        Debug.Log("销毁插屏AD");
        if (m_InterAdIns != null)
            m_InterAdIns.Destroy();
        m_InterAdIns = null;
    }


}
