using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class GodhomeOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    private float healthThreshold = 0.7f;

    [SerializeField]
    private Rect platformArea;

    [SerializeField]
    float cameraLockAreaHeightOverride = 14.52f;

    [SerializeField]
    Vector2 playerTargetOffset = new Vector2(0f, 0f);

    [SerializeField]
    AncientAspid boss;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
    }

    private void Start()
    {
        if (boss == null)
        {
            boss = GameObject.FindObjectOfType<AncientAspid>(true);
        }

        boss.OffensiveMode.OffensiveAreaProvider = this;
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return platformArea.ClampWithin(Player.Player1.transform.position + (Vector3)playerTargetOffset);
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f && (boss.HealthManager.Health / (float)boss.StartingHealth) <= healthThreshold;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawCube(platformArea.center, new Vector3(platformArea.size.x, platformArea.size.y, 0.1f));
    }

    public Vector2 GetLockAreaOverride(Vector2 oldPos, out bool clampWithinArea)
    {
        clampWithinArea = false;
        return oldPos.With(y: cameraLockAreaHeightOverride);
    }
}
