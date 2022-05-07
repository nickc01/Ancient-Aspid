using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;

public class ClawController : AspidBodyPart
{
    public List<ClawAnimator> claws;

    public ClawAnimator FrontLeftClaw => claws[0];

    public ClawAnimator FrontRightClaw => claws[3];

    public IEnumerable<ClawAnimator> LeftClaws => claws.Where(c => c.transform.localPosition.x < 0f);
    public IEnumerable<ClawAnimator> RightClaws => claws.Where(c => c.transform.localPosition.x >= 0f);

    [Header("Mantis Attack")]
    [SerializeField]
    AudioClip mantisPrepareSound;

    [SerializeField]
    float mantisPreparePitch = 1f;

    [SerializeField]
    List<AudioClip> mantisShootSounds;

    [SerializeField]
    float swingDistance = 3f;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(TestRoutine());
    }

    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.Lerp(Color.magenta,default,0.5f);
        Gizmos.DrawSphere(transform.parent.position, swingDistance);
    }*/
    /*private void Awake()
    {
        
    }*/

    IEnumerator TestRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            if (Vector2.Distance(Boss.Head.transform.position,Player.Player1.transform.position) <= swingDistance && !FrontLeftClaw.ClawLocked && !FrontRightClaw.ClawLocked)
            {
                yield return DoBasicAttackNEW();
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
            //yield return new WaitForSeconds(2f);
            //DoBasicAttackNEW();
            //yield return DoMantisShots();
        }
    }

    public IEnumerator DoMantisShots(Action onSwing)
    {
        void OnAttack()
        {
            foreach (var clip in mantisShootSounds)
            {
                WeaverAudio.PlayAtPoint(clip, Boss.Head.transform.position);
            }
            onSwing?.Invoke();
        }

        yield return RoutineAwaiter.AwaitBoundRoutines(claws.Select(c => c.LockClaw()), Boss).WaitTillDone();

        List<IEnumerator> AttackRoutines = new List<IEnumerator>();
        for (int i = 0; i < claws.Count; i++)
        {
            if (i == 0)
            {
                AttackRoutines.Add(claws[i].PlayAttackAnimation(false,false,false, OnAttack));
            }
            else
            {
                AttackRoutines.Add(claws[i].PlayAttackAnimation(false, false, false));
            }
        }

        var prepAudio = WeaverAudio.PlayAtPoint(mantisPrepareSound, Boss.Head.transform.position);
        prepAudio.AudioSource.pitch = mantisPreparePitch;

        yield return RoutineAwaiter.AwaitBoundRoutines(AttackRoutines, Boss).WaitTillDone();

        foreach (var claw in claws)
        {
            claw.UnlockClaw();
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

        RoutineAwaiter attackAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.PlayAttackAnimation(true,true,true), FrontRightClaw.PlayAttackAnimation(false,false,true));

        yield return attackAwaiter.WaitTillDone();

        FrontLeftClaw.UnlockClaw();
        FrontRightClaw.UnlockClaw();
    }


    /*public IEnumerator DoBasicAttack()
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
    }*/

}
