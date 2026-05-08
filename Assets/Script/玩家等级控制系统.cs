using UnityEngine;

public class PlayerLevelController : MonoBehaviour
{
    [Header("当前等级")]
    [Range(0, 3)]
    public int currentLevel = 0;

    [Header("玩家可缩放的模型根节点")]
    public Transform visualRoot;

    [Header("不同等级对应边长")]
    public float level0Size = 0.4f;
    public float level1Size = 0.6f;
    public float level2Size = 0.7f;
    public float level3Size = 0.8f;

    private void Start()
    {
        ApplyLevelSize();
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void UpgradeToLevel(int targetLevel)
    {
        targetLevel = Mathf.Clamp(targetLevel, 0, 3);

        if (targetLevel <= currentLevel)
            return;

        currentLevel = targetLevel;
        ApplyLevelSize();

        Debug.Log("玩家等级提升到：" + currentLevel);
    }

    public bool CanPassLevelBarrier(int barrierLevel)
    {
        return currentLevel >= barrierLevel;
    }

    private void ApplyLevelSize()
    {
        if (visualRoot == null)
            visualRoot = transform;

        float targetSize = GetSizeByLevel(currentLevel);
        visualRoot.localScale = new Vector3(targetSize, targetSize, targetSize);
    }

    private float GetSizeByLevel(int level)
    {
        switch (level)
        {
            case 0: return level0Size;
            case 1: return level1Size;
            case 2: return level2Size;
            case 3: return level3Size;
            default: return level0Size;
        }
    }
}