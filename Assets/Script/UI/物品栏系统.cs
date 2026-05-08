using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI 引用")]
    public GameObject highlightMask; // 高亮暗色遮罩
    public GameObject trashCanPanel; // 垃圾桶面板
    public Image[] slotImages;       // 把那5个用来显示道具的 Image 拖到这里

    private bool isInventoryOpen = false;

    void Awake() { Instance = this; }

    void Start()
    {
        trashCanPanel.SetActive(false);
        highlightMask.SetActive(false);

        // 初始清空所有槽位
        foreach (var img in slotImages)
        {
            img.sprite = null;
            img.color = new Color(1, 1, 1, 0); // 让没物品的槽位图标完全透明
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleInventory();
        }
    }

    // 切换物品栏状态
    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        trashCanPanel.SetActive(isInventoryOpen);
        highlightMask.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // 冻结系统时间
            Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            Cursor.visible = true;                  // 显示鼠标
        }
        else
        {
            Time.timeScale = 1f; // 恢复系统时间
            Cursor.lockState = CursorLockMode.Locked; // 重新锁定鼠标回屏幕中央
            Cursor.visible = false;
        }
    }

    // 当捡到道具时调用此方法（传入道具的图片）
    public bool AddItem(Sprite itemSprite)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i].sprite == null) // 找到第一个空槽位
            {
                slotImages[i].sprite = itemSprite;
                slotImages[i].color = new Color(1, 1, 1, 1); // 恢复不透明
                return true;
            }
        }
        Debug.Log("存储空间不足，无法获取数据。");
        return false;
    }
}