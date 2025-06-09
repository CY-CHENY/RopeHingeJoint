using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertextDeletion : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("总销毁时间(秒)")]
    public float totalDuration = 5f;

    [Tooltip("顶点删除模式")]
    public DeleteMode deletionMode = DeleteMode.Sequential;

    public Transform lastTransform;

    private Mesh originalMesh;
    private Vector3[] originalVertices;
    private int[] originalTriangles;
    private bool[] vertexAliveStatus;
    private Transform meshTransform;

    private Coroutine destructionCoroutine;


    public enum DeleteMode
    {
        Random,
        Sequential,
        TopToBotton
    }

    void Start()
    {
        InitializeMeshData();
        destructionCoroutine = StartCoroutine(ProgressiveDestruction());
    }
    private void InitializeMeshData()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No MeshFilter Component Found!");
            return;
        }

        originalMesh = Instantiate(meshFilter.mesh);
        if (originalMesh == null)
        {
            Debug.LogError("No Mesh Assigned to MeshFilter!");
            return;
        }

        meshTransform = meshFilter.transform;

        originalVertices = originalMesh.vertices;
        originalTriangles = originalMesh.triangles;
        vertexAliveStatus = new bool[originalVertices.Length];
        System.Array.Fill(vertexAliveStatus, true);
    }

    IEnumerator ProgressiveDestruction()
    {
        float timer = 0;
        int totalVertices = originalVertices.Length;
        while (timer < totalDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / totalDuration);

            int remainingVertices = Mathf.CeilToInt(totalVertices * (1 - progress));

            UpdateMesh(remainingVertices);
            yield return null;
        }

        UpdateMesh(0);
        Debug.Log("delete all vertices!");
        Destroy(gameObject);
    }

    void UpdateMesh(int targetVertextCount)
    {
        List<int> verticesToDelete = GetVerticesToDelete(targetVertextCount);

        foreach (int index in verticesToDelete)
        {
            vertexAliveStatus[index] = false;
            if (lastTransform != null)
            {
                lastTransform.position = meshTransform.TransformPoint(originalVertices[index]);
            }
        }

        RebuildMesh();
    }

    List<int> GetVerticesToDelete(int targetCount)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < vertexAliveStatus.Length; i++)
        {
            if (vertexAliveStatus[i]) candidates.Add(i);
        }

        switch (deletionMode)
        {
            case DeleteMode.Sequential:
                candidates.Sort((a, b) => a.CompareTo(b));
                break;
            case DeleteMode.Random:
                ShuffleList(candidates);
                break;
            case DeleteMode.TopToBotton:
                TopToBottomList(candidates);
                break;
        }

        int deleteCount = candidates.Count - targetCount;
        if (deleteCount <= 0) return new List<int>();

        return candidates.GetRange(0, deleteCount);
    }

    private void RebuildMesh()
    {
        // 重建顶点数据
        List<Vector3> newVertices = new List<Vector3>();
        Dictionary<int, int> indexMap = new Dictionary<int, int>(); // 旧索引 -> 新索引

        // 收集存活顶点并建立映射
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (vertexAliveStatus[i])
            {
                indexMap[i] = newVertices.Count;
                newVertices.Add(originalVertices[i]);
            }
        }

        // 重建三角形数据
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int v0 = originalTriangles[i];
            int v1 = originalTriangles[i + 1];
            int v2 = originalTriangles[i + 2];

            // 仅保留所有顶点都存活的三角形
            if (vertexAliveStatus[v0] && vertexAliveStatus[v1] && vertexAliveStatus[v2])
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

    // 辅助方法：随机打乱列表
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    void TopToBottomList(List<int> list)
    {
        List<VertexData> sortedVertices = new List<VertexData>();
        for (int i = 0; i < list.Count; i++)
        {
            sortedVertices.Add(new VertexData { index = list[i], position = originalVertices[list[i]] });
        }
        list.Clear();

        sortedVertices.Sort((a, b) => b.position.y.CompareTo(a.position.y));
        foreach (var vertexData in sortedVertices)
        {
            list.Add(vertexData.index);
        }
    }

    // 外部控制方法
    public void PauseDestruction() => StopCoroutine(destructionCoroutine);
    public void ResumeDestruction() => destructionCoroutine = StartCoroutine(ProgressiveDestruction());

    private class VertexData
    {
        public int index;
        public Vector3 position;
    }
}
