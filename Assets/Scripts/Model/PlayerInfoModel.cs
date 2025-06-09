using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

[System.Serializable]
public class PropBase
{
    public int id; // 道具ID
    public int amount; // 道具数量

    public PropBase()
    {
    }

    public PropBase(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}

[System.Serializable]
public class BackpackData
{
    public List<PropBase> props = new List<PropBase>();
}

public class PlayerInfoModel : AbstractModel
{
    public List<PropBase> showRewards;
    private PlayerPrefsStorage storage;
    private BackpackData BackpackInfo { get; set; }
    private BindableProperty<string> _backpackDataJson = new BindableProperty<string>();

    public BindableProperty<bool> IsCollectedGame = new BindableProperty<bool>();

    public BindableProperty<bool> GetCollectedGameAward = new BindableProperty<bool>();

    //public BindableProperty<InventoryData> BackpackData = new BindableProperty<InventoryData>();
    protected override void OnInit()
    {
        storage = this.GetUtility<PlayerPrefsStorage>();
#if UNITY_EDITOR
        //清空分享数据
        // storage.SaveString(nameof(IsCollectedGame), "false");
        // storage.SaveString(nameof(GetCollectedGameAward), "false");
#endif
        IsCollectedGame.Value = bool.Parse(storage.LoadString(nameof(IsCollectedGame), "false"));
        IsCollectedGame.Register(collect => { storage.SaveString(nameof(IsCollectedGame), collect.ToString()); });
        GetCollectedGameAward.Value = bool.Parse(storage.LoadString(nameof(GetCollectedGameAward), "false"));
        GetCollectedGameAward.Register(get => { storage.SaveString(nameof(GetCollectedGameAward), get.ToString()); });
        LoadBackpackInfo();
    }

    public int GetItemAmount(int id)
    {
        PropBase @base = BackpackInfo.props.Find(v => v.id == id);
        if (@base != null)
        {
            return @base.amount;
        }

        BackpackInfo.props.Add(new PropBase() {id = id, amount = 0});
        return 0;
    }

    public void GetCollectGameAward()
    {
        GetCollectedGameAward.Value = true;
        ChangePropAmount(Util.CollectGameAward);
    }

    public void ChangePropAmount(List<PropBase> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].amount == 0) continue;
            PropBase @base = BackpackInfo.props.Find(v => v.id == data[i].id);
            if (@base != null)
            {
                @base.amount += data[i].amount;
            }
            else
            {
                if (data[i].amount > 0)
                    BackpackInfo.props.Add(new PropBase() {id = data[i].id, amount = data[i].amount});
                else
                {
                    Log.Debug($"改变道具数量失败，道具数量<0 ,propId ={data[i].id},changeAmount = {data[i].amount}");
                    return;
                }
            }
        }


        SaveBackpackInfo();

        //弹出获得道具弹窗
        var getProp = data.Where(item => item.amount > 0).ToList();
        if (getProp.Count > 0)
        {
            showRewards = getProp;
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.GetRewardUI, UILevelType.UIPage));
        }
    }

    public void ChangePropAmount(PropBase data)
    {
        if (data.amount == 0) return;
        PropBase @base = BackpackInfo.props.Find(v => v.id == data.id);
        if (@base != null)
        {
            @base.amount += data.amount;
        }
        else
        {
            if (data.amount > 0)
                BackpackInfo.props.Add(new PropBase() {id = data.id, amount = data.amount});
            else
            {
                Log.Debug($"改变道具数量失败，道具数量<0 ,propId ={data.id},changeAmount = {data.amount}");
                return;
            }
        }

        SaveBackpackInfo();
        if (data.amount > 0)
        {
            showRewards = new List<PropBase>() {data};
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.GetRewardUI, UILevelType.UIPage));
        }
    }

    public void ChangePropAmount(int propId, int changeAmount, bool needSave = true)
    {
        if (changeAmount == 0)
        {
            Log.Debug($"propId =  {propId},changeAmount = {changeAmount}");
            return;
        }

        PropBase @base = BackpackInfo.props.Find(v => v.id == propId);
        if (@base != null)
        {
            @base.amount += changeAmount;
        }
        else
        {
            if (changeAmount > 0) BackpackInfo.props.Add(new PropBase() {id = propId, amount = changeAmount});
            else
            {
                Log.Debug($"改变道具数量失败，道具数量<0 ,propId ={propId},changeAmount = {changeAmount}");
                return;
            }
        }

        if (needSave) SaveBackpackInfo();
    }

    void SaveBackpackInfo()
    {
        string json = JsonUtility.ToJson(BackpackInfo);
        storage.SaveString(nameof(_backpackDataJson), json);
        Debug.Log("保存背包数据 " + json);
    }

    void LoadBackpackInfo()
    {
        if (storage.HasKey(nameof(_backpackDataJson)))
        {
            string json = storage.LoadString(nameof(_backpackDataJson));
            Log.Debug("加载背包数据 " + json);
            BackpackInfo = JsonUtility.FromJson<BackpackData>(json);
        }
        else
        {
            BackpackInfo = new BackpackData();
        }
    }
}