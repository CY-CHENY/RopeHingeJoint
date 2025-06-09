using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarDownloadSystem : AbstractSystem
{
    private Dictionary<string, List<Action<Sprite>>> mPendingRequests = new Dictionary<string, List<Action<Sprite>>>();
    public void Download(string url, Action<Texture2D> onSuccess,Action onFail)
    {
        CoroutineController.Instance.StartCoroutine(DownloadAvatar(url,onSuccess,onFail));
    }
    
    IEnumerator DownloadAvatar(string imageUrl,Action<Texture2D> succ = null,Action fail =null)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            // 发送请求
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 获取下载的纹理并显示
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
                succ?.Invoke(downloadedTexture);
            }
            else
            {
                Log.Error("头像加载失败: " + webRequest.error);
                // 显示错误或使用占位图
                fail?.Invoke();
            }
        }
    }
    
    
   

    protected override void OnInit()
    {
        // 初始化缓存等
    }
    
    
}
