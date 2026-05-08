using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("加载图片（UI面板）")]
    public GameObject loadingImagePanel; // 你的全屏加载背景图

    private void Awake()
    {
        Instance = this;
        // 游戏刚开始时，确保加载图是隐藏的
        if (loadingImagePanel != null)
        {
            loadingImagePanel.SetActive(false);
        }
    }

    // 核心方法：显示图片 -> 等待设定的秒数 -> 跳转场景
    public void ShowLoadingAndLoadScene(string sceneName, float waitTime)
    {
        StartCoroutine(LoadRoutine(sceneName, waitTime));
    }

    private IEnumerator LoadRoutine(string sceneName, float waitTime)
    {
        // 1. 弹出加载图片，盖住原本的UI
        if (loadingImagePanel != null)
        {
            loadingImagePanel.SetActive(true);
        }

        // 2. 关键修复：使用不受 Time.timeScale 影响的真实时间等待
        yield return new WaitForSecondsRealtime(waitTime);

        // 3. 时间到了，正式进入关卡
        // 加一道保险：强制解冻时间，确保进入新关卡时一切正常
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}