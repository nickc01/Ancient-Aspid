using System.Collections;
using UnityEngine;

public class FinalClimbPhase : Phase
{
    [SerializeField]
    Transform leftSideTarget;

    [SerializeField]
    Transform rightSideTarget;

    [SerializeField]
    int targetPriority = 0;

    [SerializeField]
    float minBossHeight = 200;

    public Transform SelectTarget(Vector3 bossPos)
    {
        if (leftSideTarget == null && rightSideTarget == null)
        {
            return null;
        }
        else if (leftSideTarget != null && rightSideTarget != null)
        {
            if (Vector2.Distance(leftSideTarget.position,bossPos) < Vector2.Distance(rightSideTarget.position, bossPos))
            {
                return leftSideTarget;
            }
            else
            {
                return rightSideTarget;
            }
        }
        else
        {
            if (leftSideTarget != null)
            {
                return leftSideTarget;
            }
            else
            {
                return rightSideTarget;
            }
        }
    }

    TargetOverride targetOverride;

    public override bool CanGoToNextPhase(AncientAspid boss)
    {
        return base.CanGoToNextPhase(boss) && boss.transform.position.y >= minBossHeight;
    }

    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        var foundTarget = SelectTarget(boss.transform.position);
        if (foundTarget != null)
        {
            targetOverride = boss.AddTargetOverride(targetPriority);
            targetOverride.SetTarget(foundTarget);
        }
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        if (targetOverride != null)
        {
            boss.RemoveTargetOverride(targetOverride);
            targetOverride = null;
        }
        yield break;
    }
}
