using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GridPlayerMover : MonoBehaviour
{
    [Header("移动设置")]
    public float moveDuration = 0.2f;          // 走一格要多久
    public float stepDistance = 1f;            // 每次移动一格
    public float repeatMoveDelay = 0.02f;      // 长按时，两步之间的间隔
    public float maxStepUpHeight = 0.2f;       // 允许向上迈的最大高度（防止爬墙）
    public float maxStepDownHeight = 1.2f;     // 允许向下走的最大高度

    [Header("数据资产")]
    public int dataAsset = 0;          // 当前拥有的核心资料数量
    public int targetDataAsset = 0;    // 目标资料数量，可选，不需要就填 0

    [Header("墙体检测")]
    public float bodyCheckHeight = 0.5f;       // 玩家身体检测高度
    public Vector3 bodyCheckHalfExtents = new Vector3(0.3f, 0.4f, 0.3f);

    [Header("地面检测")]
    public float groundSearchUp = 3f;          // 从目标点上方多高开始往下找
    public float groundSearchDown = 5f;        // 往下搜索多远

    [Header("引用")]
    public Transform cameraTransform;
    private bool isMoving = false;
    private float nextMoveAllowedTime = 0f;

    [Header("路径点系统")]
    public GameObject pathPointPrefab;
    public Transform pathPointParent;
    public float pathPointYOffset = 0.05f;
    private List<Vector3> pathHistory = new List<Vector3>();
    private Dictionary<Vector3, GameObject> pathPointMap = new Dictionary<Vector3, GameObject>();

    [Header("传送门设置")]
    public KeyCode teleportKey = KeyCode.Alpha2;
    public float teleporterCheckRadius = 0.2f;
    public float teleporterCheckHeight = 0.5f;

    [Header("跳板系统")]
    public bool isLaunching = false;          // 是否正在被跳板弹起
    public float launchSpeed = 5f;            // 上升速度
    public float landingOffset = 0.5f;        // 玩家站在方块顶部时，脚底到中心点的偏移
    private float targetLandingY;             // 最终落点Y
    private bool hasLaunchTarget = false;     // 是否已找到目标承接方块

    private Rigidbody rb;
    private Collider playerCol;
    private Collider currentLandingBlockCol;
    private bool oldUseGravity;

    [Header("木马模块")]
    public KeyCode trojanModuleKey = KeyCode.Alpha6;
    public int trojanModuleCount = 0;

    public float trojanDuration = 8f;
    public float trojanCooldown = 28f;

    private bool isTrojanActive = false;
    private float trojanTimer = 0f;
    private float trojanCooldownTimer = 0f;
    [Header("木马模块视觉效果")]
    public Renderer[] trojanVisualRenderers;   // 不填也可以，脚本会自动找
    [Range(0f, 1f)] public float normalBodyAlpha = 1f;
    [Range(0f, 1f)] public float trojanBodyAlpha = 0.45f;

    [Header("透视模块")]
    public KeyCode xRayModuleKey = KeyCode.Alpha7;
    public int xRayModuleCount = 0;

    public float xRayDuration = 10f;
    public float xRayCooldown = 60f;

    public Color xRayOutlineColor = Color.red;
    public float xRayOutlineWidth = 0.06f;

    private bool isXRayActive = false;
    private float xRayTimer = 0f;
    private float xRayCooldownTimer = 0f;

    [Header("系统容量")]
    public int systemCapacity = 5;          // 开局系统容量，可以在 Inspector 自选
    public int maxSystemCapacity = 99;      // 最大系统容量，防止无限加太离谱
    [Header("稳定值")]
    public int stability = 5;          // 玩家当前稳定值
    public int maxStability = 5;       // 玩家最大稳定值

    [Header("增速模块")]
    public KeyCode speedModuleKey = KeyCode.Alpha5;
    public int speedModuleCount = 0;

    public float speedBoostPercent = 0.75f;
    public float speedBoostDuration = 12f;
    public float speedBoostCooldown = 40f;

    private bool isSpeedBoostActive = false;
    private float speedBoostTimer = 0f;
    private float speedCooldownTimer = 0f;

    [Header("超越模块")]
    public float skyRiseSpeed = 5f;          // 升空/降落速度

    private bool hasSkyModule = false;       // 是否拥有超越模块
    private bool isSkyTransitioning = false; // 是否正在升空/降落
    private bool isInSkyMode = false;        // 是否处于天空模式

    private float skyTargetY = 0f;           // 当前升空/降落目标Y
    private float groundY = 0f;              // 地面Y
    private int skyMovesRemaining = 0;       // 天空剩余移动次数

    private float currentSkyHeight = 5f;     // 当前模块提供的飞行高度
    private int currentSkyMoveLimit = 5;     // 当前模块提供的天空移动次数

    public float skyModeDuration = 10f;      // 天空模式持续时间（秒）
    private float skyModeTimer = 0f;         //
                                             
    [Header("侵入模块")]
    public KeyCode intrusionModuleKey = KeyCode.Alpha8;
    public KeyCode fireFormatBulletKey = KeyCode.Mouse0;

    public int intrusionModuleCount = 0;

    public int formatBulletMaxCount = 3;
    public int currentFormatBulletCount = 0;

    public float formatBulletRefreshTime = 30f;
    public float formatBulletVirtualizedDuration = 6f;

    public GameObject formatBulletPrefab;
    public Transform formatBulletSpawnPoint;
    public float formatBulletSpeed = 8f;

    private bool hasFormatBullets = false;
    private float formatBulletRefreshTimer = 0f;
    [Header("重力系统")]
    public Vector3 currentUp = Vector3.up;
    [Header("糖豆收集系统")]
    public int beanCount = 0;                          // 玩家吃掉的糖豆总数
    public TMPro.TextMeshProUGUI beanCountText;        // 指向屏幕上方的UI文字

    // 核心转换工具：获取当前重力平面与基准地面的旋转差值
    private Quaternion GetGravityRotation()
    {
        return Quaternion.FromToRotation(Vector3.up, currentUp);
    }

    // 暴露给 GravityPad 调用的方法
    public void ChangeGravity(Vector3 newUp)
    {
        if (currentUp == newUp) return;

        currentUp = newUp.normalized;

        // 旋转玩家本体，让玩家的“脚底”贴合新的墙面
        transform.rotation = Quaternion.FromToRotation(Vector3.up, currentUp);

        Debug.Log("重力与视角切换！当前的 '上' 方向变更为：" + currentUp);
        // 注意：如果你的相机是玩家的子物体，此时视角会自动跟着旋转！
    }


    private void Start()
    {

        if (PlayerDataBridge.SelectedCharacter != null)
        {
            CharacterConfig config = PlayerDataBridge.SelectedCharacter;

            // 1. 动态改写基础移速、生命值属性
            moveDuration = config.moveDuration;
            maxStability = config.maxStability;
            stability = maxStability;

            // 2. 根据该角色的限制定型物品栏的显示格子数
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SetupSlots(config.inventorySlots);
            }

            // 3. 循环发放开局自带的道具资产
            foreach (ModuleType item in config.initialItems)
            {
                GiveInitialItem(item);
            }
        }
        rb = GetComponent<Rigidbody>();
        playerCol = GetComponent<Collider>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        SnapToGridCenter();
        groundY = transform.position.y;

        Vector3 startGrid = GetGridCenter(transform.position);
        startGrid.y = transform.position.y;

        pathHistory.Add(startGrid);
        if (trojanVisualRenderers == null || trojanVisualRenderers.Length == 0)
        {
            trojanVisualRenderers = GetComponentsInChildren<Renderer>();
        }

        SetPlayerBodyAlpha(normalBodyAlpha);
    }

    // 辅助方法：用来给开局发放道具并增加对应变量计数
    private void GiveInitialItem(ModuleType item)
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.TryAddItem(item))
        {
            switch (item)
            {
                case ModuleType.Speed: speedModuleCount++; break;
                case ModuleType.Trojan: trojanModuleCount++; break;
                case ModuleType.XRay: xRayModuleCount++; break;
                case ModuleType.Intrusion: intrusionModuleCount++; break;
            }
        }
    }
    private void Update()
    {
        if (isLaunching)
        {
            HandleLaunching();
            return;
        }

        if (isSkyTransitioning)
        {
            HandleSkyTransition();
            return;
        }

        if (cameraTransform == null) return;

       
        HandleSkyModuleInput();
        UpdateSkyModeTimer();

        HandleSpeedModuleInput();
        UpdateSpeedModuleTimers();

        HandleTrojanModuleInput();
        UpdateTrojanModuleTimers();

        HandleXRayModuleInput();        
        UpdateXRayModuleTimers();      

        HandleIntrusionModuleInput();   
        UpdateIntrusionModuleTimers();  
       
        if (isMoving || Time.time < nextMoveAllowedTime)
            return;

        if (Input.GetKeyDown(teleportKey))
        {
            TryTeleport();
            return;
        }

      

        Vector3 moveDir;

        
   
            moveDir = GetInputMoveDirection();

       
        if (moveDir == Vector3.zero) return;

        TryMove(moveDir);
    }

    private void HandleSkyModuleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryActivateSkyModule();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TryFallFromSky();
        }
    }

    private Vector3 GetInputMoveDirection()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // 【关键修复】：彻底去掉了 camForward.y = 0f 和 camRight.y = 0f !
        // 让底层逻辑自己去适配墙面方向！

        if (camForward.sqrMagnitude < 0.001f || camRight.sqrMagnitude < 0.001f)
            return Vector3.zero;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 forwardDir = GetNearestCardinalDirection(camForward);
        Vector3 rightDir = GetNearestCardinalDirection(camRight);

        if (Input.GetKey(KeyCode.W)) return forwardDir;
        if (Input.GetKey(KeyCode.S)) return -forwardDir;
        if (Input.GetKey(KeyCode.A)) return -rightDir;
        if (Input.GetKey(KeyCode.D)) return rightDir;

        return Vector3.zero;
    }

    private void TryMove(Vector3 dir)
    {
        // ==========================================
        // 1. 优先检测：攀爬块探测拦截
        // ==========================================
        Vector3 checkPos = transform.position + dir * stepDistance + currentUp * bodyCheckHeight;
        Collider[] hits = Physics.OverlapBox(
            checkPos,
            bodyCheckHalfExtents,
            Quaternion.identity,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider col in hits)
        {
            WallClimbBlock climbBlock = col.GetComponent<WallClimbBlock>();
            if (climbBlock != null)
            {
                // 如果前方这一格是攀爬块，直接上墙，并 return 终止后续的普通移动判定！
                ExecuteWallClimb(climbBlock);
                return;
            }
        }

        // ==========================================
        // 2. 核心移动坐标准备
        // ==========================================
        Vector3 currentGridCenter = GetGridCenter(transform.position);
        Vector3 targetWorldPos = currentGridCenter + dir * stepDistance;

        float playerHalfHeight = GetPlayerHalfHeight();

        // ==========================================
        // 3. 天空模式（超越模块保持在全局平面的逻辑）
        // ==========================================
        if (isInSkyMode)
        {
            Vector3 targetPosition = new Vector3(targetWorldPos.x, GetCurrentMoveY(), targetWorldPos.z);
            Vector3 bodyCheckPosition = new Vector3(targetWorldPos.x, GetCurrentMoveY(), targetWorldPos.z);

            if (IsBlockedAt(bodyCheckPosition) || IsBlockedAt(targetPosition))
            {
                nextMoveAllowedTime = Time.time + repeatMoveDelay;
                return;
            }

            StartCoroutine(MoveToTarget(targetPosition));
            return;
        }

        // ==========================================
        // 4. 地面/墙面通用模式（动态适应重力方向）
        // ==========================================

        // 4.1 获取玩家脚底在【当前重力轴】上的相对高度
        float currentFootHeight = Vector3.Dot(transform.position, currentUp) - playerHalfHeight;

        // 4.2 障碍检测点要跟着重力方向变
        if (IsBlockedAt(targetWorldPos))
        {
            nextMoveAllowedTime = Time.time + repeatMoveDelay;
            return;
        }

        // 4.3 向下寻找当前重力方向的真实落脚点
        if (!TryFindGroundAt(targetWorldPos, out Vector3 targetGroundPoint))
        {
            nextMoveAllowedTime = Time.time + repeatMoveDelay;
            return;
        }

        // 4.4 验证高度差是否允许通行（根据相对高度计算）
        float targetFootHeight = Vector3.Dot(targetGroundPoint, currentUp);
        float heightDifference = targetFootHeight - currentFootHeight;

        if (heightDifference > maxStepUpHeight || heightDifference < -maxStepDownHeight)
        {
            nextMoveAllowedTime = Time.time + repeatMoveDelay;
            return;
        }

        // 4.5 最终落脚坐标 = 射线打到的真实表面落脚点 + 往当前的“上方”推半个身位
        Vector3 targetPositionGround = targetGroundPoint + currentUp * playerHalfHeight;

        if (IsBlockedAt(targetPositionGround))
        {
            nextMoveAllowedTime = Time.time + repeatMoveDelay;
            return;
        }

        // 通过所有检测，开始移动！
        StartCoroutine(MoveToTarget(targetPositionGround));
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 previousGrid = GetGridCenter(startPos);
        previousGrid.y = startPos.y;

        float elapsed = 0f;
        float currentMoveDuration = GetCurrentMoveDuration();

        while (elapsed < currentMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / currentMoveDuration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        if (!isInSkyMode)
        {
            Vector3 currentGrid = GetGridCenter(transform.position);
            currentGrid.y = transform.position.y;
            UpdatePathSystem(previousGrid, currentGrid);
        }
        isMoving = false;
        nextMoveAllowedTime = Time.time + repeatMoveDelay;

        if (isInSkyMode)
        {
            skyMovesRemaining--;

            if (skyMovesRemaining <= 0)
            {
                TryFallFromSky();
            }
        }

        CheckBouncePad();
        CheckSkyModulePickup();
        CheckSpeedModulePickup();
        CheckCapacityModulePickup();
        CheckTrojanModulePickup();
        CheckXRayModulePickup();
        CheckIntrusionModulePickup();
        CheckCoreDataPickup();
        CheckBeanPickup();
    }

    private bool TryFindGroundAt(Vector3 targetPos, out Vector3 groundPoint)
    {
        groundPoint = Vector3.zero;

        // 从目标点沿着当前的“上”方多高处开始，往现在的“下”方发射射线
        Vector3 rayStart = targetPos + currentUp * groundSearchUp;

        RaycastHit[] hits = Physics.RaycastAll(
            rayStart,
            -currentUp,
            groundSearchUp + groundSearchDown,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0) return false;

        // 获取玩家当前在重力轴上的高度
        float currentFootHeight = Vector3.Dot(transform.position, currentUp) - GetPlayerHalfHeight();
        float bestHeight = float.NegativeInfinity;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            Collider col = hit.collider;
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

            float candidateHeight = Vector3.Dot(hit.point, currentUp);
            float deltaHeight = candidateHeight - currentFootHeight;

            if (deltaHeight > maxStepUpHeight) continue;
            if (deltaHeight < -maxStepDownHeight) continue;

            if (candidateHeight > bestHeight)
            {
                bestHeight = candidateHeight;
                groundPoint = hit.point;
                found = true;
            }
        }
        return found;
    }

    private bool IsBlockedAt(Vector3 targetPosition)
    {
        // 【关键修复1】：让检测中心点根据“当前重力方向”进行偏移，而不是写死的 Vector3.up
        Vector3 checkCenter = targetPosition + currentUp * bodyCheckHeight;

        // 【关键修复2】：物理检测盒的旋转角度必须和玩家当前的重力（GetGravityRotation）保持一致！
        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            bodyCheckHalfExtents,
            GetGravityRotation(), // 以前是 Quaternion.identity，导致盒子不转
            ~0,
            QueryTriggerInteraction.Collide
        );

        PlayerColorController playerColor = GetComponent<PlayerColorController>();
        PlayerLevelController playerLevel = GetComponent<PlayerLevelController>();

        foreach (Collider col in hits)
        {
            if (col.transform == transform || col.transform.IsChildOf(transform))
                continue;

            WallWalkPortal wallPortal = col.GetComponent<WallWalkPortal>();
            if (wallPortal != null)
            {
                continue;
            }

            // 警戒区判断
            SecurityZone securityZone = col.GetComponent<SecurityZone>();
            if (securityZone != null)
            {
                if (isTrojanActive) continue;
                if (securityZone.IsBlocking()) return true;
                else continue;
            }

            // 颜色门判断
            ColorGateSingle gate = col.GetComponent<ColorGateSingle>();
            if (gate != null)
            {
                if (gate.CanPass(playerColor)) continue;
                else return true;
            }

            // 等级障碍判断
            LevelBarrier barrier = col.GetComponent<LevelBarrier>();
            if (barrier != null)
            {
                if (barrier.CanPass(playerLevel)) continue;
                else return true;
            }

            // 其他普通碰撞体仍然算障碍（包括正在爬的墙壁，如果盒子错误插进墙里就会触发这里）
            if (!col.isTrigger)
                return true;
        }

        return false;
    }
    private float GetPlayerHalfHeight()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            return col.bounds.extents.y;

        return 0.5f;
    }

    private Vector3 GetGridCenter(Vector3 worldPos)
    {
        // 魔法步骤：将真实世界的 3D 坐标，临时“拍平”到相对地面上
        Quaternion invRot = Quaternion.Inverse(GetGravityRotation());
        Vector3 localPos = invRot * worldPos;

        // 在相对地面上，依然安心地使用 X 和 Z 进行 0.5 格对齐
        localPos.x = Mathf.Floor(localPos.x) + 0.5f;
        localPos.z = Mathf.Floor(localPos.z) + 0.5f;

        // 算好之后，再将其转回 3D 世界坐标
        return GetGravityRotation() * localPos;
    }

    private void SnapToGridCenter()
    {
        Vector3 center = GetGridCenter(transform.position);

        // 保持玩家在当前平面的高度不变
        Quaternion invRot = Quaternion.Inverse(GetGravityRotation());
        Vector3 localCenter = invRot * center;
        Vector3 localPos = invRot * transform.position;

        localCenter.y = localPos.y;
        transform.position = GetGravityRotation() * localCenter;
    }

    private Vector3 GetNearestCardinalDirection(Vector3 dir)
    {
        // 先把按键方向转回相对空间
        Quaternion invRot = Quaternion.Inverse(GetGravityRotation());
        Vector3 localDir = invRot * dir;

        localDir.y = 0f;
        localDir.Normalize();

        Vector3 bestLocalDir = Vector3.forward;
        float maxDot = Vector3.Dot(localDir, Vector3.forward);

        if (Vector3.Dot(localDir, Vector3.back) > maxDot) { maxDot = Vector3.Dot(localDir, Vector3.back); bestLocalDir = Vector3.back; }
        if (Vector3.Dot(localDir, Vector3.right) > maxDot) { maxDot = Vector3.Dot(localDir, Vector3.right); bestLocalDir = Vector3.right; }
        if (Vector3.Dot(localDir, Vector3.left) > maxDot) { maxDot = Vector3.Dot(localDir, Vector3.left); bestLocalDir = Vector3.left; }

        // 最后转回真实方向
        return GetGravityRotation() * bestLocalDir;
    }

    private void OnDrawGizmosSelected()
    {
        float playerHalfHeight = GetPlayerHalfHeight();
        Vector3 currentGridCenter = GetGridCenter(transform.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentGridCenter, 0.1f);

        Vector3 checkCenter = transform.position + Vector3.up * bodyCheckHeight;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(checkCenter, bodyCheckHalfExtents * 2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(transform.position.x, transform.position.y + groundSearchUp, transform.position.z),
            new Vector3(transform.position.x, transform.position.y - groundSearchDown, transform.position.z)
        );
    }

    private void UpdatePathSystem(Vector3 previousGrid, Vector3 currentGrid)
    {
        // 安全检查
        if (pathHistory.Count == 0)
        {
            pathHistory.Add(currentGrid);
            return;
        }

        // 如果玩家往回走：当前格子等于“上一个历史点”
        if (pathHistory.Count >= 2 && IsSameGrid(currentGrid, pathHistory[pathHistory.Count - 2]))
        {
            Vector3 removedGrid = pathHistory[pathHistory.Count - 1];

            RemovePathPointAt(removedGrid);
            pathHistory.RemoveAt(pathHistory.Count - 1);
        }
        else
        {
            // 正常前进：在离开的格子生成路径点
            CreatePathPointAt(previousGrid);
            pathHistory.Add(currentGrid);
        }
    }

    private void CreatePathPointAt(Vector3 gridPosition)
    {
        if (pathPointPrefab == null) return;
        if (pathPointMap.ContainsKey(gridPosition)) return;

        Vector3 spawnPos = new Vector3(
            gridPosition.x,
            gridPosition.y - GetPlayerHalfHeight() + pathPointYOffset,
            gridPosition.z
        );

        GameObject obj;

        if (pathPointParent != null)
            obj = Instantiate(pathPointPrefab, spawnPos, Quaternion.identity, pathPointParent);
        else
            obj = Instantiate(pathPointPrefab, spawnPos, Quaternion.identity);

        pathPointMap.Add(gridPosition, obj);
    }

    private void RemovePathPointAt(Vector3 gridPosition)
    {
        if (pathPointMap.TryGetValue(gridPosition, out GameObject obj))
        {
            if (obj != null)
                Destroy(obj);

            pathPointMap.Remove(gridPosition);
        }
    }

    private bool IsSameGrid(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x) &&
               Mathf.Approximately(a.y, b.y) &&
               Mathf.Approximately(a.z, b.z);
    }

    private void TryTeleport()
    {
        ColorTeleporter currentTeleporter = GetTeleporterUnderPlayer();
        if (currentTeleporter == null)
        {
            Debug.Log("玩家当前不在传送门上");
            return;
        }

        ColorTeleporter targetTeleporter = FindMatchingTeleporter(currentTeleporter);
        if (targetTeleporter == null)
        {
            Debug.Log("没有找到对应颜色的另一个传送门");
            return;
        }

        TeleportTo(targetTeleporter);
    }

    private ColorTeleporter GetTeleporterUnderPlayer()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + teleporterCheckHeight,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            teleporterCheckRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider col in hits)
        {
            ColorTeleporter teleporter = col.GetComponent<ColorTeleporter>();
            if (teleporter != null)
                return teleporter;
        }

        return null;
    }

    private ColorTeleporter FindMatchingTeleporter(ColorTeleporter currentTeleporter)
    {
        ColorTeleporter[] allTeleporters = FindObjectsOfType<ColorTeleporter>();

        foreach (ColorTeleporter teleporter in allTeleporters)
        {
            if (teleporter == null) continue;
            if (teleporter == currentTeleporter) continue;

            if (teleporter.teleporterColor == currentTeleporter.teleporterColor)
                return teleporter;
        }

        return null;
    }

    private void TeleportTo(ColorTeleporter targetTeleporter)
    {
        Vector3 targetPoint = targetTeleporter.GetTeleportPoint();

        float playerHalfHeight = GetPlayerHalfHeight();
        Vector3 finalPosition = new Vector3(
            targetPoint.x,
            targetTeleporter.transform.position.y + playerHalfHeight,
            targetPoint.z
        );

        transform.position = finalPosition;
        SnapToGridCenter();

        nextMoveAllowedTime = Time.time + 0.1f;
        CheckBouncePad();
        CheckSkyModulePickup();
        CheckSpeedModulePickup();
        CheckCapacityModulePickup();
        CheckTrojanModulePickup();
    }

    void CheckBouncePad()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            BouncePad pad = hit.GetComponent<BouncePad>();
            if (pad != null)
            {
                StartLaunch(pad);
                return;
            }
        }
    }

    void StartLaunch(BouncePad pad)
    {
        if (isLaunching) return;

        LandingBlock targetBlock = FindLandingBlockAbove(pad.maxCheckHeight);

        if (targetBlock == null)
        {
            Debug.LogWarning("上方没有找到承接方块，无法弹起。");
            return;
        }

        float playerHeight = GetPlayerHeight();
        float blockTopY = targetBlock.transform.position.y + GetBlockHeight(targetBlock.transform) / 2f;

        targetLandingY = blockTopY + playerHeight / 2f;
        hasLaunchTarget = true;
        isLaunching = true;
        isMoving = true;

        currentLandingBlockCol = targetBlock.GetComponent<Collider>();

        if (rb != null)
        {
            oldUseGravity = rb.useGravity;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }

        if (playerCol != null && currentLandingBlockCol != null)
        {
            Physics.IgnoreCollision(playerCol, currentLandingBlockCol, true);
        }
    }

    LandingBlock FindLandingBlockAbove(float maxHeight)
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.up, maxHeight);
        float nearestDistance = float.MaxValue;
        LandingBlock nearestBlock = null;

        foreach (RaycastHit hit in hits)
        {
            LandingBlock block = hit.collider.GetComponent<LandingBlock>();
            if (block != null)
            {
                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestBlock = block;
                }
            }
        }

        return nearestBlock;
    }

    void HandleLaunching()
    {
        if (!hasLaunchTarget)
        {
            EndLaunch();
            return;
        }

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetLandingY, launchSpeed * Time.deltaTime);
        transform.position = pos;

        if (Mathf.Abs(transform.position.y - targetLandingY) < 0.001f)
        {
            Vector3 finalPos = transform.position;
            finalPos.y = targetLandingY;
            transform.position = finalPos;

            EndLaunch();
        }
    }

    float GetPlayerHeight()
    {
        return GetPlayerHalfHeight() * 2f;
    }

    float GetBlockHeight(Transform block)
    {
        Renderer rd = block.GetComponent<Renderer>();
        if (rd != null)
        {
            return rd.bounds.size.y;
        }

        return block.localScale.y;
    }

    void EndLaunch()
    {
        if (playerCol != null && currentLandingBlockCol != null)
        {
            Physics.IgnoreCollision(playerCol, currentLandingBlockCol, false);
        }

        if (rb != null)
        {
            rb.useGravity = oldUseGravity;
            rb.velocity = Vector3.zero;
        }

        isLaunching = false;
        hasLaunchTarget = false;
        isMoving = false;
        currentLandingBlockCol = null;

        SnapToGridCenter();
    }

    private void TryActivateSkyModule()
    {
        if (!hasSkyModule) return;
        if (isInSkyMode) return;
        if (isSkyTransitioning) return;
        if (isLaunching) return;
        if (isMoving) return;
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItem(ModuleType.Sky);
        groundY = transform.position.y;
        skyTargetY = groundY + currentSkyHeight;
        skyMovesRemaining = currentSkyMoveLimit;
        skyModeTimer = skyModeDuration;   // 新增：开始计时

        BeginSkyPhysicsLock();
        isSkyTransitioning = true;
    }

    private void TryFallFromSky()
    {
        if (!isInSkyMode) return;
        if (isSkyTransitioning) return;

        skyModeTimer = 0f;

        // 向下打射线寻找当前位置下方的真实地面
        if (Physics.Raycast(transform.position, -currentUp, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            skyTargetY = hit.point.y;
        }
        else
        {
            skyTargetY = groundY; // 兜底保护
        }

        isInSkyMode = false;
        isSkyTransitioning = true;
    }

    private void HandleSkyTransition()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, skyTargetY, skyRiseSpeed * Time.deltaTime);
        transform.position = pos;

        if (Mathf.Abs(transform.position.y - skyTargetY) < 0.001f)
        {
            pos.y = skyTargetY;
            transform.position = pos;

            isSkyTransitioning = false;

            if (Mathf.Abs(skyTargetY - groundY) < 0.001f)
            {
                // 已经落地
                hasSkyModule = false;
                skyMovesRemaining = 0;
                skyModeTimer = 0f;   // 新增
                isInSkyMode = false;
                EndSkyPhysicsLock();
            }
            else
            {
                // 已进入天空模式
                isInSkyMode = true;
            }

            SnapToGridCenter();
        }
    }

    private void CheckSkyModulePickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            SkyModulePickup pickup = hit.GetComponent<SkyModulePickup>();
            if (pickup != null)
            {
                // 【核心修改】：先问背包能塞下吗？能塞下才执行拾取
                if (InventoryManager.Instance != null && InventoryManager.Instance.TryAddItem(ModuleType.Sky))
                {
                    hasSkyModule = true;
                    currentSkyHeight = pickup.skyHeight;
                    currentSkyMoveLimit = pickup.skyMoveCount;
                    Debug.Log("获得超越模块");
                    Destroy(hit.gameObject);
                }
                else
                {
                    Debug.Log("道具栏已满，无法拾取超越模块！");
                }
                return;
            }
        }
    }

    private float GetCurrentMoveY()
    {
        if (isInSkyMode)
            return groundY + currentSkyHeight;

        return groundY;
    }

    private void BeginSkyPhysicsLock()
    {
        if (rb != null)
        {
            oldUseGravity = rb.useGravity;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }
    }

    private void EndSkyPhysicsLock()
    {
        if (rb != null)
        {
            rb.useGravity = oldUseGravity;
            rb.velocity = Vector3.zero;
        }
    }

    private void UpdateSkyModeTimer()
    {
        if (!isInSkyMode) return;
        if (isSkyTransitioning) return;

        skyModeTimer -= Time.deltaTime;

        if (skyModeTimer <= 0f)
        {
            skyModeTimer = 0f;
            TryFallFromSky();
        }
    }

    private void HandleSpeedModuleInput()
    {
        if (Input.GetKeyDown(speedModuleKey))
        {
            TryUseSpeedModule();
        }
    }

    private void TryUseSpeedModule()
    {
        if (speedModuleCount <= 0)
        {
            Debug.Log("没有增速模块。");
            return;
        }

        if (isSpeedBoostActive)
        {
            Debug.Log("增速模块正在生效中。");
            return;
        }

        if (speedCooldownTimer > 0f)
        {
            Debug.Log("增速模块冷却中，剩余时间：" + speedCooldownTimer.ToString("F1") + " 秒");
            return;
        }

        speedModuleCount--;
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItem(ModuleType.Speed);
       
        isSpeedBoostActive = true;
        speedBoostTimer = speedBoostDuration;
        speedCooldownTimer = speedBoostCooldown;

        Debug.Log("增速模块启动！当前速度提升 75%，持续 " + speedBoostDuration + " 秒。");
    }

    private void UpdateSpeedModuleTimers()
    {
        if (speedCooldownTimer > 0f)
        {
            speedCooldownTimer -= Time.deltaTime;

            if (speedCooldownTimer < 0f)
                speedCooldownTimer = 0f;
        }

        if (!isSpeedBoostActive)
            return;

        speedBoostTimer -= Time.deltaTime;

        if (speedBoostTimer <= 0f)
        {
            speedBoostTimer = 0f;
            isSpeedBoostActive = false;

            Debug.Log("增速模块效果结束。");
        }
    }

    private float GetCurrentMoveDuration()
    {
        float finalMoveDuration = moveDuration;

        if (isSpeedBoostActive)
        {
            float speedMultiplier = 1f + speedBoostPercent;
            finalMoveDuration = moveDuration / speedMultiplier;
        }

        return Mathf.Max(0.01f, finalMoveDuration);
    }

    private void CheckSpeedModulePickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            SpeedModulePickup pickup = hit.GetComponent<SpeedModulePickup>();

            if (pickup != null)
            {
                // ================== 【修改这里】 ==================
                // 先问问 UI 大管家，能塞进去吗？
                if (InventoryManager.Instance.TryAddItem(ModuleType.Speed))
                {
                    // 塞进去了！数量+1，销毁地上的道具
                    speedModuleCount += pickup.amount;
                    Debug.Log("获得增速模块");
                    Destroy(hit.gameObject);
                }
                else
                {
                    // 背包满了，直接 return，不 Destroy，让道具留在地上
                    Debug.Log("道具栏已满，无法拾取！");
                }
                return;
                // ===================================================
            }
        }
    }

    private void CheckCapacityModulePickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            CapacityModulePickup pickup = hit.GetComponent<CapacityModulePickup>();

            if (pickup != null)
            {
                AddSystemCapacity(pickup.amount);

                Destroy(hit.gameObject);
                return;
            }
        }
    }

    public void AddSystemCapacity(int amount)
    {
        systemCapacity += amount;

        if (systemCapacity > maxSystemCapacity)
        {
            systemCapacity = maxSystemCapacity;
        }

        Debug.Log("系统容量增加 +" + amount + "，当前系统容量：" + systemCapacity);
    }

    public int GetSystemCapacity()
    {
        return systemCapacity;
    }

    private void HandleTrojanModuleInput()
    {
        if (Input.GetKeyDown(trojanModuleKey))
        {
            TryUseTrojanModule();
        }
    }

    private void TryUseTrojanModule()
    {
        if (trojanModuleCount <= 0)
        {
            Debug.Log("没有木马模块。");
            return;
        }

        if (isTrojanActive)
        {
            Debug.Log("木马模块正在生效中。");
            return;
        }

        if (trojanCooldownTimer > 0f)
        {
            Debug.Log("木马模块冷却中，剩余时间：" + trojanCooldownTimer.ToString("F1") + " 秒");
            return;
        }

        trojanModuleCount--;
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItem(ModuleType.Trojan);
        isTrojanActive = true;
        trojanTimer = trojanDuration;
        trojanCooldownTimer = trojanCooldown;

        SetPlayerBodyAlpha(trojanBodyAlpha);

        Debug.Log("木马模块启动！玩家进入隐身状态，持续 " + trojanDuration + " 秒。");
    }

    private void UpdateTrojanModuleTimers()
    {
        if (trojanCooldownTimer > 0f)
        {
            trojanCooldownTimer -= Time.deltaTime;

            if (trojanCooldownTimer < 0f)
                trojanCooldownTimer = 0f;
        }

        if (!isTrojanActive)
            return;

        trojanTimer -= Time.deltaTime;

        if (trojanTimer <= 0f)
        {
            trojanTimer = 0f;
            isTrojanActive = false;

            SetPlayerBodyAlpha(normalBodyAlpha);

            Debug.Log("木马模块效果结束。");
        }
    }

    private void CheckTrojanModulePickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            TrojanModulePickup pickup = hit.GetComponent<TrojanModulePickup>();

            if (pickup != null)
            {
                if (InventoryManager.Instance != null && InventoryManager.Instance.TryAddItem(ModuleType.Trojan))
                {
                    trojanModuleCount += pickup.amount;
                    Debug.Log("获得木马模块");
                    Destroy(hit.gameObject);
                }
                return;
            }
        }
    }

    public bool IsTrojanActive()
    {
        return isTrojanActive;
    }

    public bool IsHiddenFromEnemy()
    {
        return isTrojanActive;
    }

    private void SetPlayerBodyAlpha(float alpha)
    {
        if (trojanVisualRenderers == null || trojanVisualRenderers.Length == 0)
        {
            trojanVisualRenderers = GetComponentsInChildren<Renderer>();
        }

        foreach (Renderer rend in trojanVisualRenderers)
        {
            if (rend == null)
                continue;

            Material[] mats = rend.materials;

            foreach (Material mat in mats)
            {
                if (mat == null)
                    continue;

                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
            }
        }
    }

    public void TakeDamage(int amount)
    {
        stability -= amount;

        if (stability < 0)
            stability = 0;

        Debug.Log("玩家受到 " + amount + " 点伤害，当前稳定值：" + stability);

        if (stability <= 0)
        {
            Debug.Log("玩家稳定值归零，游戏失败。");

            // 这里之后可以接你的关卡管理器失败逻辑
            // 例如：
            // FindObjectOfType<LevelManager>().FailLevel();
        }
    }

  

    private void HandleXRayModuleInput()
    {
        if (Input.GetKeyDown(xRayModuleKey))
        {
            TryUseXRayModule();
        }
    }

    private void TryUseXRayModule()
    {
        if (xRayModuleCount <= 0)
        {
            Debug.Log("没有透视模块。");
            return;
        }

        if (isXRayActive)
        {
            Debug.Log("透视模块正在生效中。");
            return;
        }

        if (xRayCooldownTimer > 0f)
        {
            Debug.Log("透视模块冷却中，剩余时间：" + xRayCooldownTimer.ToString("F1") + " 秒");
            return;
        }

        xRayModuleCount--;
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItem(ModuleType.XRay);
        isXRayActive = true;
        xRayTimer = xRayDuration;
        xRayCooldownTimer = xRayCooldown;

        StartXRayReveal();

        Debug.Log("透视模块启动！显示自律单元和警戒区，持续 " + xRayDuration + " 秒。");
    }

    private void UpdateXRayModuleTimers()
    {
        if (xRayCooldownTimer > 0f)
        {
            xRayCooldownTimer -= Time.deltaTime;

            if (xRayCooldownTimer < 0f)
                xRayCooldownTimer = 0f;
        }

        if (!isXRayActive)
            return;

        xRayTimer -= Time.deltaTime;

        if (xRayTimer <= 0f)
        {
            xRayTimer = 0f;
            isXRayActive = false;

            StopXRayReveal();

            Debug.Log("透视模块效果结束。");
        }
    }

    private void StartXRayReveal()
    {
        // 方案要求：全局敌方单位和警戒区的即时定位
        // 我们统一使用青蓝色 (Color.cyan) 来作为透视边框，它在暗色墙壁下视觉效果最好，也符合科幻感。
        Color revealColor = Color.cyan;

        // 1. 查找并标记所有警戒区 (SecurityZone)
        SecurityZone[] zones = FindObjectsOfType<SecurityZone>();
        foreach (SecurityZone zone in zones)
        {
            if (zone != null)
            {
                ApplyXRayTo(zone.gameObject, revealColor);
            }
        }

        // 2. 查找并标记所有巡逻敌人 (PatrolEnemy)
        PatrolEnemy[] enemies = FindObjectsOfType<PatrolEnemy>();
        foreach (PatrolEnemy enemy in enemies)
        {
            if (enemy != null)
            {
                ApplyXRayTo(enemy.gameObject, revealColor);
            }
        }
    }

    private void ApplyXRayTo(GameObject targetObj, Color color)
    {
        XRayRevealTarget reveal = targetObj.GetComponent<XRayRevealTarget>();

        if (reveal == null)
        {
            reveal = targetObj.AddComponent<XRayRevealTarget>();
        }

        reveal.Reveal(color, xRayOutlineWidth);
    }

    private void StopXRayReveal()
    {
        XRayRevealTarget[] revealTargets = FindObjectsOfType<XRayRevealTarget>();

        foreach (XRayRevealTarget reveal in revealTargets)
        {
            if (reveal != null)
            {
                reveal.Hide();
            }
        }
    }

    private void CheckXRayModulePickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        // 统一扩充雷达探测半径为 0.5f，防止网格移动错位吃不到
        Collider[] hits = Physics.OverlapSphere(checkCenter, 0.5f, ~0, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            XRayModulePickup pickup = hit.GetComponent<XRayModulePickup>();

            if (pickup != null)
            {
                // ================== 【端点注入：核心安全拦截与销毁锁】 ==================
                // 先问大管家：背包有空位吗？
                if (InventoryManager.Instance != null && InventoryManager.Instance.TryAddItem(ModuleType.XRay))
                {
                    xRayModuleCount += pickup.amount;
                    Debug.Log("获得透视模块");

                    // 核心动作：吃掉道具后，必须让模型彻底在世界中物理湮灭（消失）！
                    Destroy(hit.gameObject);
                }
                else
                {
                    // 背包满了，或者大管家不在家，打印提示，并且绝不销毁地上的道具
                    Debug.Log("道具栏已满或未初始化，无法拾取透视模块！");
                }
                // ===================================================================

                return; // 只要处理了该格物体的探测，立刻终止，防止雷达过载
            }
        }
    }

    private void HandleIntrusionModuleInput()
    {
        if (Input.GetKeyDown(intrusionModuleKey))
        {
            TryUseIntrusionModule();
        }

        if (Input.GetKeyDown(fireFormatBulletKey))
        {
            TryFireFormatBullet();
        }
    }

    private void TryUseIntrusionModule()
    {
        if (hasFormatBullets && currentFormatBulletCount > 0)
        {
            Debug.Log("你已经拥有格式弹，剩余：" + currentFormatBulletCount);
            return;
        }

        if (formatBulletRefreshTimer > 0f)
        {
            Debug.Log("格式弹刷新中，剩余时间：" + formatBulletRefreshTimer.ToString("F1") + " 秒");
            return;
        }

        if (intrusionModuleCount <= 0)
        {
            Debug.Log("没有侵入模块。");
            return;
        }

        intrusionModuleCount--;
        if (InventoryManager.Instance != null) InventoryManager.Instance.RemoveItem(ModuleType.Intrusion);
        currentFormatBulletCount = formatBulletMaxCount;
        hasFormatBullets = true;

        Debug.Log("侵入模块启动！获得 " + currentFormatBulletCount + " 枚格式弹。鼠标左键发射。");
    }

    private void TryFireFormatBullet()
    {
        if (!hasFormatBullets || currentFormatBulletCount <= 0)
        {
            return;
        }

        Vector3 shootDirection = GetFormatBulletShootDirection();

        if (shootDirection.sqrMagnitude < 0.01f)
        {
            shootDirection = transform.forward;
        }
        // 【关键修复】：将 Vector3.up 改为 currentUp，让子弹生成位置的偏移也适应墙面重力！
        Vector3 spawnPosition = transform.position + shootDirection.normalized * 0.45f + currentUp * 0.25f;

        if (formatBulletSpawnPoint != null)
        {
            spawnPosition = formatBulletSpawnPoint.position;
        }

        GameObject bulletObj = null;

        if (formatBulletPrefab != null)
        {
            bulletObj = Instantiate(formatBulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
        }
        else
        {
            bulletObj = CreateDefaultFormatBullet(spawnPosition, shootDirection);
        }

        FormatBullet bullet = bulletObj.GetComponent<FormatBullet>();

        if (bullet == null)
        {
            bullet = bulletObj.AddComponent<FormatBullet>();
        }

        bullet.speed = formatBulletSpeed;
        bullet.virtualizedDuration = formatBulletVirtualizedDuration;
        bullet.Init(shootDirection);

        currentFormatBulletCount--;

        Debug.Log("发射格式弹，剩余：" + currentFormatBulletCount);

        if (currentFormatBulletCount <= 0)
        {
            hasFormatBullets = false;
            formatBulletRefreshTimer = formatBulletRefreshTime;

            Debug.Log("格式弹用尽，开始刷新，" + formatBulletRefreshTime + " 秒后可再次获得 3 枚。");
        }
    }

    private Vector3 GetFormatBulletShootDirection()
    {
        // 优先使用我们已经绑定好的相机引用，直接返回相机的正前方！
        // 绝对不要对 .y 进行清零，让子弹完全顺着视线飞
        if (cameraTransform != null)
        {
            return cameraTransform.forward;
        }

        // 如果没有配置引用，尝试寻找主相机
        Camera cam = Camera.main;
        if (cam != null)
        {
            return cam.transform.forward;
        }

        // 如果连相机都没找到，就兜底向玩家身体的正前方发射
        return transform.forward;
    }

    private GameObject CreateDefaultFormatBullet(Vector3 spawnPosition, Vector3 shootDirection)
    {
        GameObject bulletObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        bulletObj.name = "Format Bullet";
        bulletObj.transform.position = spawnPosition;
        bulletObj.transform.rotation = Quaternion.LookRotation(shootDirection);
        bulletObj.transform.localScale = Vector3.one * 0.18f;

        Collider col = bulletObj.GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }

        Rigidbody rb = bulletObj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        Renderer rend = bulletObj.GetComponent<Renderer>();

        if (rend != null)
        {
            rend.material.color = Color.magenta;
        }

        return bulletObj;
    }

    private void UpdateIntrusionModuleTimers()
    {
        if (formatBulletRefreshTimer > 0f)
        {
            formatBulletRefreshTimer -= Time.deltaTime;
            if (formatBulletRefreshTimer <= 0f)
            {
                formatBulletRefreshTimer = 0f;

                currentFormatBulletCount = formatBulletMaxCount;
                hasFormatBullets = true;
                Debug.Log("格式弹刷新完成，重新获得 " + currentFormatBulletCount + " 枚格式弹。");
            }
        }
    }

    private void CheckIntrusionModulePickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            IntrusionModulePickup pickup = hit.GetComponent<IntrusionModulePickup>();

            if (pickup != null)
            {
                // 👇 用下面这段替换掉原来直接加数量的代码
                if (InventoryManager.Instance != null && InventoryManager.Instance.TryAddItem(ModuleType.Intrusion))
                {
                    intrusionModuleCount += pickup.amount;
                    Debug.Log("获得侵入模块");
                    Destroy(hit.gameObject);
                }
                return;
            }
        }
    }

    private void CheckCoreDataPickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.2f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            CoreDataPickup pickup = hit.GetComponent<CoreDataPickup>();

            if (pickup != null)
            {
                AddDataAsset(pickup.amount);

                Debug.Log("获得核心资料 +" + pickup.amount + "，当前数据资产：" + dataAsset);

                Destroy(hit.gameObject);
                return;
            }
        }
    }

    public void AddDataAsset(int amount)
    {
        dataAsset += amount;

        if (dataAsset < 0)
            dataAsset = 0;

        Debug.Log("数据资产增加，当前数据资产：" + dataAsset);

        if (targetDataAsset > 0 && dataAsset >= targetDataAsset)
        {
            Debug.Log("核心资料收集完成。");
        }
    }

    public int GetDataAsset()
    {
        return dataAsset;
    }

    private void ExecuteWallClimb(WallClimbBlock block)
    {
        Vector3 newUp = block.newUpDirection.normalized;
        if (currentUp == newUp) return;

        // 记录新的重力方向
        currentUp = newUp;

        // 旋转玩家本体，让玩家的“脚底”贴合新的墙面
        transform.rotation = Quaternion.FromToRotation(Vector3.up, currentUp);

        // 【关键修复】：加上 0.5f (墙面方块的半厚度)，让玩家踩在墙壁表面，而不是插进中心！
        // 如果你的墙面不是标准的 1x1x1 立方体，你可以根据实际厚度调整这个 0.5f。
        float blockHalfThickness = 0.5f;
        Vector3 targetPos = block.transform.position + currentUp * (blockHalfThickness + GetPlayerHalfHeight());

        // 开启物理锁，剥夺物理引擎在爬墙期间的干预权，防止穿模弹射
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        transform.position = targetPos;
        SnapToGridCenter();

        Debug.Log("攀爬吸附成功！当前的 '上' 方向变更为：" + currentUp);

        // 给予短暂的移动冷却，防止误触导致连续乱飞
        nextMoveAllowedTime = Time.time + moveDuration;
    }
    private void CheckBeanPickup()
    {
        Vector3 checkCenter = new Vector3(
            transform.position.x,
            transform.position.y - GetPlayerHalfHeight() + 0.1f,
            transform.position.z
        );

        // 使用 0.5f 的半径，防止像之前那样擦肩而过吃不到
        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            0.5f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            DataBeanPickup pickup = hit.GetComponent<DataBeanPickup>();

            if (pickup != null)
            {
                // 加分
                beanCount += pickup.scoreValue;

                // 更新屏幕上的文字
                if (beanCountText != null)
                {
                    beanCountText.text = " " + beanCount;
                }

                // 播放个吃豆音效（如果你有的话可以写在这里）

                // 吃掉糖豆，销毁模型
                Destroy(pickup.gameObject);
                return;
            }
        }
    }
}