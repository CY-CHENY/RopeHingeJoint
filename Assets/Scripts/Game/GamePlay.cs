using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class GamePlay : BaseController
{
    //public Block[] listBlock;

    public Transform collectBoxParent;
    public Transform spareBlockParent;

    public Transform LegoSpawn;
    public Transform collectSpwan;

    public GameObject CollectLegoPrefab;
    public GameObject BlockPrefab;

    // public LegoData[] legoDatas;
    // public ColorMaterialData[] colorMaterialDatas;

    // public LegoDataSO legoDataSO;
    // public ColorMaterialSO colorMaterialSO;
    public Vector3 LegoSpawnPos;
    void Awake()
    {
        LegoSpawn.position = LegoSpawnPos;
    }

    void Start()
    {
        var model = this.GetModel<RuntimeModel>();

        UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.GameUI));

        model.GameStart.Value = false;

        this.GetSystem<LegoSystem>().LegoPrefab = CollectLegoPrefab;
        this.GetSystem<LegoSystem>().SpawnTransform = collectSpwan;

        this.GetSystem<RopeSystem>().Clear();

        model.SpareCapacity.Value = 5;
        model.SpareBlockItems.Clear();

        model.ActiveBoxes.Clear();
        model.ActiveBoxes.OnAdd.Register(OnActiveBoxAdded).UnRegisterWhenGameObjectDestroyed(gameObject);
        model.SpareCapacity.RegisterWithInitValue(OnSpareBlockChanged).UnRegisterWhenGameObjectDestroyed(gameObject);

        this.GetModel<RuntimeModel>().SpareBlockItems.OnCountChanged.Register(OnChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<ItemToSparEvent>(OnItemToSpareEvent).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<UpdateBoxLayoutEvent>(OnUpdateBoxLayoutEvent).UnRegisterWhenGameObjectDestroyed(gameObject);

        this.SendCommand(new SpawnBrickObjectCommand(LegoSpawn.localPosition));
        this.GetSystem<AudioSystem>().PlayBackgroundMusic("BGM");
    }

    private void OnChanged(int obj)
    {
        Debug.Log("OnChanged:" + obj);
    }

    private void OnItemToSpareEvent(ItemToSparEvent evt)
    {
        // var itemData = evt.item;
        // foreach (var block in this.GetModel<RuntimeModel>().SpareBlockItems)
        // {
        //     if (block.Item == null)
        //     {
        //         block.Item = itemData;
        //         itemData.ItemTransform.LocalPositionX(block.Block.transform.position.x);
        //         itemData.ItemTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //         itemData.ItemTransform.DOMove(block.Block.transform.position, 0.3f)
        //         .OnComplete(() =>
        //         {
        //             block.Block.SetLego(itemData.ItemTransform.gameObject);
        //         });
        //         break;
        //     }
        // }
        bool lose = true;
        foreach (var block in this.GetModel<RuntimeModel>().SpareBlockItems)
        {
            if (block.Item == null)
            {
                lose = false;
                break;
            }
        }

        if (lose)
        {
            Debug.Log("失败");

            if (UIController.Instance.IsShow(UIPageType.FuHuoUI) || UIController.Instance.IsShow(UIPageType.GameLoseUI))
                return;

            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.FuHuoUI, UILevelType.UIPage));
        }
    }

    private void OnSpareBlockChanged(int count)
    {
        var spareBlockItems = this.GetModel<RuntimeModel>().SpareBlockItems;
        if (count > spareBlockItems.Count)
        {
            int addCount = count - spareBlockItems.Count;
            for (int i = 0; i < addCount; i++)
            {
                var gameobject = Instantiate(BlockPrefab);
                var block = gameobject.GetComponent<Block>();
                gameobject.transform.SetParent(spareBlockParent);
                this.GetModel<RuntimeModel>().SpareBlockItems.Add(new BlockData() { Block = block });
            }
            var horizontal = spareBlockParent.GetComponent<HorizontalLayout>();
            horizontal.UpdateLayoutIfNeeded();
        }
    }

    private void OnSpareItemsAdded(int index, ItemData data)
    {
        // Block block = listBlock[index];
        // data.ItemTransform.LocalPositionX(block.transform.position.x);
        // data.ItemTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        // data.ItemTransform.DOMove(block.transform.position, 0.3f)
        // .OnComplete(() =>
        // {
        //     block.SetLego(data.ItemTransform.gameObject);
        // });
    }

    private void OnActiveBoxAdded(int index, BoxData data)
    {
        Debug.Log("OnActiveBoxAdded");
        data.BoxTransform.SetParent(collectBoxParent);
        data.BoxTransform.SetSiblingIndex(index);
        data.BoxTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        data.BoxTransform.DOScale(Vector3.one, 0.3f);
        data.BoxTransform.localPosition = Vector3.zero;

        var horizontal = collectBoxParent.GetComponent<HorizontalLayout>();
        horizontal.UpdateLayoutIfNeeded();

        if (index == 0)
        {
            if (this.GetModel<RuntimeModel>().Guide.Value)
                UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.GuideUI));
        }
    }

    private void OnUpdateBoxLayoutEvent(UpdateBoxLayoutEvent evt)
    {
        var horizontal = collectBoxParent.GetComponent<HorizontalLayout>();
        horizontal.UpdateLayoutIfNeeded();
    }
}