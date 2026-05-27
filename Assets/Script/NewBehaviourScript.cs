using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollableText : MonoBehaviour
{
    [Header("组件")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public TextMeshProUGUI textUI;

    [Header("设置")]
    [TextArea(10, 30)]
    public string longText;

    void Start()
    {
        SetupText();
    }

    void SetupText()
    {
        // 设置文本
        textUI.text = longText;

        // 强制刷新 TMP
        textUI.ForceMeshUpdate();

        // 获取文本真实高度
        float textHeight = textUI.preferredHeight;

        // 修改文本区域高度
        Vector2 textSize = textUI.rectTransform.sizeDelta;
        textSize.y = textHeight;
        textUI.rectTransform.sizeDelta = textSize;

        // 修改 Content 高度
        Vector2 contentSize = content.sizeDelta;
        contentSize.y = textHeight + 20f;
        content.sizeDelta = contentSize;

        // 回到顶部
        scrollRect.verticalNormalizedPosition = 1f;
    }

    // 动态修改文本
    public void SetText(string newText)
    {
        longText = newText;
        SetupText();
    }
}