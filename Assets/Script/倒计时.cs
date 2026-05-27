using UnityEngine;
using TMPro;

public class LevelTimerManager : MonoBehaviour
{
    [Header("满星通关设置")]
    [Tooltip("在本关卡中，达到满星的限定时间（单位：秒）")]
    public float fullStarTimeLimit = 30f; // 让你可以在每关独立设置满星时间

    [Header("UI 数据接入")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isTimerRunning = false;

    void Start()
    {
        currentTime = 0f;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // 新增：停止计时器（终点调用）
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // 新增：判定是否达成满星（终点调用）
    public bool IsFullStar()
    {
        return currentTime <= fullStarTimeLimit;
    }
}