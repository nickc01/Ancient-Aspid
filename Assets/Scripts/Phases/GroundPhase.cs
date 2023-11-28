using System.Collections;
using UnityEngine;

public class GroundPhase : Phase
{
    [SerializeField]
    float groundMinFlightHeight = 2f;


    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        boss.FlightRange = PhaseBoundaries;
        boss.FlightRange.yMax = 9999f;
        boss.FlightRange.yMin = groundMinFlightHeight;
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        yield break;
    }
}