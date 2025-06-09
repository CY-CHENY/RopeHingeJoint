using TTSDK;
using UnityEngine;

/// <summary>
/// banner SDK
/// </summary>
public class TTBannerSDK
{
    private TTBannerStyle m_style = new TTBannerStyle();

    private TTBannerAd m_bannerAdIns;

    private string m_bannerId = "";

    private string TAG = "TTSDK: ";

    public TTBannerSDK()
    {
       // CreateBannerSDK();
    }

    private void CreateBannerSDK()
    {
        m_style.top = 10;
        m_style.left = 10;
        m_style.width = 320;

        if (m_bannerAdIns != null && m_bannerAdIns.IsInvalid())
        {
            m_bannerAdIns.Destroy();
            m_bannerAdIns = null;
        }

        if (m_bannerAdIns == null)
        {
            var param = new CreateBannerAdParam
            {
                BannerAdId = m_bannerId,
                Style = m_style,
                AdIntervals = 60
            };
            m_bannerAdIns = TT.CreateBannerAd(param);
            m_bannerAdIns.OnError += OnAdError;
            m_bannerAdIns.OnClose += OnClose;
            m_bannerAdIns.OnResize += OnBannerResize;
            m_bannerAdIns.OnLoad += OnBannerLoaded;
        }
        m_bannerAdIns.Show();
    }

    void OnAdError(int iErrCode, string errMsg)
    {
        Debug.Log(TAG + "创建Banner广告失败: " + iErrCode + "  " + errMsg);
    }

    private void OnBannerLoaded()
    {
        m_bannerAdIns?.Show();
        //m_result.text = m_result.text + "/n" + "banner锟斤拷锟絣oaded";
    }

    private void OnBannerResize(int width, int height)
    {
        Debug.Log($"OnBannerResize - width:{width} height:{height}");
    }

    private void OnClose()
    {
        Debug.Log("Banner SDK Close");
    }

    //展示
    public void ShowBannerAd()
    {
        m_bannerAdIns?.Show();
    }

    //隐藏
    public void HideBannerAd()
    {
        m_bannerAdIns?.Hide();
    }

    //重新设置大小
    private void ResizeBannerAd()
    {
        //m_style.top = int.Parse(inputTop.text);
        //m_style.left = int.Parse(inputLeft.text);
        //m_style.width = int.Parse(inputWidth.text);
        //m_bannerAdIns?.ReSize(m_style);
    }

    //销毁
    public void DestroyBannerAd()
    {
        m_bannerAdIns?.Destroy();
    }

}
