using QFramework;

public interface IYooAssetsSystem : ISystem
{
    // public Task<T> LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
    // public T LoadAssetSync<T>(string path) where T : UnityEngine.Object;
    // public T[] LoadAllAssetsSync<T>(string path) where T : UnityEngine.Object;
}


public class YooAssetsSystem : AbstractSystem, IYooAssetsSystem
{

    protected override void OnInit()
    {
    }


    // public T[] LoadAllAssetsSync<T>(string path) where T : UnityEngine.Object
    // {
    //     var handle = YooAssets.LoadSubAssetsSync<T>(path);
    //     return handle.GetSubAssetObjects<T>();
    // }

    // public async Task<T> LoadAssetAsync<T>(string path) where T : UnityEngine.Object
    // {
    //     AssetHandle handle = YooAssets.LoadAssetAsync<T>(path);
    //     await handle.Task;
    //     return handle.AssetObject as T;
    // }

    // public T LoadAssetSync<T>(string path) where T : UnityEngine.Object
    // {
    //     AssetHandle handle = YooAssets.LoadAssetSync<T>(path);
    //     return handle.AssetObject as T;
    // }
}