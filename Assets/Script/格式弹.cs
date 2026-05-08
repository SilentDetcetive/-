using UnityEngine;

public class FormatBullet : MonoBehaviour
{
    [Header("格式弹设置")]
    public float speed = 8f;
    public float lifeTime = 3f;
    [Tooltip("格式弹打中敌人后，强制敌人虚化的时间")]
    public float virtualizedDuration = 6f;

    private Vector3 moveDirection;

    public void Init(Vector3 direction)
    {
        moveDirection = direction.normalized;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 尝试获取被击中物体身上的 PatrolEnemy (通用巡逻敌人) 脚本
        PatrolEnemy enemy = other.GetComponent<PatrolEnemy>();
        if (enemy == null)
        {
            enemy = other.GetComponentInParent<PatrolEnemy>();
        }

        // 如果打中了敌人，直接让它虚化，然后子弹销毁！
        if (enemy != null)
        {
            enemy.ApplyVirtualized(virtualizedDuration);
            Destroy(gameObject);
            return;
        }

        // 如果打到了普通墙体（排除了隐形触发器），子弹直接销毁
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}