using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("系统暂停界面 UI")]
    public GameObject pauseMenuUI;

    // 记录当前是否处于暂停状态
    private bool isPaused = false;

    void Start()
    {
        // 游戏启动时强制关闭暂停界面，防止意外阻挡视线
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        // 监听鼠标右键点击 (参数 1 代表右键)
        if (Input.GetMouseButtonDown(1))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);

        // 冻结时间：这会同时暂停你的关卡倒计时以及所有物理运动
        Time.timeScale = 0f;

        // 解锁并显示鼠标，以便玩家可以点击界面上的按钮（如果以后有的话）
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);

        // 恢复时间流逝
        Time.timeScale = 1f;

        // 隐藏并重新锁定鼠标回到屏幕中心（第一人称/第三人称视角游戏必备）
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}