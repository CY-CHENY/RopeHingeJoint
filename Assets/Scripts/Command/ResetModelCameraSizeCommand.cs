using DG.Tweening;
using QFramework;
using UnityEngine;

public class ResetModelCameraSizeCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        Camera modelCamera = GameObject.FindWithTag("ModelCamera").GetComponent<Camera>();
        modelCamera.DOOrthoSize(5,0.3f);
    }
}