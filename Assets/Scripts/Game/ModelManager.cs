using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using GogoGaga.OptimizedRopesAndCables;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class ModelManager : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
    Dictionary<string, Transform> group = new Dictionary<string, Transform>();

    
    public async UniTask InitModel()
    {
        var model = this.GetModel<RuntimeModel>();

        Material material = null;
        var objHandle = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Material>("Wool");
        if (objHandle.Status == AsyncOperationStatus.Succeeded)
        {
            material = objHandle.Result;
        }
        
        //初始化主干
        transform.Find("mainBody").GetOrAddComponent<Rigidbody>().isKinematic = true;
        transform.Find("mainBody").GetOrAddComponent<BoxCollider>();
        for (int i = 0; i < transform.Find("mainBody").childCount; i++)
        {
            Transform child = transform.Find("mainBody").GetChild(i);
            child.GetOrAddComponent<Rigidbody>().isKinematic = true;
            child.GetOrAddComponent<BoxCollider>().isTrigger = false;
        }
        
        var mainRigidBody = transform.Find("mainBody").GetComponent<Rigidbody>();
        var dicRopeRigid = new Dictionary<int, Rigidbody>();
        var dicFaceRigid = new Dictionary<int, Rigidbody>();
        //初始化面
        for (int i = 0; i < transform.Find("Face").childCount; i++)
        {
            Transform child = transform.Find("Face").GetChild(i);
            child.GetOrAddComponent<Rigidbody>().drag = 8;
            child.GetOrAddComponent<BoxCollider>().isTrigger = true;
            if (child.name.Contains("rope"))
            {
                string[] index = child.name.Split('_');
                dicRopeRigid.Add(int.Parse(index[1]),child.GetComponent<Rigidbody>());
            } 
            if (child.name.Contains("face"))
            {
                string[] index = child.name.Split('_');
                dicFaceRigid.Add(int.Parse(index[1]),child.GetComponent<Rigidbody>());
            }
            var meshRenderer = child.GetComponent<MeshRenderer>();
            //Log.Debug("meshRenderer = "+meshRenderer);
            // Debug.Log(meshRenderer.material.name);
                //渲染节点
                var color = meshRenderer.material.color;
                ItemColor itemColor = Util.FindClosestColor(color);
                //只把绳子加进可以消除
                if (child.name.Contains("rope"))
                {
                    model.AllItems.Add(new ItemData {Color = itemColor, ItemTransform = child});
                    //渲染绳子
                    if (itemColor != ItemColor.None)
                    {
                        meshRenderer.material = Instantiate(material);
                        meshRenderer.material.SetTexture("_BaseMap", this.GetSystem<ColorSystem>().ColorTex[itemColor]);
                        if (meshRenderer.material.HasProperty("_DissolveOffest"))
                        {
                            // 获取模型的局部包围盒
                            Bounds bounds = meshRenderer.localBounds;
                            float top = bounds.max.y + 0.5f;
                            top = Mathf.Ceil(top * 100);
                            top /= 100;
                            meshRenderer.material.SetVector("_DissolveOffest", new Vector4(0, top, 0));
                        }
                    }
                }
                else if (child.name.Contains("face"))
                {
                    meshRenderer.material = Instantiate(material);
                    meshRenderer.material.SetTexture("_BaseMap", this.GetSystem<ColorSystem>().GetRandomTex());
                }

        }

        foreach (var v in dicRopeRigid)
        {
            HingeJoint hingeJoint = v.Value.GetOrAddComponent<HingeJoint>();
            hingeJoint.connectedBody = mainRigidBody;
            hingeJoint.useLimits = true;
            hingeJoint.limits = new JointLimits {
                min = -10,  // 最小角度
                max = 10,   // 最大角度
                bounciness = 0,  // 弹性设为0
                contactDistance = 0.1f
            };
            hingeJoint.useSpring = true;
            hingeJoint.spring = new JointSpring()
            {
                spring = 10,
            };
            hingeJoint.axis = Vector3.forward;//(0,0,1)
            hingeJoint.anchor = new Vector3(0, 0.5f, 0);
            hingeJoint.GetOrAddComponent<ModelRope>();
        }
        
        foreach (var v in dicFaceRigid)
        {
            HingeJoint hingeJoint = v.Value.GetOrAddComponent<HingeJoint>();
            hingeJoint.connectedBody = dicRopeRigid[v.Key];
            hingeJoint.useLimits = true;
            hingeJoint.limits = new JointLimits {
                min = -30,  // 最小角度
                max = 30,   // 最大角度
                bounciness = 0,  // 弹性设为0
                contactDistance = 0.1f
            };
            hingeJoint.useSpring = true;
            hingeJoint.spring = new JointSpring()
            {
                spring = 10,
            };
            hingeJoint.axis = Vector3.forward;//(0,0,1)
            hingeJoint.anchor = new Vector3(0, 0.5f, 0);
            hingeJoint.GetOrAddComponent<ModelFace>().Init();
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        this.RegisterEvent<ClickModelEvent>(OnModeClicked).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<LegoRaiseEvent>(OnCheckLegoRaise).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnCheckLegoRaise(LegoRaiseEvent obj)
    {
        Debug.Log("抬高模型");
    }

    private void OnModeClicked(ClickModelEvent evt)
    {
        Debug.Log($"OnModeClicked:{evt.transform.name}");
        var zhuangshi = evt.transform.Find("zhuangshi");
        if (zhuangshi != null)
        {
            zhuangshi.transform.SetParent(null);
            var rb = zhuangshi.GetOrAddComponent<Rigidbody>();
            rb.AddForce(zhuangshi.forward);
            Util.DelayExecuteWithSecond(3f, () =>
            {
                Destroy(zhuangshi.gameObject);
            });
        }

        var parentName = evt.transform.parent.name;
        if (group.ContainsKey(parentName))
        {
            Transform parentNode = group[parentName];
            int nextIndex = 0;
            for (int i = 0; i < parentNode.childCount; i++)
            {
                Transform child = parentNode.GetChild(i);
                if (child == evt.transform)
                {
                    nextIndex = i + 1;
                    break;
                }
            }

            if (nextIndex < parentNode.childCount)
            {
                Transform next = parentNode.GetChild(nextIndex);
                next.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                next.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.1f).SetDelay(1f).OnComplete(() =>
                {
                    next.GetOrAddComponent<MeshCollider>();
                });
                next.gameObject.SetActive(true);
            }
        }
    }
    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {

    }
}
