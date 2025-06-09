using System.ComponentModel;
using QFramework;

public class ScaleModel : AbstractModel
{
    public BindableProperty<float> DefaultScale = new BindableProperty<float>(0.8f);
    public BindableProperty<float> Scale = new BindableProperty<float>(1f);

    protected override void OnInit()
    {
        
    }
}