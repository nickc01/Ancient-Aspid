using UnityEngine;
using WeaverCore;
using WeaverCore.Features;

public class UpperGroundAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    Vector3 lungeTargetOffset;

    [SerializeField]
    float maxDistance = 30f;

    Collider2D mainCollider;

    private void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
        mainCollider = GetComponent<Collider2D>();
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        if (boss.Head.LookingDirection >= 0f)
        {
            return Player.Player1.transform.position + lungeTargetOffset;
        }
        else
        {
            return Player.Player1.transform.position + new Vector3(-lungeTargetOffset.x, lungeTargetOffset.y, lungeTargetOffset.z);
        }
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss))
        {
            this.boss = boss;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss))
        {
            boss = null;
        }
    }*/

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss) && boss.GroundMode.GroundAreaProvider == null)
        {
            this.boss = boss;
            boss.GroundMode.GroundAreaProvider = this;
            boss.OffensiveMode.OffensiveAreaProvider = new DefaultOffensiveAreaProvider(boss, boss.OffensiveMode.OffensiveHeight);
            WeaverLog.Log("BOSS ENTERING UPPER AREA");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss) && boss.GroundMode.GroundAreaProvider == (IModeAreaProvider)this)
        {
            boss.GroundMode.GroundAreaProvider = null;
            boss.OffensiveMode.OffensiveAreaProvider = null;
            WeaverLog.Log("BOSS LEAVING UPPER AREA");
        }
    }*/

    public bool IsTargetActive(AncientAspid boss)
    {
        var bounds = mainCollider.bounds;
        var pos = boss.transform.position;
        pos.z = bounds.center.z;
        return bounds.Contains(pos) && Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= maxDistance;
    }
}
