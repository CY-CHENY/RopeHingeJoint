using LitJson;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PrefabRecorder
{
    [SerializeField]
    // A list of paths of the prefabs
    public static List<string> prefabPaths;

    // The maximum number of recent prefabs to store
    public static int maxRecentPrefabs = 30;


    static PrefabRecorder()
    {
        prefabPaths = new List<string>();

        Load();

        //打开Prefab事件
        PrefabStage.prefabStageOpened += OnPrefabStageOpened;

        //Debug.Log("PrefabRecorder init finish");
    }

    private static void OnPrefabStageOpened(PrefabStage prefabStage)
    {
        //Debug.Log(prefabStage.assetPath);
        if (prefabPaths.Contains(prefabStage.assetPath))
        {
            var index = prefabPaths.IndexOf(prefabStage.assetPath);
            prefabPaths.RemoveAt(index);
        }
        prefabPaths.Add(prefabStage.assetPath);
        if (prefabPaths.Count > maxRecentPrefabs)
        {
            prefabPaths.RemoveAt(0);
        }

        Save();
    }

    static void Save()
    {
        EditorPrefs.SetString("PrefabRecordData", JsonMapper.ToJson(prefabPaths));
    }

    static void Load()
    {
        var data = EditorPrefs.GetString("PrefabRecordData");
        if (string.IsNullOrEmpty(data))
        {
            return;
        }
        prefabPaths = JsonMapper.ToObject<List<string>>(data);
    }
}