using System;
using System.Collections;
using QFramework;
using UnityEngine;
using UnityEngine.Networking;

public class ImageDownloader : IUtility
{

    public void Download(string url, Action<Texture2D> callback)
    {
        CoroutineController.Instance.StartCoroutine(DoDownload(url, callback));
    }

    IEnumerator DoDownload(string url, Action<Texture2D> callback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Download failed:{request.error}");
                yield break;
            }

            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);

            callback?.Invoke(downloadedTexture);
        }
    }


}