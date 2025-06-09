using QFramework;

public class NextLevelCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetModel<RuntimeModel>().CurrentLevel.Value++;
        if (this.GetModel<RuntimeModel>().CurrentLevel.Value >= 10)
            this.GetModel<RuntimeModel>().CurrentLevel.Value = 10;
        this.SendCommand(new LoadSceneCommand(Utils.SceneID.Game));
    }
}