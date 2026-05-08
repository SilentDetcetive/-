using UnityEngine;

public class SystemExitManager : MonoBehaviour
{
    // 这个公开方法将暴露给 UI 按钮使用
    public void QuitGame()
    {
        // 1. 发送控制台日志：让我在 Unity 编辑器里测试时也能看到效果
        Debug.Log("⚠️ 系统协议已终止，正在断开连接...");

        // 2. 核心退出指令：真正打包成 exe 游戏后，这行代码会关闭游戏窗口
        Application.Quit();
    }
}