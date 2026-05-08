using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (image.sprite == null) return; // 空槽位不让拖

        originalPosition = rectTransform.anchoredPosition; // 记住老家位置
        canvasGroup.blocksRaycasts = false; // 拖拽时让射线穿透自己，这样才能摸到下面的垃圾桶
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (image.sprite == null) return;
        // 让图标跟着鼠标走
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (image.sprite == null) return;

        canvasGroup.blocksRaycasts = true; // 恢复射线阻挡
        rectTransform.anchoredPosition = originalPosition; // 没碰到垃圾桶就自动弹回老家
    }

    // 清空此槽位的方法
    public void ClearItem()
    {
        image.sprite = null;
        image.color = new Color(1, 1, 1, 0); // 重新变回透明
        rectTransform.anchoredPosition = originalPosition;
    }
}