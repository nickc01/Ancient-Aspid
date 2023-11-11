using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;


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

    //bool allowSwingAttack = true;
    Coroutine attackRoutine = null;

    bool attacking = false;

    public bool SwingAttackEnabled { get; private set; }

    [SerializeField]
    Vector2 shakeAmount;

    [SerializeField]
    float shakeRate = 1f / 30f;

    uint shakeRoutineID = 0;

    public bool DoingBasicAttack { get; private set; }


    [Header("Ground Jump")]
    [SerializeField]
    Transform rightClawsOrigin;

    [SerializeField]
    Transform leftClawsOrigin;

    [SerializeField]
    Sprite clawMidAirSprite;

    /*[SerializeField]
    PosAndRot groundJumpPrepareIncrement;

    [SerializeField]
    int groundJumpFrames = 3;

    [SerializeField]
    float groundJumpPrepareFPS = 8;

    [SerializeField]
    float groundJumpLaunchFPS = 16;

    [SerializeField]
    float groundJumpLandFPS = 16;*/





    /*public bool AllowSwingAttack
    {
        get => allowSwingAttack;
        set
        {
            if (allowSwingAttack != value)
            {
                allowSwingAttack = value;
                if (allowSwingAttack)
                {
                    attackRoutine = StartCoroutine(AttackRoutine());
                }
                else
                {
                    if (attackRoutine != null)
                    {
                        StopCoroutine(attackRoutine);
                        attackRoutine = null;
                    }
                }
            }
        }
    }*/

    Vector3 leftClawOriginBase;
    Vector3 rightClawOriginBase;

    protected override void Awake()
    {
        leftClawOriginBase = leftClawsOrigin.localPosition;
        rightClawOriginBase = rightClawsOrigin.localPosition;
        base.Awake();
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    public IEnumerator EnableSwingAttack(bool enabled)
    {
        if (enabled != SwingAttackEnabled)
        {
            if (attacking)
            {
                yield return new WaitUntil(() => !attacking);
            }
            SwingAttackEnabled = enabled;
            if (SwingAttackEnabled)
            {
                attackRoutine = StartCoroutine(AttackRoutine());
            }
            else
            {
                if (attackRoutine != null)
                {
                    StopCoroutine(attackRoutine);
                    attackRoutine = null;
                }
            }
        }
    }

    public void DisableSwingAttackImmediate()
    {
        SwingAttackEnabled = false;
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
            attacking = false;
            DoingBasicAttack = false;
            if (FrontLeftClaw.ClawLocked)
            {
                FrontLeftClaw.UnlockClaw();
            }

            if (FrontRightClaw.ClawLocked)
            {
                FrontRightClaw.UnlockClaw();
            }
        }
    }

    public bool OnGround
    {
        get => claws[0].OnGround;
        set
        {
            if (claws[0].OnGround != value)
            {
                foreach (var claw in claws)
                {
                    claw.OnGround = value;
                }

                if (value)
                {
                    leftClawsOrigin.localPosition = leftClawOriginBase;
                    rightClawsOrigin.localPosition = rightClawOriginBase;
                }
            }
        }
    }


    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.Lerp(Color.magenta,default,0.5f);
        Gizmos.DrawSphere(transform.parent.position, swingDistance);
    }*/
    /*private void Awake()
    {
        
    }*/

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (Boss.TrailerMode)
        {
            yield break;
        }
        while (true)
        {
            if (Vector2.Distance(Boss.Head.transform.position,Player.Player1.transform.position) <= swingDistance && !FrontLeftClaw.ClawLocked && !FrontRightClaw.ClawLocked)
            {
                DoingBasicAttack = true;
                yield return DoBasicAttack();
                DoingBasicAttack = false;
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

        if (claws.Any(c => c.ClawLocked))
        {
            yield break;
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

    public IEnumerator DoBasicAttack()
    {
        attacking = true;
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

        attacking = false;
    }

    public void PrepareForLunge()
    {
        foreach (var claw in claws)
        {
            claw.Animator.PlayAnimation("Lunge Antic");
        }
    }

    public void DoLunge()
    {
        foreach (var claw in claws)
        {
            claw.Animator.PlayAnimation("Lunge");
        }
    }

    /*public void ExitGroundMode()
    {
        foreach (var claw in claws)
        {
            claw.ExitGroundMode();
        }
    }*/

    public IEnumerator PlayLanding(bool slide)
    {
        /*if (Boss.Orientation == AspidOrientation.Right)
        {
            transform.SetXLocalScale(-1f);
        }
        else
        {
            transform.SetXLocalScale(1f);
        }*/
        if (slide)
        {
            shakeRoutineID = Boss.StartBoundRoutine(ShakeRoutine());
        }

        List<IEnumerator> awaitables = new List<IEnumerator>();
        foreach (var claw in claws)
        {
            awaitables.Add(claw.PlayLanding(slide));
        }
        var awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, awaitables.ToArray());

        yield return awaiter.WaitTillDone();

    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        if (shakeRoutineID != 0)
        {
            Boss.StopBoundRoutine(shakeRoutineID);
            shakeRoutineID = 0;
        }
        transform.localPosition = transform.localPosition.With(x: 0f, y: 0f);

        List<IEnumerator> awaitables = new List<IEnumerator>();
        foreach (var claw in claws)
        {
            awaitables.Add(claw.FinishLanding(slammedIntoWall));
        }
        var awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, awaitables.ToArray());

        yield return awaiter.WaitTillDone();
    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        List<IEnumerator> awaitables = new List<IEnumerator>();
        foreach (var claw in claws)
        {
            awaitables.Add(claw.SlideSwitchDirection(oldDirection, newDirection));
        }
        var awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, awaitables.ToArray());

        yield return awaiter.WaitTillDone();
    }

    IEnumerator ShakeRoutine()
    {
        var parentBody = transform.parent.GetComponent<Rigidbody2D>();
        while (true)
        {
            var newPos = UnityEngine.Random.insideUnitCircle * shakeAmount;
            newPos *= new Vector3(Mathf.Clamp01(Mathf.Abs(parentBody.velocity.x / 3)), Mathf.Clamp01(Mathf.Abs(parentBody.velocity.y / 3)));
            transform.localPosition = transform.localPosition.With(x: newPos.x,y: newPos.y);
            yield return new WaitForSeconds(shakeRate);
        }
    }

    public override void OnStun()
    {
        foreach (var claw in claws)
        {
            claw.OnStun();
        }
        if (shakeRoutineID != 0)
        {
            Boss.StopBoundRoutine(shakeRoutineID);
            shakeRoutineID = 0;
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        transform.localPosition = transform.localPosition.With(x: 0f,y: 0f);

        leftClawsOrigin.localPosition = leftClawOriginBase;
        rightClawsOrigin.localPosition = rightClawOriginBase;
    }

    public IEnumerator GroundPrepareJump()
    {
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            leftClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements;
            //leftClawsOrigin.transform.localEulerAngles -= Boss.jumpRotIncrements;
            leftClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements;

            rightClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements * 1.5f;
            rightClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements;
            //rightClawsOrigin.transform.localEulerAngles += Boss.jumpRotIncrements;
            yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            leftClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
            //leftClawsOrigin.transform.localEulerAngles += Boss.jumpRotIncrements;
            leftClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;

            rightClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements * 1.5f;
            //rightClawsOrigin.transform.localEulerAngles -= Boss.jumpRotIncrements;
            rightClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;
            if (i != Boss.GroundMode.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
        }

        foreach (var claw in claws)
        {
            claw.MainRenderer.sprite = clawMidAirSprite;
        }

        yield break;
    }

    public IEnumerator GroundLand(bool finalLanding)
    {
        foreach (var claw in claws)
        {
            claw.UpdateGroundSprite();
        }

        leftClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements * Boss.GroundMode.groundJumpFrames;
        //leftClawsOrigin.transform.localEulerAngles -= Boss.jumpRotIncrements * Boss.groundJumpFrames;
        leftClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements * Boss.GroundMode.groundJumpFrames;

        rightClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements * 1.5f * Boss.GroundMode.groundJumpFrames;
        //rightClawsOrigin.transform.localEulerAngles += Boss.jumpRotIncrements * Boss.groundJumpFrames;
        rightClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements * Boss.GroundMode.groundJumpFrames;

        yield return new WaitForSeconds(Boss.GroundMode.groundJumpLandDelay);

        if (finalLanding)
        {
            for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
            {
                leftClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
                //leftClawsOrigin.transform.localEulerAngles += Boss.jumpRotIncrements;
                leftClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;

                rightClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements * 1.5f;
                //rightClawsOrigin.transform.localEulerAngles -= Boss.jumpRotIncrements;
                rightClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
            yield break;
        }
    }

    public override IEnumerator WaitTillChangeDirectionMidJump()
    {
        yield break;
    }

    public override IEnumerator MidJumpChangeDirection(AspidOrientation oldOrientation, AspidOrientation newOrientation)
    {
        yield break;
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
