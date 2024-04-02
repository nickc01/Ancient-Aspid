using System.Collections;
using UnityEngine;
using WeaverCore.Implementations;

public class GodhomePhase : Phase
{
    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        var bounds = GetComponent<Collider2D>().bounds;
        var rect = new Rect(bounds.center, bounds.size);
        rect.center = bounds.center;
        boss.FlightRange = rect;

        if (boss.GodhomeMode)
        {
            boss.HealthManager.AddHealthMilestone(Mathf.RoundToInt(boss.StartingHealth * 0.7f), () =>
            {
                boss.GetComponent<LaserRapidFireMove>().EnableMove(true);
            });

        }

        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        yield break;
    }
}
