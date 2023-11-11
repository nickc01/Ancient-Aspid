using UnityEngine;
using WeaverCore;

public class UpperOffensiveAreaProvider : MonoBehaviour, IModeAreaProvider
{
    static RaycastHit2D[] hitCache = new RaycastHit2D[1];

    //[SerializeField]
    //Vector3 lungeTargetOffset;

    Collider2D mainCollider;

    [field: SerializeField]
    public float OffensiveY { get; private set; } = 288.3985f;

    [SerializeField]
    float maxDistance = 20f;

    private void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Enemy Detector");
        mainCollider = GetComponent<Collider2D>();
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        /*if (boss.Head.LookingDirection >= 0f)
        {
            return Player.Player1.transform.position + lungeTargetOffset;
        }
        else
        {
            return Player.Player1.transform.position + new Vector3(-lungeTargetOffset.x, lungeTargetOffset.y, lungeTargetOffset.z);
        }*/

        //var rect = RoomScanner.GetRoomBoundaries(Player.Player1.transform.position);
        var bounds = mainCollider.bounds;

        var targetX = boss.transform.position.x;

        if (targetX > bounds.max.x - 7f)
        {
            targetX = bounds.max.x - 7f;
        }

        if (targetX < bounds.min.x + 7f)
        {
            targetX = bounds.min.x + 7f;
        }

        //float floor = boss.Head.transform.position.y - 10f;

        //Debug.Log("GETTING OFFENSIVE TARGET");

        /*if (Physics2D.RaycastNonAlloc(boss.Head.transform.position,Vector2.down, hitCache,10f,LayerMask.GetMask("Terrain")) > 0)
        {
            floor = hitCache[0].point.y;
            Debug.Log("FLOOR HIT POINT = " + floor);
        }
        else
        {
            Debug.Log("NOT FLOOR HIT POINT = " + floor);
        }*/


        //Debug.Log("FLOOR HEIGHT = " + floor);

        //Mathf.Lerp(rect.Rect.xMin, rect.Rect.xMax, 0.5f)
        var newTarget = new Vector3(targetX, OffensiveY);

        //Debug.Log("FINAL OFFENSIVE TARGET = " + newTarget);

        //newTarget.x = Mathf.Clamp(newTarget.x, Player.Player1.transform.position.x - 10f, Player.Player1.transform.position.x + 10f);
        //newTarget.y = Mathf.Clamp(newTarget.y, Player.Player1.transform.position.y + 4f, Player.Player1.transform.position.y + 20f);

        return newTarget;
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

    static bool IsInBounds(Bounds bounds, Vector2 pos2D)
    {
        Vector3 pos = pos2D;
        pos.z = bounds.center.z;
        return bounds.Contains(pos);
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        var bounds = mainCollider.bounds;
        return IsInBounds(bounds, boss.transform.position) && IsInBounds(bounds, Player.Player1.transform.position) && Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= maxDistance;
    }
}
