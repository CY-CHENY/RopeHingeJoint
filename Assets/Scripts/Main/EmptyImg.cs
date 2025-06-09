using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 空白背景图片点击
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasRenderer))]
public class EmptyImg : MaskableGraphic
{
    protected EmptyImg()
    {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
