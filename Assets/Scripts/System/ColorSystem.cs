using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class ColorSystem : AbstractSystem
{
    public Dictionary<ItemColor, Texture> ColorTex = new Dictionary<ItemColor, Texture>();

    public async UniTask LoadTex()
    {
        if (!ColorTex.ContainsKey(ItemColor.Red))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("red");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Red, obj.Result);
            }
        }

        if (!ColorTex.ContainsKey(ItemColor.Blue))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("blue");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Blue, obj.Result);
            }
        }

        if (!ColorTex.ContainsKey(ItemColor.Green))
        {

            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("green");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Green, obj.Result);
            }
        }

        if (!ColorTex.ContainsKey(ItemColor.Orange))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("orange");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Orange, obj.Result);
            }
        }

        if (!ColorTex.ContainsKey(ItemColor.Violet))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("purple");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Violet, obj.Result);
            }
        }

        if (!ColorTex.ContainsKey(ItemColor.Yellow))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("yellow");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Yellow, obj.Result);
            }
        }

        if (!ColorTex.ContainsKey(ItemColor.White))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("white");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.White, obj.Result);
            }
        }
        if (!ColorTex.ContainsKey(ItemColor.Black))
        {
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Texture>("black");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                ColorTex.Add(ItemColor.Black, obj.Result);
            }
        }
    }

    protected override void OnInit()
    {

    }
}