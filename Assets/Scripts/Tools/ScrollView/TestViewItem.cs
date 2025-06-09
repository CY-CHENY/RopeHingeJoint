using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestViewItem : RecyclingListViewItem
{
    public Text titleText;
    public Text rowText;
    private bool isInit = false;
    private TestChildData childData;
    public TestChildData ChildData
    {
        get { return childData; }
        set
        {
            childData = value;
            titleText.text = childData.Title[0];
            rowText.text = $"行号：{childData.Row}";
        }
    }

    void Initialize()
    {
        items = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            items.Add(transform.GetChild(i).gameObject);
        }
    }
    

    private List<GameObject> items;

    public void Init(TestChildData data)
    {
        if (!isInit) Initialize();
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(i <data.Title.Count);
            if (i < data.Title.Count)
            {
                InitItem(items[i],data.Idx[i]);
            }
        }
    }

    private void InitItem(GameObject go,int idx)
    {
        go.transform.Find("Text (Legacy)").GetComponent<Text>().text = idx.ToString();
    }
}


public struct TestChildData
{
    public List<string> Title;
    public List<int> Idx;
    public int Row;

    public TestChildData(List<string> title, List<int> idx,int row)
    {
        Title = title;
        Idx = idx;
        Row = row;
    }
}