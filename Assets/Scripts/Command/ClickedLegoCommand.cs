using DG.Tweening;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;

public class ClickedLegoCommand : AbstractCommand
{
    private GameObject gameObject;
    public ClickedLegoCommand(GameObject go)
    {
        gameObject = go;
    }

    protected override void OnExecute()
    {
        // int boxIndex = -1;
        // var runtimeMode = this.GetModel<RuntimeModel>();
        // runtimeMode.LeftBrick.Value--;

        // if (runtimeMode.LegoColors.TryGetValue(gameObject.name, out var legoColor))
        // {
        //     Debug.Log("点击:" + legoColor);
        //     int count = -1;
        //     for (int i = 0; i < runtimeMode.CurrentBox.Count; i++)
        //     {
        //         var box = runtimeMode.CurrentBox[i];
        //         if (box.type == BoxType.Normal && box.color == legoColor)
        //         {
        //             if (box.count > count)
        //             {
        //                 count = box.count;
        //                 boxIndex = i;
        //             }
        //         }
        //     }
        // }

        // this.SendCommand(new FadeLegoCommand(gameObject, boxIndex));
        
       
    }
}