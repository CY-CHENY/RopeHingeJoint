using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class DownloadResUI : MonoBehaviour, IController, ICanSendEvent
{
    private Slider slider;

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    void Start()
    {
        DownloadRes();
    }

    private void DownloadRes()
    {
        this.GetSystem<IAddressableSystem>().SetCallBack(
            OnCheckCompleteNeedUpate: (size) =>
            {
                Debug.Log("需要更新");
                this.GetSystem<IAddressableSystem>().DownloadAsset();
            },
            OnCompleteDownload: () =>
            {
                Debug.Log("下载完成");
                FinishDownloadRes(true);
            },
            OnCheckCompleteNoUpdate: () =>
            {
                Debug.Log("不需要更新");
                FinishDownloadRes(true);
            },
            OnUpdate: (percent, totalSize) =>
            {
                UpdateProgress(percent, totalSize);
            }
        );

        this.GetSystem<IAddressableSystem>().GetDownloadAssets();
    }

    private void FinishDownloadRes(bool isFinish)
    {
        this.SendCommand(new FinishDownloadResCommand(isFinish));
    }

    private float lastProgress = 0;
    private float lastTime = -1;
    private void UpdateProgress(float value, float totalBytes)
    {
        if (value < 0.01f)
            return;

        if (value < 1f)
        {
            var t = Time.realtimeSinceStartup;
            float speed = 0f;
            try
            {
                speed = ((value - lastProgress) * totalBytes) / (t - lastTime);
                speed /= 1024;
            }
            catch
            {

            }

            if (lastTime < 0)
            {
                speed = 0f;
                lastTime = t;
            }
            slider.value = value;

            // if (speed > 1024) {
            //     showText.text = (speed / 1024).ToString("0.0") + "M/s";
            // } else {
            //     showText.text = speed.ToString("0.0") + "K/s";
            // }
        }
    }

    void Awake()
    {
        slider = transform.Find("Slider").GetComponent<Slider>();
        slider.value = 0.1f;

        // this.RegisterEvent<InitializeFailedEvent>(OnHandleInitializeFailed);
        // this.RegisterEvent<PatchStepChangeEvent>(OnHandlePatchStepChange);
        // this.RegisterEvent<FoundUpdateFilesEvent>(OnHandleFoundUpdateFiles);
        // this.RegisterEvent<PackageVersionRequestFailedEvent>(OnHandlePackageVersionRequestFailed);
        // this.RegisterEvent<PackageManifestUpdateFailedEvent>(OnHandlePackageManifestUpdateFailed);
        // this.RegisterEvent<WebFileDownloadFailedEvent>(OnHandleWebFileDownloadFailed);
        // this.RegisterEvent<DownloadUpdateEvent>(OnHandleDownloadUpdate);
    }
    // private float timer = 0f;
    // void Update()
    // {
    //     timer += Time.deltaTime;

    //     if(timer < 0.5f)
    //     {
    //         slider.value = (timer / 0.5f) * 0.8f;
    //     }
    // }

    // private void OnHandleDownloadUpdate(DownloadUpdateEvent e)
    // {
    //     slider.value =  0.8f + 0.4f *( (float)e.CurrentDownloadCount / e.TotalDownloadCount);
    //     // string currentSizeMB = (e.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
    //     // string totalSizeMB = (e.TotalDownloadSizeBytes / 1048576f).ToString("f1");
    //     // _tips.text = $"{e.CurrentDownloadCount}/{e.TotalDownloadCount} {currentSizeMB}/{totalSizeMB}";
    // }

    // private void OnHandleWebFileDownloadFailed(WebFileDownloadFailedEvent e)
    // {
    //     ShowMessageBox($"文件:{e.FileName} 下载失败。",
    //         () => { this.SendEvent<TryDownloadWebFilesEvent>(); });
    // }

    // private void OnHandlePatchStepChange(PatchStepChangeEvent e)
    // {
    //     //_tips.text = e.Tips;
    //     Debug.Log(e.Tips);
    // }

    // private void OnHandleInitializeFailed(InitializeFailedEvent e)
    // {
    //     ShowMessageBox($"初始化包体失败!",
    //         () => { this.SendEvent<TryInitializeEvent>(); });
    // }

    // private void OnHandlePackageVersionRequestFailed(PackageVersionRequestFailedEvent e)
    // {
    //     ShowMessageBox($"更新资源版本失败，请检查网络状态。",
    //         () => { this.SendEvent<TryUpdatePackageVersionEvent>(); });
    // }

    // private void OnHandlePackageManifestUpdateFailed(PackageManifestUpdateFailedEvent e)
    // {
    //     ShowMessageBox($"更新资源清单失败，请检查网络状态。",
    //         () => { this.SendEvent<TryUpdatePatchManifestEvent>(); });
    // }

    // private void OnHandleFoundUpdateFiles(FoundUpdateFilesEvent e)
    // {
    //     float sizeMB = e.TotalSizeBytes / 1048576f;
    //     sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
    //     string totalSizeMB = sizeMB.ToString("f1");
    //     ShowMessageBox($"发现可下载资源，需要下载{e.TotalCount}个文件，需要{totalSizeMB}MB",
    //         () => { this.SendEvent<BeginDownloadWebFilesEvent>(); });
    // }

    // private void ShowMessageBox(string content, System.Action ok)
    // {
    //     // InfoConfirmInfo info = new InfoConfirmInfo(content: content, success: ok, confirmText: "OK",
    //     //     type: ConfirmAlertType.Single);


    //     // UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.AssetsUpdateAlert, UILevelType.Alert, info,
    //     //     isLocal: true));
    // }
}
