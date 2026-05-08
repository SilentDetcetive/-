using UnityEngine;
using TMPro;

public class RightClickInfoManager : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoText;

    // 记录面板当前是打开还是关闭的
    private bool isPanelOpen = false;

    void Start()
    {
        // 游戏开始时确保面板是隐藏的
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    void Update()
    {
        // 监听鼠标右键点击 (参数 1 代表右键)
        if (Input.GetMouseButtonDown(1))
        {
            if (isPanelOpen)
            {
                CloseInfoPanel();
            }
            else
            {
                OpenInfoPanel();
            }
        }
    }

    public void OpenInfoPanel()
    {
        isPanelOpen = true;
        infoPanel.SetActive(true);

        // 冻结游戏时间并解锁鼠标
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseInfoPanel()
    {
        isPanelOpen = false;
        infoPanel.SetActive(false);

        // 恢复游戏时间
        Time.timeScale = 1f;

        // 注意：如果你平时的游戏状态需要隐藏鼠标，请在这里把两行代码取消注释
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}