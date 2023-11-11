using UnityEngine;
using WeaverCore;
using WeaverCore.Features;

public class DefaultGroundAreaProvider : IModeAreaProvider
{
    Vector3 lungeTargetOffset;

    public DefaultGroundAreaProvider(Vector3 lungeTargetOffset)
    {
        this.lungeTargetOffset = lungeTargetOffset;
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

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f;
    }
}
