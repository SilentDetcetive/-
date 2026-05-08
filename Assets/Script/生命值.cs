using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("系统稳定值")]
    public int maxStability = 100; // 在这里自选初始值
    private int currentStability;

    void Start()
    {
        // 协议初始化时，状态回满
        currentStability = maxStability;
    }

    // 当受到伤害时调用这个方法
    public void TakeDamage(int damage)
    {
        currentStability -= damage;
        Debug.Log("警告：稳定值受损！当前: " + currentStability);

        if (currentStability <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("稳定值归零，协议强行终止。");
        // 这里可以加上你的死亡逻辑：比如重新加载场景，或者弹出Game Over面板
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}