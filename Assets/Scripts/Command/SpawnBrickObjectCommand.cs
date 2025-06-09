using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using Cysharp.Threading.Tasks;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;



public class SpawnBrickObjectCommand : AbstractCommand
{
    private Vector3 customPosition;
    public SpawnBrickObjectCommand(Vector3 customPosition)
    {
        this.customPosition = customPosition;
    }



    protected override async void OnExecute()
    {
        var runtimeModel = this.GetModel<RuntimeModel>();
        runtimeModel.BoxPool.Clear();
        runtimeModel.ColorPool.Clear();
        runtimeModel.AllItems.Clear();

        int level = runtimeModel.CurrentLevel.Value;
        LevelConfig levelConfig = ConfigSystem.GetTable().TbLevelConfig.Get(level);//runtimeModel.LevelLegoData[level]);

        await this.GetSystem<ColorSystem>().LoadTex();

        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>
            ($"Assets/GameResources/Prefabs/Level/{int.Parse(levelConfig.PrefabName)+11}.prefab");
        GameObject instance = null;
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            instance = obj.Result.Instantiate();
        }

        instance.transform.localPosition = customPosition;
        instance.AddComponent<MBrick>();
        await instance.AddComponent<ModelManager>().InitModel();

        var boxObj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<LevelConfigSO>
            ($"Assets/GameResources/Datas/{levelConfig.BoxConfig}.asset");
        List<int> configBox = new List<int>();
        if (boxObj.Status == AsyncOperationStatus.Succeeded)
        {
            configBox.AddRange(boxObj.Result.boxPool);
        }

        foreach (var nColor in configBox)
        {
            runtimeModel.BoxPool.Add(new BoxData() { Type = BoxType.Normal, Color = (ItemColor)nColor, CurrentCount = 0 });
        }

        Debug.Log($"总共{runtimeModel.AllItems.Count}个模型");
        PrepareColor(level);
        await PrepareBox();
        SetColorToModel();

        Debug.Log($"总颜色:{runtimeModel.ColorPool.Count}");
        Debug.Log($"总盒子数:{runtimeModel.BoxPool.Count}[{runtimeModel.TotalBox.Value}]");

        runtimeModel.GuideOne.Value = instance;

