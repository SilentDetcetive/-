using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class Endpoint : MonoBehaviour
{
    // ==========================================
    // 关卡参数
    // ==========================================
    [Header("关卡进度管理")]
    [Tooltip("当前关卡是第几关？（比如这是第一关就填 1）")]
    public int currentLevelIndex = 1;

    [Header("Endpoint Settings")]
    public bool isActive = true;
    public GameObject winEffectPrefab;

    [Header("Visual")]
    public bool usePulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.15f;

    // ==========================================
    // 内部组件
    // ==========================================
    private BoxCollider triggerCollider;
    private Vector3 baseScale;
    private bool reached = false;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        // 触发范围稍微大一点，更稳
        triggerCollider.size = new Vector3(1.3f, 1.3f, 1.3f);

        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (!usePulse || reached)
            return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTriggerWin(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryTriggerWin(other);
    }

    private void TryTriggerWin(Collider other)
    {
        if (!isActive || reached)
            return;

        GridPlayerMover player = other.GetComponent<GridPlayerMover>();

        if (player == null)
        {
            player = other.GetComponentInParent<GridPlayerMover>();
        }

        if (player == null)
            return;

        reached = true;
        isActive = false;

        ReachEndpoint(player);
    }

    private void ReachEndpoint(GridPlayerMover player)
    {
        Debug.Log("Level Complete: 系统节点已攻破");

        // 1. 禁用玩家操作 (保留原有代码)
        player.enabled = false;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. 播放胜利特效 (保留原有代码)
        if (winEffectPrefab != null)
        {
            Instantiate(winEffectPrefab, transform.position, Quaternion.identity);
        }

        // ==========================================
        // 【新增】：存档协议 - 解锁下一关
        // ==========================================
        // 去系统里查一下目前最高解锁到了第几关（如果没查到，默认是第 1 关）
        int currentUnlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // 如果当前打通的这关，等于或大于已解锁的最高进度，就让最高进度 +1
        if (currentLevelIndex >= currentUnlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevelIndex + 1);
            PlayerPrefs.Save(); // 强制写入硬盘
            Debug.Log("系统权限已更新！已解锁第 " + (currentLevelIndex + 1) + " 关。");
        }

        // ==========================================
        // 【新增协议】：时间评级与数据结构结算
        // ==========================================
        // ==========================================
        // 【新代码：精准替换区】
        // ==========================================
        bool isFullStar = false; // 默认状态为普通通关
        LevelTimerManager timer = FindObjectOfType<LevelTimerManager>();

        if (timer != null)
        {
            timer.StopTimer(); // 停止正计时
            isFullStar = timer.IsFullStar(); // 获取时间评级判定
        }

        // 1. 先去本地缓存里读取：玩家之前一共攒了多少个“数据原型”
        int currentPrototypes = PlayerPrefs.GetInt("DataPrototype", 0);

        // 2. 根据是否满星，将新获得的奖励累加进去
        if (isFullStar)
        {
            currentPrototypes += 2;
            Debug.Log("满星通关！获得 2 个数据原型。当前总计：" + currentPrototypes);
        }
        else
        {
            currentPrototypes += 1;
            Debug.Log("普通通关！获得 1 个数据原型。当前总计：" + currentPrototypes);
        }

        // 3. 关键核心：将最新的总数保存进硬盘！这样回到选人界面才能有分扣除
        PlayerPrefs.SetInt("DataPrototype", currentPrototypes);
        PlayerPrefs.Save(); // 强制存盘写入
        // ==========================================
        // 【漏掉的致命核心】：必须呼叫 VictoryUI 弹出胜利面板
        // ==========================================
        if (VictoryUI.Instance != null)
        {
            VictoryUI.Instance.ShowVictoryPanel(isFullStar);
        }
        else
        {
            Debug.LogError("系统警告：未找到 VictoryUI 实例！");
        }
    }
}