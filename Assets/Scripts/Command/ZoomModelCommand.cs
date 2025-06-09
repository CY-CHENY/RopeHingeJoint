using QFramework;
using UnityEngine;
public class ZoomModelCommand : AbstractCommand
{
    private bool zoomIn;
    public ZoomModelCommand(bool zoomIn)
    {
        this.zoomIn = zoomIn;
    }

    protected override void OnExecute()
    {
        var model = this.GetModel<ScaleModel>();


        if (zoomIn)
        {
            var newScale = model.Scale.Value + 0.1f;

            model.Scale.Value = Mathf.Min(newScale, 1.3f);
        }
        else
        {
            var newScale = model.Scale.Value - 0.1f;

            model.Scale.Value = Mathf.Max(newScale, 0.6f);
        }
    }
}