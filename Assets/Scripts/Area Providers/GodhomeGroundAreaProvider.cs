using UnityEngine;
using WeaverCore;

public class GodhomeGroundAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    private float healthThreshold = 0.45f;

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
        return boss.Head.LookingDirection >= 0f
            ? (Vector2)(Player.Player1.transform.position + boss.GroundMode.lungeTargetOffset)
            : (Vector2)(Player.Player1.transform.position + new Vector3(-boss.GroundMode.lungeTargetOffset.x, boss.GroundMode.lungeTargetOffset.y, boss.GroundMode.lungeTargetOffset.z));
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f && (boss.HealthManager.Health / (float)boss.StartingHealth) <= healthThreshold;
    }

}
