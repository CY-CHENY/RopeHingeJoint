#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class JointConfigureTool : EditorWindow
{
    private GameObject targetModel;
    private ModelJointConfig currentConfig;
    private Vector2 scrollPosition;
    private Transform selectedBone;
    private bool showAllBones = false;
    private string searchFilter = "";
    private Dictionary<string, bool> boneSelection = new Dictionary<string, bool>();
    private Dictionary<string, Color> originalColors = new Dictionary<string, Color>();
    
    // 模型预览相关
    private PreviewRenderUtility previewRenderUtility;
    private Vector2 previewScrollPosition;
    private float previewZoom = 1f;
    private Vector3 previewRotation = new Vector3(0, 0, 0);
    private Vector3 previewPosition = Vector3.zero;
    private bool isDraggingPreview = false;
    private Vector2 lastMousePosition;
    private bool showWireframe = false;
    private bool showBones = true;
    private bool autoRotate = false;
    private float autoRotateSpeed = 20f;
    
    // 布局相关
    private float previewPanelHeight = 300f;
    private float previewPanelWidth = 300f;
    private bool resizingPreviewPanel = false;

    [MenuItem("工具/关节配置工具")]
    public static void ShowWindow()
    {
        GetWindow<JointConfigureTool>("关节配置工具");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        
        // 初始化预览渲染
        if (previewRenderUtility == null)
        {
            previewRenderUtility = new PreviewRenderUtility();
            previewRenderUtility.cameraFieldOfView = 30f;
            previewRenderUtility.lights[0].intensity = 1.4f;
            previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            previewRenderUtility.lights[1].intensity = 1.4f;
        }
        
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= Update;
        
        RestoreOriginalColors();
        
        if (previewRenderUtility != null)
        {
            previewRenderUtility.Cleanup();
            previewRenderUtility = null;
        }
    }
    
    private void Update()
    {
        if (autoRotate && targetModel != null)
        {
            previewRotation.y += autoRotateSpeed * Time.deltaTime;
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        
        // 左侧面板 - 骨骼层级和配置
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - previewPanelWidth - 5));
        DrawLeftPanel();
        EditorGUILayout.EndVertical();
        
        // 分隔线和拖动调整大小
        Rect resizeHandleRect = new Rect(position.width - previewPanelWidth - 5, 0, 5, position.height);
        EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeHorizontal);
        
        if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition))
        {
            resizingPreviewPanel = true;
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            resizingPreviewPanel = false;
        }
        
        if (resizingPreviewPanel && Event.current.type == EventType.MouseDrag)
        {
            previewPanelWidth -= Event.current.delta.x;
            previewPanelWidth = Mathf.Clamp(previewPanelWidth, 200, position.width - 200);
            Repaint();
        }
        
        EditorGUI.DrawRect(resizeHandleRect, new Color(0.5f, 0.5f, 0.5f, 1));
        
        // 右侧面板 - 模型预览
        EditorGUILayout.BeginVertical(GUILayout.Width(previewPanelWidth));
        DrawRightPanel();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLeftPanel()
    {
        GUILayout.Label("3D模型关节配置工具", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GameObject newModel = (GameObject)EditorGUILayout.ObjectField("目标模型", targetModel, typeof(GameObject), true);
        
        if (newModel != targetModel)
        {
            RestoreOriginalColors();
            targetModel = newModel;
            if (targetModel != null)
            {
                ScanBones();
                currentConfig = CreateInstance<ModelJointConfig>();
                currentConfig.modelName = targetModel.name;
                
                // 重置预览相关参数
                previewZoom = 1f;
                previewRotation = Vector3.zero;
                previewPosition = Vector3.zero;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (targetModel == null)
        {
            EditorGUILayout.HelpBox("请先选择一个3D模型", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        // 配置文件操作
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("新建配置"))
        {
            currentConfig = CreateInstance<ModelJointConfig>();
            currentConfig.modelName = targetModel.name;
            currentConfig.joints.Clear();
        }

        if (GUILayout.Button("加载配置"))
        {
            string path = EditorUtility.OpenFilePanel("选择配置文件", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                currentConfig = AssetDatabase.LoadAssetAtPath<ModelJointConfig>(path);
                LoadConfigToBoneSelection();
            }
        }

        if (GUILayout.Button("保存配置"))
        {
            string path = EditorUtility.SaveFilePanel("保存配置文件", "Assets", targetModel.name + "JointConfig", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                AssetDatabase.CreateAsset(currentConfig, path);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("保存成功", "配置已保存到: " + path, "确定");
            }
        }

        if (GUILayout.Button("导出JSON"))
        {
            string path = EditorUtility.SaveFilePanel("导出JSON", "Assets", targetModel.name + "JointConfig", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = JsonUtility.ToJson(currentConfig, true);
                File.WriteAllText(path, json);
                EditorUtility.DisplayDialog("导出成功", "JSON已导出到: " + path, "确定");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 骨骼选择和过滤
        EditorGUILayout.BeginHorizontal();
        searchFilter = EditorGUILayout.TextField("搜索骨骼", searchFilter);
        if (GUILayout.Button("清除", GUILayout.Width(60)))
        {
            searchFilter = "";
        }
        EditorGUILayout.EndHorizontal();

        showAllBones = EditorGUILayout.Toggle("显示所有骨骼", showAllBones);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("选择需要添加关节的骨骼：", EditorStyles.boldLabel);

        // 骨骼列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (targetModel != null)
        {
            DisplayBoneHierarchy(targetModel.transform, 0);
        }
        
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 当前选择的骨骼配置
        if (selectedBone != null)
        {
            EditorGUILayout.LabelField("当前选择: " + selectedBone.name, EditorStyles.boldLabel);
            
            JointConfig jointConfig = GetOrCreateJointConfig(selectedBone.name);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.BeginChangeCheck();
            bool hasJoint = EditorGUILayout.Toggle("添加HingeJoint", jointConfig != null);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (hasJoint)
                {
                    if (jointConfig == null)
                    {
                        jointConfig = new JointConfig();
                        jointConfig.jointName = selectedBone.name;
                        jointConfig.anchor = Vector3.zero;
                        jointConfig.axis = Vector3.up;
                        jointConfig.minLimit = -45f;
                        jointConfig.maxLimit = 45f;
                        currentConfig.joints.Add(jointConfig);
                    }
                }
                else
                {
                    RemoveJointConfig(selectedBone.name);
                    jointConfig = null;
                }
                
                EditorUtility.SetDirty(currentConfig);
            }
            
            if (jointConfig != null)
            {
                jointConfig.anchor = EditorGUILayout.Vector3Field("锚点 (Anchor)", jointConfig.anchor);
                jointConfig.axis = EditorGUILayout.Vector3Field("轴向 (Axis)", jointConfig.axis);
                jointConfig.minLimit = EditorGUILayout.FloatField("最小角度", jointConfig.minLimit);
                jointConfig.maxLimit = EditorGUILayout.FloatField("最大角度", jointConfig.maxLimit);
                
                EditorGUILayout.Space();
                
                // 连接体设置
                jointConfig.connectedBodyType = (ConnectedBodyType)EditorGUILayout.EnumPopup("连接体类型", jointConfig.connectedBodyType);
                
                if (jointConfig.connectedBodyType == ConnectedBodyType.Custom)
                {
                    jointConfig.customConnectedBodyName = EditorGUILayout.TextField("连接体名称", jointConfig.customConnectedBodyName);
                }
                
                // 弹簧设置
                jointConfig.useSpring = EditorGUILayout.Toggle("使用弹簧", jointConfig.useSpring);
                
                if (jointConfig.useSpring)
                {
                    jointConfig.spring = EditorGUILayout.FloatField("弹力", jointConfig.spring);
                    jointConfig.damper = EditorGUILayout.FloatField("阻尼", jointConfig.damper);
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("预览"))
                {
                    PreviewJoint(selectedBone, jointConfig);
                }
                
                if (GUILayout.Button("清除预览"))
                {
                    ClearPreview(selectedBone);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        // 应用按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("应用到模型"))
        {
            ApplyConfigToModel();
        }
    }
    
    private void DrawRightPanel()
    {
        GUILayout.Label("模型预览", EditorStyles.boldLabel);
        
        if (targetModel == null)
        {
            EditorGUILayout.HelpBox("请先选择一个3D模型", MessageType.Info);
            return;
        }
        
        // 预览控制
        EditorGUILayout.BeginHorizontal();
        
        showWireframe = EditorGUILayout.Toggle("线框模式", showWireframe);
        showBones = EditorGUILayout.Toggle("显示骨骼", showBones);
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        autoRotate = EditorGUILayout.Toggle("自动旋转", autoRotate);
        if (autoRotate)
        {
            autoRotateSpeed = EditorGUILayout.Slider("旋转速度", autoRotateSpeed, 1f, 100f);
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("重置视图"))
        {
            previewZoom = 1f;
            previewRotation = Vector3.zero;
            previewPosition = Vector3.zero;
        }
        
        // 预览区域
        Rect previewRect = GUILayoutUtility.GetRect(previewPanelWidth - 10, previewPanelHeight);
        
        if (Event.current.type == EventType.Repaint)
        {
            DrawModelPreview(previewRect);
        }
        
        // 处理鼠标事件
        HandlePreviewInput(previewRect);
        
        // 显示当前选中骨骼的信息
        if (selectedBone != null)
        {
            EditorGUILayout.LabelField("选中骨骼: " + selectedBone.name);
            
            JointConfig jointConfig = GetOrCreateJointConfig(selectedBone.name);
            if (jointConfig != null)
            {
                EditorGUILayout.LabelField("已配置HingeJoint");
            }
            else
            {
                EditorGUILayout.LabelField("未配置HingeJoint");
            }
        }
        
        // 显示所有已配置关节的列表
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("已配置关节列表:", EditorStyles.boldLabel);
        
        previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition);
        
        if (currentConfig != null && currentConfig.joints != null && currentConfig.joints.Count > 0)
        {
            foreach (JointConfig config in currentConfig.joints)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(config.jointName);
                
                if (GUILayout.Button("选择", GUILayout.Width(60)))
                {
                    Transform bone = FindDeepChild(targetModel.transform, config.jointName);
                    if (bone != null)
                    {
                        selectedBone = bone;
                        Selection.activeGameObject = bone.gameObject;
                        SceneView.FrameLastActiveSceneView();
                        HighlightBone(bone);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("暂无配置的关节");
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawModelPreview(Rect rect)
    {
        if (targetModel == null || previewRenderUtility == null) return;
        
        previewRenderUtility.BeginPreview(rect, GUIStyle.none);
        
        // 设置相机
        previewRenderUtility.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        previewRenderUtility.camera.clearFlags = CameraClearFlags.Color;
        previewRenderUtility.camera.transform.position = new Vector3(0, 0, -6f / previewZoom);
        previewRenderUtility.camera.transform.rotation = Quaternion.identity;
        
        // 渲染模型
        GameObject previewInstance = (GameObject)Object.Instantiate(targetModel);
        previewInstance.transform.position = previewPosition;
        previewInstance.transform.rotation = Quaternion.Euler(previewRotation);
        
        // 获取模型的包围盒，以便自动调整缩放
        Bounds bounds = CalculateBounds(previewInstance);
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float scale = 1.0f / (maxSize > 0 ? maxSize : 1);
        previewInstance.transform.localScale = Vector3.one * scale;
        
        // 渲染选项
        if (showWireframe)
        {
            GL.wireframe = true;
        }
        
        // 渲染模型
        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
            {
                previewRenderUtility.DrawMesh(
                    renderer is SkinnedMeshRenderer ? 
                        ((SkinnedMeshRenderer)renderer).sharedMesh : 
                        renderer.GetComponent<MeshFilter>().sharedMesh,
                    renderer.transform.localToWorldMatrix,
                    renderer.sharedMaterial,
                    0);
            }
        }
        
        // 渲染骨骼
        if (showBones)
        {
            DrawBones(previewInstance.transform);
        }
        
        // 渲染已配置的关节
        if (currentConfig != null && currentConfig.joints != null)
        {
            foreach (JointConfig config in currentConfig.joints)
            {
                Transform bone = FindDeepChild(previewInstance.transform, config.jointName);
                if (bone != null)
                {
                    DrawJointGizmo(bone, config);
                }
            }
        }
        
        // 渲染相机
        previewRenderUtility.camera.Render();
        
        if (showWireframe)
        {
            GL.wireframe = false;
        }
        
        // 清理
        Object.DestroyImmediate(previewInstance);
        
        // 结束预览
        Texture resultRender = previewRenderUtility.EndPreview();
        GUI.DrawTexture(rect, resultRender);
        
        // 绘制边框
        GUI.Box(rect, GUIContent.none);
    }
    
    private void HandlePreviewInput(Rect previewRect)
    {
        Event e = Event.current;
        
        if (previewRect.Contains(e.mousePosition))
        {
            // 缩放
            if (e.type == EventType.ScrollWheel)
            {
                previewZoom = Mathf.Clamp(previewZoom - e.delta.y * 0.05f, 0.1f, 10f);
                e.Use();
                Repaint();
            }
            
            // 旋转
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isDraggingPreview = true;
                lastMousePosition = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDraggingPreview = false;
                e.Use();
            }
            
            if (isDraggingPreview && e.type == EventType.MouseDrag)
            {
                Vector2 delta = e.mousePosition - lastMousePosition;
                
                if (e.shift)
                {
                    // 平移
                    previewPosition.x += delta.x * 0.01f;
                    previewPosition.y -= delta.y * 0.01f;
                }
                else
                {
                    // 旋转
                    previewRotation.y += delta.x;
                    previewRotation.x += delta.y;
                }
                
                lastMousePosition = e.mousePosition;
                e.Use();
                Repaint();
            }
            
            // 点击选择骨骼
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                // 在预览窗口中实现点击选择骨骼的功能
                // 这需要射线检测，比较复杂，这里简化处理
                e.Use();
            }
        }
    }
    
    private void DrawBones(Transform bone)
    {
        if (bone == null) return;
        
        // 绘制到子骨骼的连线
        foreach (Transform child in bone)
        {
            // 计算世界坐标下的位置
            Vector3 startPos = bone.position;
            Vector3 endPos = child.position;
            
            Handles.color = Color.white;
            Handles.DrawLine(startPos, endPos);
            
            // 递归绘制子骨骼
            DrawBones(child);
        }
    }
    
    private void DrawJointGizmo(Transform bone, JointConfig config)
    {
        if (bone == null || config == null) return;
        
        // 计算世界坐标下的锚点位置
        Vector3 anchorPos = bone.TransformPoint(config.anchor);
        
        // 绘制锚点
        Handles.color = Color.yellow;
        Handles.SphereHandleCap(0, anchorPos, Quaternion.identity, 0.05f, EventType.Repaint);
        
        // 绘制轴向
        Vector3 axisDir = bone.TransformDirection(config.axis).normalized;
        Handles.color = Color.blue;
        Handles.DrawLine(anchorPos, anchorPos + axisDir * 0.2f);
        
        // 绘制限制范围
        Handles.color = Color.green;
        Vector3 perpVector = GetPerpendicularVector(axisDir);
        Quaternion axisRotation = Quaternion.LookRotation(axisDir);
        
        // 绘制最小角度限制
        Quaternion minRotation = axisRotation * Quaternion.Euler(0, 0, config.minLimit);
        Vector3 minDir = minRotation * Vector3.forward * 0.15f;
        Handles.DrawLine(anchorPos, anchorPos + minDir);
        
        // 绘制最大角度限制
        Quaternion maxRotation = axisRotation * Quaternion.Euler(0, 0, config.maxLimit);
        Vector3 maxDir = maxRotation * Vector3.forward * 0.15f;
        Handles.DrawLine(anchorPos, anchorPos + maxDir);
        
        // 绘制弧形表示角度范围
        Handles.color = new Color(0, 1, 0, 0.2f);
        Handles.DrawSolidArc(anchorPos, axisDir, minDir, config.maxLimit - config.minLimit, 0.15f);
    }
    
    private Bounds CalculateBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        
        return bounds;
    }

    private void DisplayBoneHierarchy(Transform bone, int depth)
    {
        if (bone == null) return;
        
        // 过滤骨骼
        if (!string.IsNullOrEmpty(searchFilter) && !bone.name.ToLower().Contains(searchFilter.ToLower()))
        {
            // 如果不显示所有骨骼，则跳过不匹配的
            if (!showAllBones)
            {
                // 递归检查子骨骼
                foreach (Transform child in bone)
                {
                    DisplayBoneHierarchy(child, depth + 1);
                }
                return;
            }
        }
        
        // 缩进
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(depth * 20);
        
        // 检查是否有关节配置
        bool hasJoint = HasJointConfig(bone.name);
        
        // 显示骨骼选择
        EditorGUI.BeginChangeCheck();
        bool isSelected = EditorGUILayout.ToggleLeft(bone.name, hasJoint);
        
        if (EditorGUI.EndChangeCheck())
        {
            if (isSelected)
            {
                // 添加关节配置
                JointConfig config = new JointConfig();
                config.jointName = bone.name;
                config.anchor = Vector3.zero;
                config.axis = Vector3.up;
                config.minLimit = -45f;
                config.maxLimit = 45f;
                currentConfig.joints.Add(config);
                
                // 选中该骨骼
                selectedBone = bone;
                
                // 高亮显示
                HighlightBone(bone);
            }
            else
            {
                // 移除关节配置
                RemoveJointConfig(bone.name);
                
                // 如果当前选中的是这个骨骼，则取消选中
                if (selectedBone == bone)
                {
                    selectedBone = null;
                }
                
                // 恢复颜色
                RestoreBoneColor(bone);
            }
            
            EditorUtility.SetDirty(currentConfig);
        }
        
        // 选择按钮
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            selectedBone = bone;
            Selection.activeGameObject = bone.gameObject;
            SceneView.FrameLastActiveSceneView();
            
            // 高亮显示
            HighlightBone(bone);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 递归显示子骨骼
        foreach (Transform child in bone)
        {
            DisplayBoneHierarchy(child, depth + 1);
        }
    }

    private void ScanBones()
    {
        boneSelection.Clear();
        originalColors.Clear();
        
        if (targetModel != null)
        {
            // 递归收集所有骨骼
            CollectBones(targetModel.transform);
        }
    }

    private void CollectBones(Transform bone)
    {
        if (bone == null) return;
        
        boneSelection[bone.name] = false;
        
        // 保存原始颜色
        Renderer renderer = bone.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            originalColors[bone.name] = renderer.sharedMaterial.color;
        }
        
        foreach (Transform child in bone)
        {
            CollectBones(child);
        }
    }

    private bool HasJointConfig(string boneName)
    {
        if (currentConfig == null || currentConfig.joints == null)
            return false;
        
        foreach (JointConfig config in currentConfig.joints)
        {
            if (config.jointName == boneName)
                return true;
        }
        
        return false;
    }

    private JointConfig GetOrCreateJointConfig(string boneName)
    {
        if (currentConfig == null || currentConfig.joints == null)
            return null;
        
        foreach (JointConfig config in currentConfig.joints)
        {
            if (config.jointName == boneName)
                return config;
        }
        
        return null;
    }

    private void RemoveJointConfig(string boneName)
    {
        if (currentConfig == null || currentConfig.joints == null)
            return;
        
        for (int i = 0; i < currentConfig.joints.Count; i++)
        {
            if (currentConfig.joints[i].jointName == boneName)
            {
                currentConfig.joints.RemoveAt(i);
                return;
            }
        }
    }

    private void LoadConfigToBoneSelection()
    {
        if (currentConfig == null || currentConfig.joints == null)
            return;
        
        // 重置所有选择
        foreach (string key in boneSelection.Keys.ToArray())
        {
            boneSelection[key] = false;
        }
        
        // 根据配置设置选择
        foreach (JointConfig config in currentConfig.joints)
        {
            boneSelection[config.jointName] = true;
            
            // 高亮显示
            Transform bone = FindDeepChild(targetModel.transform, config.jointName);
            if (bone != null)
            {
                HighlightBone(bone);
            }
        }
    }

    private void HighlightBone(Transform bone)
    {
        if (bone == null) return;
        
        Renderer renderer = bone.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            // 保存原始颜色
            if (!originalColors.ContainsKey(bone.name))
            {
                originalColors[bone.name] = renderer.sharedMaterial.color;
            }
            
            // 设置高亮颜色
            Material tempMaterial = new Material(renderer.sharedMaterial);
            tempMaterial.color = Color.green;
            renderer.material = tempMaterial;
        }
    }

        private void RestoreBoneColor(Transform bone)
    {
        if (bone == null) return;
        
        Renderer renderer = bone.GetComponent<Renderer>();
        if (renderer != null && originalColors.ContainsKey(bone.name))
        {
            Material tempMaterial = new Material(renderer.sharedMaterial);
            tempMaterial.color = originalColors[bone.name];
            renderer.material = tempMaterial;
        }
    }

    private void RestoreOriginalColors()
    {
        if (targetModel == null) return;
        
        foreach (KeyValuePair<string, Color> pair in originalColors)
        {
            Transform bone = FindDeepChild(targetModel.transform, pair.Key);
            if (bone != null)
            {
                Renderer renderer = bone.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material tempMaterial = new Material(renderer.sharedMaterial);
                    tempMaterial.color = pair.Value;
                    renderer.material = tempMaterial;
                }
            }
        }
    }

    private void PreviewJoint(Transform bone, JointConfig config)
    {
        if (bone == null || config == null) return;
        
        // 移除现有的关节组件
        ClearPreview(bone);
        
        // 添加新的关节组件
        HingeJoint joint = bone.gameObject.AddComponent<HingeJoint>();
        joint.anchor = config.anchor;
        joint.axis = config.axis;
        
        JointLimits limits = joint.limits;
        limits.min = config.minLimit;
        limits.max = config.maxLimit;
        joint.limits = limits;
        joint.useLimits = true;
        
        // 设置弹簧
        if (config.useSpring)
        {
            JointSpring spring = joint.spring;
            spring.spring = config.spring;
            spring.damper = config.damper;
            joint.spring = spring;
            joint.useSpring = true;
        }
        
        // 设置连接体
        switch (config.connectedBodyType)
        {
            case ConnectedBodyType.Parent:
                if (bone.parent != null)
                {
                    Rigidbody parentRb = bone.parent.GetComponent<Rigidbody>();
                    if (parentRb == null)
                    {
                        parentRb = bone.parent.gameObject.AddComponent<Rigidbody>();
                    }
                    joint.connectedBody = parentRb;
                }
                break;
            
            case ConnectedBodyType.None:
                joint.connectedBody = null;
                break;
            
            case ConnectedBodyType.Custom:
                if (!string.IsNullOrEmpty(config.customConnectedBodyName))
                {
                    Transform connectedTransform = FindDeepChild(targetModel.transform, config.customConnectedBodyName);
                    if (connectedTransform != null)
                    {
                        Rigidbody connectedRb = connectedTransform.GetComponent<Rigidbody>();
                        if (connectedRb == null)
                        {
                            connectedRb = connectedTransform.gameObject.AddComponent<Rigidbody>();
                        }
                        joint.connectedBody = connectedRb;
                    }
                }
                break;
        }
        
        // 确保有Rigidbody组件
        Rigidbody rb = bone.GetComponent<Rigidbody>();
        if (rb == null)
        {
            bone.gameObject.AddComponent<Rigidbody>();
        }
        
        // 标记为预览
        joint.gameObject.AddComponent<PreviewJointMarker>();
    }

    private void ClearPreview(Transform bone)
    {
        if (bone == null) return;
        
        // 移除所有预览关节组件
        HingeJoint joint = bone.GetComponent<HingeJoint>();
        if (joint != null)
        {
            DestroyImmediate(joint);
        }
        
        PreviewJointMarker marker = bone.GetComponent<PreviewJointMarker>();
        if (marker != null)
        {
            DestroyImmediate(marker);
        }
        
        // 移除预览时添加的Rigidbody
        Rigidbody rb = bone.GetComponent<Rigidbody>();
        if (rb != null && bone.GetComponent<PreviewJointMarker>() != null)
        {
            DestroyImmediate(rb);
        }
    }

    private void ApplyConfigToModel()
    {
        if (targetModel == null || currentConfig == null || currentConfig.joints == null)
            return;
        
        // 清除所有预览
        PreviewJointMarker[] markers = targetModel.GetComponentsInChildren<PreviewJointMarker>();
        foreach (PreviewJointMarker marker in markers)
        {
            DestroyImmediate(marker);
        }
        
        // 应用配置
        foreach (JointConfig config in currentConfig.joints)
        {
            Transform bone = FindDeepChild(targetModel.transform, config.jointName);
            if (bone != null)
            {
                // 移除现有的关节组件
                HingeJoint existingJoint = bone.GetComponent<HingeJoint>();
                if (existingJoint != null)
                {
                    DestroyImmediate(existingJoint);
                }
                
                // 添加新的关节组件
                HingeJoint joint = bone.gameObject.AddComponent<HingeJoint>();
                joint.anchor = config.anchor;
                joint.axis = config.axis;
                
                JointLimits limits = joint.limits;
                limits.min = config.minLimit;
                limits.max = config.maxLimit;
                joint.limits = limits;
                joint.useLimits = true;
                
                // 设置弹簧
                if (config.useSpring)
                {
                    JointSpring spring = joint.spring;
                    spring.spring = config.spring;
                    spring.damper = config.damper;
                    joint.spring = spring;
                    joint.useSpring = true;
                }
                
                // 设置连接体
                switch (config.connectedBodyType)
                {
                    case ConnectedBodyType.Parent:
                        if (bone.parent != null)
                        {
                            Rigidbody parentRb = bone.parent.GetComponent<Rigidbody>();
                            if (parentRb == null)
                            {
                                parentRb = bone.parent.gameObject.AddComponent<Rigidbody>();
                            }
                            joint.connectedBody = parentRb;
                        }
                        break;
                    
                    case ConnectedBodyType.None:
                        joint.connectedBody = null;
                        break;
                    
                    case ConnectedBodyType.Custom:
                        if (!string.IsNullOrEmpty(config.customConnectedBodyName))
                        {
                            Transform connectedTransform = FindDeepChild(targetModel.transform, config.customConnectedBodyName);
                            if (connectedTransform != null)
                            {
                                Rigidbody connectedRb = connectedTransform.GetComponent<Rigidbody>();
                                if (connectedRb == null)
                                {
                                    connectedRb = connectedTransform.gameObject.AddComponent<Rigidbody>();
                                }
                                joint.connectedBody = connectedRb;
                            }
                        }
                        break;
                }
                
                // 确保有Rigidbody组件
                Rigidbody rb = bone.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    bone.gameObject.AddComponent<Rigidbody>();
                }
            }
        }
        
        EditorUtility.DisplayDialog("应用成功", "已将配置应用到模型", "确定");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (targetModel == null || currentConfig == null || currentConfig.joints == null)
            return;
        
        // 绘制关节的可视化表示
        foreach (JointConfig config in currentConfig.joints)
        {
            Transform bone = FindDeepChild(targetModel.transform, config.jointName);
            if (bone != null)
            {
                // 绘制关节锚点
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, bone.TransformPoint(config.anchor), Quaternion.identity, 0.01f, EventType.Repaint);
                
                // 绘制关节轴向
                Handles.color = Color.blue;
                Handles.ArrowHandleCap(0, bone.TransformPoint(config.anchor), 
                    bone.rotation * Quaternion.LookRotation(config.axis), 0.05f, EventType.Repaint);
                
                // 绘制关节限制
                Handles.color = Color.green;
                Vector3 center = bone.TransformPoint(config.anchor);
                Vector3 normal = bone.TransformDirection(config.axis);
                Handles.DrawWireArc(center, normal, 
                    GetPerpendicularVector(normal), config.maxLimit, 0.05f);
                
                Handles.color = Color.red;
                Handles.DrawWireArc(center, normal, 
                    GetPerpendicularVector(normal), config.minLimit, 0.05f);
                
                // 如果是当前选中的骨骼，提供交互式编辑
                if (selectedBone == bone)
                {
                    EditorGUI.BeginChangeCheck();
                    
                    // 编辑锚点
                    Vector3 newAnchor = Handles.PositionHandle(bone.TransformPoint(config.anchor), bone.rotation);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(currentConfig, "Edit Joint Anchor");
                        config.anchor = bone.InverseTransformPoint(newAnchor);
                        EditorUtility.SetDirty(currentConfig);
                    }
                    
                    EditorGUI.BeginChangeCheck();
                    
                    // 编辑轴向
                    Quaternion axisRotation = Handles.RotationHandle(
                        bone.rotation * Quaternion.LookRotation(config.axis),
                        bone.TransformPoint(config.anchor));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(currentConfig, "Edit Joint Axis");
                        config.axis = bone.InverseTransformDirection(axisRotation * Vector3.forward);
                        EditorUtility.SetDirty(currentConfig);
                    }
                }
            }
        }
    }

    private Vector3 GetPerpendicularVector(Vector3 v)
    {
        if (v.x != 0 || v.y != 0)
            return new Vector3(-v.y, v.x, 0).normalized;
        else
            return new Vector3(0, 1, 0);
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
        
        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        
        return null;
    }
}

// 标记预览关节的组件
public class PreviewJointMarker : MonoBehaviour
{
    // 这只是一个标记组件，不需要任何功能
}
#endif