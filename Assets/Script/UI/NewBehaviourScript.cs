using UnityEngine;
using UnityEngine.SceneManagement; // 如果你需要重置后立刻刷新界面，保留此行

public class ProgressResetManager : MonoBehaviour
{
    void Update()
    {
        // 监听 U 键输入
        if (Input.GetKeyDown(KeyCode.U))
        {
            ResetGameProgress();
        }
    }

    void ResetGameProgress()
    {
        // 1. 将解锁的最大关卡数强制设为 1
        PlayerPrefs.SetInt("MaxLevelReached", 1);

        // 2. 强制保存到硬盘
        PlayerPrefs.Save();

        // 3. 在控制台反馈，方便调试
        Debug.Log("系统提示：关卡进度已强制重置为第 1 关。");

        // 4. (可选) 如果你希望按下 U 后画面立刻刷新，取消下面这行的注释：
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}