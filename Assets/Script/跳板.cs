using UnityEngine;

public class BouncePad : MonoBehaviour
{
    // 这个跳板向上寻找承接方块的最大高度，防止无限查找
    public float maxCheckHeight = 20f;
}