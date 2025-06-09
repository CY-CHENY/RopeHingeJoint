using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestPanel : MonoBehaviour
{
    public RecyclingListView scrollList;
    /// <summary>
    /// 列表数据
    /// </summary>
    private List<TestChildData> data = new List<TestChildData>();

    public int rowNum = 10;

    // Start is called before the first frame update
    void Start()
    {
        scrollList.ItemCallback = PopulateItem;
    }

    private void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateList();
        }
    }

    /// <summary>
    /// item更新回调
    /// </summary>
    /// <param name="item">复用的item对象</param>
    /// <param name="rowIndex">行号</param>
    private void PopulateItem(RecyclingListViewItem item, int rowIndex)
    {
        var child = item as TestViewItem;
        //child.ChildData = data[rowIndex];
        child.Init(data[rowIndex]);
    }
    
    private void CreateList()
    {

        var rowCnt = (rowNum/scrollList.RowItemCount) + (rowNum%scrollList.RowItemCount== 0?0:1);

        data.Clear();
        // 模拟数据
        string[] randomTitles = new[] {
            "黄沙百战穿金甲，不破楼兰终不还",
            "且将新火试新茶，诗酒趁年华",
            "苟利国家生死以，岂因祸福避趋之",
            "枫叶经霜艳，梅花透雪香",
            "夏虫不可语于冰",
            "落花无言，人淡如菊",
            "宠辱不惊，闲看庭前花开花落；去留无意，漫随天外云卷云舒",
            "衣带渐宽终不悔，为伊消得人憔悴",
            "从善如登，从恶如崩",
            "欲穷千里目，更上一层楼",
            "草木本无意，荣枯自有时",
            "纸上得来终觉浅，绝知此事要躬行",
            "不是一番梅彻骨，怎得梅花扑鼻香",
            "青青子衿，悠悠我心",
            "瓜田不纳履，李下不正冠"
        };

       
        for (int i = 0; i < rowCnt; i++)
        {
            List<string> listData = new List<string>();
            List<int> listIndex = new List<int>();
            for (int j = 0; j < scrollList.RowItemCount; j++)
            {
                if ((i * scrollList.RowItemCount) + j == rowNum) break;
                listData.Add(randomTitles[Random.Range(0, randomTitles.Length)]);
                listIndex.Add((i*scrollList.RowItemCount)+j);
            }
            
            data.Add(new TestChildData(listData,listIndex,i));
        }
        
        
        // for (int i = 0; i < rowCnt; ++i)
        // {
        //     List<string> listStr = new List<string>();
        //     List<int> listIndex = new List<int>();
        //     int tempRow = rowNum/scrollList.RowItemCount == 0 ? scrollList.RowItemCount : 1;
        //     for (int j = 0; j < tempRow; j++)
        //     {
        //         listStr.Add(randomTitles[Random.Range(0, randomTitles.Length)]);
        //         listIndex.Add((i*scrollList.RowItemCount)+j);
        //     }
        //     data.Add(new TestChildData(listStr,listIndex,i));
        // }

        // 设置数据，此时列表会执行更新
        scrollList.RowCount = data.Count;
    }
    
    
    private void ClearList()
    {
        data.Clear();
        scrollList.Clear();
    }

    private void DeleteItem(int rowIndex)
    {
       
        data.RemoveAll(item => (item.Row == rowIndex));

        scrollList.RowCount = data.Count;
    }

    private void AddItem()
    {
        data.Add(new TestChildData(new List<string>{"我是新增的行"},new List<int>(){1} ,data.Count));
        scrollList.RowCount = data.Count;
    }

    private void MoveToRow(RecyclingListView.ScrollPosType posType,int rowIndex)
    {
        scrollList.ScrollToRow(rowIndex, posType);
    }
    
}
