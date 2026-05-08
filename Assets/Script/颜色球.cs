using UnityEngine;

public class ColorPickup : MonoBehaviour
{
    public EndpointColorType pickupColor;

    private void OnTriggerEnter(Collider other)
    {
        PlayerColorController playerColor = other.GetComponent<PlayerColorController>();
        if (playerColor == null)
            return;

        playerColor.AddColor(pickupColor);

        Destroy(gameObject);
    }
}