using QFramework;
using UnityEngine;

public class RotateModelCommand : AbstractCommand
{
    private float deltaPosition;
    public RotateModelCommand(float deltaPosition)
    {
        this.deltaPosition = deltaPosition;
    }
    protected override void OnExecute()
    {
       
        var model = this.GetModel<RotationModel>();
        model.EulerAngles.Value = deltaPosition;
        
    }
}