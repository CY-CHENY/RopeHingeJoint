using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    private Renderer modelRenderer;
    private Material material;
    private Bounds bounds;
    private float rayCount = 30;

    private float rotationSpeed = 60f;// 每次发射转角度 = 360° / 6 = 60°
    private float currentAngle = 0f;
    private float elapsedTime = 0f;
    private float timeBetweenRays;
    private float lastRayTime = 0f;
    private int raysFired = 0;

    // private GameObject[] hitPointMarkers;

    private MeshFilter meshFilter;

    private bool Enable = false;

    private float Max;
    private float Min;
    private bool Up;

    void Awake()
    {
        // Debug.Log(transform.right);    
        // Debug.DrawRay(transform.position, transform.right * 100f, Color.red, 5f);
    }


    public void InitCtl()
    {
        modelRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        material = modelRenderer.material;
        // 获取模型的局部包围盒
        bounds = modelRenderer.localBounds;
    }

    //从上往下消除
    // private void GetOffset(bool up, out float max, out float min)
    // {
    //     if (up)
    //     {
    //         float top = bounds.max.y + 0.5f;
    //         top = Mathf.Ceil(top * 100);
    //         top /= 100;
    //         max = top;
    //         min = bounds.min.y + 0.5f;
    //     }
    //     else
    //     {
    //         float top = bounds.min.y - 0.5f;
    //         //正常情况top应该小于0
    //         top = Mathf.Floor(top * 100);
    //         top /= 100;
    //         max = top;
    //         min = bounds.max.y - 0.5f;
    //     }
    // }

    private void GetOffset(bool up, out float max, out float min)
    {
        if (up)
        {
            float top = bounds.max.y + 0.5f;
            top = Mathf.Ceil(top * 100);
            top /= 100;
            max = top;
            min = bounds.min.y + 0.5f;
        }
        else
        {
            float top = bounds.max.x + 0.5f;
            //正常情况top应该小于0
            top = Mathf.Floor(top * 100);
            top /= 100;
            max = top;
            min = bounds.min.x + 0.5f;
        }
    }

    private void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit, 100f))
        //     {
        //         var localPos = transform.InverseTransformPoint(hit.point);
        //         Debug.Log(localPos);
        //         InitCtl();
        //         StartDissolve(10f, localPos.y > 0);
        //     }
        //     return;
        // }
        if (!Enable)
            return;
        if (raysFired >= rayCount)
            return;

        elapsedTime += Time.deltaTime;
        currentAngle = rotationSpeed * (elapsedTime / timeBetweenRays);

        if (elapsedTime - lastRayTime >= timeBetweenRays)
        {
            FireRay();
            lastRayTime = elapsedTime;
            raysFired++;
        }
    }

    public Ray Reverse(Ray ray, float distance)
    {
        return new Ray(ray.origin + ray.direction * distance, -ray.direction);
    }

    private void FireRay()
    {
       // Debug.Log("FireRay");
        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 direction = rotation * transform.right;

        var offset = material.GetVector("_DissolveOffest");
        float percent = (float)Math.Round((offset.y - Min) / (Max - Min), 2);
        // Debug.Log($"max:{Max} min:{Min} offset:{offset.y} percent:{percent}");

        //float baseY = Up ? bounds.center.y - (bounds.size.y / 2f) : bounds.center.y + (bounds.size.y / 2f);
        //局部y坐标
        //float yPos = Up ? baseY + (percent * bounds.size.y) : baseY - (percent * bounds.size.y);
        float baseY = Up ? bounds.center.y - (bounds.size.y / 2f) : bounds.center.x + (bounds.size.x / 2f);
        //局部y坐标
        float yPos = Up ? baseY + (percent * bounds.size.y) : baseY - (percent * bounds.size.x);

        var localPos = new Vector3(Up?0:yPos, Up?yPos:0, 0);
        var rayOrigin = meshFilter.transform.TransformPoint(localPos);
        //由于模型可能不规则 无法确定模型内部点 所以射线反向 从模型外向模型发射
        Ray ray = Reverse(new Ray(rayOrigin, direction), 3f);
        //Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 100f);
        // CreateHitMarker(ray.origin, Color.green);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.Equals(gameObject))
                {
                    // Debug.DrawRay(rayOrigin, direction * 100f, Color.red, 5f);
                    //CreateHitMarker(hit.point, Color.red);
                    this.callback?.Invoke(hit.point);
                    break;
                }
            }
        }
        else
        {
            // Debug.DrawRay(rayOrigin, direction * 100f, Color.gray, 5f);
            //Debug.LogWarning("射线未击中模型边界");
        }
    }

    private void CreateHitMarker(Vector3 position, Color color)
    {
        Debug.Log("CreateHitMarker");
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.1f;
        marker.GetComponent<Renderer>().material.color = color;
        marker.GetComponent<Collider>().enabled = false;
        // hitPointMarkers[index] = marker;
    }

    private Action<Vector3> callback;

    public void StartDissolve(float time, bool up, Action<Vector3> callback = null)
    {
        float max, min;
        GetOffset(up, out max, out min);
        Max = max;
        Min = min;
        Up = up;
        if (up)
        {
            material.SetVector("_DissolveDirection", new Vector3(0,  -1, 0));
            material.SetVector("_DissolveOffest", new Vector3(0, max, 0));
        }
        else
        {
            material.SetVector("_DissolveDirection", new Vector3(0, 0, -1));
            material.SetVector("_DissolveOffest", new Vector3(0, 0, max));
        }
       

        timeBetweenRays = time / rayCount;
        Enable = true;
        this.callback = callback;
        material.DOVector(new Vector4(up?0:min, up?min:0, 0), "_DissolveOffest", time).SetEase(Ease.Linear).OnComplete(() =>
        {
            this.callback = null;
            Enable = false;
            GetComponent<ModelRope>().Detach();
            Destroy(gameObject);
            
        });
    }
}
