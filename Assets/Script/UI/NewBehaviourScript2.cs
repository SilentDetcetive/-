using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalLoadingSystem : MonoBehaviour
{
    public static GlobalLoadingSystem Instance;

    [Header("UI 元素绑定（只需绑定一次）")]
    public GameObject loadingPanel;       // 全屏全黑的加载背景底板
    public RectTransform redProgressBar; // 从左向右延伸的红色能量线

    [Header("赛博读取时间")]
    [Tooltip("每换一次场景，红色能量线强行延伸读取多少秒")]
    public float loadDuration = 4.0f;

    private void Awake()
    {
        // =================== 【终极持久化锁】 ===================
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 让本系统和旗下的UI彻底独立于关卡之外，永不销毁！

            // 核心魔法：直接向 Unity 底层总线注册监听
            // 不管 Helen 用什么方式加载场景，只要换了场景，Unity 就会自动触发 OnSceneLoadedProtocol
            SceneManager.sceneLoaded += OnSceneLoadedProtocol;
        }
        else
        {
            Destroy(gameObject); // 防止重复生成
            return;
        }
        // =======================================================

        // 游戏刚启动时，先隐藏加载面板
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // 安全解绑，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoadedProtocol;
    }

    // ★ 关键：当任何场景加载成功时，Unity 会在第一时间自动呼叫这个方法
    private void OnSceneLoadedProtocol(Scene scene, LoadSceneMode mode)
    {
        // 因为本脚本挂在独立的、永远开启的 GameObject 上，所以 StartCoroutine 绝对绝对不会报 Inactive 错误！
        StartCoroutine(ExecuteRedLineAnimation());
    }

    private IEnumerator ExecuteRedLineAnimation()
    {
        // 1. 新场景刚出来的瞬间，立刻拉起全黑遮罩，把红线归零（防止玩家看到新场景穿模或穿帮）
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (redProgressBar != null) redProgressBar.localScale = new Vector3(0, 1, 1);

        // 2. 顺滑延伸红色能量线（使用 unscaledDeltaTime，免疫任何关卡内的暂停）
        float elapsedTime = 0f;
        while (elapsedTime < loadDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / loadDuration);

            if (redProgressBar != null)
            {
                redProgressBar.localScale = new Vector3(progress, 1, 1);
            }
            yield return null; // 逐帧刷新
        }

        // 确保红线填满
        if (redProgressBar != null) redProgressBar.localScale = new Vector3(1, 1, 1);

        // 3. 数据同步完毕！解冻可能存在的时间流速，安全撤除遮罩
        Time.timeScale = 1f;
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
}