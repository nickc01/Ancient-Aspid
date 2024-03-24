using System.Collections;
using UnityEngine;

public class OffensivePhase : Phase
{
    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        if (boss.GodhomeMode)
        {
            if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
            {
                boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.UPWARDING);
            }
        }
        else
        {
            if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
            {
                boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR2);
            }
        }
        yield return boss.OffensiveMode.EnterFromBottomRoutine();
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        if (boss.GodhomeMode)
        {
            if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
            {
                boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR1);
            }
        }
        boss.OffensiveMode.OffensiveAreaProvider = null;
        if (boss.CurrentRunningMode == boss.OffensiveMode)
        {
            yield return boss.StopCurrentMode();
        }
        yield break;
    }
}
