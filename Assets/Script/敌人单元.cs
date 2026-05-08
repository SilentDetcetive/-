using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("核心属性")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public int damage = 1;
    public float ghostDuration = 5f;

    [Header("视野侦测")]
    public float viewDistance = 6f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("巡逻路径设置")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 0.5f;

    // --- 内部运行变量 ---
    private int currentPointIndex = 0;
    private bool isGhosted = false;
    private bool isChasing = false;
    private GridPlayerMover targetPlayer;

    // 🌟 全域警报新增：强制追击计时器
    private float forceChaseTimer = 0f;

    private Collider col;
    private Rigidbody rb;
    private Renderer[] unitRenderers;
    private Color[] originalColors;

    private void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        CacheOriginalColors();

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
            StartCoroutine(PatrolRoutine());
        }
    }

    private void Update()
    {
        // 1. 如果正在虚化罚站，拦截一切逻辑
        if (isGhosted) return;

        // 🌟 警报系统：倒计时处理
        if (forceChaseTimer > 0)
        {
            forceChaseTimer -= Time.deltaTime;
        }

        // 2. 视野判断与强制追击判定
        CheckForPlayerVision();

        // 3. 核心修复：物理碰撞失效补底方案 (距离强制检测)
        if (targetPlayer != null && !targetPlayer.IsHiddenFromEnemy())
        {
            float distToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

            // 假设方块边长为1，中心点距离小于 0.65 绝对算撞上了
            if (distToPlayer <= 0.65f)
            {
                targetPlayer.TakeDamage(damage);
                StartCoroutine(GhostRoutine());
                return; // 立刻拦截当前帧的后续动作，开始罚站
            }
        }

        // 4. 追击逻辑
        if (isChasing && targetPlayer != null)
        {
            ExecuteChase();
        }
    }

    private void CheckForPlayerVision()
    {
        if (targetPlayer == null) targetPlayer = FindObjectOfType<GridPlayerMover>();
        if (targetPlayer == null) return;

        if (targetPlayer.IsHiddenFromEnemy())
        {
            isChasing = false;
            return;
        }

        // 🌟 警报系统：如果接收到检疫单元的广播，无视距离和墙壁强制追击
        if (forceChaseTimer > 0)
        {
            isChasing = true;
            return;
        }

        // 常规视野侦测计算
        Vector3 dirToPlayer = (targetPlayer.transform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

        if (distToPlayer <= viewDistance && Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
        {
            if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask))
            {
                isChasing = true;
                return;
            }
        }

        isChasing = false;
    }

    private void ExecuteChase()
    {
        Vector3 playerPos = targetPlayer.transform.position;
        playerPos.y = transform.position.y; // 保持高度不变

        transform.position = Vector3.MoveTowards(transform.position, playerPos, chaseSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, playerPos) > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(playerPos - transform.position);
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (isChasing || isGhosted)
            {
                yield return null;
                continue;
            }

            if (patrolPoints == null || patrolPoints.Length < 2) yield break;

            Transform targetPoint = patrolPoints[currentPointIndex];
            Vector3 endPos = targetPoint.position;

            if (Vector3.Distance(transform.position, endPos) > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(endPos - transform.position);
            }

            while (Vector3.Distance(transform.position, endPos) > 0.01f)
            {
                if (isChasing || isGhosted) break;

                transform.position = Vector3.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            if (!isChasing && !isGhosted)
            {
                transform.position = endPos;
                yield return new WaitForSeconds(waitTimeAtPoint);
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            }
        }
    }

    // 依然保留常规物理检测
    private void HandleEncounter(Collider other)
    {
        if (isGhosted) return;

        GridPlayerMover player = other.GetComponent<GridPlayerMover>();
        if (player == null) player = other.GetComponentInParent<GridPlayerMover>();

        if (player != null)
        {
            player.TakeDamage(damage);
            StartCoroutine(GhostRoutine());
        }
    }

    private void OnCollisionEnter(Collision collision) { HandleEncounter(collision.collider); }
    private void OnTriggerEnter(Collider other) { HandleEncounter(other); }

    // 🌟 接收全局警报的公共接口
    public void ForceAlertChase(float duration)
    {
        // 如果当前正在罚站（例如被格式弹击中或刚撞到玩家），就不参与追击了
        if (isGhosted) return;

        forceChaseTimer = Mathf.Max(forceChaseTimer, duration);
    }

    private IEnumerator GhostRoutine()
    {
        isGhosted = true;
        isChasing = false;

        if (rb != null) rb.velocity = Vector3.zero;

        col.isTrigger = true;
        SetBodyAlpha(0.3f);

        yield return new WaitForSeconds(ghostDuration);

        isGhosted = false;
        col.isTrigger = false;
        RestoreOriginalColors();
    }

    public void ApplyVirtualized(float duration)
    {
        if (!isGhosted && gameObject.activeInHierarchy) StartCoroutine(BulletGhostRoutine(duration));
    }

    private IEnumerator BulletGhostRoutine(float duration)
    {
        isGhosted = true;
        isChasing = false;
        if (rb != null) rb.velocity = Vector3.zero;
        col.isTrigger = true;
        SetBodyAlpha(0.3f);
        yield return new WaitForSeconds(duration);
        isGhosted = false;
        col.isTrigger = false;
        RestoreOriginalColors();
    }

    private void CacheOriginalColors()
    {
        unitRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[unitRenderers.Length];
        for (int i = 0; i < unitRenderers.Length; i++)
        {
            if (unitRenderers[i].material.HasProperty("_Color"))
                originalColors[i] = unitRenderers[i].material.color;
            else if (unitRenderers[i].material.HasProperty("_BaseColor"))
                originalColors[i] = unitRenderers[i].material.GetColor("_BaseColor");
        }
    }

    private void SetBodyAlpha(float alpha)
    {
        foreach (Renderer rend in unitRenderers)
        {
            if (rend == null) continue;
            Material mat = rend.material;
            if (mat.HasProperty("_Color")) { Color c = mat.color; c.a = alpha; mat.color = c; }
            else if (mat.HasProperty("_BaseColor")) { Color c = mat.GetColor("_BaseColor"); c.a = alpha; mat.SetColor("_BaseColor", c); }
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < unitRenderers.Length; i++)
        {
            if (unitRenderers[i] == null) continue;
            Material mat = unitRenderers[i].material;
            if (mat.HasProperty("_Color")) mat.color = originalColors[i];
            else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", originalColors[i]);
        }
    }
}