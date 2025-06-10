using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class HorizontalLayout : MonoBehaviour
{
    public float spacingX = 0f;
    private float lastSpacingX;
    private RectTransform parentRect;
    private int lastChildCount;
    private Vector2 lastParentSize;

    private void OnEnable()
    {
        UpdateLayout();
    }

    void OnDisable()
    {
    }

    void OnValidate()
    {
        if (spacingX != lastSpacingX)
        {
            UpdateLayout();
            lastSpacingX = spacingX;
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (parentRect != null && parentRect.rect.size != lastParentSize)
        {
            UpdateLayout();
            lastParentSize = parentRect.rect.size;
        }
    }

    public void UpdateLayoutIfNeeded()
    {
        Debug.Log("UpdateLayoutIfNeeded");
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        if (parentRect == null)
            parentRect = GetComponent<RectTransform>();

        int currentChildCount = transform.childCount;
        Vector2 currentParentSize = parentRect.rect.size;

        if (currentChildCount == 0 /*|| (lastChildCount == currentChildCount && lastSpacingX == spacingX && lastParentSize == currentParentSize)*/) return;

        float parentWidth = parentRect.rect.width;
        float childWidth = (parentWidth - (currentChildCount - 1) * spacingX) / currentChildCount;
        Debug.Log($"childWidth:{childWidth} childCount:{currentChildCount}");
        float startX = childWidth / 2f;

        for (int i = 0; i < currentChildCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            child.anchorMin = new Vector2(0, 1);
            child.anchorMax = new Vector2(0, 1);

            float posX = startX + i * (childWidth + spacingX);
            child.sizeDelta = new Vector2(childWidth, child.sizeDelta.y);
            child.anchoredPosition = new Vector2(posX, -child.sizeDelta.y / 2f);
        }

        // 更新缓存
        lastChildCount = currentChildCount;
        lastSpacingX = spacingX;
        lastParentSize = currentParentSize;
    }
}
