using UnityEngine;
using QFramework;
using System;
using DG.Tweening;
using System.Collections.Generic;
public class MBrick : MonoBehaviour, IController
{

    private RotationModel rotaionModel;
    private ScaleModel scaleModel;

    private Transform rotateBody;

    void Awake()
    {
        if (transform.Find("mainBody") != null)
        {
            rotateBody = transform.Find("mainBody");
        }
        else
        {
            rotateBody = transform;
        }
       
        rotaionModel = this.GetModel<RotationModel>();
        rotaionModel.EulerAngles.RegisterWithInitValue(OnRotationChanged).UnRegisterWhenGameObjectDestroyed(gameObject);

        scaleModel = this.GetModel<ScaleModel>();
        scaleModel.Scale.RegisterWithInitValue(OnScaleChanged).UnRegisterWhenGameObjectDestroyed(gameObject);

        this.RegisterEvent<ResetModelRotationEvent>(OnResetModelRotation).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<BrickObjectSpawnedEvent>(OnBrickObjectSpawned).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnBrickObjectSpawned(BrickObjectSpawnedEvent evt)
    {
        Debug.Log("OnBrickObjectSpawned");
        this.GetModel<RuntimeModel>().GameStart.Value = true;
        return;
        transform.localEulerAngles = new Vector3(-10, 0, 0);
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOBlendableLocalRotateBy(new Vector3(0, 360, 0), 1f, RotateMode.FastBeyond360).SetDelay(0.3f))
        .Append(transform.DOBlendableRotateBy(new Vector3(-10, 0, 0), 0.5f, RotateMode.FastBeyond360))
        .Append(transform.DOScale(0.9f, 0.3f)).OnComplete(() =>
        {
            this.GetModel<RuntimeModel>().GameStart.Value = true;
        });
    }

    private void OnScaleChanged(float newScale)
    {
        transform.DOScale(newScale, 0.3f);
    }

    private void OnResetModelRotation(ResetModelRotationEvent evt)
    {
        rotateBody.DORotate(new Vector3(0, 0, 0), 0.3f);
    }

    private void OnRotationChanged(float newEuler)
    {
        rotateBody.Rotate(Vector3.up, -newEuler*0.2f, Space.World);
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}