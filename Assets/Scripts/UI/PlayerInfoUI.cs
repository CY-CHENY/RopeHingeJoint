using System.Collections;
using System.Collections.Generic;
using QFramework;
using TTSDK;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour, IController
{
    public RawImage rimg_head;
    public Texture placeholderAvatar;
    public Button btn_changeHead;
    public Button btn_changeArea;
    public Button btn_close;
    public Text txt_name;
    public Text txt_area;

    private bool isInit = false;

    // Start is called before the first frame update
    void Start()
    {
        btn_changeHead.onClick.AddListener(() => { });
        btn_changeArea.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new(UIPageType.SelectAreaUI, UILevelType.UIPage));
        });
        btn_close.onClick.AddListener(() => { UIController.Instance.HidePage(UIPageType.PlayerInfoUI); });
        btn_changeHead.SetActive(false);
        btn_changeArea.SetActive(false);
        isInit = true;
        InitUI();
    }

    void OnEnable()
    {
        InitUI();
    }

    private void InitUI()
    {
        if (!isInit)
            return;
        TTUserInfo info = SDKMgr.InStance().userInfo;

        if (info != null)
        {
            Log.Debug($"info.nickName = {info.nickName},info.country={info.country},info.city={info.city}");
            txt_name.text = info.nickName;
            txt_area.text = info.city == "" ? "地区尚未选择" : info.city;
            var downloadSystem = this.GetSystem<AvatarDownloadSystem>();
            downloadSystem.Download(info.avatarUrl, (downloadedTexture) => { rimg_head.texture = downloadedTexture; },
                () => { rimg_head.texture = placeholderAvatar; });
        }
        else
        {
            txt_name.text = "";
            txt_area.text = "地区尚未选择";
            rimg_head.texture = placeholderAvatar;
        }
    }

    IEnumerator DownloadAvatar(string imageUrl)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            // 发送请求
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 获取下载的纹理并显示
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
                rimg_head.texture = downloadedTexture;
            }
            else
            {
                Log.Error("头像加载失败: " + webRequest.error);
                // 显示错误或使用占位图
                rimg_head.texture = placeholderAvatar;
            }
        }
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}