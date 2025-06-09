using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Utils;
using System.Linq;
using System;
using System.IO.Ports;
using System.IO;
using Unity.VisualScripting;
using NPOI.SS.Formula.Functions;
using Codice.Client.BaseCommands.FastExport;

public class PrefabViewerWindow : EditorWindow
{
    // 存储当前拖拽的预制体
    private GameObject currentPrefab;
    private GameObject instantiatedPrefab;
    private Transform selectedTransform;

    // 视图分割相关
    private float splitPosition = 0.3f;
    private Rect leftPanelRect;
    private Rect rightPanelRect;
    private Rect splitterRect;
    private bool isResizing;

    // 渲染视图相关
    private Vector2 dragStart;
    private Quaternion cameraRotation = Quaternion.Euler(30, 30, 0);
    private float cameraDistance = 10f;
    private Vector3 cameraTarget = Vector3.zero;
    private RenderTexture previewRenderTexture;
    private Camera previewCamera;
    private GameObject previewCameraGO;
    private bool needsRender = true;

    // 缓存相关
    private Dictionary<Transform, bool> foldoutStates = new Dictionary<Transform, bool>();
    private GUIStyle headerStyle;
    private GUIStyle selectedStyle;
    private GUIStyle toggleStyle;
    private int lastRenderedFrame = -1;

    Dictionary<ItemColor, Texture> ColorTex = new Dictionary<ItemColor, Texture>();

    Material woolMat;

    //盒子颜色
    private List<ItemColor> boxColors = new List<ItemColor>();
    private int draggingIndex = -1;
    private int dropIndex = -1;
    private Vector2 boxScrollPosition = Vector2.zero;
    private Vector2 hierarchyScrollPos = Vector2.zero;
    private const float BOX_PANEL_HEIGHT = 200f; //盒子区域
    private const float HIERARCHY_HEIGHT = 300f;//节点区域
    private bool showHierarchy = true;       // Hierarchy展开状态
    private bool showBoxPanel = true;      // 颜色面板展开状态

    private bool colorsInitialized = false;

    //data
    HashSet<ItemColor> colors = new HashSet<ItemColor>(); //关卡需要的颜色
    Dictionary<Transform, ItemColor> modelColor = new Dictionary<Transform, ItemColor>();//当前模型颜色
    List<Transform> unknownColorTrans = new List<Transform>(); //未知颜色的模型，用来随机颜色

    Dictionary<ItemColor, Material> colorMaterial = new Dictionary<ItemColor, Material>();
    Material defaultMaterial;

    //模型层级
    List<List<Transform>> groupedTransforms = new List<List<Transform>>();
    private int renderGroup = 0;

    private string levelNumber = "1";

