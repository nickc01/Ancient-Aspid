using UnityEngine;
using WeaverCore;

public class PlatformOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    Vector3 platformTarget;

    Collider2D mainCollider;

    [SerializeField]
    float maxDistance = 22.5f;

    private void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
        mainCollider = GetComponent<Collider2D>();
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return platformTarget;
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        var bounds = mainCollider.bounds;
        var pos = boss.transform.position;
        pos.z = bounds.center.z;
        return bounds.Contains(pos) && Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= maxDistance;
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<OffensiveMode>(out var offensiveMode) && offensiveMode.Boss.CurrentPhase is OffensivePhase && offensiveMode.OffensiveAreaProvider != (IModeAreaProvider)this)
        {
            offensiveMode.OffensiveAreaProvider = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<OffensiveMode>(out var offensiveMode) && offensiveMode.Boss.CurrentPhase is OffensivePhase && offensiveMode.OffensiveAreaProvider == (IModeAreaProvider)this)
        {
            offensiveMode.OffensiveAreaProvider = null;
        }
    }*/
}
