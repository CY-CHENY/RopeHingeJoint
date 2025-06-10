using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cfg;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class SpawnMainUIBrickObjectCommand : AbstractCommand
{
    private Vector3 customPosition;
    private int index;
    public SpawnMainUIBrickObjectCommand(Vector3 customPosition,int index)
    {
        this.customPosition = customPosition;
        this.index = index;
    }
    protected override async void OnExecute()
    {
        var runtimeModel = this.GetModel<RuntimeModel>();
        runtimeModel.BoxPool.Clear();
        runtimeModel.ColorPool.Clear();
        runtimeModel.AllItems.Clear();
        int level = runtimeModel.CurrentLevel.Value;
        if (level > runtimeModel.LevelLegoData.Count) level = runtimeModel.LevelLegoData.Count;
        LevelConfig levelConfig = runtimeModel.LevelLegoData[level];
        
        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>
            ($"Assets/GameResources/Prefabs/Level/{int.Parse(levelConfig.PrefabName)+11}.prefab");
        GameObject instance = null;
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            instance = obj.Result.Instantiate();
        }

        if (instance != null)
        {
            instance.transform.localPosition = customPosition;
            await instance.AddComponent<ModelManager>().InitModel();
        }

       // RoatateObj(instance.transform);

    }

    void RoatateObj(Transform trans)
    {
        trans.localEulerAngles = new Vector3(-10, 0, 0);
        var sequence = DOTween.Sequence();
        sequence.Append(trans.DOBlendableLocalRotateBy(new Vector3(0, 360, 0), 1f, RotateMode.FastBeyond360).SetDelay(0.3f))
            .Append(trans.DOBlendableRotateBy(new Vector3(-10, 0, 0), 0.5f, RotateMode.FastBeyond360))
            .Append(trans.DOScale(0.9f, 0.3f)).OnComplete(() =>
            {
             //   this.GetModel<RuntimeModel>().GameStart.Value = true;
            });
    }
    
    
}
