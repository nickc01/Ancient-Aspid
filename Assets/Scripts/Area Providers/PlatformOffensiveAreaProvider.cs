using UnityEngine;

public class PlatformOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    Vector3 platformTarget;

    public Vector2 GetModeTarget()
    {
        return platformTarget;
    }

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss) && boss.OffensiveAreaProvider != (IModeAreaProvider)this)
        {
            boss.OffensiveAreaProvider = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss) && boss.OffensiveAreaProvider == (IModeAreaProvider)this)
        {
            boss.OffensiveAreaProvider = null;
        }
    }
}
