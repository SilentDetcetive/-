using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间

public class MathMarquee : MonoBehaviour
{
    [Header("UI 引用")]
    public RectTransform text1;
    public RectTransform text2;

    [Header("流动设置")]
    public float scrollSpeed = 25f;
    public float angle = 30f;
    public float spacing = 150f; // ★ 新增：两段文字之间的留白距离

    private float realWidth;

    void Start()
    {
        // 获取 TextMeshPro 组件
        TextMeshProUGUI tmp1 = text1.GetComponent<TextMeshProUGUI>();

        // 强行刷新网格，让 Unity 算出这串字到底有多长
        tmp1.ForceMeshUpdate();

        // 真实计算宽度 = 文字本身的长度 + 我们设定的留白距离
        realWidth = tmp1.preferredWidth + spacing;

        // 倾斜父物体
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // 用真实宽度来进行循环，完美避开重叠
        float offset = Mathf.Repeat(Time.unscaledTime * scrollSpeed, realWidth);

        text1.localPosition = new Vector3(-offset, 0, 0);
        text2.localPosition = new Vector3(realWidth - offset, 0, 0);
    }
}