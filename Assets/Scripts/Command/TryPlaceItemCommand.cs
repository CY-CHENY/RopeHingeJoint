using System.Diagnostics;
using System.Linq;
using QFramework;

public class TryPlaceItemCommand : AbstractCommand
{
    private readonly ItemData item;
    private readonly bool up;//true上下 fasle 左右
    public TryPlaceItemCommand(ItemData item, bool up)
    {
        this.item = item;
        this.up = up;
    }

    protected override void OnExecute()
    {
        var model = this.GetModel<RuntimeModel>();

        var unEmptyMatchedBox = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Normal && b.Color == item.Color && b.CurrentCount > 0 && b.CurrentCount < 3);
        if (unEmptyMatchedBox != null)
        {
            //先找盒子里面有相同颜色的盒子
            this.SendEvent(new ClickModelEvent { transform = item.ItemTransform });
            UnityEngine.Debug.Log("移动到盒子");
            //移动到盒子
            model.AllItems.Remove(item);
            this.SendCommand(new MoveToBoxCommand(item, unEmptyMatchedBox, up));
            return;
        }

        var matchedBox = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Normal && b.Color == item.Color && b.CurrentCount < 3);
        //var matchedBox = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Normal && b.CurrentCount < 3);
        if (matchedBox != null)
        {
            this.SendEvent(new ClickModelEvent { transform = item.ItemTransform });
            UnityEngine.Debug.Log("移动到盒子");
            //移动到盒子
            model.AllItems.Remove(item);
            this.SendCommand(new MoveToBoxCommand(item, matchedBox, up));
            return;
        }

        //超级盒子 如果当前盒子没有匹配的颜色 该超级盒子就收集选中毛线的颜色
        var superBox = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Super && b.CurrentCount < 3);
        if (superBox != null)
        {
            UnityEngine.Debug.Log("移动到超级盒子");
            //this.SendEvent(new ClickModelEvent{transform = item.ItemTransform});
            model.AllItems.Remove(item);

            var removeBox = model.BoxPool.FirstOrDefault(i => i.Color == item.Color);
            if (removeBox != null)
            {
                var ret = model.BoxPool.Remove(removeBox);
                UnityEngine.Debug.Log($"删除盒子:{removeBox.Color}--->{ret}");
            }

            this.SendCommand(new MoveToBoxCommand(item, superBox, up));
            //还需要个数
            int needCount = 2;
            //相同颜色的毛线
            var sameColorWool = model.AllItems.Where(i => i.Color == item.Color).Take(needCount).ToArray();
            if (sameColorWool != null)
            {
                foreach (var sameWool in sameColorWool)
                {
                    model.AllItems.Remove(sameWool);
                    this.SendCommand(new MoveToBoxCommand(sameWool, superBox, up));
                }
                needCount -= sameColorWool.Length;
            }
            //如果还不够 就从未上色的毛线里面随机找
            if (needCount > 0)
            {
                sameColorWool = model.AllItems.Where(i => i.Color == Utils.ItemColor.None).Take(needCount).ToArray();
                if (sameColorWool != null)
                {
                    foreach (var sameWool in sameColorWool)
                    {
                        model.AllItems.Remove(sameWool);
                        this.SendCommand(new MoveToBoxCommand(sameWool, superBox, up));
                    }
                }
                needCount -= sameColorWool.Length;
            }
            //从备用区找
            if (needCount > 0)
            {
                var sameWool = model.SpareBlockItems.Where(b => b.Item != null && b.Item.Color == item.Color).Take(needCount).ToArray();
                if (sameWool == null || sameWool.Length < needCount)
                {
                    int count = sameWool == null ? 0 : sameWool.Length;
                    UnityEngine.Debug.LogError($"超级盒子还需要{needCount}个毛线 现在备用区找到{count}个 检查问题");
                    return;
                }

                foreach (var same in sameWool)
                {
                    this.SendCommand(new BlockToBoxCommand(same, superBox));
                }
            }

            return;
        }


        var matchedBlock = model.SpareBlockItems.FirstOrDefault(b => b.Item == null);
        if (matchedBlock != null)
        {
            UnityEngine.Debug.Log("移动到备用区");
            this.SendEvent(new ClickModelEvent { transform = item.ItemTransform });
            //移动到备用区
            model.AllItems.Remove(item);
            this.SendCommand(new MoveToSpareCommand(item,up));
        }
    }
}