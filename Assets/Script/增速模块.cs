using UnityEngine;

public class SpeedModulePickup : MonoBehaviour
{
    [Header("道具设置")]
    [Tooltip("捡起这个道具后，增加几个增速模块的使用次数？")]
    public int amount = 1;
}