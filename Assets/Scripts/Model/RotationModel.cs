using UnityEngine;
using QFramework;

public class RotationModel : AbstractModel
{
    public BindableProperty<float> EulerAngles = new BindableProperty<float>();
    public Vector2 RotationSensitivity {get;} = new Vector2(0.3f, 0.3f);
    protected override void OnInit()
    {
       
    }
}