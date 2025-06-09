using UnityEngine;

public class CapsuleRope : MonoBehaviour
{
    private Vector3 start;
    private Vector3 end;
    public void UpdateStart(Vector3 start)
    {
        if (end != null)
            UpdatePoint(start, end);
    }

    public void UpdateEnd(Vector3 end)
    {
        if (start != null)
            UpdatePoint(start, end);
    }

    public void UpdatePoint(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;
        // 计算方向向量和距离
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        // 处理距离过小的情况
        if (distance < Mathf.Epsilon) return;

        // 设置胶囊体位置为两点中点
        transform.position = (start + end) / 2f;

        // 旋转胶囊体，使本地Y轴对齐到方向向量
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

        // 调整Y轴缩放，使胶囊体长度匹配两点距离
        // 默认胶囊体高度为1.174单位，因此缩放系数为distance/1.174
        transform.localScale = new Vector3(
            transform.localScale.x,
            distance / 1.174f,
            transform.localScale.z
        );
    }
}