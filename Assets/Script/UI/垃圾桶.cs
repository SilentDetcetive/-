using UnityEngine;
using UnityEngine.EventSystems;

public class TrashCan : MonoBehaviour, IDropHandler
{
    // 当有东西丢在垃圾桶上时，Unity会自动调用这个方法
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // 获取拖过来的那个物体上的 DraggableItem 脚本
            DraggableItem droppedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedItem != null)
            {
                droppedItem.ClearItem(); // 清空它！
                Debug.Log("数据已粉碎，释放存储空间。");
            }
        }
    }
}