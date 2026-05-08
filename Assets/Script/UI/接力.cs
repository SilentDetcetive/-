using UnityEngine;

public class SeamlessNewsScroller : MonoBehaviour
{
    [Header("滚动设置")]
    public float scrollSpeed = 150f; // 滚动速度
    [Tooltip("必须和你在 UI 面板里设置的文字 Width 保持完全一致！")]
    public float textWidth = 2000f;

    [Header("文字接力引用")]
    public RectTransform text1;
    public RectTransform text2;

    void Start()
    {
        // 游戏启动时，自动把第二段文字无缝拼接到第一段文字的尾部
        Vector2 pos1 = text1.anchoredPosition;
        text2.anchoredPosition = new Vector2(pos1.x + textWidth, pos1.y);
    }

    void Update()
    {
        // 两段文字同时向左移动
        text1.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
        text2.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

        // 核心接力逻辑：
        // 如果 text1 完全移出了左侧边界，把它瞬间移动到 text2 的尾部
        if (text1.anchoredPosition.x <= -textWidth)
        {
            text1.anchoredPosition = new Vector2(text2.anchoredPosition.x + textWidth, text1.anchoredPosition.y);
        }

        // 如果 text2 完全移出了左侧边界，把它瞬间移动到 text1 的尾部
        if (text2.anchoredPosition.x <= -textWidth)
        {
            text2.anchoredPosition = new Vector2(text1.anchoredPosition.x + textWidth, text2.anchoredPosition.y);
        }
    }
}