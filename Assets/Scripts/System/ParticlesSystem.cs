using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ParticlesSystem : AbstractSystem
{
    GameObject baofa;
    GameObject xingxing;

    protected override async void OnInit()
    {
        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>("baofa");
        if(obj.Status == AsyncOperationStatus.Succeeded)
        {
            baofa = obj.Result;
        }

        var xxObj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>("xingxing");
        if(xxObj.Status == AsyncOperationStatus.Succeeded)
        {
            xingxing = xxObj.Result;
        }
    }

    public GameObject GetBaoFa()
    {
        return Object.Instantiate(baofa);
    }

    public GameObject GetXingXing()
    {
        return Object.Instantiate(xingxing);
    }
}