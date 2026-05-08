using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间，用于重载关卡

public class LevelTimerManager : MonoBehaviour
{
    [Header("系统倒计时设置")]
    [Tooltip("在 Inspector 中自定义本关卡的倒计时总时长（单位：秒）")]
    public float levelTimeInSeconds = 60f;

    [Header("UI 数据接入")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isTimerRunning = false;

    void Start()
    {
        // 初始化当前时间为你在面板里设置的时间，并启动计时器
        currentTime = levelTimeInSeconds;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            // Time.deltaTime 会根据现实时间流逝扣减（并且不受帧率影响）
            // 巧合的是：当你在之前做的“右键弹出文本”中将 Time.timeScale 设为 0 时，
            // 倒计时也会随之完美暂停！
            currentTime -= Time.deltaTime;

            // 倒计时结束检测
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isTimerRunning = false;
                ExecuteProtocol_ReloadLevel();
            }

            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 将纯秒数转换为 分:秒 的标准格式
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime % 60F);

            // 使用 string.Format 确保个位数时也能显示前面的 0，例如 01:05
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void ExecuteProtocol_ReloadLevel()
    {
        // 获取当前正在运行的场景索引，并重新加载它
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}