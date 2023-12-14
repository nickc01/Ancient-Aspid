using UnityEngine;
using WeaverCore;

public class UpperOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    private Collider2D mainCollider;

    [field: SerializeField]
    public float OffensiveY { get; private set; } = 288.3985f;

    [SerializeField]
    private float maxDistance = 20f;

    private void Awake()
    {
        mainCollider = GetComponent<Collider2D>();
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        Bounds bounds = mainCollider.bounds;

        float targetX = boss.transform.position.x;

        if (targetX > bounds.max.x - 7f)
        {
            targetX = bounds.max.x - 7f;
        }

        if (targetX < bounds.min.x + 7f)
        {
            targetX = bounds.min.x + 7f;
        }


        Vector3 newTarget = new Vector2(targetX, OffensiveY);

        return newTarget;
    }

    private static bool IsInBounds(Bounds bounds, Vector2 pos2D)
    {
        Vector3 pos = pos2D;
        pos.z = bounds.center.z;
        return bounds.Contains(pos);
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        Bounds bounds = mainCollider.bounds;
        return IsInBounds(bounds, boss.transform.position) && IsInBounds(bounds, Player.Player1.transform.position) && Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= maxDistance;
    }

    public Vector2 GetLockAreaOverride(Vector2 oldPos, out bool clampWithinArea)
    {
        clampWithinArea = true;
        return oldPos;
    }
}
