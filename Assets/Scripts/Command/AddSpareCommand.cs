using QFramework;

public class AddSpareCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        if (this.GetModel<RuntimeModel>().SpareCapacity.Value >= 8)
            return;

        var amount = this.GetModel<PlayerInfoModel>().GetItemAmount(1000);
        if (amount > 0)
        {
            this.GetModel<PlayerInfoModel>().ChangePropAmount(1000, -1);
            this.GetModel<RuntimeModel>().SpareCapacity.Value++;
            return;
        }

        this.GetUtility<SDKUtility>().ShowAd((ret) =>
        {
            if (ret)
            {
                this.GetModel<RuntimeModel>().SpareCapacity.Value++;
            }

        });
    }
}