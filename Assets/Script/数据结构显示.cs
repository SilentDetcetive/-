using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间

public class DataStructureDisplay : MonoBehaviour
{
    [Header("UI 接入")]
    [Tooltip("把用来显示数量的 Text (TMP) 物体拖到这里")]
    public TextMeshProUGUI dataText;

    [Header("显示格式")]
    [Tooltip("显示在数字前面的文字，例如：'数据结构: '")]
    public string prefix = "数据结构：";

    // 每次显示这个 UI 时，先刷新一次
    private void Start()
    {
        UpdateDataUI();
    }

    // 使用 Update 可以确保如果数量发生变化，UI 会瞬间同步
    private void Update()
    {
        UpdateDataUI();
    }

    // 核心刷新逻辑
    private void UpdateDataUI()
    {
        if (dataText != null)
        {
            // 拼接文字，最终效果例如："数据结构：3"
            // \n 代表换行，如果你希望文字和数字上下排列，可以把 prefix 写成 "数据结构\n"
            dataText.text = prefix + GlobalData.dataStructureCount.ToString();
        }
    }
}