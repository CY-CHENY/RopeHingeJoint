using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GogoGaga.OptimizedRopesAndCables;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

// public class RopeData
// {
//     public GameObject obj;
//     public CapsuleRope rope;
//     public Transform start;
//     public Transform end;
// }

public class RopeSystem : AbstractSystem
{
    private GameObject capsuleRopePrefab;
    //private List<RopeData> ropes;
    private List<CapsuleRope> ropes;
    protected override async void OnInit()
    {
        ropes = new List<CapsuleRope>();

        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<GameObject>("Rope");
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            capsuleRopePrefab = obj.Result;
        }
    }

    public void Clear()
    {
        ropes.Clear();
    }

    // public async UniTask<RopeData> CreateRope()
    // {
    //     var ropeData = ropes.Find(r => !r.obj.activeSelf);
    //     if (ropeData == null)
    //     {
    //         RopeData data = new RopeData();
    //         GameObject ropeObj = new GameObject($"rope{ropes.Count + 1}");
    //         ropeObj.transform.position = Vector3.zero;
    //         GameObject start = new GameObject("start");
    //         start.transform.SetParent(ropeObj.transform);
    //         GameObject end = new GameObject("end");
    //         end.transform.SetParent(ropeObj.transform);
    //         data.obj = ropeObj;
    //         data.start = start.transform;
    //         data.end = end.transform;

    //         CapsuleRope rope = capsuleRopePrefab.Instantiate().AddComponent<CapsuleRope>();
    //         rope.gameObject.name = "rope";
    //         rope.transform.SetParent(ropeObj.transform);
    //         rope.startPoint = start.transform;
    //         rope.endPoint = end.transform;

    //         data.rope = rope;

    //         // var meshRenderer = RopeObj.AddComponent<MeshRenderer>();
    //         // var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("RopeWire");
    //         // if (obj.Status == AsyncOperationStatus.Succeeded)
    //         // {
    //         //     meshRenderer.material = Object.Instantiate(obj.Result);
    //         // }
    //         // // meshRenderer.material = this.GetSystem<IYooAssetsSystem>().LoadAssetSync<Material>("RopeWire");
    //         // //rope.ropeWidth = 0.03f;
    //         // var lineRenderer = rope.GetComponent<LineRenderer>();
    //         // if (lineRenderer != null)
    //         //     lineRenderer.enabled = false;

    //         // RopeObj.AddComponent<RopeMesh>().ropeWidth = 0.03f;

    //         // data.rope = rope;

    //         ropeData = data;
    //         ropes.Add(ropeData);
    //     }
    //     ropeData.obj.SetActive(true);
    //     return ropeData;
    // }

    public async UniTask<CapsuleRope> CreateRope()
    {
        var rope = ropes.Find(r => !r.gameObject.activeSelf);
        if (rope == null)
        {
            CapsuleRope capsuleRope = capsuleRopePrefab.Instantiate().AddComponent<CapsuleRope>();
            capsuleRope.name = $"rope{ropes.Count + 1}";
            capsuleRope.transform.position = Vector3.zero;

            rope = capsuleRope;
            ropes.Add(capsuleRope);
        }

        return rope;
    }
}
