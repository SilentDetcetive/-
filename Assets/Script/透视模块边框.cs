using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class XRayRevealTarget : MonoBehaviour
{
    private LineRenderer[] lines;
    private Material xrayMaterial;

    // 对外提供的开启接口
    public void Reveal(Color color, float width)
    {
        if (lines == null)
        {
            CreateLines(color, width);
        }

        foreach (var line in lines)
        {
            if (line != null) line.enabled = true;
        }
    }

    // 对外提供的关闭接口
    public void Hide()
    {
        if (lines == null) return;

        foreach (var line in lines)
        {
            if (line != null) line.enabled = false;
        }
    }

    private void CreateLines(Color color, float width)
    {
        // 【核心魔法】：使用 Unity 内置的 GUI Shader，它天生自带“穿透墙壁显示”的特性，绝不出错！
        xrayMaterial = new Material(Shader.Find("GUI/Text Shader"));
        xrayMaterial.color = color;

        lines = new LineRenderer[12];
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        // 获取碰撞盒的中心和大小，计算8个顶点（相对坐标）
        Vector3 center = col.center;
        Vector3 extents = col.size * 0.5f;
        Vector3[] localCorners = new Vector3[8]
        {
            center + new Vector3(-extents.x, -extents.y, -extents.z),
            center + new Vector3(extents.x, -extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y, extents.z),
            center + new Vector3(extents.x, -extents.y, extents.z),
            center + new Vector3(-extents.x, extents.y, -extents.z),
            center + new Vector3(extents.x, extents.y, -extents.z),
            center + new Vector3(-extents.x, extents.y, extents.z),
            center + new Vector3(extents.x, extents.y, extents.z)
        };

        // 12条边的顶点连线关系
        int[,] edges = { { 0, 1 }, { 1, 3 }, { 3, 2 }, { 2, 0 }, { 4, 5 }, { 5, 7 }, { 7, 6 }, { 6, 4 }, { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 } };

        // 动态生成12条线
        for (int i = 0; i < 12; i++)
        {
            GameObject lineObj = new GameObject("XRayEdge_" + i);
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;
            lineObj.transform.localRotation = Quaternion.identity;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = xrayMaterial;
            lr.useWorldSpace = false; // 让线条跟随敌人一起移动
            lr.positionCount = 2;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;

            lr.SetPosition(0, localCorners[edges[i, 0]]);
            lr.SetPosition(1, localCorners[edges[i, 1]]);

            // 关掉阴影提升性能
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.enabled = false;

            lines[i] = lr;
        }
    }

    private void OnDestroy()
    {
        // 养成好习惯，销毁物体时清理动态生成的材质，防止内存泄漏
        if (xrayMaterial != null)
        {
            Destroy(xrayMaterial);
        }
    }
}