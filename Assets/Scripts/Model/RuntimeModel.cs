using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public enum BoxType
{
    Normal = 1,
    Advertisement = 2,
    Super = 3
}

public class BoxData
{
    public BoxType Type;
    public ItemColor Color;
    public int CurrentCount;
    public Transform BoxTransform;
}

public class ItemData
{
    public ItemColor Color;
    public Transform ItemTransform;
    // public bool IsInBox;
}

public class BlockData
{
    public Block Block;
    public ItemData Item;
}

public class RuntimeModel : AbstractModel
{
    public BindableProperty<int> SpareCapacity = new BindableProperty<int>(5);
    public BindableProperty<bool> GameWin = new BindableProperty<bool>();
    public BindableProperty<bool> GameOver = new BindableProperty<bool>();

    public BindableList<BoxData> ActiveBoxes = new BindableList<BoxData>();
    //public BindableList<ItemData> SpareItems = new BindableList<ItemData>();
    public BindableList<BlockData> SpareBlockItems = new BindableList<BlockData>();
    public BindableList<ItemData> AllItems = new BindableList<ItemData>();
    public BindableProperty<int> totalCount = new BindableProperty<int>();

    /// <summary>
    /// 盒子总数
    /// </summary>
    public BindableProperty<int> TotalBox = new BindableProperty<int>();

    //public BindableList<BoxData> BoxQueue = new BindableList<BoxData>();
    /// <summary>
    /// 盒子池 每用完一个就从这里面取
    /// </summary>
    public BindableList<BoxData> BoxPool = new BindableList<BoxData>();
    /// <summary>
    /// 颜色池 每一局要用到的所有颜色
    /// </summary>
    public BindableList<ItemColor> ColorPool = new BindableList<ItemColor>();

    public BindableProperty<int> CurrentLevel = new BindableProperty<int>() { Value = 1 };
    public BindableDictionary<int, LevelConfig> LevelLegoData = new BindableDictionary<int, LevelConfig>();

    public BindableProperty<bool> GameStart = new BindableProperty<bool>() { Value = false };

    public BindableProperty<bool> Guide = new BindableProperty<bool>() { Value = true };

    public BindableProperty<GameObject> GuideOne = new BindableProperty<GameObject>();
    // public BindableProperty<GameObject> GuideTwo = new BindableProperty<GameObject>();
    // public BindableProperty<GameObject> GuideThree = new BindableProperty<GameObject>();

    protected override void OnInit()
    {
        var storage = this.GetUtility<PlayerPrefsStorage>();
        CurrentLevel.Value = storage.LoadInt(nameof(CurrentLevel), 1);
        CurrentLevel.Register(level => storage.SaveInt(nameof(CurrentLevel), level));

        Guide.Value = bool.Parse(storage.LoadString(nameof(Guide), "true"));
        Guide.Register(guide => storage.SaveString(nameof(Guide), guide.ToString()));
    }

    public BoxData GetBoxData()
    {
        if(BoxPool.Count <= 0)
            return null;

        int index = 0;
        var returnBoxData = BoxPool[index];
        BoxPool.RemoveAt(index);
        return returnBoxData;
    }

}