using UnityEngine;
using UnityEngine.EventSystems;

// 继承 IPointerClickHandler 接管底层鼠标点击
public class CharacterBannerNode : MonoBehaviour, IPointerClickHandler
{
    [Header("角色数据")]
    public string characterID = "C01";
    [TextArea(3, 5)]
    public string characterDesc = "默认单位，初始携带2个增速模块。";

    [Header("视觉表现")]
    public RectTransform rectTransform;
    public float enlargeScale = 1.15f; // 选中时放大的倍数

    private bool isSelected = false; // 当前是否处于“待确认”状态
    private Vector3 originalScale;

    void Start()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale; // 记录初始大小
    }

    // 鼠标点击事件监听
    public void OnPointerClick(PointerEventData eventData)
    {
        // === 鼠标左键逻辑 ===
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!isSelected)
            {
                // 第一次点击：设为选中状态，变大，呼出描述框
                isSelected = true;
                rectTransform.localScale = originalScale * enlargeScale;
                CharacterSelectManager.Instance.ShowDescription(this, characterDesc);
            }
            else
            {
                // 第二次点击：确认为该角色，执行替换和退出
                CharacterSelectManager.Instance.ConfirmCharacter(characterID);
            }
        }
        // === 鼠标右键逻辑 ===
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 如果它正在被选中，右键则取消选中并缩回去
            if (isSelected)
            {
                CharacterSelectManager.Instance.CloseDescription();
            }
        }
    }

    // 恢复初始状态（由 Manager 统一调用）
    public void ResetState()
    {
        isSelected = false;
        rectTransform.localScale = originalScale;
    }
}