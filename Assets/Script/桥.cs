using UnityEngine;

public class WallClimbBlock : MonoBehaviour
{
    [Header("玩家爬上这个方块后，头顶的'上'方向指向哪？")]
    [Tooltip("例如：如果是正前方的墙壁，填 (0, 0, -1)")]
    public Vector3 newUpDirection = Vector3.forward;
}