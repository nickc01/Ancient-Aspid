using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class GroundMode : AncientAspidMode
{
    [NonSerialized]
    private List<GroundModeVariationBase> variations = null;
    public const string GROUND_MODE_VARIATION = nameof(GROUND_MODE_VARIATION);

    public IModeAreaProvider GroundAreaProvider { get; set; }

    [SerializeField]
    bool modeEnabled = true;

    [Header("Lunge")]

    [field: SerializeField]
    public Vector3 lungeTargetOffset;

    [SerializeField]
    public float lungeAnticTime = 0.65f;

    [SerializeField]
    public AudioClip lungeAnticSound;

    [SerializeField]
    public AudioClip lungeSound;

    [SerializeField]
    public List<Collider2D> collidersDisableOnLunge;

    [SerializeField]
    public List<Collider2D> collidersEnableOnLunge;

    [SerializeField]
    public float lungeSpeed = 10f;

    [SerializeField]
    public AudioClip lungeLandSoundLight;

    [SerializeField]
    public float lungeLandSoundLightVolume = 0.75f;

    [SerializeField]
    public AudioClip lungeLandSoundHeavy;

    [SerializeField]
    public AudioClip lungeSlideSound;

    [SerializeField]
    public float lungeSlideSoundVolume = 0.7f;

    [SerializeField]
    public float lungeSlideDeacceleration = 2f;

    [SerializeField]
    public float lungeSlideSlamThreshold = 2f;

    [SerializeField]
    public List<float> lungeXShiftValues = new List<float>()
    {
        -0.56f,
        -0.1f,
        0
    };

    public float lungeTurnaroundSpeed = 1.5f;

    [SerializeField]
    public List<ParticleSystem> groundSkidParticles;

    [SerializeField]
    public float onGroundGravity = 0.75f;

    [SerializeField]
    public GameObject lungeDashEffect;

    [SerializeField]
    public ParticleSystem lungeRockParticles;

    [SerializeField]
    public Transform lungeDashRotationOrigin;

    [field: SerializeField]
    public float lungeDownwardsLandDelay { get; private set; } = 0.5f;

    [field: Space]
    [field: Header("Explosion Stomp")]
    [field: SerializeField]
    public AudioClip megaExplosionSound { get; private set; }

    [field: SerializeField]
    public float megaExplosionSize { get; private set; } = 3f;

    [field: SerializeField]
    [field: Tooltip("If the landing angle is greater than this number, then the boss will slide upon landing on the ground")]
    public float DegreesSlantThreshold { get; private set; } = 30f;

    private AudioPlayer lungeSlideSoundPlayer;

    [Header("Ground Laser")]
    [SerializeField]
    private Vector2 groundLaserMinMaxAngle = new Vector2(-15f, 45f);

    [SerializeField]
    private float groundLaserFireDuration = 1f;

    [SerializeField]
    private float groundLaserMaxDelay = 0.75f;

    [field: Header("Ground Jump")]
    [field: SerializeField]
    public Vector3 jumpPosIncrements { get; private set; }

    [field: SerializeField]
    public Vector3 jumpRotIncrements { get; private set; }

    [field: SerializeField]
    public Vector3 jumpScaleIncrements { get; private set; }

    [field: SerializeField]
    public int groundJumpFrames { get; private set; } = 3;

    [field: SerializeField]
    public float groundJumpPrepareFPS { get; private set; } = 8;

    [field: SerializeField]
    public float groundJumpLaunchFPS { get; private set; } = 16;

    [field: SerializeField]
    public float groundJumpLandFPS { get; private set; } = 16;

    [field: SerializeField]
    public float MidAirSwitchSpeed { get; private set; } = 2f;

    [field: SerializeField]
    public float groundJumpLandDelay { get; private set; } = 0.2f;

    //public bool GroundModeEnabled => GroundAreaProvider != null;

    [field: Space]
    [field: Header("Glob Stomp")]
    [field: SerializeField]
    public ParticleSystem stompSplash { get; private set; }

    [field: SerializeField]
    public ParticleSystem stompPillar { get; private set; }

    [field: SerializeField]
    public Vector3 groundSplashSpawnOffset { get; private set; }

    [field: SerializeField]
    public Vector2Int groundSplashBlobCount { get; private set; } = new Vector2Int(5, 10);

    [field: SerializeField]
    public Vector2 groundSplashAngleRange { get; private set; } = new Vector2(15, 180 - 15);

    [field: SerializeField]
    public Vector2 groundSplashVelocityRange { get; private set; } = new Vector2(3, 15);

    private TargetOverride groundTargetOverride;
    private bool stop = false;
    private GroundModeVariationBase runningVariation;
    private VomitShotMove vomitShotMove;
    private GroundJumpMove groundJumpMove;

    private void Awake()
    {
        vomitShotMove = GetComponent<VomitShotMove>();
        groundJumpMove = GetComponent<GroundJumpMove>();
    }

    protected override bool ModeEnabled(Dictionary<string, object> args)
    {
        return modeEnabled && GroundAreaProvider != null && GroundAreaProvider.IsTargetActive(Boss) && ((Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x > Boss.transform.position.x) || (Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x < Boss.transform.position.x));
    }

    protected override IEnumerator OnExecute(Dictionary<string, object> customArgs)
    {
        if (Boss.GodhomeMode && Boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            Boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR1);
        }
        using Recoiler.RecoilOverride recoilOverride = Boss.Recoil.AddRecoilOverride(0);
        stop = false;
        yield return EnterGroundNew(customArgs);
        if (stop)
        {
            yield break;
        }

        List<Func<IEnumerator>> groundActions = new List<Func<IEnumerator>>()
        {
                Part1_Jump,
                Part2_Vomit,
                Part3_Jump
            };


        foreach (Func<IEnumerator> action in groundActions)
        {
            yield return action();
            if (stop)
            {
                yield return DoGroundCancel(false);
                yield break;
            }
        }


        if (!stop)
        {
            yield return ExitGroundMode();
        }

        yield break;
    }

    private IEnumerator Part1_Jump()
    {
        if (Boss.CanSeeTarget)
        {
            yield return RunAspidMove(groundJumpMove, new Dictionary<string, object>
            {
                {GroundJumpMove.JUMP_TIMES, 2f}
            });

            if (groundJumpMove.Cancelled)
            {
                stop = true;
            }
        }
        else
        {
            stop = true;
        }
        yield break;
    }

    private IEnumerator Part2_Vomit()
    {
        if ((Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x <= transform.position.x) || (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x >= transform.position.x))
        {
            yield return RunAspidMove(vomitShotMove, null);
        }
        yield break;
    }

    private IEnumerator Part3_Jump()
    {
        if (Boss.CanSeeTarget)
        {
            yield return RunAspidMove(groundJumpMove, new Dictionary<string, object>
            {
                {GroundJumpMove.JUMP_TIMES, 3f}
            });

            if (groundJumpMove.Cancelled)
            {
                stop = true;
            }

        }
        else
        {
            stop = true;
        }
        yield break;
    }

    public override void Stop()
    {
        stop = true;
        StopCurrentMove();
    }

    public override void OnDeath()
    {
        if (runningVariation != null)
        {
            runningVariation.OnDeath();
            runningVariation = null;
        }
        if (groundTargetOverride != null)
        {
             Boss.RemoveTargetOverride(groundTargetOverride);
            groundTargetOverride = null;
        }

        if (lungeSlideSoundPlayer != null)
        {
            lungeSlideSoundPlayer.Delete();
            lungeSlideSoundPlayer = null;
        }

        VomitGlob groundGlob = GetComponent<EnterGround_BigVomitShotMove>().SpawnedGlob;
        if (groundGlob != null)
        {
            groundGlob.ForceDisappear();
        }
    }

    public IEnumerator EnterGroundNew(Dictionary<string, object> args)
    {
        if (!args.TryGetValueOfType(GROUND_MODE_VARIATION, out GroundModeVariationBase variation))
        {
            if (variations == null || variations.Count == 0)
            {
                variations = new List<GroundModeVariationBase>()
                {
                    new GroundModeBasicVariation(this),
                    new GroundModeExplosiveVariation(this),
                    new GroundModeVomitVariation(this)
                };
            }
            variation = variations.Where(v => v.VariationEnabled).ToList().GetRandomElement();
        }

        runningVariation = variation;

        Boss.Body.PlayDefaultAnimation = false;

        Boss.ApplyFlightVariance = false;

        Vector2 target = GroundAreaProvider.GetModeTarget(Boss);

        groundTargetOverride = Boss.AddTargetOverride(-10);
         groundTargetOverride.SetTarget(target);

        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        IEnumerator headRoutine = Boss.Head.LockHead(Boss.Orientation);
        IEnumerator bodyRoutine = Boss.Body.RaiseTail();
        IEnumerator minWaitTimeRoutine = Wait(0.5f);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, headRoutine, bodyRoutine, minWaitTimeRoutine);

        yield return awaiter.WaitTillDone();

        Boss.FlightEnabled = false;
        yield return variation.OnBegin();

        yield return new WaitUntil(() => !Boss.Claws.DoingBasicAttack);

        foreach (ClawAnimator claw in Boss.Claws.claws)
        {
            yield return claw.LockClaw();
        }

        if (lungeAnticSound != null)
        {
             WeaverAudio.PlayAtPoint(lungeAnticSound, transform.position);
        }

        Boss.Wings.PrepareForLunge();
        Boss.Claws.PrepareForLunge();

        yield return variation.LungeAntic();
        yield return new WaitForSeconds(lungeAnticTime);

        Boss.Wings.DoLunge();
        Boss.Claws.DoLunge();
        Boss.WingPlates.DoLunge();
        Boss.Head.DoLunge();

        if (lungeSound != null)
        {
             WeaverAudio.PlayAtPoint(lungeSound, transform.position);
        }

        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.SmallShake);

        foreach (Collider2D c in collidersEnableOnLunge)
        {
            if (c != null)
            {
                c.enabled = true;
            }
        }

        foreach (Collider2D c in collidersDisableOnLunge)
        {
            if (c != null)
            {
                c.enabled = false;
            }
        }

        Vector3 destination = variation.GetLungeTarget();

        float angleToDestination = VectorUtilities.VectorToDegrees((destination - transform.position).normalized);

        float downwardAngle = Vector3.Dot(Vector3.right, (destination - transform.position).normalized) * 90f;

        lungeDashEffect.SetActive(true);
         lungeDashRotationOrigin.SetZLocalRotation(angleToDestination);


        bool doSlide = variation.DoSlide(destination);

        Boss.Rbody.velocity = (destination - transform.position).normalized * lungeSpeed;

        yield return null;

        float yVelocity = Boss.Rbody.velocity.y;
        float xVelocity = Boss.Rbody.velocity.x;
        float halfVelocity = Mathf.Abs(Boss.Rbody.velocity.y) / 2f;


        variation.LungeStart();

        bool landed = false;

        for (float t = 0; t < 2; t += Time.deltaTime)
        {
            if (Mathf.Abs(Boss.Rbody.velocity.y) < halfVelocity)
            {
                landed = true;
                break;
            }

            if (t > 0.5f && Vector3.Distance(transform.position, Player.Player1.transform.position) >= 30)
            {
                break;
            }

            yield return null;
        }

        if (!landed)
        {
            variation.LungeCancel();
            yield return DoGroundCancel(false);
            stop = true;

            yield break;
        }
        Boss.Rbody.velocity = default;

        variation.LungeLand(doSlide);

         Boss.StartBoundRoutine(PlayLungeLandAnimation());

        IEnumerator headLandRoutine = Boss.Head.PlayLanding(doSlide);
        IEnumerator bodyLandRoutine = Boss.Body.PlayLanding(doSlide);
        IEnumerator wingPlateLandRoutine = Boss.WingPlates.PlayLanding(doSlide);
        IEnumerator wingsLandRoutine = Boss.Wings.PlayLanding(doSlide);
        IEnumerator clawsLandRoutine = Boss.Claws.PlayLanding(doSlide);
        IEnumerator shiftPartsRoutine = Boss.ShiftBodyParts(doSlide, Boss.Body, Boss.WingPlates, Boss.Wings);

        List<uint> landingRoutines = new List<uint>();

        bool landingCancelled = false;

        RoutineAwaiter landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        bool slammed = false;

        uint switchDirectionRoutine = 0;
        AspidOrientation oldOrientation = Boss.Orientation;

        Boss.Rbody.gravityScale = onGroundGravity;

        void OnTouchGround()
        {
            variation.SlidingOnGround(true);
            if (lungeSlideSoundPlayer == null)
            {
                lungeSlideSoundPlayer = WeaverAudio.PlayAtPointLooped(lungeSlideSound, transform.position, lungeSlideSoundVolume);
                lungeSlideSoundPlayer.transform.parent = transform;
                foreach (ParticleSystem p in groundSkidParticles)
                {
                    p.Play();
                }
            }
        }

        void OnUntouchGround()
        {
            variation.SlidingOnGround(false);
            if (lungeSlideSoundPlayer != null)
            {
                lungeSlideSoundPlayer.Delete();
                lungeSlideSoundPlayer = null;
                foreach (ParticleSystem p in groundSkidParticles)
                {
                    p.Stop();
                }
            }
        }

        lungeRockParticles.Play();

        if (doSlide)
        {
            variation.SlidingStart();
            if (lungeLandSoundLight != null)
            {
                 WeaverAudio.PlayAtPoint(lungeLandSoundLight, transform.position, lungeLandSoundLightVolume);
            }

            CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.AverageShake);

            OnTouchGround();

            float slideSpeed = (destination - transform.position).magnitude * lungeSpeed;

            float horizontalSpeed = Mathf.Lerp(Mathf.Abs(yVelocity), Mathf.Abs(xVelocity), 0.5f);

            Boss.Rbody.velocity = downwardAngle >= 0f ? new Vector2(horizontalSpeed, 0f) : new Vector2(-horizontalSpeed, 0f);

            yield return null;

            float lastVelocityX = Boss.Rbody.velocity.x;

            do
            {
                lastVelocityX = Boss.Rbody.velocity.x;
                Boss.Rbody.velocity = Boss.Rbody.velocity.x >= 0f
                    ? Boss.Rbody.velocity.With(x: Boss.Rbody.velocity.x - (lungeSlideDeacceleration * Time.deltaTime))
                    : Boss.Rbody.velocity.With(x: Boss.Rbody.velocity.x + (lungeSlideDeacceleration * Time.deltaTime));

                if (switchDirectionRoutine == 0)
                {
                    if ((Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x < transform.position.x) ||
                        (Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x > transform.position.x))
                    {
                        IEnumerator SwitchDirections()
                        {
                            variation.OnSlideSwitchDirection(Boss.Orientation);
                            Boss.Head.ToggleLaserBubbles(true);
                            yield return SwitchDirectionDuringSlide();
                            if (Boss.Rbody.velocity.x != 0)
                            {
                                yield return new WaitUntil(() => Boss.Rbody.velocity.x == 0);
                            }
                            Boss.Head.ToggleLaserBubbles(false);
                            yield break;
                        }

                        switchDirectionRoutine = Boss.StartBoundRoutine(SwitchDirections());
                    }
                }

                if (Boss.Rbody.velocity.y < -0.5f)
                {
                    OnUntouchGround();
                }
                else
                {
                    OnTouchGround();
                }

                if (Boss.Rbody.velocity.y < -0.5f && Vector3.Distance(transform.position, Player.Player1.transform.position) >= 30)
                {
                    variation.SlideCancel();
                    yield return DoGroundCancel(false);
                    stop = true;
                    yield break;
                }

                yield return null;
            } while (Mathf.Abs(Boss.Rbody.velocity.x) > Mathf.Abs(lastVelocityX / 3f) && Mathf.Abs(Boss.Rbody.velocity.x) > 0.1f);

            if (Mathf.Abs(Boss.Rbody.velocity.x - lastVelocityX) >= lungeSlideSlamThreshold)
            {
                if (lungeLandSoundHeavy != null)
                {
                     WeaverAudio.PlayAtPoint(lungeLandSoundHeavy, transform.position);
                }
                CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
                slammed = true;
                variation.OnSlideSlam();
            }
            else
            {
                variation.OnSlideStop();
            }

            OnUntouchGround();

            if (!landingAwaiter.Done)
            {
                landingCancelled = true;
                foreach (uint id in landingRoutines)
                {
                    Boss.StopBoundRoutine(id);
                }
            }
        }
        else
        {
            if (lungeLandSoundHeavy != null)
            {
                 WeaverAudio.PlayAtPoint(lungeLandSoundHeavy, transform.position);
            }
            CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
        }

        Boss.Rbody.velocity = Boss.Rbody.velocity.With(x: 0f);

        yield return new WaitUntil(() => landingCancelled || landingAwaiter.Done);

        IEnumerator headFinishRoutine = Boss.Head.FinishLanding(slammed);
        IEnumerator bodyFinishRoutine = Boss.Body.FinishLanding(slammed);
        IEnumerator wingPlateFinishRoutine = Boss.WingPlates.FinishLanding(slammed);
        IEnumerator wingsFinishRoutine = Boss.Wings.FinishLanding(slammed);
        IEnumerator clawsFinishRoutine = Boss.Claws.FinishLanding(slammed);

        RoutineAwaiter finishAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, headFinishRoutine, bodyFinishRoutine, wingPlateFinishRoutine, wingsFinishRoutine, clawsFinishRoutine);

        yield return finishAwaiter.WaitTillDone();

        if (switchDirectionRoutine != 0 && Boss.Orientation == oldOrientation)
        {
            yield return new WaitUntil(() => Boss.Orientation != oldOrientation);
        }

        if (Boss.Rbody.velocity.y < -2)
        {
            variation.OnFallCancel();
            yield return DoGroundCancel(false);
            stop = true;
            yield break;
        }

        if (variation.FireGroundLaser(out Quaternion laserDirection))
        {
            yield return FireGroundLaser(laserDirection, laserDirection);
        }

        if (Boss.Rbody.velocity.y < -2)
        {
            variation.OnFallCancel();
            yield return DoGroundCancel(false);
            stop = true;
            yield break;
        }


    }

    public IEnumerator ExitGroundMode()
    {
        yield return JumpPrepare();
        yield return JumpLaunch();

        foreach (AudioClip sound in groundJumpMove.jumpSounds)
        {
             WeaverAudio.PlayAtPoint(sound, transform.position);
        }

        Boss.Claws.OnGround = false;
        Boss.Wings.PlayDefaultAnimation = true;

        foreach (ClawAnimator claw in Boss.Claws.claws)
        {
            claw.UnlockClaw();
        }

        CameraShaker.Instance.Shake(groundJumpMove.jumpLaunchShakeType);

        Vector3 target = transform.position + new Vector3(0f, 20f, 0f);

        if (target.y < transform.position.y + 2f && target.y > transform.position.y - 2)
        {
            target.y = transform.position.y;
        }

        Vector2 velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, groundJumpMove.jumpTime, groundJumpMove.jumpGravity / 2);

        Boss.Rbody.gravityScale = groundJumpMove.jumpGravity;
        Boss.Rbody.velocity = velocity;

        yield return null;
        yield return null;


        yield return new WaitUntil(() => Boss.Rbody.velocity.y <= 0);

        Boss.Rbody.velocity = default;
        Boss.Rbody.gravityScale = 0f;

        if (runningVariation != null)
        {
            yield return runningVariation.OnEnd();
        }

        Boss.FlightEnabled = true;




        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }

        Boss.Body.PlayDefaultAnimation = true;



        foreach (Collider2D c in collidersEnableOnLunge)
        {
            if (c != null)
            {
                c.enabled = false;
            }
        }

        foreach (Collider2D c in collidersDisableOnLunge)
        {
            if (c != null)
            {
                c.enabled = true;
            }
        }

        Boss.ApplyFlightVariance = true;

        if (groundTargetOverride != null)
        {
             Boss.RemoveTargetOverride(groundTargetOverride);
            groundTargetOverride = null;
        }

        runningVariation = null;

        yield break;
    }

    private IEnumerator FireGroundLaser(Quaternion start, Quaternion end)
    {
        yield return new WaitForSeconds(groundLaserMaxDelay);

        yield return Boss.Head.FireGroundLaser(start, end, groundLaserFireDuration, true);
    }

    private IEnumerator SwitchDirectionDuringSlide()
    {
        AspidOrientation oldDirection = Boss.Orientation;
        AspidOrientation newDirection = oldDirection == AspidOrientation.Left ? AspidOrientation.Right : AspidOrientation.Left;

        IEnumerator clawsRoutine = Boss.Claws.SlideSwitchDirection(oldDirection, newDirection);
        IEnumerator headRoutine = Boss.Head.SlideSwitchDirection(oldDirection, newDirection);
        IEnumerator bodyRoutine = Boss.Body.SlideSwitchDirection(oldDirection, newDirection);
        IEnumerator wingPlateRoutine = Boss.WingPlates.SlideSwitchDirection(oldDirection, newDirection);
        IEnumerator wingsRoutine = Boss.Wings.SlideSwitchDirection(oldDirection, newDirection);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, bodyRoutine, wingPlateRoutine, wingsRoutine);

        yield return awaiter.WaitTillDone();

        Boss.Orientation = newDirection;
    }

    private IEnumerator PlayLungeLandAnimation()
    {
        yield break;
    }

    public IEnumerator DoGroundCancel(bool onGround)
    {
        Boss.Claws.OnGround = false;
        Boss.Wings.PlayDefaultAnimation = true;

        Boss.FlightEnabled = true;
        Boss.Rbody.velocity = default;
        Boss.Rbody.gravityScale = 0f;

        foreach (ClawAnimator claw in Boss.Claws.claws)
        {
            claw.UnlockClaw();
        }

        IEnumerator clawsRoutine = Boss.Claws.GroundMoveCancel(onGround);
        IEnumerator headRoutine = Boss.Head.GroundMoveCancel(onGround);
        IEnumerator wingsRoutine = Boss.Wings.GroundMoveCancel(onGround);
        IEnumerator wingPlatesRoutine = Boss.WingPlates.GroundMoveCancel(onGround);
        IEnumerator bodyRoutine = Boss.Body.GroundMoveCancel(onGround);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        Boss.Head.UnlockHead();

        Boss.Body.PlayDefaultAnimation = true;



        foreach (Collider2D c in collidersEnableOnLunge)
        {
            if (c != null)
            {
                c.enabled = false;
            }
        }

        foreach (Collider2D c in collidersDisableOnLunge)
        {
            if (c != null)
            {
                c.enabled = true;
            }
        }

        Boss.ApplyFlightVariance = true;
        if (groundTargetOverride != null)
        {
             Boss.RemoveTargetOverride(groundTargetOverride);
            groundTargetOverride = null;
        }

    }

    private IEnumerator JumpSwitchDirectionPrepare()
    {
        IEnumerator clawsRoutine = Boss.Claws.WaitTillChangeDirectionMidJump();
        IEnumerator headRoutine = Boss.Head.WaitTillChangeDirectionMidJump();
        IEnumerator wingsRoutine = Boss.Wings.WaitTillChangeDirectionMidJump();
        IEnumerator wingPlatesRoutine = Boss.WingPlates.WaitTillChangeDirectionMidJump();
        IEnumerator bodyRoutine = Boss.Body.WaitTillChangeDirectionMidJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }

    private IEnumerator JumpPrepare()
    {
        IEnumerator clawsRoutine = Boss.Claws.GroundPrepareJump();
        IEnumerator headRoutine = Boss.Head.GroundPrepareJump();
        IEnumerator wingsRoutine = Boss.Wings.GroundPrepareJump();
        IEnumerator wingPlatesRoutine = Boss.WingPlates.GroundPrepareJump();
        IEnumerator bodyRoutine = Boss.Body.GroundPrepareJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    private IEnumerator JumpLaunch()
    {
        IEnumerator clawsRoutine = Boss.Claws.GroundLaunch();
        IEnumerator headRoutine = Boss.Head.GroundLaunch();
        IEnumerator wingsRoutine = Boss.Wings.GroundLaunch();
        IEnumerator wingPlatesRoutine = Boss.WingPlates.GroundLaunch();
        IEnumerator bodyRoutine = Boss.Body.GroundLaunch();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    private IEnumerator JumpLand(bool finalLanding)
    {
        IEnumerator clawsRoutine = Boss.Claws.GroundLand(finalLanding);
        IEnumerator headRoutine = Boss.Head.GroundLand(finalLanding);
        IEnumerator wingsRoutine = Boss.Wings.GroundLand(finalLanding);
        IEnumerator wingPlatesRoutine = Boss.WingPlates.GroundLand(finalLanding);
        IEnumerator bodyRoutine = Boss.Body.GroundLand(finalLanding);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }
}
