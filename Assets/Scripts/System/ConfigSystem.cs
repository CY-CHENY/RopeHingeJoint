using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using QFramework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using SimpleJSON;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;
using cfg;
using UnityEngine.AddressableAssets;

public class ConfigSystem : AbstractSystem
{

    public LegoDataSO LegoConfig { get; set; }
    
    public List<ItemEntity> ItemConfig { get; set; }

    public ColorMaterialSO ColorConfig {get; set;}
     
    private static Tables tables;
    
    public static Tables GetTable()
    {
        return tables;
    }

    
    protected override void OnInit()
    {

    }

    public async UniTask LoadConfig()
    {
        ItemConfig = Util.ItemConfig;
        //Luban初始化
        Dictionary<string, string> dicData = new Dictionary<string, string>();
        // Debug.Log(DataUtility.GetTable().TbTest.DataList.Count);
        var locationsHandle = Addressables.LoadResourceLocationsAsync("default");
        await locationsHandle.Task;
        if (locationsHandle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var location in locationsHandle.Result)
            {
                    Debug.Log($"Loading asset: {location.PrimaryKey}");
                
                // 加载单个资源
                var loadHandle = Addressables.LoadAssetAsync<TextAsset>(location);
                await loadHandle.Task;
                
                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                          Debug.Log($"Successfully loaded: {loadHandle.Result}");
                    // 使用加载的资源...
                    dicData.Add(location.PrimaryKey,loadHandle.Result.text);
                }
                else
                {
                    Debug.LogError($"Failed to load: {location.PrimaryKey}");
                }
                
                Addressables.Release(loadHandle);
            }
        }

        tables = new Tables((b) => { return JSON.Parse(dicData[b]); });
        // 现在可以用 tempTable 访问数据了
        Log.Debug("------luban加载完成-------"+GetTable().TbPlayer.DataList.Count);
    }

    public async UniTask<Sprite> GetPropSprite(int id)
    {
        ItemEntity data= ItemConfig.Find(v => v.Id == id);
        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.iconPath);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            return obj.Result;    
        }
        return null;
    }
}