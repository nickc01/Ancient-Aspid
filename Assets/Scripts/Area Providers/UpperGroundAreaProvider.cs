using UnityEngine;
using WeaverCore;

public class UpperGroundAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    private Vector3 lungeTargetOffset;

    [SerializeField]
    private float maxDistance = 30f;
    private Collider2D mainCollider;

    private void Awake()
    {
        mainCollider = GetComponent<Collider2D>();
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return boss.Head.LookingDirection >= 0f
            ? (Vector2)(Player.Player1.transform.position + lungeTargetOffset)
            : (Vector2)(Player.Player1.transform.position + new Vector3(-lungeTargetOffset.x, lungeTargetOffset.y, lungeTargetOffset.z));
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        Bounds bounds = mainCollider.bounds;
        Vector3 pos = boss.transform.position;
        pos.z = bounds.center.z;
        return bounds.Contains(pos) && Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= maxDistance;
    }
}
