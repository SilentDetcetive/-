using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColorGateSingle : MonoBehaviour
{
    public EndpointColorType requiredColor;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public bool CanPass(PlayerColorController playerColor)
    {
        if (playerColor == null)
            return false;

        return playerColor.IsCurrentColor(requiredColor);
    }
}