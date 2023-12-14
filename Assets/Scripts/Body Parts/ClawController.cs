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
    private AudioClip mantisPrepareSound;

    [SerializeField]
    private float mantisPreparePitch = 1f;

    [SerializeField]
    private List<AudioClip> mantisShootSounds;

    [SerializeField]
    private float swingDistance = 3f;
    private Coroutine attackRoutine = null;
    private bool attacking = false;

    public bool SwingAttackEnabled { get; private set; }

    [SerializeField]
    private Vector2 shakeAmount;

    [SerializeField]
    private float shakeRate = 1f / 30f;
    private uint shakeRoutineID = 0;

    public bool DoingBasicAttack { get; private set; }


    [Header("Ground Jump")]
    [SerializeField]
    private Transform rightClawsOrigin;

    [SerializeField]
    private Transform leftClawsOrigin;

    [SerializeField]
    private Sprite clawMidAirSprite;
    private Vector3 leftClawOriginBase;
    private Vector3 rightClawOriginBase;

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
                foreach (ClawAnimator claw in claws)
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

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (Boss.TrailerMode)
        {
            yield break;
        }
        while (true)
        {
            if (Vector2.Distance(Boss.Head.transform.position, Player.Player1.transform.position) <= swingDistance && !FrontLeftClaw.ClawLocked && !FrontRightClaw.ClawLocked)
            {
                DoingBasicAttack = true;
                yield return DoBasicAttack();
                DoingBasicAttack = false;
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }
    }

    public IEnumerator DoMantisShots(Action onSwing)
    {
        void OnAttack()
        {
            foreach (AudioClip clip in mantisShootSounds)
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
                AttackRoutines.Add(claws[i].PlayAttackAnimation(false, false, false, OnAttack));
            }
            else
            {
                AttackRoutines.Add(claws[i].PlayAttackAnimation(false, false, false));
            }
        }

        AudioPlayer prepAudio = WeaverAudio.PlayAtPoint(mantisPrepareSound, Boss.Head.transform.position);
        prepAudio.AudioSource.pitch = mantisPreparePitch;

        yield return RoutineAwaiter.AwaitBoundRoutines(AttackRoutines, Boss).WaitTillDone();

        foreach (ClawAnimator claw in claws)
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

        int currentFrameLeft = FrontLeftClaw.CurrentFrame;
        int currentFrameRight = FrontRightClaw.CurrentFrame;

        RoutineAwaiter lockAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.LockClaw(), FrontRightClaw.LockClaw());

        yield return lockAwaiter.WaitTillDone();

        RoutineAwaiter attackAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, FrontLeftClaw.PlayAttackAnimation(true, true, true), FrontRightClaw.PlayAttackAnimation(false, false, true));

        yield return attackAwaiter.WaitTillDone();

        FrontLeftClaw.UnlockClaw();
        FrontRightClaw.UnlockClaw();

        attacking = false;
    }

    public void PrepareForLunge()
    {
        foreach (ClawAnimator claw in claws)
        {
            claw.Animator.PlayAnimation("Lunge Antic");
        }
    }

    public void DoLunge()
    {
        foreach (ClawAnimator claw in claws)
        {
            claw.Animator.PlayAnimation("Lunge");
        }
    }

    public IEnumerator PlayLanding(bool slide)
    {
        if (slide)
        {
            shakeRoutineID = Boss.StartBoundRoutine(ShakeRoutine());
        }

        List<IEnumerator> awaitables = new List<IEnumerator>();
        foreach (ClawAnimator claw in claws)
        {
            awaitables.Add(claw.PlayLanding(slide));
        }
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, awaitables.ToArray());

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
        foreach (ClawAnimator claw in claws)
        {
            awaitables.Add(claw.FinishLanding(slammedIntoWall));
        }
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, awaitables.ToArray());

        yield return awaiter.WaitTillDone();
    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        List<IEnumerator> awaitables = new List<IEnumerator>();
        foreach (ClawAnimator claw in claws)
        {
            awaitables.Add(claw.SlideSwitchDirection(oldDirection, newDirection));
        }
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, awaitables.ToArray());

        yield return awaiter.WaitTillDone();
    }

    private IEnumerator ShakeRoutine()
    {
        Rigidbody2D parentBody = transform.parent.GetComponent<Rigidbody2D>();
        while (true)
        {
            Vector2 newPos = UnityEngine.Random.insideUnitCircle * shakeAmount;
            newPos *= new Vector3(Mathf.Clamp01(Mathf.Abs(parentBody.velocity.x / 3)), Mathf.Clamp01(Mathf.Abs(parentBody.velocity.y / 3)));
            transform.localPosition = transform.localPosition.With(x: newPos.x, y: newPos.y);
            yield return new WaitForSeconds(shakeRate);
        }
    }

    public override void OnStun()
    {
        foreach (ClawAnimator claw in claws)
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

        transform.localPosition = transform.localPosition.With(x: 0f, y: 0f);

        leftClawsOrigin.localPosition = leftClawOriginBase;
        rightClawsOrigin.localPosition = rightClawOriginBase;
    }

    public IEnumerator GroundPrepareJump()
    {
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            leftClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements;
            leftClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements;

            rightClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements * 1.5f;
            rightClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements;
            yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            leftClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
            leftClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;

            rightClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements * 1.5f;
            rightClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;
            if (i != Boss.GroundMode.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
        }

        foreach (ClawAnimator claw in claws)
        {
            claw.MainRenderer.sprite = clawMidAirSprite;
        }

        yield break;
    }

    public IEnumerator GroundLand(bool finalLanding)
    {
        foreach (ClawAnimator claw in claws)
        {
            claw.UpdateGroundSprite();
        }

        leftClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements * Boss.GroundMode.groundJumpFrames;
        leftClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements * Boss.GroundMode.groundJumpFrames;

        rightClawsOrigin.transform.localPosition += Boss.GroundMode.jumpPosIncrements * 1.5f * Boss.GroundMode.groundJumpFrames;
        rightClawsOrigin.transform.localScale += Boss.GroundMode.jumpScaleIncrements * Boss.GroundMode.groundJumpFrames;

        yield return new WaitForSeconds(Boss.GroundMode.groundJumpLandDelay);

        if (finalLanding)
        {
            for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
            {
                leftClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
                leftClawsOrigin.transform.localScale -= Boss.GroundMode.jumpScaleIncrements;

                rightClawsOrigin.transform.localPosition -= Boss.GroundMode.jumpPosIncrements * 1.5f;
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


}
