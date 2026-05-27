using UnityEngine;

public class PlayerDataBridge : MonoBehaviour
{
    // 全局静态变量，全游戏唯一，哪怕切换场景，里面的数据也绝对不会丢失
    public static CharacterConfig SelectedCharacter;
}