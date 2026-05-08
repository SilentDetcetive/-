using UnityEngine;
using UnityEngine.UI;

// 强制要求挂载的物体上必须有 Image 或 Text 等图形组件
[RequireComponent(typeof(Graphic))]
public class UITrapezoidModifier : BaseMeshEffect
{
    [Header("梯形倾斜参数")]
    [Tooltip("正数向左缩进，负数向右延伸")]
    public float topSkewOffset = 50f; // 顶部右上角的 X 轴偏移量

    // 这个方法会在 UI 渲染网格时自动被 Unity 底层调用
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        UIVertex vertex = new UIVertex();

        // 遍历当前 UI 图形的所有顶点 (普通的 Simple Image 通常有 4 个顶点)
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // 顶点索引规律：0=左下, 1=左上, 2=右上, 3=右下
            // 我们通过修改索引 2 (右上角) 的 X 坐标，将其向左移动，形成直角梯形
            if (i == 2)
            {
                vertex.position.x -= topSkewOffset;
            }

            vh.SetUIVertex(vertex, i); // 将修改后的顶点写回
        }
    }

#if UNITY_EDITOR
    // 当你在编辑器里修改 topSkewOffset 参数时，实时刷新画面
    protected override void OnValidate()
    {
        base.OnValidate();
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }
#endif
}