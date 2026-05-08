using UnityEngine;

public class ColorTeleporter : MonoBehaviour
{
    public EndpointColorType teleporterColor;

    public Vector3 GetTeleportPoint()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.z = Mathf.Floor(pos.z) + 0.5f;
        return pos;
    }
}