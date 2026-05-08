using UnityEngine;
using TMPro;
using System; // 必须引入 System 命名空间才能获取现实时间

public class SystemClock : MonoBehaviour
{
    [Header("时钟文本组件")]
    public TextMeshProUGUI clockText;

    void Update()
    {
        // 确保组件不为空，防止报错
        if (clockText != null)
        {
            // DateTime.Now 会自动获取你设备当前的真实系统时间
            // "HH:mm:ss" 表示 24小时制:分钟:秒（例如 14:30:05）
            clockText.text = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}