    [MenuItem("Tools/Prefab Viewer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabViewerWindow>("Prefab Viewer");
    }

    private void OnEnable()
    {
        // 初始化样式
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.alignment = TextAnchor.MiddleCenter;

        selectedStyle = new GUIStyle(EditorStyles.label);
        selectedStyle.normal.background = EditorGUIUtility.whiteTexture;
        selectedStyle.normal.textColor = Color.black;

        toggleStyle = new GUIStyle(EditorStyles.toggle);
        toggleStyle.fixedWidth = 15;

        // 注册撤销回调boxColors
        Undo.undoRedoPerformed += Repaint;

        // 注册更新回调
        EditorApplication.update += OnEditorUpdate;

        LoadResources();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
        EditorApplication.update -= OnEditorUpdate;
        CleanupInstantiatedPrefab();
        CleanupPreviewResources();
    }

    void LoadResources()
    {
        ColorTex.Clear();
        string prefix = "Assets/GameResources/Textures/";
        ColorTex.Add(ItemColor.Red, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "red.png"));
        ColorTex.Add(ItemColor.Blue, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "blue.png"));
        ColorTex.Add(ItemColor.Green, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "green.png"));
        ColorTex.Add(ItemColor.Orange, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "orange.png"));
        ColorTex.Add(ItemColor.Violet, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "purple.png"));
        ColorTex.Add(ItemColor.Yellow, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "yellow.png"));
        ColorTex.Add(ItemColor.White, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "white.png"));
        ColorTex.Add(ItemColor.Black, AssetDatabase.LoadAssetAtPath<Texture>(prefix + "black.png"));

        woolMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/GameResources/Materials/Wool.mat");


        // Material DefaultMaterial =  AssetDatabase.LoadAssetAtPath<Material>("Assets/GameResources/Materials/DefaultMaterial.mat");
        //默认白模
        defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/GameResources/Materials/DefaultMaterial.mat");

        for (int i = 0; i < 8; i++)
        {
            colorMaterial[(ItemColor)i] = AssetDatabase.LoadAssetAtPath<Material>($"Assets/GameResources/Materials/{(ItemColor)i}.mat");
            //colorMaterial[(ItemColor)i].color = Util.ColorMapping[(ItemColor)i];
        }
    }

    private void CleanupInstantiatedPrefab()
    {
        if (instantiatedPrefab != null)
        {
            DestroyImmediate(instantiatedPrefab);
            instantiatedPrefab = null;
        }
    }

    private void CleanupPreviewResources()
    {
        // 先解除相机对渲染纹理的引用
        if (previewCamera != null)
        {
            previewCamera.targetTexture = null;
        }

        // 然后销毁渲染纹理
        if (previewRenderTexture != null)
        {
            previewRenderTexture.Release();
            DestroyImmediate(previewRenderTexture);
            previewRenderTexture = null;
        }

        if (previewCameraGO != null)
        {
            DestroyImmediate(previewCameraGO);
            previewCameraGO = null;
            previewCamera = null;
        }
    }

    private void OnEditorUpdate()
    {
        // 仅在需要时重绘
        if (needsRender && (lastRenderedFrame != Time.frameCount || Event.current.type == EventType.Repaint))
        {
            Repaint();
            lastRenderedFrame = Time.frameCount;
            needsRender = false;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("关卡数:", GUILayout.Width(60));
        levelNumber = EditorGUILayout.TextField(levelNumber, GUILayout.Width(50));
        // 验证输入是否为数字
        if (!string.IsNullOrEmpty(levelNumber) && !int.TryParse(levelNumber, out _))
        {
            EditorGUILayout.HelpBox("请输入数字", MessageType.Error, true);
        }
        EditorGUILayout.EndHorizontal();
        // 绘制预制体拖放区域
        DrawPrefabDropArea();

        // 如果有预制体，则绘制分割视图
        if (currentPrefab != null)
        {
            CalculatePanelRects();
            DrawSplitView();
            HandleEvents();
        }
    }

    private void DrawPrefabDropArea()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Drag a Prefab here", EditorStyles.centeredGreyMiniLabel);

        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(dropArea, new Color(0.8f, 0.8f, 0.8f, 0.3f));

        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                {
                    EditorGUILayout.EndVertical();
                    return;
                }


                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    //检查是否有fbx文件
                    foreach (string path in DragAndDrop.paths)
                    {
                        if (path.EndsWith(".fbx") || path.EndsWith(".FBX"))
                        {
                            LoadFbxAndCreatePrefab(path);
                            break;
                        }
                    }

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject prefab = draggedObject as GameObject;
                        if (prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab))
                        {
                            SetCurrentPrefab(prefab);
                            break;
                        }
                    }

                    evt.Use();
                }
                break;
        }

        if (currentPrefab != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Prefab: " + currentPrefab.name, EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                currentPrefab = null;
                CleanupInstantiatedPrefab();
                CleanupPreviewResources();
            }

            //保存
            if (GUILayout.Button("保存", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                // 提示保存ScriptableObject配置
                if (EditorUtility.DisplayDialog(
                    "保存预制体和配置文件",
                    "是否保存预制体和关卡配置文件？",
                    "是",
                    "否"))
                {
                    SavePrefabChanges();
                    SaveLevelConfig();
                }

            }


            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    void LoadFbxAndCreatePrefab(string fbxPath)
    {
        GameObject fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxModel == null)
        {
            Debug.LogError($"无法加载FBX模型:{fbxPath}");
            return;
        }

        var prefab = Instantiate(fbxModel);
        prefab.hideFlags = HideFlags.HideAndDontSave;
        prefab.SetActive(false);
        SetCurrentPrefab(prefab);
    }

    void SavePrefabChanges()
    {
        if (instantiatedPrefab == null || currentPrefab == null)
            return;

        // 确保所有材质都是已保存的资产
        EnsureAllMaterialsAreSavedAssets(instantiatedPrefab);

        string path = $"Assets/GameResources/Prefabs/Level/{levelNumber}.prefab";
        instantiatedPrefab.SetActive(true);
        PrefabUtility.SaveAsPrefabAsset(instantiatedPrefab, path, out bool success);
        instantiatedPrefab.SetActive(false);
        if (success)
        {
            //刷新资源数据库
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("成功", "预制体修改已保存", "确定");
        }
        else
        {
            Debug.LogError("保存预制体失败");
        }

        // try
        // {
        //     string prefabPath = AssetDatabase.GetAssetPath(currentPrefab);
        //     if (string.IsNullOrEmpty(prefabPath))
        //     {
        //         Debug.LogError("无法获取预制体路径");
        //         return;
        //     }

        //     // 临时恢复正常的HideFlags以便保存
        //     HideFlags originalFlags = instantiatedPrefab.hideFlags;
        //     instantiatedPrefab.hideFlags = HideFlags.None;

        //     PrefabUtility.ApplyPrefabInstance(instantiatedPrefab, InteractionMode.UserAction);

        //     //恢复原来Hide Flags
        //     instantiatedPrefab.hideFlags = originalFlags;

        //     //刷新资源数据库
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();

        //     EditorUtility.DisplayDialog("成功", "预制体修改已保存", "确定");
        // }
        // catch (System.Exception e)
        // {
        //     Debug.LogError("保存预制体失败: " + e.Message);
        //     EditorUtility.DisplayDialog("错误", "保存预制体失败: " + e.Message, "确定");
        // }
    }
    //将模型材质转换成颜色
    private void EnsureAllMaterialsAreSavedAssets(GameObject root)
    {
        foreach (var model in modelColor)
        {
            Transform transform = model.Key;
            ItemColor color = model.Value;
            if (color != ItemColor.None)
            {
                var meshRenderer = transform.GetComponent<MeshRenderer>();

                // Material newMaterial = new Material(colorMaterial[color]);

                // string materialPath = $"Assets/GameResources/Materials/{color}.asset";
                // AssetDatabase.CreateAsset(newMaterial, materialPath);

                meshRenderer.material = colorMaterial[color];
                // meshRenderer.sharedMaterial.color = Util.ColorMapping[color];
                //meshRenderer.sharedMaterial = Instantiate(colorMaterial[color]);
                // meshRenderer.sharedMaterial.color = Util.ColorMapping[color];
            }
        }
    }

    void SaveLevelConfig()
    {
        if (!int.TryParse(levelNumber, out int level))
        {
            Debug.LogError("请输入有效的关卡数");
            return;
        }

        string configPath = $"Assets/GameResources/Datas/LevelConfig_{levelNumber}.asset";
        LevelConfigSO config = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(configPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<LevelConfigSO>();
            AssetDatabase.CreateAsset(config, configPath);
        }

        config.level = level;
        config.boxPool.Clear();
        foreach (var color in boxColors)
        {
            config.boxPool.Add((int)color);
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"关卡配置已保存到: {configPath}");
        EditorUtility.DisplayDialog("保存成功", $"关卡配置已保存到:\n{configPath}", "确定");
    }

    private void SetCurrentPrefab(GameObject prefab)
    {

        currentPrefab = prefab;
        CleanupInstantiatedPrefab();
        CleanupPreviewResources();

        // 在临时场景中实例化预制体
        // instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(currentPrefab);
        instantiatedPrefab = Instantiate(currentPrefab);
        instantiatedPrefab.hideFlags = HideFlags.HideInHierarchy;
        instantiatedPrefab.SetActive(false);

        // instantiatedPrefab.transform.SetParent(previewRoot.transform, false);

        // 重置视图
        cameraRotation = Quaternion.Euler(30, 30, 0);
        cameraDistance = 10f;
        cameraTarget = Vector3.zero;

        // 计算包围盒来调整视角
        Bounds bounds = CalculateBounds(instantiatedPrefab.transform);
        cameraTarget = bounds.center;
        cameraDistance = bounds.size.magnitude * 1.5f;

        // 重置折叠状态
        foldoutStates.Clear();
        selectedTransform = null;

        modelColor.Clear();
        boxColors.Clear();
        groupedTransforms.Clear();

        InitializeGroupTranform(instantiatedPrefab.transform, 0);

        InitializeMatFromModel(instantiatedPrefab.transform, 0);
        //初始化颜色数组
        InitializeColorsFromModel();

        RenderGroup(renderGroup);
        // 标记需要渲染
        needsRender = true;
    }

    void RenderGroup(int group)
    {
        for (int i = 0; i < groupedTransforms.Count; i++)
        {
            for (int j = 0; j < groupedTransforms[i].Count; j++)
            {
                groupedTransforms[i][j].gameObject.SetActive(i == group);
            }
        }
    }

    bool InitializeGroupTranform(Transform transform, int depth)
    {
        bool findNode = false;
        MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();
        if (meshRenderer != null && !transform.name.StartsWith("zhuangshi"))
        {
            findNode = true;
            if (depth >= groupedTransforms.Count)
                groupedTransforms.Add(new List<Transform>());
            groupedTransforms[depth].Add(transform);
        }

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                bool find = InitializeGroupTranform(transform.GetChild(i), depth);
                if (find && child.parent != instantiatedPrefab.transform) //找到并且不是根节点下的第一个节点
                {
                    depth++;
                }
            }
        }

        return findNode;
    }


    void InitializeMatFromModel(Transform transform, int depth)
    {
        if (transform == null)
            return;

        MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();
        if (meshRenderer != null && !transform.name.StartsWith("zhuangshi"))
        {
            var sharedMaterial = meshRenderer.sharedMaterial;
            if (sharedMaterial != null)
            {
                var color = meshRenderer.sharedMaterial.color;
                ItemColor itemColor = Util.FindClosestColor(color);
                Debug.Log($"模型:{transform.name}->颜色值:{color}->被识别颜色:{itemColor}");
                modelColor.Add(transform, itemColor);
                if (itemColor == ItemColor.None)
                    unknownColorTrans.Add(transform);
                else
                    colors.Add(itemColor);

                SetTransformColor(transform, itemColor);
            }

        }

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                InitializeMatFromModel(transform.GetChild(i), depth + 1);
            }
        }
    }

    private void InitializeColorsFromModel()
    {
        // Dictionary<ItemColor, int> itemColors = new Dictionary<ItemColor, int>();
        // foreach (var color in modelColor.Values)
        // {
        //     if (color == ItemColor.None) continue;
        //     if (itemColors.ContainsKey(color))
        //     {
        //         itemColors[color]++;
        //     }
        //     else
        //     {
        //         itemColors[color] = 1;
        //     }
        // }

        // List<ItemColor> boxs = new List<ItemColor>();

        // foreach (var color in itemColors.Keys)
        // {
        //     int count = itemColors[color];
        //     Debug.Log($"{color}总共有{count}个");
        //     int boxCount = count / 3;
        //     for (int i = 0; i < boxCount; i++)
        //     {
        //         boxs.Add(color);
        //     }
        // }

        // boxColors = boxs.ToArray();
        colorsInitialized = true;
    }

    private Bounds CalculateBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
    private void CalculatePanelRects()

    {
        // 计算面板区域，排除顶部的拖放区域
        Rect contentRect = new Rect(0, 60, position.width, position.height - 60);

        // 左侧面板 (30%)
        leftPanelRect = new Rect(
            contentRect.x,
            contentRect.y + 40,
            contentRect.width * splitPosition,
            contentRect.height
        );

        // 右侧面板 (70%)
        rightPanelRect = new Rect(
            contentRect.x + leftPanelRect.width + 2,
            contentRect.y + 40,
            contentRect.width * (1 - splitPosition) - 2,
            contentRect.height
        );

        // 分隔器
        splitterRect = new Rect(
            leftPanelRect.xMax,
            contentRect.y,
            4,
            contentRect.height
        );
    }

    private void DrawSplitView()
    {
        // 绘制分隔线
        EditorGUI.DrawRect(splitterRect, new Color(0.4f, 0.4f, 0.4f, 1f));

        // 绘制左侧层级面板
        GUILayout.BeginArea(leftPanelRect, EditorStyles.helpBox);
        DrawHierarchyPanel();
        GUILayout.EndArea();

        // 绘制右侧渲染面板
        GUILayout.BeginArea(rightPanelRect, EditorStyles.helpBox);
        DrawPreviewPanel();
        GUILayout.EndArea();
    }

    private void DrawHierarchyPanel()
    {
        EditorGUILayout.Space();
        showHierarchy = EditorGUILayout.Foldout(showHierarchy, "Hierarchy", true);
        if (showHierarchy)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox,
            GUILayout.Height(HIERARCHY_HEIGHT), GUILayout.ExpandWidth(true));
            hierarchyScrollPos = EditorGUILayout.BeginScrollView(hierarchyScrollPos);
            EditorGUILayout.LabelField("Hierarchy", headerStyle);

            if (instantiatedPrefab != null)
            {
                // 绘制预制体根节点
                DrawTransformNode(instantiatedPrefab.transform, 0);

                // 如果有选中的对象，显示Inspector
                if (selectedTransform != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Inspector", headerStyle);
                    DrawInspector(selectedTransform);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        //添加盒子排序
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("盒子顺序", headerStyle);

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("加载盒子", EditorStyles.miniButton))
        {
            LoadBox();
        }

        if (GUILayout.Button("添加盒子", EditorStyles.miniButton))
        {
            AddBox();
        }

        EditorGUILayout.EndHorizontal();
        DrawColorOrderPanel();
    }

    void LoadBox()
    {
        if (!int.TryParse(levelNumber, out int level))
        {
            Debug.LogError("请输入有效的关卡数");
            return;
        }

        string configPath = $"Assets/GameResources/Datas/LevelConfig_{levelNumber}.asset";
        LevelConfigSO config = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(configPath);
        if (config == null)
        {
            Debug.LogError("没有盒子配置文件");
            return;
        }

        boxColors.AddRange(config.boxPool);
    }

    void AddBox()
    {
        boxColors.Add(ItemColor.Red);
    }

    private void DrawColorOrderPanel()
    {
        if (!colorsInitialized || boxColors.Count == 0)
        {
            EditorGUILayout.HelpBox("Load a prefab to initialize colors from model", MessageType.Info);
            return;
        }

        Dictionary<ItemColor, int> colorCounts = new Dictionary<ItemColor, int>();

        foreach (var c in boxColors)
        {
            if (colorCounts.ContainsKey(c))
            {
                colorCounts[c]++;
            }
            else
            {
                colorCounts[c] = 1;
            }
        }

        var sortedColors = colorCounts.OrderByDescending(pair => pair.Value).ToList();
        string content = "";
        foreach (var colorCount in sortedColors)
        {
            content += $"{GetColorName(colorCount.Key)}: {colorCount.Value} ";
        }

        showBoxPanel = EditorGUILayout.Foldout(showBoxPanel, $"盒子顺序 {content}", true);
        if (showBoxPanel)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox,
            GUILayout.Height(BOX_PANEL_HEIGHT), GUILayout.ExpandWidth(true));

            //创建滚动视图
            Rect scrollViewStartRect = GUILayoutUtility.GetRect(0, 0);

            boxScrollPosition = EditorGUILayout.BeginScrollView(boxScrollPosition);

            //存储每个颜色的Rect
            Rect[] itemRects = new Rect[boxColors.Count];

            // 绘制颜色列表
            for (int i = 0; i < boxColors.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (i == draggingIndex)
                {
                    EditorGUILayout.LabelField("→", EditorStyles.boldLabel, GUILayout.Width(15));
                }
                else
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(15));
                }

                //绘制颜色预览
                Rect colorRect = GUILayoutUtility.GetRect(20, 20);
                EditorGUI.DrawRect(colorRect, Util.ColorMapping[boxColors[i]]);

                //绘制颜色信息
                string colorName = GetColorName(boxColors[i]);
                EditorGUILayout.LabelField(colorName, GUILayout.Width(120));

                //绘制索引
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));
                EditorGUILayout.EndHorizontal();

                //保存当前项的rect（相对于滚动视图内部）
                itemRects[i] = GUILayoutUtility.GetLastRect();

                //处理右键点击
                if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 1 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    ShowColorContextMenu(i);
                    Event.current.Use();
                }

                // 处理拖拽
                if (Event.current.type == EventType.MouseDown &&
                    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    draggingIndex = i;
                    Event.current.Use();
                }
            }
            EditorGUILayout.EndScrollView();

            Rect scrollViewEndRect = GUILayoutUtility.GetLastRect();
            // 计算完整的滚动视图Rect
            Rect scrollViewRect = new Rect(
                scrollViewStartRect.x,
                scrollViewStartRect.y,
                scrollViewEndRect.xMax - scrollViewStartRect.x,
                scrollViewEndRect.yMax - scrollViewStartRect.y
            );
            // GUI.EndScrollView();

            //拖拽处理（在滚动视图外，避免坐标问题）
            if (draggingIndex >= 0)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    dropIndex = -1;
                    Vector2 mousePosition = Event.current.mousePosition;

                    // 将全局鼠标坐标转换为滚动视图内部坐标
                    Vector2 localMousePosition = mousePosition - new Vector2(scrollViewRect.x, scrollViewRect.y - boxScrollPosition.y);

                    for (int i = 0; i < itemRects.Length; i++)
                    {
                        if (i == draggingIndex) continue;

                        if (itemRects[i].Contains(localMousePosition))
                        {
                            dropIndex = i;
                            break;
                        }
                    }
                }

                // 绘制拖拽指示器
                if (dropIndex >= 0 && dropIndex != draggingIndex)
                {
                    Rect indicatorRect = itemRects[dropIndex];
                    indicatorRect.y = scrollViewRect.y + indicatorRect.y - boxScrollPosition.y; // 转换回屏幕坐标
                    indicatorRect.height = 2;

                    EditorGUI.DrawRect(indicatorRect, Color.green);
                }
                // 拖拽完成处理
                if (Event.current.type == EventType.MouseUp)
                {
                    if (dropIndex >= 0 && dropIndex != draggingIndex)
                    {
                        // 交换颜色位置
                        ItemColor temp = boxColors[draggingIndex];
                        boxColors[draggingIndex] = boxColors[dropIndex];
                        boxColors[dropIndex] = temp;

                        // 保存排序结果
                        SaveColorOrder();

                        // 刷新界面
                        needsRender = true;
                    }

                    draggingIndex = -1;
                    dropIndex = -1;
                    Event.current.Use();
                }
            }

            EditorGUILayout.EndVertical();

        }
    }

    void ShowColorContextMenu(int index)
    {
        GenericMenu menu = new GenericMenu();
        foreach (ItemColor c in System.Enum.GetValues(typeof(ItemColor)))
        {
            menu.AddItem(new GUIContent(GetColorName(c)),
            false,
            () => SetColor(index, c));
        }
        menu.ShowAsContext();
    }

    void SetColor(int index, ItemColor color)
    {
        if (index >= 0 && index < boxColors.Count)
        {
            boxColors[index] = color;
            needsRender = true;
        }
    }

    private void SaveColorOrder()
    {
        // // 这里可以添加持久化保存逻辑
        // Debug.Log("Color order saved: " + string.Join(", ", customColors.Select(c => GetColorName(c))));

        // // 示例：使用EditorPrefs保存
        // string colorString = string.Join(";", customColors.Select(c =>
        //     $"{c.r},{c.g},{c.b},{c.a}"));
        // EditorPrefs.SetString("PrefabViewerColorOrder", colorString);

        // // 标记需要重新渲染
        // needsRender = true;
    }

    private void DrawTransformNode(Transform transform, int depth)
    {
        if (transform == null)
            return;

        // 确保有折叠状态
        if (!foldoutStates.ContainsKey(transform))
            foldoutStates[transform] = true;

        EditorGUILayout.BeginHorizontal();

        // 缩进
        GUILayout.Space(depth * 15);

        // 折叠/展开按钮 (只有有子节点时显示)
        if (transform.childCount > 0)
        {
            foldoutStates[transform] = EditorGUILayout.Foldout(foldoutStates[transform], "", true);
        }
        else
        {
            GUILayout.Space(15); // 占位
        }

        //显示颜色
        if (modelColor.TryGetValue(transform, out ItemColor transColor))
        {
            Rect colorRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(colorRect, Util.ColorMapping[transColor]);
        }

        // 显示/隐藏切换
        bool isActive = transform.gameObject.activeSelf;
        EditorGUI.BeginChangeCheck();
        isActive = EditorGUILayout.Toggle(isActive, toggleStyle, GUILayout.Width(20));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(transform.gameObject, "Toggle Active State");
            transform.gameObject.SetActive(isActive);
            needsRender = true;
        }

        // 名称标签 (选中时有特殊样式)
        bool isSelected = (selectedTransform == transform);
        Rect labelRect = GUILayoutUtility.GetRect(
       new GUIContent(transform.name),
       isSelected ? EditorStyles.boldLabel : EditorStyles.label,
       GUILayout.ExpandWidth(false));

        // 绘制Label
        EditorGUI.LabelField(labelRect, transform.name,
            isSelected ? EditorStyles.boldLabel : EditorStyles.label);

        // 处理左键点击
        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&  // 左键
            labelRect.Contains(Event.current.mousePosition))
        {
            selectedTransform = transform;
            Event.current.Use();
        }

        if (Event.current.type == EventType.MouseDown &&
                    Event.current.button == 1 &&
                     GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) &&
                               transform.GetComponent<MeshRenderer>() != null &&
                               !transform.name.Equals("zhuangshi"))
        {
            selectedTransform = transform;
            Debug.Log("ShowNodeColorContextMenu");
            ShowNodeColorContextMenu(transform);
            Event.current.Use();
        }

        // 删除按钮
        if (transform != instantiatedPrefab.transform && GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
        {
            if (EditorUtility.DisplayDialog("Delete GameObject",
                "Are you sure you want to delete " + transform.name + "?",
                "Yes", "No"))
            {
                Undo.RegisterCompleteObjectUndo(currentPrefab, "Delete GameObject");
                GameObject toDelete = transform.gameObject;
                Transform parent = transform.parent;
                int siblingIndex = transform.GetSiblingIndex();

                // 从场景中删除
                DestroyImmediate(toDelete);

                // 更新预制体
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(currentPrefab);
                GameObject prefabVariant = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefabRoot);
                if (prefabVariant != null)
                {
                    PrefabUtility.ApplyPrefabInstance(prefabRoot, InteractionMode.UserAction);
                }

                // 重新实例化预制体
                SetCurrentPrefab(currentPrefab);

                // 尝试恢复选择状态
                if (parent != null)
                {
                    if (parent.childCount > siblingIndex)
                    {
                        selectedTransform = parent.GetChild(siblingIndex);
                    }
                    else if (parent.childCount > 0)
                    {
                        selectedTransform = parent.GetChild(parent.childCount - 1);
                    }
                }

                needsRender = true;
            }
        }

        EditorGUILayout.EndHorizontal();

        // 绘制子节点
        if (foldoutStates[transform] && transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                DrawTransformNode(transform.GetChild(i), depth + 1);
            }
        }
    }

    void ShowNodeColorContextMenu(Transform transform)
    {
        GenericMenu menu = new GenericMenu();
        foreach (ItemColor c in System.Enum.GetValues(typeof(ItemColor)))
        {
            menu.AddItem(new GUIContent(GetColorName(c)),
            false,
            () =>
            {
                SetTransformColor(transform, c);
                modelColor[transform] = c;
            });
        }
        menu.ShowAsContext();
    }

    void SetTransformColor(Transform transform, ItemColor color)
    {
        if (color == ItemColor.None)
        {
            var meshRenderer = transform.GetComponent<MeshRenderer>();
            meshRenderer.material = defaultMaterial;
            return;
        }

        // if (color == ItemColor.None) return;
        var mr = transform.GetComponent<MeshRenderer>();

        mr.sharedMaterial = Instantiate(woolMat);
        mr.sharedMaterial.SetTexture("_BaseMap", ColorTex[color]);
        if (mr.sharedMaterial.HasProperty("_DissolveOffest"))
        {
            // 获取模型的局部包围盒
            Bounds bounds = mr.localBounds;
            //float centerY = bounds.center.y;
            // float top = (bounds.size.y - 1.0f) / 2.0f + 1.0f - centerY;
            float top = bounds.max.y + 0.5f;
            top = Mathf.Ceil(top * 100);
            top /= 100;
            mr.sharedMaterial.SetVector("_DissolveOffest", new Vector4(0, top, 0));
        }
    }

    private void DrawInspector(Transform target)
    {
        if (target == null)
            return;

        EditorGUI.BeginChangeCheck();

        // 名称
        string newName = EditorGUILayout.TextField("Name", target.name);
        if (newName != target.name)
        {
            Undo.RecordObject(target, "Rename GameObject");
            target.name = newName;
        }

        // 标签
        string newTag = EditorGUILayout.TagField("Tag", target.tag);
        if (newTag != target.tag)
        {
            Undo.RecordObject(target, "Change Tag");
            target.tag = newTag;
        }

        // 层
        int newLayer = EditorGUILayout.LayerField("Layer", target.gameObject.layer);
        if (newLayer != target.gameObject.layer)
        {
            Undo.RecordObject(target.gameObject, "Change Layer");
            target.gameObject.layer = newLayer;
            needsRender = true;
        }

        // Transform组件
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);

        Vector3 newPosition = EditorGUILayout.Vector3Field("Position", target.localPosition);
        Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", target.localEulerAngles);
        Vector3 newScale = EditorGUILayout.Vector3Field("Scale", target.localScale);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Modify Transform");
            target.localPosition = newPosition;
            target.localEulerAngles = newRotation;
            target.localScale = newScale;
            needsRender = true;
        }

        // 添加组件按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("Add Component"))
        {
            GenericMenu menu = new GenericMenu();
            AddComponentMenuItem(menu, "Transform", typeof(Transform));
            AddComponentMenuItem(menu, "Mesh Filter", typeof(MeshFilter));
            AddComponentMenuItem(menu, "Mesh Renderer", typeof(MeshRenderer));
            AddComponentMenuItem(menu, "Box Collider", typeof(BoxCollider));
            AddComponentMenuItem(menu, "Rigidbody", typeof(Rigidbody));
            menu.ShowAsContext();
        }
    }

    private void AddComponentMenuItem(GenericMenu menu, string name, System.Type type)
    {
        menu.AddItem(new GUIContent(name), false, () =>
        {
            if (selectedTransform != null && !selectedTransform.GetComponent(type))
            {
                Undo.AddComponent(selectedTransform.gameObject, type);
                needsRender = true;
            }
        });
    }

    private void DrawPreviewPanel()
    {
        EditorGUILayout.LabelField("Preview", headerStyle);

        if (instantiatedPrefab != null)
        {
            // 获取预览区域
            Rect previewRect = GUILayoutUtility.GetRect(rightPanelRect.x + 5,
            rightPanelRect.y + 25,
            rightPanelRect.width - 10,
            rightPanelRect.height - 30);

            // 绘制预览背景
            EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.1f, 1f));

            // 初始化渲染资源
            InitializePreviewResources(previewRect);

            // 仅在需要时渲染
            if (needsRender || previewRenderTexture == null ||
                previewRenderTexture.width != (int)previewRect.width ||
                previewRenderTexture.height != (int)previewRect.height)
            {
                RenderPreview(previewRect);
            }

            // 绘制渲染结果
            if (previewRenderTexture != null)
            {
                GUI.DrawTexture(previewRect, previewRenderTexture);
            }

            // 绘制颜色统计信息
            DrawColorStats(previewRect);

            //显示按钮
            DrawControlButton(previewRect);

            //显示层级
            DrawGroup(previewRect);

            // 绘制帮助信息
            Rect helpRect = new Rect(previewRect.x + 10, previewRect.yMax - 60, previewRect.width - 20, 50);
            EditorGUI.DrawRect(helpRect, new Color(0, 0, 0, 0.5f));
            EditorGUI.LabelField(helpRect, "Drag to rotate, Scroll to zoom", EditorStyles.centeredGreyMiniLabel);

            // //绘制色彩统计
            // Rect colorCountRect = new Rect(previewRect.x + 10, 10, 300, 300);
            // EditorGUI.DrawRect(colorCountRect, new Color(0, 0, 0, 0.5f));
            // HashSet<ItemColor> itemColors = new HashSet<ItemColor>();
            // foreach (var color in modelColor.Values)
            // {
            //     if (color != ItemColor.None)
            //         itemColors.Add(color);
            // }

            // EditorGUI.LabelField()
        }
    }

    private void DrawColorStats(Rect previewRect)
    {
        if (instantiatedPrefab == null || modelColor.Count == 0)
            return;

        // Debug.Log("DrawColorStats");
        // 计算每种颜色的使用次数

        Dictionary<ItemColor, int> colorCounts = new Dictionary<ItemColor, int>();

        foreach (var c in modelColor.Values)
        {
            if (colorCounts.ContainsKey(c))
            {
                colorCounts[c]++;
                //Debug.Log($"黄：++");
            }
            else
            {
                colorCounts[c] = 1;
                //Debug.Log($"黄：1");
            }
        }

        // 绘制统计信息背景
        float statsHeight = colorCounts.Count * 20 + 10;
        Rect statsRect = new Rect(previewRect.x + 10, previewRect.y + 10, 150, statsHeight);
        EditorGUI.DrawRect(statsRect, new Color(0, 0, 0, 0.7f));

        // 绘制每种颜色的统计信息
        float yOffset = 5;
        var sortedColors = colorCounts.OrderByDescending(pair => pair.Value).ToList();


        foreach (var pair in sortedColors)
        {
            // Rect colorRect = new Rect(statsRect.x + 5, statsRect.y + yOffset, 15, 15);
            // EditorGUI.DrawRect(colorRect, pair.Key);
            Rect labelRect = new Rect(statsRect.x + 25, statsRect.y + yOffset, statsRect.width - 30, 15);
            string colorName = GetColorName(pair.Key);
            EditorGUI.LabelField(labelRect, $"{colorName}: {pair.Value}", EditorStyles.whiteMiniLabel);

            yOffset += 20;
        }
    }

    private void DrawControlButton(Rect previewRect)
    {
        if (instantiatedPrefab == null || modelColor.Count == 0)
            return;

        Rect buttonRect = new Rect(previewRect.width - 100, previewRect.y + 10, 100, 50);
        // 绘制保存按钮
        // if (GUI.Button(new Rect(buttonRect.x, buttonRect.y, 100, 20), "随机"))
        // {
        //     RandomUnknownColor();
        // }

        buttonRect.y += 50;

        if (GUI.Button(new Rect(buttonRect.x, buttonRect.y, 100, 20), "加载配置"))
        {
            LoadModeColor();
        }

        buttonRect.y += 50;
        if (GUI.Button(new Rect(buttonRect.x, buttonRect.y, 100, 20), "保存配置"))
        {
            SaveModelColor();
        }
    }

    private void DrawGroup(Rect previewRect)
    {
        if (instantiatedPrefab == null || groupedTransforms.Count == 0)
            return;

        Rect groupRect = new Rect(previewRect.width - 100, previewRect.y + 200, 100, 50);
        GUI.Label(groupRect, "总层数: " + groupedTransforms.Count);
        groupRect.y += 50;
        if (GUI.Button(new Rect(groupRect.x, groupRect.y, 100, 20), "+"))
        {
            renderGroup++;
            if (renderGroup >= groupedTransforms.Count)
                renderGroup = groupedTransforms.Count - 1;
            else
            {
                RenderGroup(renderGroup);
            }

        }

        groupRect.y += 50;

        GUI.Label(groupRect, $"{renderGroup + 1}");
        groupRect.y += 50;
        if (GUI.Button(new Rect(groupRect.x, groupRect.y, 100, 20), "-"))
        {
            renderGroup--;
            if (renderGroup < 0)
                renderGroup = 0;
            else
                RenderGroup(renderGroup);
        }
    }

    void LoadModeColor()
    {
        string configPath = $"Assets/GameResources/Datas/ModelColor_{levelNumber}.asset";
        ModelColorSO config = AssetDatabase.LoadAssetAtPath<ModelColorSO>(configPath);
        if (config == null)
        {
            Debug.Log($"未找到模型颜色配置: {configPath}");
            EditorUtility.DisplayDialog("错误", $"未找到模型颜色配置:\n{configPath}", "确定");
            return;
        }

        //先把所有模型设置为白模
        for (int i = 0; i < modelColor.Keys.Count; i++)
        {
            var key = modelColor.Keys.ElementAt(i);
            modelColor[key] = ItemColor.None;
            SetTransformColor(key, ItemColor.None);
        }

        foreach (var nameColor in config.nameColor)
        {
            Transform transform = modelColor.Keys.FirstOrDefault(i => i.name.Equals(nameColor.Name));
            if (transform != null)
            {
                ItemColor color = (ItemColor)nameColor.Color;
                SetTransformColor(transform, color);
                modelColor[transform] = color;
            }
        }
    }

    void SaveModelColor()
    {
        string configPath = $"Assets/GameResources/Datas/ModelColor_{levelNumber}.asset";
        ModelColorSO config = AssetDatabase.LoadAssetAtPath<ModelColorSO>(configPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<ModelColorSO>();
            AssetDatabase.CreateAsset(config, configPath);
        }

        config.nameColor.Clear();

        foreach (var item in modelColor.Keys)
        {
            var color = modelColor[item];
            if (color != ItemColor.None)
            {
                var name = item.name;
                config.nameColor.Add(new NameColor() { Name = name, Color = (int)color });
            }
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"模型颜色配置已保存到: {configPath}");
        EditorUtility.DisplayDialog("保存成功", $"模型颜色配置已保存到:\n{configPath}", "确定");
    }

    //暂时不需要
    // void RandomUnknownColor()
    // {
    //     if (unknownColorTrans.Count <= 0)
    //         return;

    //     //目标颜色个数
    //     int targetColorCount = colors.Count;

    //     int boxCount = modelColor.Count / 3;
    //     if (boxCount * 3 != modelColor.Count)
    //     {
    //         Debug.LogError($"模型数量错误:{modelColor.Count}");
    //         return;
    //     }

    //     List<Transform> coloredModels = new List<Transform>();
    //     List<Transform> uncoloredModels = unknownColorTrans.ToList();
    //     foreach (var kv in modelColor)
    //     {
    //         if (kv.Value != ItemColor.None && !uncoloredModels.Contains(kv.Key))
    //             coloredModels.Add(kv.Key);
    //     }

    //     Debug.Log($"有颜色:{coloredModels.Count}");
    //     Debug.Log($"没有颜色:{uncoloredModels.Count}");

    //     Dictionary<ItemColor, int> currentCounts = new Dictionary<ItemColor, int>();
    //     foreach (var c in modelColor.Values)
    //     {
    //         if (c == ItemColor.None) continue;

    //         if (currentCounts.ContainsKey(c))
    //         {
    //             currentCounts[c]++;
    //         }
    //         else
    //         {
    //             currentCounts[c] = 1;
    //         }
    //     }

    //     Dictionary<ItemColor, int> required = new Dictionary<ItemColor, int>();
    //     foreach (var color in colors)
    //     {
    //         int count = currentCounts.ContainsKey(color) ? currentCounts[color] : 0;
    //         int remainder = count % 3;
    //         int needed = (3 - remainder) % 3;
    //         required[color] = needed;
    //     }

    //     int sumRequired = required.Values.Sum();
    //     int n = uncoloredModels.Count;
    //     if (sumRequired > n)
    //         throw new InvalidOperationException("模型不够");

    //     int delta = n - sumRequired;
    //     if (delta < 0 || delta % 3 != 0)
    //         throw new InvalidOperationException("不能分配颜色");

    //     int k = delta / 3;
    //     List<ItemColor> colorList = colors.ToList();

    //     int index = 0;

    //     while (k > 0)
    //     {
    //         if (colorList.Count == 0)
    //             throw new InvalidOperationException("没有可用颜色");

    //         ItemColor currentColor = colorList[index % colorList.Count];
    //         required[currentColor] += 3;
    //         k--;
    //         index++;
    //     }

    //     int uncoloredIndex = 0;
    //     foreach (var color in colors)
    //     {
    //         int needed = required[color];
    //         while (needed > 0 && uncoloredIndex < uncoloredModels.Count)
    //         {
    //             //Debug.Log($"{uncoloredModels[uncoloredIndex].name}设置颜色--->{color}");
    //             modelColor[uncoloredModels[uncoloredIndex]] = color;
    //             SetTransformColor(uncoloredModels[uncoloredIndex], color);

    //             if (!currentCounts.ContainsKey(color))
    //                 currentCounts[color] = 0;
    //             currentCounts[color]++;

    //             uncoloredIndex++;
    //             needed--;
    //         }
    //     }

    //     InitializeColorsFromModel();

    //     // foreach (var color in currentCounts.Keys)
    //     // {
    //     //     int count = currentCounts[color] / 3;
    //     //     Debug.Log($"颜色:{color} 总共:{currentCounts[color]}个 盒子:{count}个");
    //     //     for (int i = 0; i < count; i++)
    //     //     {
    //     //         model.BoxPool.Add(new BoxData() { Type = BoxType.Normal, Color = color, CurrentCount = 0 });
    //     //     }
    //     // }
    // }

    private string GetColorName(ItemColor color)
    {
        switch (color)
        {
            case ItemColor.Red:
                return "红色";
            case ItemColor.Blue:
                return "蓝色";
            case ItemColor.Green:
                return "绿色";
            case ItemColor.Orange:
                return "橙色";
            case ItemColor.Violet:
                return "紫色";
            case ItemColor.Yellow:
                return "黄色";
            case ItemColor.White:
                return "白色";
            case ItemColor.Black:
                return "黑色";
            default:
                return "未知";
        }
    }

    private void InitializePreviewResources(Rect rect)
    {
        // 确保在创建新渲染纹理之前先释放旧的
        if (previewRenderTexture != null &&
            (previewRenderTexture.width != (int)rect.width || previewRenderTexture.height != (int)rect.height))
        {
            // 先解除相机对渲染纹理的引用
            if (previewCamera != null)
            {
                previewCamera.targetTexture = null;
            }

            previewRenderTexture.Release();
            DestroyImmediate(previewRenderTexture);
            previewRenderTexture = null;
        }

        // 初始化渲染纹理
        if (previewRenderTexture == null)
        {
            previewRenderTexture = new RenderTexture((int)rect.width, (int)rect.height, 24);
            previewRenderTexture.name = "PrefabViewerRT";
            needsRender = true;
        }

        // 初始化相机
        if (previewCameraGO == null)
        {
            previewCameraGO = new GameObject("PreviewCamera");
            previewCameraGO.hideFlags = HideFlags.HideAndDontSave;
            previewCamera = previewCameraGO.AddComponent<Camera>();

            // 设置相机参数
            previewCamera.orthographic = false;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 1000f;
            previewCamera.fieldOfView = 60f;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            previewCamera.hideFlags = HideFlags.HideAndDontSave;
            previewCamera.enabled = false; //禁用自动渲染

            needsRender = true;
        }
    }

    private void RenderPreview(Rect rect)
    {
        if (previewCamera == null || previewRenderTexture == null || instantiatedPrefab == null)
            return;

        // 设置相机位置和旋转
        previewCamera.transform.rotation = cameraRotation;
        previewCamera.transform.position = cameraTarget - previewCamera.transform.forward * cameraDistance;
        previewCamera.transform.LookAt(cameraTarget);

        // 设置目标纹理
        previewCamera.targetTexture = previewRenderTexture;

        bool wasActive = instantiatedPrefab.activeSelf;
        instantiatedPrefab.SetActive(true);

        // 渲染场景
        RenderTexture tempRT = RenderTexture.active;
        RenderTexture.active = previewRenderTexture;
        GL.Clear(true, true, new Color(0.1f, 0.1f, 0.1f, 1f));
        previewCamera.Render();
        RenderTexture.active = tempRT;
        instantiatedPrefab.SetActive(wasActive);
        needsRender = false;
    }

    private void HandleEvents()
    {
        Event e = Event.current;

        // 处理分隔器拖动
        if (e.type == EventType.MouseDown && splitterRect.Contains(e.mousePosition))
        {
            isResizing = true;
            e.Use();
        }

        if (e.type == EventType.MouseDrag && isResizing)
        {
            splitPosition = Mathf.Clamp(e.mousePosition.x / position.width, 0.1f, 0.9f);
            e.Use();
            needsRender = true;
        }

        if (e.type == EventType.MouseUp)
        {
            isResizing = false;
        }

        // 处理预览区域的鼠标交互
        if (rightPanelRect.Contains(e.mousePosition) && instantiatedPrefab != null)
        {
            // 鼠标拖拽旋转
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                dragStart = e.mousePosition;
                e.Use();
            }

            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                Vector2 dragDelta = e.mousePosition - dragStart;
                dragStart = e.mousePosition;

                cameraRotation = Quaternion.Euler(
                    cameraRotation.eulerAngles.x - dragDelta.y * 0.5f,
                    cameraRotation.eulerAngles.y + dragDelta.x * 0.5f,
                    0
                );

                e.Use();
                needsRender = true;
            }

            // 滚轮缩放
            if (e.type == EventType.ScrollWheel)
            {
                cameraDistance = Mathf.Clamp(cameraDistance - e.delta.y * 0.5f, 0.5f, 100f);
                e.Use();
                needsRender = true;
            }
        }
    }
}