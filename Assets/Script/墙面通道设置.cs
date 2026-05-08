using UnityEngine;

public enum WallWalkPortalType
{
    Ground,
    Wall
}

public enum WallFaceDirection
{
    PositiveX,
    NegativeX,
    PositiveZ,
    NegativeZ
}

public class WallWalkPortal : MonoBehaviour
{
    [Header("ID Pair")]
    public int portalId = 1;

    [Header("Portal Type")]
    public WallWalkPortalType portalType = WallWalkPortalType.Ground;

    [Header("Wall Direction: player appears on this side")]
    public WallFaceDirection wallFaceDirection = WallFaceDirection.PositiveZ;

    [Header("Teleport Offset")]
    public float offsetFromSurface = 0.06f;

    public Vector3 GetWallNormal()
    {
        switch (wallFaceDirection)
        {
            case WallFaceDirection.PositiveX:
                return Vector3.right;
            case WallFaceDirection.NegativeX:
                return Vector3.left;
            case WallFaceDirection.PositiveZ:
                return Vector3.forward;
            case WallFaceDirection.NegativeZ:
                return Vector3.back;
        }

        return Vector3.forward;
    }

    public Vector3 GetTargetPosition(Collider playerCollider)
    {
        Collider portalCollider = GetComponent<Collider>();

        if (playerCollider == null || portalCollider == null)
            return transform.position;

        Bounds portalBounds = portalCollider.bounds;
        Vector3 playerExtents = playerCollider.bounds.extents;

        if (portalType == WallWalkPortalType.Ground)
        {
            return new Vector3(
                transform.position.x,
                portalBounds.max.y + playerExtents.y + offsetFromSurface,
                transform.position.z
            );
        }

        Vector3 normal = GetWallNormal().normalized;
        Vector3 target = transform.position;

        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.z))
        {
            float surfaceX = normal.x > 0 ? portalBounds.max.x : portalBounds.min.x;

            target.x = surfaceX + normal.x * (playerExtents.x + offsetFromSurface);
            target.y = transform.position.y;
            target.z = transform.position.z;
        }
        else
        {
            float surfaceZ = normal.z > 0 ? portalBounds.max.z : portalBounds.min.z;

            target.z = surfaceZ + normal.z * (playerExtents.z + offsetFromSurface);
            target.x = transform.position.x;
            target.y = transform.position.y;
        }

        return target;
    }
}