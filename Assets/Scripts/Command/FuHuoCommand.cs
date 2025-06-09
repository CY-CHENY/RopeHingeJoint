using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using QFramework;
using UnityEngine;
using Utils;

public class FuHuoCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetUtility<SDKUtility>().ShowAd((ret) =>
        {
            Debug.Log("广告结果:" + ret);
            if (ret)
            {
                UIController.Instance.HidePage(UIPageType.FuHuoUI);
                Unlock();
                ClearItems();
            }
        });

        // UIController.Instance.HidePage(UIPageType.FuHuoUI);
        // Unlock();
        // ClearItems();

        // var model = this.GetModel<RuntimeModel>();
        // var boxData = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Advertisement);
        // if (boxData != null)
        // {
        //     // //如果备用区有的话找一个备用区中的颜色
        //     // var blockData =  model.SpareBlockItems.FirstOrDefault(item => item.Item != null);
        //     // ItemColor targetColor = ItemColor.None;
        //     // if (blockData != null)
        //     // {
        //     //     targetColor = blockData.Item.Color;
        //     // }
        //     this.SendCommand(new UnlockBoxCommand(boxData.BoxTransform.gameObject));
        // }

        // this.SendCommand(new ClearCommand());

        // UnityEngine.Debug.Log("没找到可解锁的盒子");
    }

    private void Unlock()
    {
        var model = this.GetModel<RuntimeModel>();
        var boxData = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Advertisement);
        if (boxData != null)
        {
            var item = model.ActiveBoxes.FirstOrDefault(item => item.BoxTransform == boxData.BoxTransform);
            if (item != null)
            {
               this.SendCommand(new ProcessFullBoxCommand(item));
            }
        }
    }

    private void ClearItems()
    {
        //得到备用区有积木的
        var matchedBlocks = this.GetModel<RuntimeModel>().SpareBlockItems.Where(s => s.Item != null).ToList();
        //备用区同一个颜色个数
        Dictionary<ItemColor, int> legoDic = new Dictionary<ItemColor, int>();
        foreach (var block in matchedBlocks)
        {
            var itemData = block.Item;
            if (legoDic.ContainsKey(itemData.Color))
            {
                int value = legoDic[itemData.Color] + 1;
                legoDic[itemData.Color] = value;
            }
            else
            {
                legoDic.Add(itemData.Color, 1);
            }
        }

        Dictionary<ItemColor, int> needLego = new Dictionary<ItemColor, int>();
        foreach (var color in legoDic.Keys)
        {
            if (legoDic[color] > 3)
            {
                int count = legoDic[color] / 3;

                var box = this.GetModel<RuntimeModel>().BoxPool.Where(b => b.Color == color).Take(count).ToList();
                if (box != null)
                {
                    foreach (var b in box)
                    {
                        Debug.Log($"删除一个BoxQueue{color}");
                        this.GetModel<RuntimeModel>().BoxPool.Remove(b);
                    }
                }
                needLego.Add(color, 3 - (legoDic[color] - (count * 3)));
            }
            else
            {
                needLego.Add(color, 3 - legoDic[color]);
            }
        }

        //备用区的
        List<ItemData> blockItems = new List<ItemData>();
        foreach (var block in matchedBlocks)
        {
            blockItems.Add(block.Item);
            block.Item = null;
        }

        var model = this.GetModel<RuntimeModel>();
        //模型上的
        List<ItemData> modelItems = new List<ItemData>();
        foreach (var color in needLego.Keys)
        {
            int count = needLego[color];
            if (count > 0)
            {
                List<ItemData> colorItems = model.AllItems.Where(item => item.Color == color).Take(count).ToList();
                modelItems.AddRange(colorItems);
            }

        }

        //BoxPool中删除消除颜色
        foreach (var color in needLego.Keys)
        {
            var box = model.BoxPool.FirstOrDefault(b => b.Color == color);
            if (box != null)
            {
                Debug.Log($"删除一个BoxQueue{color}");
                model.BoxPool.Remove(box);
            }
        }
        // 往左边跳到屏幕外
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        foreach (var item in blockItems)
        {
            Debug.Log($"跳出去一个{item.Color}");
            var position = item.ItemTransform.position;
            item.ItemTransform.DOJump(new UnityEngine.Vector3(-width, position.y, position.z), 1.0f, 1, 0.3f)
            .OnComplete(() =>
            {
                UnityEngine.Object.Destroy(item.ItemTransform.gameObject);
            });
        }

        //模型上的直接消失
        foreach (var item in modelItems)
        {
            Debug.Log($"消失一个{item.Color}");
            model.AllItems.Remove(item);
     
            this.SendEvent(new ClickModelEvent { transform = item.ItemTransform });
            Object.Destroy(item.ItemTransform.gameObject);
        }

        this.SendEvent(new ProcessFullBoxFinishEvent { });
    }
}