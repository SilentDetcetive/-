using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 必须引用
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
    public float loadDuration = 2.0f; // 模拟加载时间

    private string scenePendingToLoad; // 存储当前待加载的场景名

    void Awake()
    {
        if (Instance == null) Instance = this;
        HideDetails();
    }

    // 1. 显示详情时，把场景名传进来存着
    public void ShowDetails(string title, string briefing, string sceneName)
    {
        titleText.text = title;
        briefingText.text = briefing;
        scenePendingToLoad = sceneName; // 记录目标场景

        detailUIContainer.SetActive(true);

        // 绑定确认按钮
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClick);
    }

    public void HideDetails()
    {
        detailUIContainer.SetActive(false);
    }

    // 2. 点击确认按钮后的逻辑
    private void OnConfirmClick()
    {
        // 🚨 核心排雷：不管之前有没有暂停，跳转前必须解冻时间
        Time.timeScale = 1f;

        // 如果你仍然想用“加载图”，可以保留协程逻辑，但直接写在下面
        StartCoroutine(DirectLoadRoutine());
    }

    private IEnumerator DirectLoadRoutine()
    {
        // 如果你有加载界面面板，可以在这里显示它
        // loadingImagePanel.SetActive(true);

        // 等待设定的时长
        yield return new WaitForSecondsRealtime(loadDuration);

        // 3. 直接跳转场景，不再调用关卡管理器的方法
        if (!string.IsNullOrEmpty(scenePendingToLoad))
        {
            SceneManager.LoadScene(scenePendingToLoad);
        }
        else
        {
            Debug.LogError("未检测到目标场景名称！");
        }
    }
}