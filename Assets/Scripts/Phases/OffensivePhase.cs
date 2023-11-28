using System.Collections;
using UnityEngine;

public class OffensivePhase : Phase
{
    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        yield return boss.OffensiveMode.EnterFromBottomRoutine();
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        boss.OffensiveMode.OffensiveAreaProvider = null;
        if (boss.CurrentRunningMode == boss.OffensiveMode)
        {
            yield return boss.StopCurrentMode();
        }
        yield break;
    }
}
