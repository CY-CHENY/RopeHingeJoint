using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QFramework;
using TTSDK;
using UnityEngine;
using UnityEngine.UI;
using static TTSDK.TTRank;

public class MainRankingUI : MonoBehaviour,IController
{
    public RecyclingListView scrollList;
    public MainrankingItem myRank;
    public Button btn_close;
    
    private List<TTRank.RankResItem> data = new List<TTRank.RankResItem>();
    private bool isInit = false;
   
    void Awake()
    {
        scrollList.ItemCallback = PopulateItem;
        // this.RegisterEvent<GetRankDataSuccessEvent>(GetRankDataSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
        // this.RegisterEvent<GetRankDataFailEvent>(GetRankDataFail).UnRegisterWhenGameObjectDestroyed(gameObject);
        btn_close.onClick.AddListener(() =>
        {
            UIController.Instance.HidePage(UIPageType.MainRankingUI);
        });
    }

    private void GetRankDataFail(GetRankDataFailEvent obj)
    {
        Log.Debug("---------GetRankDataFailEvent------");
    }

    private void GetRankDataSuccess(GetRankDataSuccessEvent obj)
    {
        Log.Debug("---------GetRankDataSuccessEvent------");
        var model = this.GetModel<MainRankingModel>();
        myRank.InitMyData(model.rankData.SelfItem);
        CreateList(model.rankData.Items);
    }

    private void Start()
    {
        
        isInit = true;
        Init();
    }

    private void PopulateItem(RecyclingListViewItem item, int rowindex)
    {
        var child = item as MainrankingItem;
        child.InitData(data[rowindex],rowindex);
    }

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        if (!isInit) return;
        
        //this.SendCommand<GetMainRankingInfoCommand>();
        this.GetUtility<SDKUtility>().GetRankData("all", 1, 10, (ref RankData rankData) =>
        {
            Debug.Log($"GetRankData Success:  {rankData.Items.Count}");
            myRank.InitMyData(rankData.SelfItem);
            CreateList(rankData.Items);
        },
            (errMsg) =>
            {
                Debug.Log($"GetRankData Fail:{errMsg}");
            }
        );

        //StartCoroutine(CreateList());
    }

    void CreateList(List<RankResItem> rankData)
    {
        Log.Debug("------CreateList---->>>"+rankData.Count);
        data.Clear();
        data = rankData;
        scrollList.RowCount = data.Count;
        
        RectTransform content =scrollList.GetComponent<ScrollRect>().content;
        var sizeDelta = content.sizeDelta;
        sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + 20);
        content.sizeDelta = sizeDelta;
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
