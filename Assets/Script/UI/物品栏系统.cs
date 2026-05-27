using UnityEngine;
using UnityEngine.UI;

// 定义你游戏里所有主动模块的枚举类型
public enum ModuleType
{
    None,
    Speed,      // 增速模块
    Trojan,     // 木马模块
    XRay,       // 透视模块
    Intrusion,  // 侵入模块 (格式弹)
    Sky         // 超越模块
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // 单例模式，方便玩家脚本直接呼叫

    [Header("UI 槽位里的图标组件 (ItemIcon)")]
    public Image[] slotImages; // 把刚才建的5个透明的 ItemIcon 拖到这里

    [Header("各类道具对应的图标素材")]
    public Sprite speedSprite;
    public Sprite trojanSprite;
    public Sprite xRaySprite;
    public Sprite intrusionSprite;
    public Sprite skySprite;

    // 记录这5个槽位里目前装了什么
    private ModuleType[] slots = new ModuleType[5];

    // ====== 【端点新增】：在动态内存中记住当前人格被允许使用的总格子数 ======
    private int currentAllowedSlots = 5; // 默认给一个5格的过载兜底保护机制

    private void Awake()
    {
        Instance = this;

        // 游戏开始时，清空所有槽位
        for (int i = 0; i < slotImages.Length; i++)
        {
            slots[i] = ModuleType.None;
            slotImages[i].color = new Color(1, 1, 1, 0); // 设为透明
        }
    }

    // 拾取物品时调用。如果满了返回 false，没满放入并返回 true
    public bool TryAddItem(ModuleType type)
    {
        // ====== 【核心修改】：将循环上限由默认的 slots.Length (永远是5) 修改为当前关卡内生效的 currentAllowedSlots ======
        // 这样一来，如果是 3 格或 4 格容量的角色，物品栏装满后，雷达探测就不会再去动用隐藏格子，而是直接判定为背包装满！
        for (int i = 0; i < currentAllowedSlots; i++)
        {
            // 找到从左到右第一个空位
            if (slots[i] == ModuleType.None)
            {
                slots[i] = type;
                UpdateSlotUI(i);
                return true;
            }
        }
        // 找了一圈没空位，说明满了
        return false;
    }

    // 消耗物品时调用。从右往左查找，优先移除“后来的”那一个图标
    public void RemoveItem(ModuleType type)
    {
        // 将循环条件改为从 slots.Length - 1（即最后一格）开始，递减到 0
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            // 从右边开始数，一旦找到符合目前被消耗类型的道具
            if (slots[i] == type)
            {
                slots[i] = ModuleType.None; // 释放该槽位
                UpdateSlotUI(i);            // 刷新UI让图标变透明
                return;                     // 成功抹掉一个，直接跳出函数，防止把前面的全删了
            }
        }
    }

    // 更新特定格子的画面
    private void UpdateSlotUI(int index)
    {
        ModuleType type = slots[index];
        Image img = slotImages[index];

        if (type == ModuleType.None)
        {
            img.sprite = null;
            img.color = new Color(1, 1, 1, 0); // 变透明
            return;
        }

        img.color = new Color(1, 1, 1, 1); // 变不透明显示
        switch (type)
        {
            case ModuleType.Speed: img.sprite = speedSprite; break;
            case ModuleType.Trojan: img.sprite = trojanSprite; break;
            case ModuleType.XRay: img.sprite = xRaySprite; break;
            case ModuleType.Intrusion: img.sprite = intrusionSprite; break;
            case ModuleType.Sky: img.sprite = skySprite; break;
        }
    }

    // 动态定型物品栏的可用格子数
    public void SetupSlots(int allowedSlots)
    {
        // ====== 【核心修改】：跨场景加载完毕后，在此处同步封存当前角色限定的栏位上限 ======
        currentAllowedSlots = allowedSlots;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            // 获取格子外层的槽位物体 (Slot_1, Slot_2 等)
            GameObject slotObject = slotImages[i].transform.parent.gameObject;

            if (i >= allowedSlots)
            {
                // 超出的格子，直接隐藏，让玩家看得到总数变少了！
                slotObject.SetActive(false);
            }
            else
            {
                slotObject.SetActive(true);
            }
        }
    }
}