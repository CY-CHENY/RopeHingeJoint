using System.ComponentModel;
using QFramework;

public class CameraSizeModel : AbstractModel
{
    public BindableProperty<float> Scale = new BindableProperty<float>(1f);

    protected override void OnInit()
    {
        
    }
}