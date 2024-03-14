using System.Collections;

public class ClimbPhase : Phase
{
    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        if (boss.MusicPlayer != null)
        {
            boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.UPWARDING);
        }
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        yield break;
    }
}
