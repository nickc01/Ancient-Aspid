using UnityEngine;
using WeaverCore;

public class GodhomeGroundAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    float healthThreshold = 0.45f;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
    }

    private void Start()
    {
        GameObject.FindObjectOfType<AncientAspid>().GroundMode.GroundAreaProvider = this;
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        if (boss.Head.LookingDirection >= 0f)
        {
            return Player.Player1.transform.position + boss.GroundMode.lungeTargetOffset;
        }
        else
        {
            return Player.Player1.transform.position + new Vector3(-boss.GroundMode.lungeTargetOffset.x, boss.GroundMode.lungeTargetOffset.y, boss.GroundMode.lungeTargetOffset.z);
        }
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f && (boss.HealthManager.Health / (float)boss.StartingHealth) <= healthThreshold;
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss))
        {
            boss.GroundMode.GroundAreaProvider = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss))
        {
            if (boss.GroundMode.GroundAreaProvider != null && boss.GroundMode.GroundAreaProvider.Equals(this))
            {
                boss.GroundMode.GroundAreaProvider = null;
            }
        }
    }*/
}
