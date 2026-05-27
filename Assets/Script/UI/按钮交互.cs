using UnityEngine;
using UnityEngine.EventSystems;

public class LevelNode : MonoBehaviour, IPointerClickHandler
{
    [Header("关卡专属数据")]
    public string levelTitle = "第一关：边缘接入";

    [TextArea(3, 5)]
    public string levelBriefing = "发现微弱的系统漏洞，请在此处建立初始数据锚点。";

    public string sceneToLoad = "Level_1";

    [Header("关卡专属 Panel")]
    public GameObject myLevelPanel;

    // ==========================================
    // 【新增】：当前节点的权限编号
    // ==========================================
    [Header("解锁条件")]
    [Tooltip("这个节点代表第几关？系统会根据存档自动判断是否显示它")]
    public int thisLevelIndex = 1;

    private void Start()
    {
        // 防呆设计：隐藏专属 Panel
        if (myLevelPanel != null)
        {
            myLevelPanel.SetActive(false);
        }

        // ==========================================
        // 【新增】：权限校验协议
        // ==========================================
        // 读取系统存档，看玩家最高拥有第几关的权限（默认给 1）
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // 如果这个节点要求的关卡数，大于玩家拥有的权限，直接隐藏自身！
        if (thisLevelIndex > unlockedLevel)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LevelDetailManager.Instance.ShowDetails(levelTitle, levelBriefing, sceneToLoad, myLevelPanel);
        }
        
    }
}