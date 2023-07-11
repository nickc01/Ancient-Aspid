using UnityEngine;
using WeaverCore;
using WeaverCore.Features;

public class DefaultGroundAreaProvider : IModeAreaProvider
{
    AncientAspid boss;

    Vector3 lungeTargetOffset;

    public DefaultGroundAreaProvider(AncientAspid boss, Vector3 lungeTargetOffset)
    {
        this.boss = boss;
        this.lungeTargetOffset = lungeTargetOffset;
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
}
