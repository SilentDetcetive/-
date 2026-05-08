using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance;

    [Header("UI 引用")]
    public GameObject victoryPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // ==========================================
            // 【新增功能 1】：解锁并显示鼠标指针
            // ==========================================
            Cursor.visible = true;                      // 让鼠标可见
            Cursor.lockState = CursorLockMode.None;     // 解除鼠标的锁定状态，允许它自由移动

            // ==========================================
            // 【新增功能 2】：停止游戏运行（时间冻结）
            // ==========================================
            Time.timeScale = 0f;                        // 将时间流逝速度设为 0，游戏画面和逻辑暂停
        }
    }

    public void ReplayLevel()
    {
        Debug.Log("重新开始本关");
        ResumeTime(); // 🚨 跳转场景前必须恢复时间！
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Debug.Log("前往下一关");
        ResumeTime(); // 🚨 跳转场景前必须恢复时间！

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("已经是最后一关了，返回选关界面！");
            LoadLevelSelect();
        }
    }

    public void LoadLevelSelect()
    {
        Debug.Log("返回选关界面");
        ResumeTime(); // 🚨 跳转场景前必须恢复时间！
        SceneManager.LoadScene("LevelSelect");
    }

    // ==========================================
    // 【防坑神器】：专门用于恢复时间的方法
    // ==========================================
    private void ResumeTime()
    {
        // 如果我们在通关时把时间设为了 0，跳到新场景时如果不改回来，
        // 你的下一关一开局就会是完全静止的！所以离开前必须恢复为 1。
        Time.timeScale = 1f;
    }
}