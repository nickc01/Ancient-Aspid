using UnityEngine;
using WeaverCore;

public class DefaultGroundAreaProvider : IModeAreaProvider
{
    private Vector3 lungeTargetOffset;

    public DefaultGroundAreaProvider(Vector3 lungeTargetOffset)
    {
        this.lungeTargetOffset = lungeTargetOffset;
    }

    public Vector2 GetModeTarget(AncientAspid boss)
    {
        return boss.Head.LookingDirection >= 0f
            ? (Vector2)(Player.Player1.transform.position + lungeTargetOffset)
            : (Vector2)(Player.Player1.transform.position + new Vector3(-lungeTargetOffset.x, lungeTargetOffset.y, lungeTargetOffset.z));
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f;
    }
}
