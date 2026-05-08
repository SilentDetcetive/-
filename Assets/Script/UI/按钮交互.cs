using UnityEngine;
using UnityEngine.EventSystems; // 必须引入这个命名空间才能使用点击接口

// 继承 IPointerClickHandler 接口，接管鼠标点击事件
public class LevelNode : MonoBehaviour, IPointerClickHandler
{
    [Header("关卡专属数据")]
    public string levelTitle = "第一关：边缘接入";

    [TextArea(3, 5)] // 让文本框在 Inspector 里变大，方便你写长篇简介
    public string levelBriefing = "发现微弱的系统漏洞，请在此处建立初始数据锚点。";

    public string sceneToLoad = "Level_1"; // 对应的场景名称

    // 当鼠标点击这个物体时，Unity会自动调用这个方法
    public void OnPointerClick(PointerEventData eventData)
    {
        // 判定：如果是鼠标左键
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 呼叫管理器，把自己的数据传过去并显示
            LevelDetailManager.Instance.ShowDetails(levelTitle, levelBriefing, sceneToLoad);
        }
        // 判定：如果是鼠标右键
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 呼叫管理器，隐藏界面
            LevelDetailManager.Instance.HideDetails();
        }
    }
}