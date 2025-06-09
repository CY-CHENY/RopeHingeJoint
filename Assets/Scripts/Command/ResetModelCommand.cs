using QFramework;
using UnityEngine;

public class ResetModelCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var scaleModel = this.GetModel<ScaleModel>();
        scaleModel.Scale.Value = 0.8f;
        
        this.SendEvent<ResetModelRotationEvent>();
    }
}