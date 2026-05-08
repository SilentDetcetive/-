using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("旋转")]
    public float mouseSensitivity = 3f;
    public float minPitch = -30f;
    public float maxPitch = 75f;

    [Header("距离")]
    public float distance = 2.5f;
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    [Header("高度偏移")]
    public float targetHeight = 1.2f;

    [Header("防穿墙")]
    public float collisionRadius = 0.2f;
    public float collisionOffset = 0.1f;
    public LayerMask collisionMask = ~0; // 默认检测所有层

    private float yaw;
    private float pitch;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonCamera 没有设置 target");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f) return;
        if (target == null) return;

        HandleMouseLook();
        HandleZoom();
        UpdateCameraPosition();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void UpdateCameraPosition()
    {
        // 【关键修复1】：高度偏移。不要用死板的 Vector3.up，而是用 target.up（玩家当前的头顶方向）
        Vector3 targetPos = target.position + target.up * targetHeight;

        // 【关键修复2】：旋转。将鼠标产生的偏航角(yaw)和俯仰角(pitch)，叠加到玩家当前的身体旋转上
        Quaternion localRotation = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion finalRotation = target.rotation * localRotation;

        // 沿着最终的旋转方向，向后退 distance 的距离
        Vector3 desiredDirection = finalRotation * new Vector3(0f, 0f, -distance);
        Vector3 desiredCameraPos = targetPos + desiredDirection;

        Vector3 dir = (desiredCameraPos - targetPos).normalized;
        float desiredDist = distance;
        float finalDist = desiredDist;

        // 防穿墙检测保持不变
        if (Physics.SphereCast(targetPos, collisionRadius, dir, out RaycastHit hit, desiredDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            finalDist = hit.distance - collisionOffset;
            finalDist = Mathf.Max(0.5f, finalDist);
        }

        // 应用计算好的位置
        transform.position = targetPos + dir * finalDist;

        // 【关键修复3】：看向玩家时，必须明确告诉相机现在的“上方”在哪里！
        transform.LookAt(targetPos, target.up);
    }
}