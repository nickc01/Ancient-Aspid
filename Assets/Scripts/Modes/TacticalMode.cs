using System.Collections;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore;
using WeaverCore.Utilities;
using System.Collections.Generic;

public class TacticalMode : AncientAspidMode
{
    public const string TACTICAL_TIME = nameof(TACTICAL_TIME);
    public const string DO_FLY_AWAY = nameof(DO_FLY_AWAY);
    //public override bool ModeEnabled => true;

    bool stop = false;



    protected override IEnumerator OnExecute(Dictionary<string, object> args)
    {
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
            Debug.Log("STARTING LOOP");
            yield return new WaitUntil(() => !Boss.Head.HeadLocked);
            yield return Boss.UpdateDirection();

            if (doFlyAway)
            {
                Debug.Log("EXIT A");
                break;
            }

            Boss.MoveRandomizer.MoveNext();

            var move = Boss.MoveRandomizer.Current;

            if (move == null)
            {
                Debug.Log("EXIT B");
                continue;
            }

            while (Time.time - lastMoveTime < move.PreDelay + lastMoveDelay)
            {
                yield return Boss.UpdateDirection();
            }

            /*if (Boss.AllMovesDisabled)
            {
                continue;
            }*/

            var oldHealth = Boss.HealthManager.Health;
            Debug.Log("RUNNING ASPID MOVE = " + move.GetType().Name);
            yield return RunAspidMove(move, args);
            //yield return Boss.RunMoveWhile(move, () => !stop);
            //yield return RunMoveWhile(move, () => !stop);

            //lastMoveTime = Time.time + move.GetPostDelay(oldHealth);
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
