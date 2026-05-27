using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelDetailManager : MonoBehaviour
{
    public static LevelDetailManager Instance;

    [Header("UI 面板引用")]
    public GameObject detailUIContainer;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI briefingText;
    public Button confirmButton;

    [Header("加载设置")]
    public float loadDuration = 3.0f;          // 默认修改为 3.0f（持续三秒）
    public GameObject loadingScreenPanel;      // 【已弃用】：全屏加载界面的 Panel 引用

    private string scenePendingToLoad;
    [Header("【新增】按钮动画设置")]
    public RectTransform buttonRect;      // 拖入按钮的 RectTransform 组件
    public float slideUpDistance = 50f;   // 向上滑动的起始距离
    public float slideDuration = 0.3f;    // 动画时长

    private Vector2 buttonTargetPos;      // 记录按钮原始的完美位置
    private Coroutine buttonAnimCoroutine;

    // ==========================================
    // 内部记忆芯片，用来记录当前哪个关卡的 Panel 被打开了
    // ==========================================
    private GameObject currentActiveExtraPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (buttonRect != null)
        {
            buttonTargetPos = buttonRect.anchoredPosition;
        }
        HideDetails();

        // 防呆设计：确保游戏一开始时，加载界面黑幕是隐藏的
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
        }
    }

    // 接收参数变成 GameObject customPanel
    public void ShowDetails(string title, string briefing, string sceneName, GameObject customPanel)
    {
        titleText.text = title;
        briefingText.text = briefing;
        scenePendingToLoad = sceneName;

        detailUIContainer.SetActive(true);

        // ==========================================
        // 面板切换逻辑
        // ==========================================
        // 如果之前有别的 Panel 开着，先把它关掉，防止重叠
        if (currentActiveExtraPanel != null)
        {
            currentActiveExtraPanel.SetActive(false);
        }

        // 记录现在传进来的这个新 Panel，并激活它
        currentActiveExtraPanel = customPanel;
        if (currentActiveExtraPanel != null)
        {
            currentActiveExtraPanel.SetActive(true);
        }

        if (buttonRect != null)
        {
            if (buttonAnimCoroutine != null) StopCoroutine(buttonAnimCoroutine);
            buttonAnimCoroutine = StartCoroutine(SlideButtonIn());
        }

        // 绑定确认按钮
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClick);
    }

    public void HideDetails()
    {
        detailUIContainer.SetActive(false);

        // ==========================================
        // 同步隐藏关卡专属 Panel
        // ==========================================
        if (currentActiveExtraPanel != null)
        {
            currentActiveExtraPanel.SetActive(false);
            currentActiveExtraPanel = null; // 清除记忆
        }
    }

    private void OnConfirmClick()
    {
        Time.timeScale = 1f;
        StartCoroutine(DirectLoadRoutine());
    }

    private IEnumerator DirectLoadRoutine()
    {
        // ===================================================================
        // 【旧加载协议解密中断】：关闭本地遮罩拉起及强行停留等待逻辑
        //  交由全局 GlobalLoadingSystem 在新场景生成时全自动接管视觉表现。
        // ===================================================================
        /*
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("端点提示：未在 LevelDetailManager 上绑定 Loading Screen Panel！");
        }

        // 持续等待指定时间（此处受 loadDuration 变量控制，即 3 秒）
        yield return new WaitForSecondsRealtime(loadDuration);
        */
        // ===================================================================

        // 核心修正：不需要在本地傻等 3 秒了，直接发出切关信号！
        if (!string.IsNullOrEmpty(scenePendingToLoad))
        {
            SceneManager.LoadScene(scenePendingToLoad);
        }
        else
        {
            Debug.LogError("未检测到目标场景名称！");
        }

        yield return null;
    }

    // 请在 LevelDetailManager.cs 中添加以下 Update 方法
    void Update()
    {
        // 全局检测右键输入 (1 代表鼠标右键)
        if (Input.GetMouseButtonDown(1))
        {
            // 只有当面板处于激活状态时，右键才触发隐藏，防止重复调用
            if (detailUIContainer != null && detailUIContainer.activeSelf)
            {
                HideDetails();
                Debug.Log("[协议中止] 关卡详情面板已由全局右键信号强制关闭");
            }
        }
    }

    private IEnumerator SlideButtonIn()
    {
        float elapsedTime = 0f;
        // 起始点：在目标位置下方 slideUpDistance 处
        Vector2 startPos = buttonTargetPos - new Vector2(0, slideUpDistance);
        buttonRect.anchoredPosition = startPos;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;

            // 简单的平滑插值
            buttonRect.anchoredPosition = Vector2.Lerp(startPos, buttonTargetPos, t);
            yield return null;
        }
        buttonRect.anchoredPosition = buttonTargetPos;
    }
}