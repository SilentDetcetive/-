using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SecurityZone : MonoBehaviour
{
    [Header("警戒区周期设置")]
    public bool startActive = true;          // 游戏开始时是否生效
    public float startDelay = 0f;            // 开局延迟几秒后才开始循环
    public float activeDuration = 3f;        // 生效持续时间
    public float inactiveDuration = 2f;      // 失效持续时间

    [Header("显示设置")]
    public Renderer zoneRenderer;
    public Material activeMaterial;
    public Material inactiveMaterial;

    [Header("运行状态，只看不改")]
    public bool isActive = true;

    private float timer = 0f;
    private bool hasStarted = false;
    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;

        if (zoneRenderer == null)
        {
            zoneRenderer = GetComponent<Renderer>();
        }

        isActive = false;
        RefreshVisual();
    }

    private void Start()
    {
        if (startDelay <= 0f)
        {
            BeginCycle();
        }
    }

    private void Update()
    {
        if (!hasStarted)
        {
            startDelay -= Time.deltaTime;

            if (startDelay <= 0f)
            {
                BeginCycle();
            }

            return;
        }

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        if (isActive)
        {
            SetActiveState(false);
        }
        else
        {
            SetActiveState(true);
        }
    }

    private void BeginCycle()
    {
        hasStarted = true;
        SetActiveState(startActive);
    }

    private void SetActiveState(bool active)
    {
        isActive = active;

        if (isActive)
        {
            timer = Mathf.Max(0.1f, activeDuration);
        }
        else
        {
            timer = Mathf.Max(0.1f, inactiveDuration);
        }

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (zoneRenderer == null)
            return;

        if (isActive)
        {
            if (activeMaterial != null)
                zoneRenderer.material = activeMaterial;

            zoneRenderer.enabled = true;
        }
        else
        {
            if (inactiveMaterial != null)
            {
                zoneRenderer.material = inactiveMaterial;
                zoneRenderer.enabled = true;
            }
            else
            {
                zoneRenderer.enabled = false;
            }
        }
    }

    public bool IsBlocking()
    {
        return isActive;
    }
}