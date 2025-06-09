using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class Block : MonoBehaviour, IController
{
    private GameObject block;

    public bool IsEmpty()
    {
        return block == null;
    }
    public async UniTask<GameObject> CreateSlot(ItemColor color)
    {
        // var woolMaterial = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<Material>("WoodMat");
        // var woodPrefab = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<GameObject>("DGLXX_wood");

        Material woolMaterial = null;
        Material woodMaterial = null;
        GameObject woodPrefab = null;

        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("Wool");
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            woolMaterial = obj.Result;
        }

        var obj1 = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("WoodMat");
        if (obj1.Status == AsyncOperationStatus.Succeeded)
        {
            woodMaterial = obj1.Result;
        }

        var obj2 = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>("Wood");
        if (obj2.Status == AsyncOperationStatus.Succeeded)
        {
            woodPrefab = obj2.Result;
        }

        block = Instantiate(woodPrefab);
        block.transform.SetParent(transform);
        block.transform.localRotation = Quaternion.Euler(new Vector3(-44, 0, 0));
        block.transform.localPosition = new Vector3(0, 0.1f, -0.22f);
        block.transform.localScale = new Vector3(1.32f, 1.32f, 1.32f);
        var model = block.transform.GetChild(0).GetChild(0);
        for (int i = 0; i < model.childCount; i++)
        {
            Transform child = model.GetChild(i);
            if (!child.name.Equals("pCube_a") && !child.name.Equals("pTorus1"))
            {
                child.gameObject.SetActive(false);

                var instanceMat = Instantiate(woolMaterial);
                var meshRender = child.GetComponent<MeshRenderer>();
                meshRender.material = instanceMat;
                meshRender.material.SetColor("_BaseColor", Util.ColorMapping[color]);
            }
            else
            {
                var instanceMat = Instantiate(woodMaterial);
                var meshRender = child.GetComponent<MeshRenderer>();
                meshRender.material = instanceMat;
                child.gameObject.SetActive(true);
            }
        }

        return block;
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}