using UnityEngine;
using WeaverCore;

public class PlatformOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    private Vector3 platformTarget;
    private Collider2D mainCollider;

    [SerializeField]
    private float maxDistance = 22.5f;

    private void Awake()
    {
        mainCollider = GetComponent<Collider2D>();
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return platformTarget;
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        Bounds bounds = mainCollider.bounds;
        Vector3 pos = boss.transform.position;
        pos.z = bounds.center.z;
        return bounds.Contains(pos) && Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= maxDistance;
    }

}
