using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class ColorSystem : AbstractSystem
{
    public Dictionary<ItemColor, Texture> ColorTex = new Dictionary<ItemColor, Texture>();
 
    public async UniTask LoadTex()
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync("color");
        await locationsHandle.Task;
        if (locationsHandle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var location in locationsHandle.Result)
            {
                Debug.Log($"Loading asset: {location.PrimaryKey}");
                
                // 加载单个资源
                var loadHandle = Addressables.LoadAssetAsync<Texture>(location);
                await loadHandle.Task;
                
                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"Successfully loaded: {loadHandle.Result}");
                    // 使用加载的资源...
                    ColorTex.Add(StringToEnum<ItemColor>(location.PrimaryKey, ItemColor.None),loadHandle.Result);
                }
                else
                {
                    Debug.LogError($"Failed to load: {location.PrimaryKey}");
                }
                
                Addressables.Release(loadHandle);
            }
        }
    }
    
    public Texture GetRandomTex()
    {
      return  ColorTex[GetRandomEnumValue<ItemColor>()];
    }
    
    public T GetRandomEnumValue<T>()
    {
        var values = System.Enum.GetValues(typeof(T));
        int index = Random.Range(0, values.Length-1);
        return (T)values.GetValue(index);
    }
    
    public T StringToEnum<T>(string str, T defaultValue = default) where T : struct
    {
        if (System.Enum.TryParse<T>(str, true, out var result))
        {
            Debug.Log(str+"  StringToEnum = " + result);
            return result;
        }
        Debug.Log(str+"  StringToEnum = " + defaultValue);
        return defaultValue;
    }

    protected override void OnInit()
    {

    }
}