using System.Collections;
using UnityEngine;

public class GodhomePhase : Phase
{
    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        var bounds = GetComponent<Collider2D>().bounds;
        var rect = new Rect(bounds.center, bounds.size);
        rect.center = bounds.center;
        //Debug.Log("RECT = " + rect);
        boss.FlightRange = rect;
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        yield break;
    }
}
