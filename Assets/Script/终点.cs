using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class Endpoint : MonoBehaviour
{
    [Header("Endpoint Settings")]
    public bool isActive = true;
    public GameObject winEffectPrefab;

    [Header("Visual")]
    public bool usePulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.15f;

    private BoxCollider triggerCollider;
    private Vector3 baseScale;
    private bool reached = false;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        // 触发范围稍微大一点，更稳
        triggerCollider.size = new Vector3(1.3f, 1.3f, 1.3f);

        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (!usePulse || reached)
            return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTriggerWin(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryTriggerWin(other);
    }

    private void TryTriggerWin(Collider other)
    {
        if (!isActive || reached)
            return;

        GridPlayerMover player = other.GetComponent<GridPlayerMover>();

        if (player == null)
        {
            player = other.GetComponentInParent<GridPlayerMover>();
        }

        if (player == null)
            return;

        reached = true;
        isActive = false;

        ReachEndpoint(player);
    }

    private void ReachEndpoint(GridPlayerMover player)
    {
        Debug.Log("Level Complete");

        // 1. 禁用玩家操作
        player.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. 播放胜利特效
        if (winEffectPrefab != null)
        {
            Instantiate(winEffectPrefab, transform.position, Quaternion.identity);
        }

        // ==========================================
        // 【关键修复】：呼叫 VictoryUI 弹出胜利面板
        // ==========================================
        if (VictoryUI.Instance != null)
        {
            VictoryUI.Instance.ShowVictoryPanel();
        }
        else
        {
            Debug.LogError("系统警告：未找到 VictoryUI 实例！请确保场景中有一个物体挂载了 VictoryUI 脚本。");
        }
    }



}