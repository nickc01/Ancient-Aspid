using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class GodhomeOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    [SerializeField]
    float healthThreshold = 0.7f;

    //[SerializeField]
    //Vector3 platformTarget;

    [SerializeField]
    Rect platformArea;

    //409.84, 14.98, 0

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
    }

    private void Start()
    {
        GameObject.FindObjectOfType<AncientAspid>().OffensiveMode.OffensiveAreaProvider = this;
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return platformArea.ClampWithin(Player.Player1.transform.position);
        //return platformTarget;
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f && (boss.HealthManager.Health / (float)boss.StartingHealth) <= healthThreshold;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f,0.5f,0.5f,0.5f);
        Gizmos.DrawCube(platformArea.center, new Vector3(platformArea.size.x,platformArea.size.y,0.1f));
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss))
        {
            boss.OffensiveMode.OffensiveAreaProvider = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<AncientAspid>(out var boss))
        {
            if (boss.OffensiveMode.OffensiveAreaProvider != null && boss.OffensiveMode.OffensiveAreaProvider.Equals(this))
            {
                boss.OffensiveMode.OffensiveAreaProvider = null;
            }
        }
    }*/
}
