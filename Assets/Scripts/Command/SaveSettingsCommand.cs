using QFramework;

public class SaveSettingsCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.SendEvent<ChangeSettingEvent>();
    }
}