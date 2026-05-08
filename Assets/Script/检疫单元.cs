using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class QuarantineUnit : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] patrolPoints;
    public float timePerGrid = 0.5f;
    public float waitTimeAtPoint = 0.5f;

    [Header("追击设置")]
    public float chaseSpeed = 2.5f;
    public float ghostDuration = 5f;
    private bool isChasing = false;
    private bool isGhosted = false;
    private GridPlayerMover targetPlayer;

    [Header("视野侦测")]
    public float viewDistance = 5f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    // --- 全局警报新增变量 ---
    private float forceChaseTimer = 0f;

    private int currentPointIndex = 0;
    private bool isPatrolling = true;

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
        if (isGhosted) return;

        // 1. 处理全域警报强制追击的倒计时
        if (forceChaseTimer > 0)
        {
            forceChaseTimer -= Time.deltaTime;
        }

        // 2. 视野判断 (现在会兼容警报状态)
        FindVisiblePlayer();

        // 3. 距离强制补底检测
        if (targetPlayer != null && !targetPlayer.IsHiddenFromEnemy())
        {
            float distToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
            if (distToPlayer <= 0.65f)
            {
                ExecuteCapture();
                return;
            }
        }

        // 4. 执行追击
        if (isChasing && targetPlayer != null)
        {
            ExecuteChase();
        }
    }

    private void FindVisiblePlayer()
    {
        if (targetPlayer == null) targetPlayer = FindObjectOfType<GridPlayerMover>();
        if (targetPlayer == null) return;

        if (targetPlayer.IsHiddenFromEnemy())
        {
            isChasing = false;
            return;
        }

        // 🌟 新增：如果处于强制追击倒计时内，无视距离和视野直接追踪！
        if (forceChaseTimer > 0)
        {
            isChasing = true;
            return;
        }

        // 常规视野侦测
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
        Vector3 targetPos = targetPlayer.transform.position;
        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(targetPos - transform.position);
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (isPatrolling)
        {
            if (isChasing || isGhosted)
            {
                yield return null;
                continue;
            }

            if (patrolPoints == null || patrolPoints.Length < 2) yield break;

            Transform targetPoint = patrolPoints[currentPointIndex];
            Vector3 startPos = transform.position;
            Vector3 endPos = targetPoint.position;

            float distance = Vector3.Distance(startPos, endPos);
            float actualMoveDuration = distance * timePerGrid;
            float elapsedTime = 0f;

            if (distance > 0.01f)
                transform.rotation = Quaternion.LookRotation(endPos - startPos);

            while (elapsedTime < actualMoveDuration)
            {
                if (isChasing || isGhosted) break;

                transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / actualMoveDuration);
                elapsedTime += Time.deltaTime;
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

    // --- 核心改动：捕获逻辑与全图警报广播 ---
    private void OnTriggerEnter(Collider other)
    {
        if (isGhosted) return;

        GridPlayerMover player = other.GetComponent<GridPlayerMover>();
        if (player == null) player = other.GetComponentInParent<GridPlayerMover>();

        if (player != null)
        {
            ExecuteCapture();
        }
    }

    private void ExecuteCapture()
    {
        if (targetPlayer != null) targetPlayer.TakeDamage(1);
        Debug.LogWarning("【检疫单元】触发全图警报！所有单位强制索敌 5 秒！");

        TriggerGlobalAlert(5f); // 广播 5 秒警报
        StartCoroutine(GhostRoutine());
    }

    private void TriggerGlobalAlert(float duration)
    {
        // 唤醒所有普通敌人 (PatrolEnemy)
        PatrolEnemy[] normalEnemies = FindObjectsOfType<PatrolEnemy>();
        foreach (PatrolEnemy enemy in normalEnemies)
        {
            enemy.ForceAlertChase(duration);
        }

        // 唤醒关卡内其他检疫单元
        QuarantineUnit[] quarantineEnemies = FindObjectsOfType<QuarantineUnit>();
        foreach (QuarantineUnit qUnit in quarantineEnemies)
        {
            if (qUnit != this) // 排除自己，因为自己要原地罚站
            {
                qUnit.ForceAlertChase(duration);
            }
        }
    }

    // 接收警报信号的接口
    public void ForceAlertChase(float duration)
    {
        // 取当前剩余时间和新警报时间的最大值，避免重复触发导致时间缩短
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

    // 视觉处理代码（保持不变）
    private void CacheOriginalColors()
    {
        unitRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[unitRenderers.Length];
        for (int i = 0; i < unitRenderers.Length; i++)
        {
            if (unitRenderers[i].material.HasProperty("_Color")) originalColors[i] = unitRenderers[i].material.color;
            else if (unitRenderers[i].material.HasProperty("_BaseColor")) originalColors[i] = unitRenderers[i].material.GetColor("_BaseColor");
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