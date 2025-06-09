using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BoxSystem : AbstractSystem
{
    private GameObject BoxPrefab;
    protected override async void OnInit()
    {
        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>("Box");
        if(obj.Status == AsyncOperationStatus.Succeeded)
        {
            BoxPrefab = obj.Result;
        }
    }

    public GameObject GetBoxPrefab()
    {
        return BoxPrefab;
    }
}