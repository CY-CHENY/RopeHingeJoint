using UnityEngine;
using UnityEditor;

public class PrefabEditorWindow : EditorWindow
{
    // The width and height of the prefab preview image
    private int previewSize = 20;

    // The scroll position of the window
    private Vector2 scrollPosition;

    [MenuItem("Window/Prefab Editor")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(PrefabEditorWindow));
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }

    void OnGUI()
    {
        // Draw a label for the window title
        EditorGUILayout.LabelField("Prefab Editor", EditorStyles.boldLabel);

        // Draw a label for the recent prefabs
        EditorGUILayout.LabelField("Recently edited prefabs:");

        // Draw a scroll view for the recent prefabs
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

        // Loop through the recent prefabs and draw them in the window
        for (int i = PrefabRecorder.prefabPaths.Count-1; i >=0; i--)
        {
            DrawPrefab(PrefabRecorder.prefabPaths[i], i);
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawPrefab(string prefabPath, int index)
    {
        // Begin a horizontal layout
        EditorGUILayout.BeginHorizontal();


        var item = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        item = (GameObject)EditorGUILayout.ObjectField(item, typeof(GameObject), false, GUILayout.Width(200), GUILayout.Height(20));

        // End the horizontal layout
        EditorGUILayout.EndHorizontal();
    }
}
