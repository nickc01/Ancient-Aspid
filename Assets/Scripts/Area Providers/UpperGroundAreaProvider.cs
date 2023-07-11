using UnityEngine;
using WeaverCore;

public class UpperGroundAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    Vector3 lungeTargetOffset;

    AncientAspid boss;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
    }

    public Vector2 GetModeTarget()
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss) && boss.GroundAreaProvider == null)
        {
            this.boss = boss;
            boss.GroundAreaProvider = this;
            boss.OffensiveAreaProvider = new DefaultOffensiveAreaProvider(boss);
            WeaverLog.Log("BOSS ENTERING UPPER AREA");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss) && boss.GroundAreaProvider == (IModeAreaProvider)this)
        {
            boss.GroundAreaProvider = null;
            boss.OffensiveAreaProvider = null;
            WeaverLog.Log("BOSS LEAVING UPPER AREA");
        }
    }
}
