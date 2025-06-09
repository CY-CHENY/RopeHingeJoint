using QFramework;
using UnityEngine;
using DG.Tweening;
public class ZoomModelCommand : AbstractCommand
{
    private bool zoomIn;
    public ZoomModelCommand(bool zoomIn)
    {
        this.zoomIn = zoomIn;
    }

    protected override void OnExecute()
    {
        Camera modelCamera = GameObject.FindWithTag("ModelCamera").GetComponent<Camera>();

        if (zoomIn)
        {
            var newScale = modelCamera.orthographicSize - 1f;
            modelCamera.DOOrthoSize(Mathf.Max(newScale, 2f),0.3f);
        }
        else
        {
            var newScale = modelCamera.orthographicSize + 1f;
            modelCamera.DOOrthoSize(Mathf.Min(newScale, 8f),0.3f);
        }
    }
}