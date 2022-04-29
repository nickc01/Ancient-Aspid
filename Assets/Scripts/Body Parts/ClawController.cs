using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClawController : AspidBodyPart
{
    public List<ClawAnimator> claws;

    public ClawAnimator FrontLeftClaw => claws[0];

    public ClawAnimator FrontRightClaw => claws[3];

    public IEnumerable<ClawAnimator> LeftClaws => claws.Where(c => c.transform.localPosition.x < 0f);
    public IEnumerable<ClawAnimator> RightClaws => claws.Where(c => c.transform.localPosition.x >= 0f);

    protected override void Awake()
    {
        StartCoroutine(TestRoutine());
        base.Awake();
    }
    /*private void Awake()
    {
        
    }*/

    IEnumerator TestRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            yield return DoBasicAttackNEW();
        }
    }

    public IEnumerator DoBasicAttackNEW()
    {
        if (FrontLeftClaw.ClawLocked || FrontRightClaw.ClawLocked)
        {
            yield break;
        }

        var currentFrameLeft = FrontLeftClaw.CurrentFrame;
        var currentFrameRight = FrontRightClaw.CurrentFrame;

        RoutineAwaiter lockAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.LockClaw(), FrontRightClaw.LockClaw());

        yield return lockAwaiter.WaitTillDone();

        //FrontLeftClaw.Flasher.flashWhiteQuick();
        //FrontRightClaw.Flasher.flashWhiteQuick();

        //Boss.StartBoundRoutine(FrontLeftClaw.PlayAttackAnimation());

        RoutineAwaiter attackAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.PlayAttackAnimationNEW(true), FrontRightClaw.PlayAttackAnimationNEW(false));

        yield return attackAwaiter.WaitTillDone();

        FrontLeftClaw.UnlockClaw();
        FrontRightClaw.UnlockClaw();
    }


    public IEnumerator DoBasicAttack()
    {
        if (FrontLeftClaw.ClawLocked || FrontRightClaw.ClawLocked)
        {
            yield break;
        }

        RoutineAwaiter lockAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.LockClaw(), FrontRightClaw.LockClaw());

        yield return lockAwaiter.WaitTillDone();

        //FrontLeftClaw.Flasher.flashWhiteQuick();
        //FrontRightClaw.Flasher.flashWhiteQuick();

        //Boss.StartBoundRoutine(FrontLeftClaw.PlayAttackAnimation());

        RoutineAwaiter attackAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.PlayAttackAnimation(true), FrontRightClaw.PlayAttackAnimation(false));

        yield return attackAwaiter.WaitTillDone();

        FrontLeftClaw.UnlockClaw();
        FrontRightClaw.UnlockClaw();
    }

}
