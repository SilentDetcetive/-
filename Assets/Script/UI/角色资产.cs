using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "入侵协议/创建角色配置")]
public class CharacterConfig : ScriptableObject
{
    [Header("身份识别")]
    public string characterID;       // 角色编号 (如 C01, C02)
    public string characterName;     // 角色名字

    [Header("核心属性调整")]
    [Tooltip("基础为0.2f，数值越小，走一格越快")]
    public float moveDuration = 0.2f;
    public int maxStability = 5;     // 初始最大稳定值 (生命值)

    [Header("存储容量")]
    [Range(1, 9)]
    public int inventorySlots = 5;   // 该角色允许使用的道具栏格子数 (1~5)

    [Header("开局自带资产")]
    public List<ModuleType> initialItems; // 初始携带的主动模块列表

    [Header("角色视觉资产")]
    public Sprite characterIcon;
}