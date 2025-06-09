using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public enum UIPageType
{
    SettingsUI,
    LoadingUI,
    GameUI,
    DownloadResUI,
    AssetsUpdateAlert,
    FuHuoUI,
    GameWinUI,
    GameLoseUI,
    GuideUI,
    MainUI,
    PlayerInfoUI,
    FeedbackUI,
    CollectGamesUI,
    SelectAreaUI,
    SignInUI,
    GetRewardUI,
    MainRankingUI,
    RankingListUI
}

public enum UILevelType
{
    Prepare = 0,
    Main,
    UIPage,
    Popup,
    Alart,
    Debug
}

public class UIController : PersistentMonoSingleton<UIController>, IController
{
     public Transform[] levels;
    private Dictionary<UIPageType, GameObject> pagesDict = new Dictionary<UIPageType, GameObject>();

    private Dictionary<UILevelType, LinkedList<UIPageType>> pagesGroup =
        new Dictionary<UILevelType, LinkedList<UIPageType>>();

    public RectTransform canvasRect;

    private void Awake()
    {
        base.Awake();
    }

    public async UniTask InitUI()
    {
        foreach (UILevelType value in Enum.GetValues(typeof(UILevelType)))
        {
            pagesGroup[value] = new LinkedList<UIPageType>();
        }

        canvasRect = GetComponent<RectTransform>();
    }

    public void HidePage(UIPageType page)
    {
        if (!pagesDict.ContainsKey(page))
        {
            Debug.Log("Not Exist Page " + page);
            return;
        }

        pagesDict[page].gameObject.SetActive(false);
    }

    public bool IsShow(UIPageType page)
    {
        if (pagesDict.ContainsKey(page))
        {
            return pagesDict[page].gameObject.activeSelf;
        }
        return false;
    }

    public void ShowPage(ShowPageInfo info)
    {
        ShowPageAsync(info).Forget();
    }

    public async UniTask<bool> ShowPageAsync(ShowPageInfo info)
    {
        if (info.closeOther)
        {
            foreach (var kv in pagesDict)
            {
                kv.Value.SetActive(false);
            }
        }

        if (pagesDict.ContainsKey(info.pageType) && pagesGroup[info.levelType].Contains(info.pageType))
        {
            pagesDict[info.pageType].SetActive(true);
            SetPageInfo(info);
        }
        else if (pagesDict.ContainsKey(info.pageType) && !pagesGroup[info.levelType].Contains(info.pageType))
        {
            pagesDict[info.pageType].transform.SetParent(levels[(int)info.levelType], false);
            pagesDict[info.pageType].SetActive(true);
            pagesGroup[GetGroupByPageType(info.pageType)].Remove(info.pageType);
            pagesGroup[info.levelType].AddLast(info.pageType);
            SetPageInfo(info);
        }
        else
        {
            //GameObject obj;
            // if (info.isLocal)
            // {
            //     obj = UnityEngine.Resources.Load<GameObject>(info.pageType.ToString());
            // }
            // else
            // {
            //     string pageUrl =  GetPageUrlByType(info.pageType);
            //     obj = await this.GetSystem<IYooAssetsSystem>().LoadAssetAsync<GameObject>(pageUrl);
            // }
            string pageUrl = GetPageUrlByType(info.pageType);
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>(pageUrl);

            //if (obj != null)
            if(obj.Status == AsyncOperationStatus.Succeeded)
            {
                // GameObject page = obj.Instantiate();
                GameObject page = Instantiate(obj.Result);
                page.transform.SetParent(levels[(int)info.levelType], false);
                pagesDict[info.pageType] = page;
                pagesGroup[info.levelType].AddLast(info.pageType);
                SetPageInfo(info);
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError($"Load {info.pageType} Page failed");
                return false;
            }
        }

        return true;
    }

    private void SetPageInfo(ShowPageInfo info)
    {
        UIPanel uiPanel = pagesDict[info.pageType].GetComponent<UIPanel>();
        if (info.data != null && uiPanel != null)
        {
            uiPanel.InitData(info.data);
        }

        pagesDict[info.pageType].transform.SetAsLastSibling();
    }

    private UILevelType GetGroupByPageType(UIPageType pageType)
    {
        foreach (var kv in pagesGroup)
        {
            if (kv.Value.Contains(pageType))
            {
                return kv.Key;
            }
        }

        return UILevelType.Main;
    }

    public void HidePageByLevel(UILevelType levelType)
    {
        foreach (var kv in pagesGroup[levelType])
        {
            if (pagesDict.ContainsKey(kv))
            {
                pagesDict[kv].SetActive(false);
            }
        }
    }

    // 查找Resources中的路径
    private string GetPageUrlByType(UIPageType type)
    {
        return Util.basePageUrl + type.ToString() + Util.pageSuffix;
    }

    private void OnDestroy()
    {
        foreach (var kv in pagesDict)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value);
            }
        }
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}