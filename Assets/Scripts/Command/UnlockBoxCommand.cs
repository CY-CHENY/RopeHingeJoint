using System.Linq;
using QFramework;
using UnityEngine;
using Utils;

public class UnlockBoxCommand : AbstractCommand
{
    private GameObject gameObject;
    private ItemColor targetColor;
    public UnlockBoxCommand(GameObject gameObject, ItemColor itemColor = ItemColor.None)
    {
        this.gameObject = gameObject;
        this.targetColor = itemColor;
    }

    protected override void OnExecute()
    {
        this.GetUtility<SDKUtility>().ShowAd((ret) =>
        {
            if (ret)
            {
                var model = this.GetModel<RuntimeModel>();
                var item = model.ActiveBoxes.FirstOrDefault(item => item.BoxTransform == gameObject.transform);
                if (item != null)
                {
                    this.SendCommand(new ProcessFullBoxCommand(item, targetColor));
                }
            }
        });


    }
}