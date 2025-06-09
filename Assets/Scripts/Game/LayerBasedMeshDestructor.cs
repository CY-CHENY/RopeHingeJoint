using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerBasedMeshDestructor : MonoBehaviour
{
    [Header("分层删除设置")]
    [Tooltip("总销毁时间（秒）")]
    public float totalDuration = 5f;

    [Tooltip("垂直分层数量")]
    public int layers = 10;

    [Tooltip("删除方向")]
    public DeleteDirection direction = DeleteDirection.BottomToTop;

    // 分层删除方向枚举
    public enum DeleteDirection
    {
        BottomToTop,    // 从下往上
        TopToBottom     // 从上往下
    }

    private Mesh originMesh;
    private Vector3[] originalVertices;
    private int[] originalTriangles;
    private List<List<int>> vertexLayers;
    private float minY, maxY;

    private Transform meshTransform;

    public Transform lastDeleteTransform;

    public bool isFinished;

    void Start()
    {
        isFinished = false;
        InitializeMeshData();
        PrecomputeVertexLayers();
        StartCoroutine(LayeredDestruction());
    }

    void InitializeMeshData()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        originMesh = Instantiate(filter.mesh);
        filter.mesh = originMesh;

        meshTransform = filter.transform;

        originalVertices = originMesh.vertices;
        originalTriangles = originMesh.triangles;

        // 计算Y轴范围
        minY = float.MaxValue;
        maxY = float.MinValue;
        foreach (Vector3 v in originalVertices)
        {
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }
    }

    // 预计算顶点分层（优化性能的关键）
    void PrecomputeVertexLayers()
    {
        vertexLayers = new List<List<int>>(layers);
        float layerHeight = (maxY - minY) / layers;

        // 初始化层容器
        for (int i = 0; i < layers; i++)
        {
            vertexLayers.Add(new List<int>());
        }

        // 分配顶点到各层
        for (int i = 0; i < originalVertices.Length; i++)
        {
            float relativeY = originalVertices[i].y - minY;
            int layerIndex = Mathf.FloorToInt(relativeY / layerHeight);
            layerIndex = Mathf.Clamp(layerIndex, 0, layers - 1);

            vertexLayers[layerIndex].Add(i);
        }

        // 根据删除方向调整层顺序
        if (direction == DeleteDirection.TopToBottom)
        {
            vertexLayers.Reverse();
        }
    }

    IEnumerator LayeredDestruction()
    {
        bool[] vertexAliveStatus = new bool[originalVertices.Length];
        System.Array.Fill(vertexAliveStatus, true);

        float layerDuration = totalDuration / layers;
        int currentLayer = 0;

        while (currentLayer < layers)
        {
            Debug.Log("删除层: " + currentLayer);
            // 标记当前层顶点为已删除
            foreach (int index in vertexLayers[currentLayer])
            {
                vertexAliveStatus[index] = false;
                if (lastDeleteTransform != null)
                {
                    lastDeleteTransform.position = meshTransform.TransformPoint(originalVertices[index]);
                }
            }

            // 重建网格
            RebuildMesh(vertexAliveStatus);

            currentLayer++;
            yield return new WaitForSeconds(layerDuration);
        }

        // 最终完全销毁
        System.Array.Fill(vertexAliveStatus, false);
        RebuildMesh(vertexAliveStatus);
        Debug.Log("Delete All Vertices");
        isFinished = true;
    }

    void RebuildMesh(bool[] aliveStatus)
    {
        List<Vector3> newVertices = new List<Vector3>();
        Dictionary<int, int> indexMap = new Dictionary<int, int>();

        // 收集存活顶点
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (aliveStatus[i])
            {
                indexMap[i] = newVertices.Count;
                newVertices.Add(originalVertices[i]);
            }
        }

        // 重建三角形
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int v0 = originalTriangles[i];
            int v1 = originalTriangles[i + 1];
            int v2 = originalTriangles[i + 2];

            if (aliveStatus[v0] && aliveStatus[v1] && aliveStatus[v2])
            {
                newTriangles.Add(indexMap[v0]);
                newTriangles.Add(indexMap[v1]);
                newTriangles.Add(indexMap[v2]);
            }
        }

        // 应用新网格
        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices.ToArray();
        newMesh.triangles = newTriangles.ToArray();
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = newMesh;
    }
}