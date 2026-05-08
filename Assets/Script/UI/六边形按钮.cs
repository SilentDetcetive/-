using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class HexButton : Graphic
{
    [Range(0.1f, 500f)]
    public float radius = 50f; // 六边形半径
    [Range(0f, 360f)]
    public float rotationSpeed = 10f; // 每秒旋转角度

    private float currentAngle = 0f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float angleStep = 360f / 6; // 六边形
        Vector2 center = Vector2.zero;

        // 六边形顶点
        Vector2[] vertices = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (angleStep * i + currentAngle);
            vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        // 中心顶点
        vh.AddVert(center, color, Vector2.zero);

        // 添加边界三角形
        for (int i = 0; i < 6; i++)
        {
            vh.AddVert(vertices[i], color, Vector2.zero);
        }

        for (int i = 0; i < 6; i++)
        {
            vh.AddTriangle(0, i + 1, i + 1 == 6 ? 1 : i + 2);
        }
    }

    void Update()
    {
        if (rotationSpeed != 0f)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;
            SetVerticesDirty(); // 刷新 Mesh
        }
    }
}