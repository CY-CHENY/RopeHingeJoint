using System.Diagnostics;
using System.Linq;
using QFramework;

public class SuperBoxCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<RuntimeModel>();
        var superBox = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Super);
        if (superBox != null)
        {
            UnityEngine.Debug.Log("已经有超级盒子了");
            return;
        }

        var amount = this.GetModel<PlayerInfoModel>().GetItemAmount(1002);
        UnityEngine.Debug.Log("超级盒子道具数量:" + amount);
        if (amount > 0)
        {
            this.GetModel<PlayerInfoModel>().ChangePropAmount(1002, -1);
            SuperBox();
            return;
        }

        this.GetUtility<SDKUtility>().ShowAd((ret) =>
            {
                UnityEngine.Debug.Log("超级盒子广告:" + ret);
                if (ret)
                {
                    SuperBox();
                }
            });

    }

    private void SuperBox()
    {
        var model = this.GetModel<RuntimeModel>();
        int index = model.ActiveBoxes.Count;
        //广告盒子
        var advertisment = model.ActiveBoxes.FirstOrDefault(b => b.Type == BoxType.Advertisement);
        if (advertisment != null)
        {
            //插入到广告前面
            index = model.ActiveBoxes.IndexOf(advertisment);
        }
        this.SendCommand(new SpawnBoxCommand(new BoxData() { Type = BoxType.Super }, index));
    }
}