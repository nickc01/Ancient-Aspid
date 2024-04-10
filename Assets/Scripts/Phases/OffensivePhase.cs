using System.Collections;
using UnityEngine;
using WeaverCore;

public class OffensivePhase : Phase
{
    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR2);
        }
        /*if (boss.GodhomeMode)
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
        }*/
        //WeaverLog.Log("BEGINNING OFFENSIVE PHASE");
        yield return boss.OffensiveMode.EnterFromBottomRoutine();
        //WeaverLog.Log("END BEGINNING OFFENSIVE PHASE");
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR1);
        }

        /*if (boss.GodhomeMode)
        {
            if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
            {
                boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR1);
            }
        }*/
        boss.OffensiveMode.OffensiveAreaProvider = null;
        while (boss.CurrentRunningMode == boss.OffensiveMode)
        {
            yield return boss.StopCurrentMode();
        }
        //WeaverLog.Log("CURRENT MODE = " + boss.CurrentRunningMode);
        yield break;
    }
}
