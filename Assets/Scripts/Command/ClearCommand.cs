using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using QFramework;
using UnityEngine;
using Utils;

//清空
public class ClearCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        // if (this.GetModel<RuntimeModel>().AllItems.Count < 20)
        // {
        //     UnityEngine.Debug.Log("积木少于20 清空无效");
        //     return;
        // }

        var amount = this.GetModel<PlayerInfoModel>().GetItemAmount(1001);
        if (amount > 0)
        {
            this.GetModel<PlayerInfoModel>().ChangePropAmount(1001, -1);
            ClearItem();
            return;
        }

        this.GetUtility<SDKUtility>().ShowAd((ret) =>
       {
           if (ret)
           {
               ClearItem();
           }
       });
    }

    private void ClearItem()
    {
        //得到备用区有积木的
        var matchedBlocks = this.GetModel<RuntimeModel>().SpareBlockItems.Where(s => s.Item != null).ToList();
        //备用区同一个颜色个数
        Dictionary<ItemColor, int> itemDic = new Dictionary<ItemColor, int>();
        foreach (var block in matchedBlocks)
        {
            var itemData = block.Item;
            if (itemDic.ContainsKey(itemData.Color))
            {
                int value = itemDic[itemData.Color] + 1;
                itemDic[itemData.Color] = value;
            }
            else
            {
                itemDic.Add(itemData.Color, 1);
            }
        }

        Dictionary<ItemColor, int> needLego = new Dictionary<ItemColor, int>();
        foreach (var color in itemDic.Keys)
        {
            if (itemDic[color] > 3)
            {
                int count = itemDic[color] / 3;

                var box = this.GetModel<RuntimeModel>().BoxPool.Where(b => b.Color == color).Take(count).ToList();
                if (box != null)
                {
                    foreach (var b in box)
                    {
                        Debug.Log($"删除一个BoxQueue{color}");
                        this.GetModel<RuntimeModel>().BoxPool.Remove(b);
                    }
                }

                needLego.Add(color, 3 - (itemDic[color] - (count * 3)));
            }
            else
            {
                needLego.Add(color, 3 - itemDic[color]);
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

        //boxqueue中删除消除颜色
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