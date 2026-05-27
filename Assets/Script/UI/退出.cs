using UnityEngine;

public class SystemExitManager : MonoBehaviour
{
    // 这个方法将绑定给你的退出按钮
    public void QuitProtocol()
    {
        Debug.Log("断开系统连接，退出游戏...");

        // 1. 核心指令：在打包后的正式游戏中关闭窗口
        Application.Quit();

        // 2. 防坑设计：让它在 Unity 编辑器里点击时也能自动退出运行模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}