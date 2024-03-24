using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

public class TacticalMode : AncientAspidMode
{
    public const string TACTICAL_TIME = nameof(TACTICAL_TIME);
    public const string DO_FLY_AWAY = nameof(DO_FLY_AWAY);
    private bool stop = false;



    protected override IEnumerator OnExecute(Dictionary<string, object> args)
    {
        if (Boss.GodhomeMode && Boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            Boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR1);
        }
        Debug.Log("TACTICAL START");
        stop = false;

        float lastMoveTime = Time.time;
        float lastMoveDelay = 0f;

        yield return CoroutineUtilities.WaitForTimeOrPredicate(0.5f, () => stop);

         args.TryGetValueOfType(TACTICAL_TIME, out float tacticalTime, 6);
         args.TryGetValueOfType(DO_FLY_AWAY, out bool doFlyAway, false);

        float endTime = Time.time + tacticalTime;

        while (Time.time < endTime && !stop)
        {
            yield return new WaitUntil(() => !Boss.Head.HeadLocked);
            yield return Boss.UpdateDirection();

            if (doFlyAway)
            {
                break;
            }

             Boss.MoveRandomizer.MoveNext();

            AncientAspidMove move = Boss.MoveRandomizer.Current;

            if (move == null)
            {
                continue;
            }

            var oldHealth = Boss.HealthManager.Health;

            while (Time.time - lastMoveTime < move.PreDelay + lastMoveDelay)
            {
                if (Boss.HealthManager.Health != oldHealth)
                {
                    oldHealth = Boss.HealthManager.Health;
                    lastMoveDelay *= 0.8f;
                }
                yield return Boss.UpdateDirection();
            }

            oldHealth = Boss.HealthManager.Health;
            Debug.Log("RUNNING ASPID MOVE = " + move.GetType().Name);
            yield return RunAspidMove(move, args);
            lastMoveTime = Time.time;
            lastMoveDelay = move.GetPostDelay(oldHealth);
        }

        yield break;
    }

    public override void Stop()
    {
        stop = true;
        StopCurrentMove();
    }

    protected override bool ModeEnabled(Dictionary<string, object> args)
    {
        return true;
    }
}
