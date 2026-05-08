using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MapEditorWindow : EditorWindow
{
    // ==================== 原有设置 ====================
    private int gridSizeX = 40;
    private int gridSizeZ = 40;
    private bool showGrid = true;
    private bool showYellowDots = true;
    private const string PLACED_OBJECTS_LAYER = "PlacedObjects";

    private GameObject currentPrefab;
    private int stackCount = 1;

    // ==================== 放置高度层级 ====================
    private int placementLevel = 1;
    private const float LEVEL_HEIGHT = 1f;

    private enum ToolMode
    {
        SinglePlace,
        DragPlace,   // 现在这个模式自带拖拽路径预览和回退功能
        Delete
    }

    private ToolMode currentTool = ToolMode.DragPlace;

    private GameObject mapRoot;

    // ==================== 拖拽预览路径专用 ====================
    private List<Vector2Int> dragPathGrids = new List<Vector2Int>();
    private List<Vector3> dragPathPositions = new List<Vector3>();

    // SceneView 鼠标控制
    private bool isDraggingPlace;
    private int sceneMouseControlId;

    // 删除模式高亮目标
    private GameObject hoveredDeleteObject;

    // 预览虚影
    private GameObject previewObject;
    private GameObject previewSourcePrefab;
    private float currentRotationY = 0f;
    private Material previewMaterial;

    [MenuItem("Tools/端点：入侵协议 - 地图编辑器")]
    public static void OpenWindow()
    {
        GetWindow<MapEditorWindow>("地图编辑器");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        CreatePreviewMaterial();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        if (GUIUtility.hotControl == sceneMouseControlId)
            GUIUtility.hotControl = 0;

        isDraggingPlace = false;
        dragPathGrids.Clear();
        dragPathPositions.Clear();

        DestroyPreviewObject();
        DestroyPreviewMaterial();
    }

    private void OnGUI()
    {
        GUILayout.Label("=== 端点：入侵协议 地图编辑器 ===", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        gridSizeX = EditorGUILayout.IntSlider("网格 X 大小", gridSizeX, 10, 100);
        gridSizeZ = EditorGUILayout.IntSlider("网格 Z 大小", gridSizeZ, 10, 100);
        EditorGUILayout.EndHorizontal();

        showGrid = EditorGUILayout.Toggle("显示网格线", showGrid);
        showYellowDots = EditorGUILayout.Toggle("显示中心小黄点", showYellowDots);

        placementLevel = EditorGUILayout.IntSlider("放置层级 (高度)", placementLevel, 1, 15);
        stackCount = EditorGUILayout.IntSlider("垂直堆叠数量", stackCount, 1, 10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("单点放置", GUILayout.Height(30))) currentTool = ToolMode.SinglePlace;
        if (GUILayout.Button("拖拽路径放置", GUILayout.Height(30))) currentTool = ToolMode.DragPlace;
        if (GUILayout.Button("删除模式", GUILayout.Height(30))) currentTool = ToolMode.Delete;
        EditorGUILayout.EndHorizontal();

        GUILayout.Label(
            $"当前模式：{GetToolName(currentTool)} | 层级：第 {placementLevel} 层 | 当前旋转：Y = {currentRotationY}°",
            EditorStyles.helpBox
        );

        currentPrefab = (GameObject)EditorGUILayout.ObjectField("当前预制体", currentPrefab, typeof(GameObject), false);

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("清除整个地图", GUILayout.Height(30))) ClearMap();
        if (GUILayout.Button("保存当前地图", GUILayout.Height(30))) SaveMap();
        if (GUILayout.Button("加载地图", GUILayout.Height(30))) LoadMap();
        EditorGUILayout.EndHorizontal();

        GUILayout.Label(
            "使用方法：\n" +
            "• 拖拽路径放置：按下左键拖拽预览路径，不松手时往回拖可取消该格子，松开左键正式放置。\n" +
            "• 青色虚影表示可以放置，红色虚影表示当前位置已被占用\n" +
            "• 按 R 可旋转虚影和放置角度（每次 90°）\n" +
            "• 放置层级：选择要在第几层放置（1=最低层）\n" +
            "• 垂直堆叠：从选定层级开始向上堆叠\n" +
            "• 删除模式：鼠标指向方块会高亮，左键点击删除",
            EditorStyles.helpBox
        );

        if (!ShouldShowPlacementPreview())
        {
            DestroyPreviewObject();
        }
    }

    private string GetToolName(ToolMode mode)
    {
        return mode switch
        {
            ToolMode.SinglePlace => "单点放置",
            ToolMode.DragPlace => "拖拽路径放置",
            ToolMode.Delete => "删除",
            _ => "未知"
        };
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        CaptureSceneMouseControl();

        if (showGrid)
            DrawGrid();

        if (showYellowDots)
            DrawYellowDots();

        UpdateDeleteHoverObject();
        DrawDeleteHighlight();

        HandleRotationInput();
        UpdatePreviewObject();
        DrawPathPreview(); // 绘制拖拽路径的绿色虚影框

        HandleMouseInput();

        HandleUtility.Repaint();
    }

    private bool ShouldShowPlacementPreview()
    {
        return currentTool == ToolMode.SinglePlace || currentTool == ToolMode.DragPlace;
    }

    private void CaptureSceneMouseControl()
    {
        Event e = Event.current;
        if (e.alt) return;

        bool canInteract = currentTool == ToolMode.Delete || currentPrefab != null;
        if (!canInteract) return;

        sceneMouseControlId = GUIUtility.GetControlID(FocusType.Passive);

        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(sceneMouseControlId);
        }
    }

    private void HandleRotationInput()
    {
        Event e = Event.current;

        if (currentTool != ToolMode.SinglePlace && currentTool != ToolMode.DragPlace)
            return;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
        {
            currentRotationY += 90f;

            if (currentRotationY >= 360f)
                currentRotationY = 0f;

            if (previewObject != null)
            {
                previewObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
            }

            e.Use();
            Repaint();
            SceneView.RepaintAll();
        }
    }

    private void UpdatePreviewObject()
    {
        if (!ShouldShowPlacementPreview() || currentPrefab == null)
        {
            DestroyPreviewObject();
            return;
        }

        if (previewObject == null || previewSourcePrefab != currentPrefab)
        {
            RebuildPreviewObject();
        }

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 snappedXZ = SnapToGridCenter(hitPoint);

            float halfHeight = GetPrefabHalfHeight(currentPrefab);
            float baseY = (placementLevel - 1) * LEVEL_HEIGHT;
            Vector3 previewPos = new Vector3(snappedXZ.x, baseY + halfHeight, snappedXZ.z);

            previewObject.transform.position = previewPos;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
            previewObject.SetActive(true);

            bool occupied = IsCellOccupied(snappedXZ, previewPos.y);

            if (occupied)
                SetPreviewColor(new Color(1f, 0f, 0f, 0.35f));
            else
                SetPreviewColor(new Color(0f, 1f, 1f, 0.35f));
        }
        else
        {
            if (previewObject != null)
                previewObject.SetActive(false);
        }
    }

    private void RebuildPreviewObject()
    {
        DestroyPreviewObject();
        if (currentPrefab == null) return;

        previewObject = Instantiate(currentPrefab);
        previewObject.name = currentPrefab.name + "_Preview";
        previewObject.hideFlags = HideFlags.HideAndDontSave;

        previewSourcePrefab = currentPrefab;

        DisableBehavioursForPreview(previewObject);
        SetPreviewAppearance(previewObject);

        previewObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }

    private void DestroyPreviewObject()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }
        previewSourcePrefab = null;
    }

    private void CreatePreviewMaterial()
    {
        if (previewMaterial != null) return;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) return;

        previewMaterial = new Material(shader);
        previewMaterial.hideFlags = HideFlags.HideAndDontSave;

        if (previewMaterial.HasProperty("_Surface"))
            previewMaterial.SetFloat("_Surface", 1f);
        if (previewMaterial.HasProperty("_Blend"))
            previewMaterial.SetFloat("_Blend", 0f);

        previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        previewMaterial.SetInt("_ZWrite", 0);

        previewMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        previewMaterial.EnableKeyword("_ALPHABLEND_ON");

        previewMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        SetPreviewColor(new Color(0f, 1f, 1f, 0.35f));
    }

    private void DestroyPreviewMaterial()
    {
        if (previewMaterial != null)
        {
            DestroyImmediate(previewMaterial);
            previewMaterial = null;
        }
    }

    private void SetPreviewColor(Color color)
    {
        if (previewMaterial == null) CreatePreviewMaterial();
        if (previewMaterial == null) return;

        if (previewMaterial.HasProperty("_BaseColor"))
            previewMaterial.SetColor("_BaseColor", color);
        if (previewMaterial.HasProperty("_Color"))
            previewMaterial.SetColor("_Color", color);
    }

    private void DisableBehavioursForPreview(GameObject obj)
    {
        foreach (Collider col in obj.GetComponentsInChildren<Collider>(true))
            col.enabled = false;
        foreach (Rigidbody rb in obj.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        foreach (MonoBehaviour mb in obj.GetComponentsInChildren<MonoBehaviour>(true))
            mb.enabled = false;
    }

    private void SetPreviewAppearance(GameObject obj)
    {
        if (previewMaterial == null) CreatePreviewMaterial();

        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true))
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = previewMaterial;
            r.sharedMaterials = mats;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
    }

    private void HandleMouseInput()
    {
        Event e = Event.current;
        if (e.alt) return;

        if (currentTool == ToolMode.Delete)
        {
            HandleDeleteMouseInput(e);
            return;
        }

        if (currentPrefab == null) return;

        if (currentTool == ToolMode.SinglePlace)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                GUIUtility.hotControl = sceneMouseControlId;
                TryPlaceSingleAtMouse(e);
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == sceneMouseControlId)
            {
                GUIUtility.hotControl = 0;
                e.Use();
            }
            return;
        }

        // ==================== 拖拽预览模式逻辑 ====================
        if (currentTool == ToolMode.DragPlace)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(ray, out float distance)) return;

            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 snappedXZ = SnapToGridCenter(hitPoint);
            Vector2Int gridKey = new Vector2Int(Mathf.FloorToInt(snappedXZ.x), Mathf.FloorToInt(snappedXZ.z));

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isDraggingPlace = true;
                dragPathGrids.Clear();
                dragPathPositions.Clear();

                dragPathGrids.Add(gridKey);
                dragPathPositions.Add(snappedXZ);

                GUIUtility.hotControl = sceneMouseControlId;
                e.Use();
                return;
            }

            if (e.type == EventType.MouseDrag && isDraggingPlace && GUIUtility.hotControl == sceneMouseControlId)
            {
                if (dragPathGrids.Count > 0)
                {
                    Vector2Int lastGrid = dragPathGrids[dragPathGrids.Count - 1];
                    if (gridKey != lastGrid)
                    {
                        // 退回取消判定：如果拖动到了倒数第二个格子，说明在往回走
                        if (dragPathGrids.Count > 1 && gridKey == dragPathGrids[dragPathGrids.Count - 2])
                        {
                            dragPathGrids.RemoveAt(dragPathGrids.Count - 1);
                            dragPathPositions.RemoveAt(dragPathPositions.Count - 1);
                        }
                        // 否则如果是新格子，就添加
                        else if (!dragPathGrids.Contains(gridKey))
                        {
                            dragPathGrids.Add(gridKey);
                            dragPathPositions.Add(snappedXZ);
                        }
                    }
                }
                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                // 松开左键，一次性放置记录的所有路径格子！
                if (isDraggingPlace)
                {
                    foreach (Vector3 pos in dragPathPositions)
                    {
                        PlaceAtLevel(pos);
                    }
                }

                isDraggingPlace = false;
                dragPathGrids.Clear();
                dragPathPositions.Clear();

                if (GUIUtility.hotControl == sceneMouseControlId)
                    GUIUtility.hotControl = 0;

                e.Use();
            }
        }
    }

    private void TryPlaceSingleAtMouse(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (!plane.Raycast(ray, out float distance)) return;

        Vector3 hitPoint = ray.GetPoint(distance);
        Vector3 snappedXZ = SnapToGridCenter(hitPoint);
        PlaceAtLevel(snappedXZ);
    }

    private void HandleDeleteMouseInput(Event e)
    {
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            GUIUtility.hotControl = sceneMouseControlId;
            if (hoveredDeleteObject != null)
            {
                Undo.DestroyObjectImmediate(hoveredDeleteObject);
                hoveredDeleteObject = null;
            }
            e.Use();
        }

        if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == sceneMouseControlId)
        {
            GUIUtility.hotControl = 0;
            e.Use();
        }
    }

    private void DrawPathPreview()
    {
        // 只有拖拽模式，且正在拖拽时，才绘制路径虚影
        if (currentTool != ToolMode.DragPlace || !isDraggingPlace || dragPathPositions.Count == 0) return;

        float baseY = (placementLevel - 1) * LEVEL_HEIGHT;
        float halfHeight = currentPrefab != null ? GetPrefabHalfHeight(currentPrefab) : 0.5f;

        Handles.color = new Color(0f, 1f, 0f, 0.4f); // 绿色半透明框
        Vector3[] linePoints = new Vector3[dragPathPositions.Count];

        for (int i = 0; i < dragPathPositions.Count; i++)
        {
            Vector3 p = dragPathPositions[i];
            Vector3 center = new Vector3(p.x, baseY + halfHeight, p.z);
            Handles.DrawWireCube(center, Vector3.one * 0.95f);
            linePoints[i] = center;
        }

        // 用黄线把路径连起来，方便查看走向
        Handles.color = Color.yellow;
        if (dragPathPositions.Count > 1)
        {
            Handles.DrawPolyLine(linePoints);
        }
    }

    private void UpdateDeleteHoverObject()
    {
        hoveredDeleteObject = null;
        if (currentTool != ToolMode.Delete) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        int placedObjectsLayer = GetOrCreatePlacedObjectsLayer();
        int placedObjectsMask = 1 << placedObjectsLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, placedObjectsMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            Transform root = GetMapRoot().transform;
            Transform t = hitObject.transform;

            while (t != null && t.parent != root) t = t.parent;
            if (t != null && t.parent == root) hoveredDeleteObject = t.gameObject;
        }
    }

    private void DrawDeleteHighlight()
    {
        if (currentTool != ToolMode.Delete || hoveredDeleteObject == null) return;
        Bounds bounds = GetObjectBounds(hoveredDeleteObject);
        Handles.color = Color.red;
        Handles.DrawWireCube(bounds.center, bounds.size * 1.05f);
    }

    private Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renders = obj.GetComponentsInChildren<Renderer>();
        if (renders.Length > 0)
        {
            Bounds b = renders[0].bounds;
            for (int i = 1; i < renders.Length; i++) b.Encapsulate(renders[i].bounds);
            return b;
        }

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) b.Encapsulate(colliders[i].bounds);
            return b;
        }

        return new Bounds(obj.transform.position, Vector3.one);
    }

    private void PlaceAtLevel(Vector3 snappedXZ)
    {
        float baseY = (placementLevel - 1) * LEVEL_HEIGHT;
        float currentTopY = baseY;

        for (int i = 0; i < stackCount; i++)
        {
            float halfHeight = GetPrefabHalfHeight(currentPrefab);
            Vector3 placePos = new Vector3(snappedXZ.x, currentTopY + halfHeight, snappedXZ.z);

            if (IsCellOccupied(snappedXZ, placePos.y)) break;

            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(currentPrefab);
            if (newObj != null)
            {
                Undo.RegisterCreatedObjectUndo(newObj, "Map Editor Place Object");
                newObj.transform.position = placePos;
                newObj.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
                newObj.transform.parent = GetMapRoot().transform;
                SetLayerRecursively(newObj, GetOrCreatePlacedObjectsLayer());
            }

            currentTopY += halfHeight * 2f;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null || layer < 0 || layer > 31) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, layer);
    }

    private int GetOrCreatePlacedObjectsLayer()
    {
        int existingLayer = LayerMask.NameToLayer(PLACED_OBJECTS_LAYER);
        if (existingLayer >= 0) return existingLayer;

        Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (tagManagerAssets == null || tagManagerAssets.Length == 0) return 0;

        SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = PLACED_OBJECTS_LAYER;
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                return i;
            }
        }
        return 0;
    }

    private float GetPrefabHalfHeight(GameObject prefab)
    {
        if (prefab == null) return 0.5f;
        BoxCollider box = prefab.GetComponentInChildren<BoxCollider>();
        if (box != null) return box.size.y * prefab.transform.localScale.y * 0.5f;

        Renderer[] renders = prefab.GetComponentsInChildren<Renderer>();
        if (renders.Length > 0)
        {
            Bounds b = renders[0].bounds;
            for (int i = 1; i < renders.Length; i++) b.Encapsulate(renders[i].bounds);
            return b.size.y * 0.5f;
        }
        return 0.5f;
    }

    private Vector3 SnapToGridCenter(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int z = Mathf.FloorToInt(pos.z);
        return new Vector3(x + 0.5f, 0f, z + 0.5f);
    }

    private GameObject GetMapRoot()
    {
        if (mapRoot == null) mapRoot = GameObject.Find("MapRoot") ?? new GameObject("MapRoot");
        return mapRoot;
    }

    private bool IsCellOccupied(Vector3 snappedXZ, float checkY)
    {
        GameObject root = GetMapRoot();
        foreach (Transform child in root.transform)
        {
            Vector3 pos = child.position;
            bool sameX = Mathf.Abs(pos.x - snappedXZ.x) < 0.01f;
            bool sameZ = Mathf.Abs(pos.z - snappedXZ.z) < 0.01f;
            bool sameY = Mathf.Abs(pos.y - checkY) < 0.01f;
            if (sameX && sameZ && sameY) return true;
        }
        return false;
    }

    private void ClearMap()
    {
        if (EditorUtility.DisplayDialog("警告", "确定删除整个地图？", "是", "取消"))
        {
            if (mapRoot != null) DestroyImmediate(mapRoot);
            mapRoot = null;
            hoveredDeleteObject = null;
        }
    }

    private void SaveMap()
    {
        if (mapRoot == null) return;
        MapData data = ScriptableObject.CreateInstance<MapData>();

        foreach (Transform child in mapRoot.transform)
        {
            data.placedItems.Add(new MapData.PlacedItem
            {
                prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject),
                position = child.position,
                rotation = child.rotation.eulerAngles
            });
        }

        string path = EditorUtility.SaveFilePanelInProject("保存地图", "MyMap", "asset", "保存");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("成功", "地图已保存！", "好");
        }
    }

    private void LoadMap()
    {
        string path = EditorUtility.OpenFilePanel("加载地图", "Assets/MapAssets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = "Assets" + path.Substring(Application.dataPath.Length);
        MapData data = AssetDatabase.LoadAssetAtPath<MapData>(path);
        if (data == null) return;

        ClearMap();
        GetMapRoot();

        foreach (var item in data.placedItems)
        {
            if (item.prefab != null)
            {
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(item.prefab);
                obj.transform.position = item.position;
                obj.transform.rotation = Quaternion.Euler(item.rotation);
                obj.transform.parent = mapRoot.transform;
                SetLayerRecursively(obj, GetOrCreatePlacedObjectsLayer());
            }
        }
        EditorUtility.DisplayDialog("成功", "地图加载完成！", "好");
    }

    private void DrawGrid()
    {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        float y = 0f;
        for (int x = 0; x <= gridSizeX; x++) Handles.DrawLine(new Vector3(x, y, 0), new Vector3(x, y, gridSizeZ));
        for (int z = 0; z <= gridSizeZ; z++) Handles.DrawLine(new Vector3(0, y, z), new Vector3(gridSizeX, y, z));
    }

    private void DrawYellowDots()
    {
        Handles.color = Color.yellow;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Handles.SphereHandleCap(0, new Vector3(x + 0.5f, 0.02f, z + 0.5f), Quaternion.identity, 0.08f, EventType.Repaint);
            }
        }
    }
}