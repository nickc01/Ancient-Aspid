using System.Collections;
using UnityEngine;

public class ClimbPhase : Phase
{
    [SerializeField]
    int healthIncrease = 32 * 4;

    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.UPWARDING);
        }
        boss.HealthManager.Health += healthIncrease;
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        yield break;
    }
}
