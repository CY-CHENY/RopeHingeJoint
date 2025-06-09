using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class PageCollectUI : MonoBehaviour,IController
{
    public RecyclingListView scrollList;
    /// <summary>
    /// 列表数据
    /// </summary>
    private List<CollectChildData> data = new List<CollectChildData>();
    
    void Awake()
    {
        scrollList.ItemCallback = PopulateItem;

      
    }
    /// <summary>
    /// item更新回调
    /// </summary>
    private void PopulateItem(RecyclingListViewItem item, int rowindex)
    {
        var child = item as CollectViewItems;
        child.InitData(data[rowindex]);
    }

    public void Init()
    {
        CreateList();
    }
    void CreateList()
    {
        
        var legoData = Util.LevelConfigs;
        int rowCount = legoData.Count;
        var rowCnt = (rowCount/scrollList.RowItemCount) + (rowCount%scrollList.RowItemCount== 0?0:1);
        data.Clear();
        
        for (int i = 0; i < rowCnt; i++)
        {
            List<LevelConfig> listData = new List<LevelConfig>();
            List<int> listIndex = new List<int>();
            for (int j = 0; j < scrollList.RowItemCount; j++)
            {
                 if ((i * scrollList.RowItemCount) + j == rowCount) break;
                 listData.Add(legoData[(i*scrollList.RowItemCount)+j]);
                listIndex.Add((i*scrollList.RowItemCount)+j);
            }
            
            data.Add(new CollectChildData(listData,listIndex,i));
          
        }

      
        // 设置数据，此时列表会执行更新
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