        this.SendCommand(new SpawnBoxCommand(runtimeModel.GetBoxData(), 0));//创建前两个收集盒子
        this.SendCommand(new SpawnBoxCommand(runtimeModel.GetBoxData(), 1));
        this.SendCommand(new SpawnBoxCommand(new BoxData() { Type = BoxType.Advertisement }, 2));//广告解锁盒子
        this.SendCommand(new SpawnBoxCommand(new BoxData() { Type = BoxType.Advertisement }, 3));

        
        this.SendEvent(new BrickObjectSpawnedEvent() { Instnace = instance });
    }

    void PrepareColor(int level)
    {
        var model = this.GetModel<RuntimeModel>();
        //配置文档中盒子出现的颜色就是本关所需要的所有颜色
        HashSet<ItemColor> colorList = new HashSet<ItemColor>();
        foreach (var item in model.BoxPool)
        {
            if (item.Color != ItemColor.None)
                colorList.Add(item.Color);
        }
        model.ColorPool.AddRange(colorList);
    }

    private class BoxColorResult
    {
        public ItemColor color;
        public int Count;
    }

    async UniTask PrepareBox()
    {
        Material woolMat = null;
        var objHandle = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("Wool");
        if (objHandle.Status == AsyncOperationStatus.Succeeded)
        {
            woolMat = objHandle.Result;
        }

        var model = this.GetModel<RuntimeModel>();
        int boxCount = model.AllItems.Count / 3;
        if (boxCount * 3 != model.AllItems.Count)
        {
            Debug.LogError($"模型数量错误:{model.AllItems.Count}");
            return;
        }
        Debug.Log($"盒子数量:{boxCount}");
        model.TotalBox.Value = boxCount;

        var uncoloredModels = model.AllItems.Where(i => i.Color == ItemColor.None).ToList();
        //已经全部分配颜色了 盒子也要相应的配置完成
        if (uncoloredModels == null || uncoloredModels.Count <= 0)
        {
            return;
        }

        Util.ShuffleList(uncoloredModels);

        if (uncoloredModels.Count % 3 != 0)
            throw new InvalidOperationException("模型不够");

        Debug.Log($"还剩:{uncoloredModels.Count}个模型");

        //剩余盒子个数
        int m = model.ColorPool.Count;
        int totalGroups = uncoloredModels.Count / 3;
        int baseGroups = totalGroups / m;
        int remainingGroups = totalGroups % m;

        var result = new List<BoxColorResult>();

        for (int i = 0; i < m; i++)
        {
            ItemColor color = model.ColorPool[i];
            int groups = i < remainingGroups ? baseGroups + 1 : baseGroups;
            int needs = groups * 3;
            result.Add(new BoxColorResult { color = color, Count = needs });
        }

        List<ItemColor> addBoxColor = new List<ItemColor>();

        int uncoloredIndex = 0;
        foreach (var ret in result)
        {
            int count = ret.Count;
            ItemColor color = ret.color;
            Debug.Log($"{color} ----> {count}");
            for (int i = 0; i < count; i++)
            {
                if (uncoloredIndex < uncoloredModels.Count)
                {
                    uncoloredModels[uncoloredIndex].Color = color;
                    var mr = uncoloredModels[uncoloredIndex].ItemTransform.GetComponent<MeshRenderer>();
                    mr.material = woolMat.Instantiate();
                    mr.material.SetTexture("_BaseMap", this.GetSystem<ColorSystem>().ColorTex[color]);
                    if (mr.material.HasProperty("_DissolveOffest"))
                    {
                        // 获取模型的局部包围盒
                        Bounds bounds = mr.localBounds;
                        float top = bounds.max.y + 0.5f;
                        top = Mathf.Ceil(top * 100);
                        top /= 100;
                        mr.material.SetVector("_DissolveOffest", new Vector4(0, top, 0));
                    }
                    uncoloredIndex++;
                }
            }
            //boxcount
            int bc = count / 3;
            for (int i = 0; i < bc; i++)
            {
                addBoxColor.Add(color);
            }
        }

        Util.ShuffleList(addBoxColor);

        foreach (var color in addBoxColor)
        {
            model.BoxPool.Add(new BoxData() { Type = BoxType.Normal, Color = color, CurrentCount = 0 });
        }


    }

    //将剩下的颜色设置到没有上色的模型
    void SetColorToModel()
    {
        return;
        var model = this.GetModel<RuntimeModel>();
        var validItems = model.AllItems.Where(item => item.Color == ItemColor.None).ToList();
        if (validItems == null)
            return;
        foreach (var color in model.ColorPool)
        {
            //已经分配的颜色数
            int count = model.AllItems.Where(i => i.Color == color).Count();
            int targetCount = model.BoxPool.Where(i => i.Color == color).Count() * 3;

            for (int i = 0; i < targetCount - count; i++)
            {
                if (validItems != null)
                {
                    int index = UnityEngine.Random.Range(0, validItems.Count);
                    var item = validItems[index];
                    item.Color = color;
                    var mr = item.ItemTransform.GetComponent<MeshRenderer>();
                    //mr.material.SetColor("_BaseColor", Util.ColorMapping[color]);
                    mr.material.SetTexture("_BaseMap", this.GetSystem<ColorSystem>().ColorTex[color]);
                    if (mr.material.HasProperty("_DissolveOffest"))
                    {
                        // 获取模型的局部包围盒
                        Bounds bounds = mr.localBounds;
                        //float centerY = bounds.center.y;
                        // float top = (bounds.size.y - 1.0f) / 2.0f + 1.0f - centerY;
                        float top = bounds.max.y + 0.5f;
                        // Debug.Log($"top:{top} maxY:{bounds.max.y}");
                        mr.material.SetVector("_DissolveOffest", new Vector4(0, top, 0));
                    }
                    validItems.RemoveAt(index);
                    Debug.Log($"分配一个颜色{color}给{item.ItemTransform.name}");
                }
            }
        }
    }
}