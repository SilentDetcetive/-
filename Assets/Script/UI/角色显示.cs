using UnityEngine;
using TMPro;

public class PlayerNameDisplay : MonoBehaviour
{
    public static PlayerNameDisplay Instance; // 单例模式，方便其他脚本随时呼叫更新

    [Header("UI 引用")]
    public TextMeshProUGUI nameText; // 刚刚创建的 Text_PlayerName

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 游戏刚开始时，给一个默认的显示文本
        UpdateName("未指派");
    }

    // 提供给外部调用的更新名称方法
    public void UpdateName(string newName)
    {
        if (nameText != null)
        {
            // 你可以在这里加上一些带有赛博感的前缀文字
            nameText.text = " [" + newName + "]";
        }
    }
}