using QFramework;

public class VibrateSystem : AbstractSystem
{
    private bool isVibrate;
    protected override void OnInit()
    {
        var settingsModel = this.GetModel<ISettingsModel>();
        this.RegisterEvent<ChangeSettingEvent>((e) =>
        {
            SetVibrate(settingsModel.IsOnVibration.Value);
        });

        SetVibrate(settingsModel.IsOnVibration.Value);
    }

    private void SetVibrate(bool vibrate)
    {
        isVibrate = vibrate;
    }

    public void VibrateLong()
    {
        if (isVibrate)
            this.GetUtility<SDKUtility>().VibrateLong();
    }

    public void VibrateShort()
    {
        if (isVibrate)
            this.GetUtility<SDKUtility>().VibrateShort();
    }
}