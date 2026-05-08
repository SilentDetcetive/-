using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelBarrier : MonoBehaviour
{
    [Range(1, 3)]
    public int barrierLevel = 1;

    private Collider barrierCollider;

    private void Awake()
    {
        barrierCollider = GetComponent<Collider>();
        barrierCollider.isTrigger = true;
    }

    public bool CanPass(PlayerLevelController playerLevel)
    {
        if (playerLevel == null)
            return false;

        return playerLevel.CanPassLevelBarrier(barrierLevel);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerLevelController playerLevel = other.GetComponent<PlayerLevelController>();
        if (playerLevel == null)
            return;

        // 只有等级足够时，障碍才会消失
        if (CanPass(playerLevel))
        {
            Destroy(gameObject);
        }
    }
}