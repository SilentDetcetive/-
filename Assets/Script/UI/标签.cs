using UnityEngine;
using TMPro;

public class ItemLabelHandler : MonoBehaviour
{
    [Header("设置")]
    public GameObject labelUI; // 把刚才创建的 InfoLabel 拖进去
    public float showDistance = 3.0f; // 距离多近时显示

    private Transform player;

    void Start()
    {
        // 自动寻找玩家，假设你的玩家标签是 "Player"
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 初始时隐藏
        if (labelUI != null) labelUI.SetActive(false);
    }

    void Update()
    {
        if (player == null || labelUI == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= showDistance)
        {
            labelUI.SetActive(true);

            // 核心修改：使用看板效果，强制对齐摄像机旋转
            // 这样文字就不会像被“翻转”一样出现镜像效果
            labelUI.transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            labelUI.SetActive(false);
        }
    }
}