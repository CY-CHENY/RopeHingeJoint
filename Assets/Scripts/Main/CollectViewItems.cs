using System.Collections;
using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Utils;

public class CollectViewItems : RecyclingListViewItem, IController
{
    public List<GameObject> items;
    public List<Sprite> ListBG;
    private bool isInit = false;
    void Init()
    {
    
        isInit = true;
    }
    public void InitData(CollectChildData collectChildData)
    {
        if (!isInit) Init();
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(i <collectChildData.Data.Count);
            if (i < collectChildData.Data.Count)
            {
                InitItem(items[i],collectChildData.Data[i]);
            }
        }
    }

    private async void InitItem(GameObject go, LevelConfig legoData)
    {
        Text txt_name = go.transform.Find("txt_name").GetComponent<Text>() ;
        Image img_icon = go.transform.Find("img_icon").GetComponent<Image>();
        Image img_kuang = go.GetComponent<Image>();
        var  model=this.GetModel<RuntimeModel>();
        go.SetActive(false);
        if (model.CurrentLevel.Value > legoData.Level)
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(legoData.IconCollectPath);
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                img_icon.sprite = obj.Result;
                go.SetActive(true);
            }
            
            img_kuang.sprite = ListBG[0];
            txt_name.text = legoData.Name;
        }
        else
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(legoData.IconCollectGrayPath);
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                img_icon.sprite = obj.Result;
                go.SetActive(true);
            }
            img_kuang.sprite = ListBG[1];
            txt_name.text = "";
        }
        
        img_icon.SetNativeSize();
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}

public struct CollectChildData
{
    public List<LevelConfig> Data;
    public List<int> Idx;
    public int Row;

    public CollectChildData(List<LevelConfig> data, List<int> idx, int row)
    {
        Data = data;
        Idx = idx;
        Row = row;
    }
}
