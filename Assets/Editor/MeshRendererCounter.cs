using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.Formula.Functions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;

public class MeshRendererCounter : EditorWindow
{
    [MenuItem("Tools/统计模型数量")]
    static void CountMeshRenderers()
    {
        GameObject selectedObj = Selection.gameObjects[0];
        MeshRenderer[] renderers = selectedObj.GetComponentsInChildren<MeshRenderer>(true);
        MeshRenderer[] renderers1 = renderers.Where(v => !v.name.Equals("zhuangshi")).ToArray();
        Debug.Log("----------------------------------------");
        Debug.Log($"总共有{renderers1.Length}个模型");
        Debug.Log("----------------------------------------");
    }

    public static Dictionary<ItemColor, Color> ColorMapping = new Dictionary<ItemColor, Color>()
    {
        {ItemColor.Red, new Color(248/255f,91/255f,106/255f)},
        { ItemColor.Blue,    new Color(52/255.0f, 61 / 255f, 202/255f) },
        { ItemColor.Green, new Color(11/255f,198/255f,29/255f)},

       // { ItemColor.Cyan,  new Color(255/255.0f, 252 / 255f, 8/255f) },

        { ItemColor.Orange,  new Color(255/255.0f, 145 / 255f, 16/255f) },
        { ItemColor.Violet,  new Color(214/255.0f, 102 / 255f, 255/255f) },

        { ItemColor.Yellow,  new Color(255/255.0f, 252 / 255f, 8/255f) },

        { ItemColor.White,  new Color(255/255.0f, 255 / 255f, 255/255f) },
         { ItemColor.Black,  new Color(0f, 0f, 0f) },
    };

    [MenuItem("Tools/模型颜色")]
    static void CountModelColor()
    {
        GameObject selectedObj = Selection.gameObjects[0];
        MeshRenderer[] renderers = selectedObj.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mesh in renderers)
        {
            Color color = mesh.material.color;
            ItemColor closestColor = Util.FindClosestColor(color);
            Debug.Log("----------------------");
            Debug.Log($"模型:{mesh.gameObject.name}---->{closestColor}");
        }
    }

    [MenuItem("Tools/统计模式数量", true)]
    static bool ValidateCountMeshRenderers()
    {
        return Selection.gameObjects.Length > 0;
    }


    [MenuItem("Tools/替换模型颜色")]
    static void ReplaceModeColor()
    {
        GameObject selectedObj = Selection.gameObjects[0];
        MeshRenderer[] renderers = selectedObj.GetComponentsInChildren<MeshRenderer>(true);
        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/GameResources/Materials/Wool.mat");
        if (material == null)
        {
            Debug.LogError("没找到材质---->");
            return;
        }
        foreach (var mesh in renderers)
        {
            Color color = mesh.material.color;
            ItemColor closestColor = Util.FindClosestColor(color);

            if (closestColor != ItemColor.None)
            {
                mesh.material = Instantiate(material);

                mesh.material.SetColor("_BaseColor", Util.ColorMapping[closestColor]);
            }
            Debug.Log("----------------------");
            Debug.Log($"模型:{mesh.gameObject.name}---->{closestColor}");
        }
    }

    [MenuItem("Tools/检查节点重名")]
    static void CheckNodeName()
    {
        Dictionary<string, int> nameCount = new Dictionary<string, int>();
        GameObject selectedObj = Selection.gameObjects[0];

        CheckTransform(selectedObj.transform, ref nameCount);

        var keys = nameCount.Where(kv => kv.Value > 1).Select(kv => kv.Key).ToList();
        if (keys != null && keys.Count > 0)
        {
            foreach (var name in keys)
            {
                Debug.LogError($"节点:{name}重名了");
            }
        }
        else
        {
            Debug.Log("没有重名");
        }
    }

    static void CheckTransform(Transform transform, ref Dictionary<string, int> nameCount)
    {
        string name = transform.name;
        if (name.StartsWith("zhuangshi")) return;
        if (nameCount.ContainsKey(name))
        {
            nameCount[name]++;
        }
        else
        {
            nameCount[name] = 1;
        }

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                CheckTransform(transform.GetChild(i), ref nameCount);
            }
        }
    }
}
