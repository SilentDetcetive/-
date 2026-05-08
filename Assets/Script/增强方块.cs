using UnityEngine;

public class LevelPickup : MonoBehaviour
{
    [Range(1, 3)]
    public int pickupLevel = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerLevelController playerLevel = other.GetComponent<PlayerLevelController>();
        if (playerLevel == null)
            return;

        playerLevel.UpgradeToLevel(pickupLevel);
        Destroy(gameObject);
    }
}