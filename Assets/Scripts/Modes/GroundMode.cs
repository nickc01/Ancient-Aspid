using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore;
using System.Linq;
using System.Net.NetworkInformation;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Utilities;
using System;

public class GroundMode : AncientAspidMode
{
    [NonSerialized]
    List<GroundModeVariationBase> variations = null;
    //static RaycastHit2D[] rayCache = new RaycastHit2D[4];

    //CUSTOM MODE ARGS
    //public const string AREA_PROVIDER = nameof(AREA_PROVIDER);
    public const string GROUND_MODE_VARIATION = nameof(GROUND_MODE_VARIATION);

    public IModeAreaProvider GroundAreaProvider { get; set; }

    /*public IModeAreaProvider GroundAreaProvider
    {
        get
        {
            if (DefaultArgs.TryGetValueOfType(AREA_PROVIDER, out IModeAreaProvider provider))
            {
                return provider;
            }
            return null;
        }
        set => DefaultArgs[AREA_PROVIDER] = value;
    }*/


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



    AudioPlayer lungeSlideSoundPlayer;

    [Header("Ground Laser")]
    [SerializeField]
    Vector2 groundLaserMinMaxAngle = new Vector2(-15f, 45f);

    [SerializeField]
    float groundLaserFireDuration = 1f;

    [SerializeField]
    float groundLaserMaxDelay = 0.75f;

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

    //[field: SerializeField]
    //public bool GroundModeEnabled { get; private set; } = true;
    //public IModeAreaProvider GroundAreaProvider { get; set; }

    public bool GroundModeEnabled => GroundAreaProvider != null;

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

    //public IModeAreaProvider 

    //public override bool ModeEnabled => true;

    TargetOverride groundTargetOverride;
    bool stop = false;

    GroundModeVariationBase runningVariation;

    VomitShotMove vomitShotMove;

    GroundJumpMove groundJumpMove;

    private void Awake()
    {
        vomitShotMove = GetComponent<VomitShotMove>();
        groundJumpMove = GetComponent<GroundJumpMove>();
    }

    protected override bool ModeEnabled(Dictionary<string, object> args)
    {
        return GroundAreaProvider != null && GroundAreaProvider.IsTargetActive(Boss);
        //return args.TryGetValueOfType(AREA_PROVIDER, out IModeAreaProvider GroundAreaProvider);
    }

    protected override IEnumerator OnExecute(Dictionary<string, object> customArgs)
    {
        using (var recoilOverride = Boss.Recoil.AddRecoilOverride(0))
        {
            stop = false;
            yield return EnterGroundNew(customArgs);
            if (stop)
            {
                yield break;
            }

            List<Func<IEnumerator>> groundActions = new List<Func<IEnumerator>>
            {
                Part1_Jump,
                Part2_Vomit,
                Part3_Jump
            };


            foreach (var action in groundActions)
            {
                yield return action();
                if (stop)
                {
                    yield return DoGroundCancel(false);
                    yield break;
                }
            }


            //yield return new 

            //yield return RunMoveWhile(move, () => !stop);

            if (!stop)
            {
                yield return ExitGroundMode();
            }

            yield break;
        }
    }

    IEnumerator Part1_Jump()
    {
        if (Boss.CanSeeTarget)
        {
            //groundJumpMove.JumpTimes = 2;

            //yield return RunMoveWhile(groundJumpMove, () => true);
            yield return RunAspidMove(groundJumpMove,new Dictionary<string, object>
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

    IEnumerator Part2_Vomit()
    {
        if ((Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x <= transform.position.x) || (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x >= transform.position.x))
        {
            //yield return Boss.RunMove(vomitShotMove);
            yield return RunAspidMove(vomitShotMove,null);
        }
        yield break;
    }

    IEnumerator Part3_Jump()
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

            /*yield return RunAspidMove(groundJumpMove, new Dictionary<string, object>
            {
                {GroundJumpMove.JUMP_TIMES, 3f}
            });*/
            /*groundJumpMove.JumpTimes = 3;

            yield return Boss.RunMove(groundJumpMove);*/

            //stop = true;
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

        var groundGlob = GetComponent<EnterGround_BigVomitShotMove>().SpawnedGlob;
        if (groundGlob != null)
        {
            groundGlob.ForceDisappear();
        }
    }

    /*IEnumerator ShootGiantBlob(EnterGround_BigVomitShotMove move, float preDelay = 0.25f)
    {
        yield return new WaitForSeconds(preDelay);
        //WeaverLog.Log("Running Move = " + move.GetType().FullName);
        yield return Boss.RunMove(move);


        for (float t = move.SpawnedGlobEstLandTime; t >= 0.5f; t -= Time.deltaTime)
        {
            yield return null;
        }
    }

    IEnumerator ShootExplosives(BombMove move, IBombController controller, float preDelay = 0.25f)
    {
        yield return new WaitForSeconds(preDelay);
        move.CustomController = controller;
        yield return Boss.RunMove(move);

    }*/

    /*enum GroundModeVersion
    {
        Normal,
        Glob,
        Explosion
    }*/

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
            variation = variations.GetRandomElement();
        }

        runningVariation = variation;

        Boss.Body.PlayDefaultAnimation = false;

        Boss.ApplyFlightVariance = false;

        var target = GroundAreaProvider.GetModeTarget(Boss);

        groundTargetOverride = Boss.AddTargetOverride(-10);
        groundTargetOverride.SetTarget(target);

        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        var headRoutine = Boss.Head.LockHead(Boss.Orientation);
        var bodyRoutine = Boss.Body.RaiseTail();
        var minWaitTimeRoutine = Wait(0.5f);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, headRoutine, bodyRoutine, minWaitTimeRoutine);

        yield return awaiter.WaitTillDone();

        Boss.FlightEnabled = false;
        //Boss.Recoil.SetRecoilSpeed(0f);

        //var bigBlobMove = GetComponent<EnterGround_BigVomitShotMove>();
        //var bombMove = GetComponent<BombMove>();

        //MegaBombsController bombController = null;

        /*if (mode == GroundModeVersion.Glob)
        {
            //Debug.Log("GLOB START");
            yield return ShootGiantBlob(bigBlobMove);
            //Debug.Log("GLOB END");
        }
        else if (mode == GroundModeVersion.Explosion)
        {
            bombController = new MegaBombsController(Boss, 3, 2f, 1f, 0.25f, bombMove.BombGravityScale);
            yield return ShootExplosives(bombMove, bombController);
        }*/

        yield return variation.OnBegin();

        yield return new WaitUntil(() => !Boss.Claws.DoingBasicAttack);

        foreach (var claw in Boss.Claws.claws)
        {
            yield return claw.LockClaw();
        }

        //yield return Body.RaiseTail();

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

        foreach (var c in collidersEnableOnLunge)
        {
            if (c != null)
            {
                c.enabled = true;
            }
        }

        foreach (var c in collidersDisableOnLunge)
        {
            if (c != null)
            {
                c.enabled = false;
            }
        }

        /*Vector3 destination;

        switch (mode)
        {
            case GroundModeVersion.Glob:
                destination = bigBlobMove.SpawnedGlob.transform.position + new Vector3(0, 2, 0);
                break;
            case GroundModeVersion.Explosion:
                destination = new Vector3(bombController.MinLandXPos, bombController.FloorHeight);
                break;
            default:
                bool towardsPlayer = UnityEngine.Random.Range(0f, 1f) >= 0.5f;

                if (UnityEngine.Random.Range(0, 2) == 1 || true)
                {
                    destination = Player.Player1.transform.position;
                }
                else
                {
                    destination = transform.position.With(y: Boss.CurrentRoomRect.yMin);
                }
                break;
        }*/

        Vector3 destination = variation.GetLungeTarget();

        /*if (!globMode)
        {
            bool towardsPlayer = UnityEngine.Random.Range(0f, 1f) >= 0.5f;

            if (UnityEngine.Random.Range(0, 2) == 1 || true)
            {
                destination = Player.Player1.transform.position;
            }
            else
            {
                destination = transform.position.With(y: CurrentRoomRect.yMin);
            }
        }
        else
        {
            destination = bigBlobMove.SpawnedGlob.transform.position + new Vector3(0,2,0);
        }*/

        var angleToDestination = VectorUtilities.VectorToDegrees((destination - transform.position).normalized);

        var downwardAngle = Vector3.Dot(Vector3.right, (destination - transform.position).normalized) * 90f;

        lungeDashEffect.SetActive(true);
        lungeDashRotationOrigin.SetZLocalRotation(angleToDestination);


        /*bool steepAngle = Mathf.Abs(downwardAngle) >= DegreesSlantThreshold;

        if (mode != GroundModeVersion.Normal)
        {
            steepAngle = false;
        }*/
        bool doSlide = variation.DoSlide(destination);

        Boss.Rbody.velocity = (destination - transform.position).normalized * lungeSpeed;

        yield return null;

        var yVelocity = Boss.Rbody.velocity.y;
        var xVelocity = Boss.Rbody.velocity.x;
        var halfVelocity = Mathf.Abs(Boss.Rbody.velocity.y) / 2f;


        variation.LungeStart();

        //
        //Wait until landing, or cancel if needed
        //

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

            /*if (mode == GroundModeVersion.Glob)
            {
                bigBlobMove.SpawnedGlob.ForceDisappear();
            }*/
            yield break;
        }
        //yield return new WaitUntil(() => Mathf.Abs(Boss.Rbody.velocity.y) < halfVelocity);

        Boss.Rbody.velocity = default;

        variation.LungeLand(doSlide);

        /*if (mode == GroundModeVersion.Glob)
        {
            if (Vector3.Distance(transform.position, bigBlobMove.SpawnedGlob.transform.position) <= 5)
            {
                bigBlobMove.SpawnedGlob.ForceDisappear();
                stompSplash.Play();
                stompPillar.Play();

                var spawnCount = UnityEngine.Random.Range(groundSplashBlobCount.x, groundSplashBlobCount.y);

                for (int i = 0; i < spawnCount; i++)
                {
                    float angle = groundSplashAngleRange.RandomInRange();
                    float magnitude = groundSplashVelocityRange.RandomInRange();

                    var velocity = MathUtilities.PolarToCartesian(angle, magnitude);

                    VomitGlob.Spawn(Boss.GlobPrefab, transform.position + groundSplashSpawnOffset, velocity);
                }
            }
        }
        else if (mode == GroundModeVersion.Explosion)
        {
            foreach (var bomb in bombMove.LastFiredBombs)
            {
                if (bomb != null)
                {
                    bomb.Explode();
                }
            }

            if (megaExplosionSound != null)
            {
                WeaverAudio.PlayAtPoint(megaExplosionSound, transform.position);
            }

            InfectedExplosion.Spawn(transform.position, megaExplosionSize);
        }*/

        Boss.StartBoundRoutine(PlayLungeLandAnimation());

        var headLandRoutine = Boss.Head.PlayLanding(doSlide);
        var bodyLandRoutine = Boss.Body.PlayLanding(doSlide);
        var wingPlateLandRoutine = Boss.WingPlates.PlayLanding(doSlide);
        var wingsLandRoutine = Boss.Wings.PlayLanding(doSlide);
        var clawsLandRoutine = Boss.Claws.PlayLanding(doSlide);
        var shiftPartsRoutine = Boss.ShiftBodyParts(doSlide, Boss.Body, Boss.WingPlates, Boss.Wings);

        List<uint> landingRoutines = new List<uint>();

        bool landingCancelled = false;

        var landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        bool slammed = false;

        uint switchDirectionRoutine = 0;
        AspidOrientation oldOrientation = Boss.Orientation;

        Boss.Rbody.gravityScale = onGroundGravity;

        //Used to play the sliding sound when on the ground
        void OnTouchGround()
        {
            variation.SlidingOnGround(true);
            if (lungeSlideSoundPlayer == null)
            {
                lungeSlideSoundPlayer = WeaverAudio.PlayAtPointLooped(lungeSlideSound, transform.position, lungeSlideSoundVolume);
                lungeSlideSoundPlayer.transform.parent = transform;
                foreach (var p in groundSkidParticles)
                {
                    p.Play();
                }
            }
        }

        //Used to stop playing the sliding sound when no longer on the ground
        void OnUntouchGround()
        {
            variation.SlidingOnGround(false);
            if (lungeSlideSoundPlayer != null)
            {
                lungeSlideSoundPlayer.Delete();
                lungeSlideSoundPlayer = null;
                foreach (var p in groundSkidParticles)
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

            var slideSpeed = (destination - transform.position).magnitude * lungeSpeed;

            //var horizontalSpeed = Mathf.Lerp(slideSpeed, Mathf.Abs(yVelocity), 0.65f);
            var horizontalSpeed = Mathf.Lerp(Mathf.Abs(yVelocity), Mathf.Abs(xVelocity), 0.5f);

            if (downwardAngle >= 0f)
            {
                Boss.Rbody.velocity = new Vector2(horizontalSpeed, 0f);
            }
            else
            {
                Boss.Rbody.velocity = new Vector2(-horizontalSpeed, 0f);
            }

            yield return null;

            var lastVelocityX = Boss.Rbody.velocity.x;

            do
            {
                //TODO - CALL SWITCH DIRECTION FUNCTIONS SOMEWHERE HERE
                lastVelocityX = Boss.Rbody.velocity.x;
                if (Boss.Rbody.velocity.x >= 0f)
                {
                    Boss.Rbody.velocity = Boss.Rbody.velocity.With(x: Boss.Rbody.velocity.x - (lungeSlideDeacceleration * Time.deltaTime));
                }
                else
                {
                    Boss.Rbody.velocity = Boss.Rbody.velocity.With(x: Boss.Rbody.velocity.x + (lungeSlideDeacceleration * Time.deltaTime));
                }

                if (switchDirectionRoutine == 0)
                {
                    if ((Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x < transform.position.x) ||
                        (Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x > transform.position.x))
                    {
                        /*if (!landingAwaiter.Done)
                        {
                            landingCancelled = true;
                            foreach (var id in landingRoutines)
                            {
                                StopBoundRoutine(id);
                            }
                        }*/

                        IEnumerator SwitchDirections()
                        {
                            variation.OnSlideSwitchDirection(Boss.Orientation);
                            Boss.Head.ToggleLaserBubbles(true);
                            yield return SwitchDirectionDuringSlide();
                            //yield return Head.PlayGroundLaserAntic(0);
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
                //THE BOSS SLAMMED INTO SOMETHING
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
                //THE BOSS STOPPED GRACEFULLY
            }

            OnUntouchGround();

            if (!landingAwaiter.Done)
            {
                landingCancelled = true;
                foreach (var id in landingRoutines)
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

        var headFinishRoutine = Boss.Head.FinishLanding(slammed);
        var bodyFinishRoutine = Boss.Body.FinishLanding(slammed);
        var wingPlateFinishRoutine = Boss.WingPlates.FinishLanding(slammed);
        var wingsFinishRoutine = Boss.Wings.FinishLanding(slammed);
        var clawsFinishRoutine = Boss.Claws.FinishLanding(slammed);

        var finishAwaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, headFinishRoutine, bodyFinishRoutine, wingPlateFinishRoutine, wingsFinishRoutine, clawsFinishRoutine);

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

        /*if (Boss.CanSeeTarget && ((Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x <= transform.position.x) || (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x >= transform.position.x)))
        {
            yield return FireGroundLaserAtPlayer();
        }*/

        if (variation.FireGroundLaser(out var laserDirection))
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


        //TODO - AIM TOWARDS THE PLAYER OR TOWARDS THE GROUND

        //TODO - PLAY PREPARE ANIMATION. The Wings stop, the claws stop and the body lifts up

        //TODO - Either Lunge Towards the player or down towards the ground. If the enemy lunges towards the player,
        //the boss will continue to slide on the ground until he stops.

        //Make sure the claws and wings stay stopped during this state

        //AspidMode = Mode.Defensive;

        /*bool jumpsCancelled = false;

        void onCancel()
        {
            jumpsCancelled = true;
        }

        //yield return new WaitForSeconds(0.1f);

        if (Boss.CanSeeTarget)
        {
            yield return DoGroundJump(2, JumpTargeter, onCancel);
        }

        if (jumpsCancelled || !Boss.CanSeeTarget)
        {
            yield return JumpCancel(false);
            yield break;
        }

        if ((Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x <= transform.position.x) || (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x >= transform.position.x))
        {
            yield return Boss.RunMove(GetComponent<VomitShotMove>());
        }

        if (Boss.CanSeeTarget)
        {
            yield return DoGroundJump(3, JumpTargeter, onCancel);
        }

        if (jumpsCancelled || !Boss.CanSeeTarget)
        {
            yield return JumpCancel(false);
            yield break;
        }

        yield return new WaitForSeconds(0.05f);

        yield break;*/
    }

    public IEnumerator ExitGroundMode()
    {
        /*if (AspidMode != Mode.Defensive)
        {
            yield break;
        }*/
        //TODO - Play the prepare animation for lifting off of the ground

        //TODO - Play the jump animation for jumping back up into the air

        //TODO - Switch back to tactic mode and play the default claw and wing animations

        //bool onGround = Claws.OnGround;

        yield return JumpPrepare();
        yield return JumpLaunch();

        foreach (var sound in groundJumpMove.jumpSounds)
        {
            WeaverAudio.PlayAtPoint(sound, transform.position);
        }

        Boss.Claws.OnGround = false;
        Boss.Wings.PlayDefaultAnimation = true;

        foreach (var claw in Boss.Claws.claws)
        {
            claw.UnlockClaw();
        }

        CameraShaker.Instance.Shake(groundJumpMove.jumpLaunchShakeType);

        var target = transform.position + new Vector3(0f, 20f, 0f);

        if (target.y < transform.position.y + 2f && target.y > transform.position.y - 2)
        {
            target.y = transform.position.y;
        }

        var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, groundJumpMove.jumpTime, groundJumpMove.jumpGravity / 2);

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




        //var headRoutine = Head.LockHead(Head.LookingDirection >= 0f ? 60f : -60f);
        //var bodyRoutine = Body.RaiseTail();
        //var minWaitTimeRoutine = Wait(0.5f);

        //RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, headRoutine, bodyRoutine, minWaitTimeRoutine);
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }

        //Boss.Recoil.ResetRecoilSpeed();

        Boss.Body.PlayDefaultAnimation = true;



        foreach (var c in collidersEnableOnLunge)
        {
            if (c != null)
            {
                c.enabled = false;
            }
        }

        foreach (var c in collidersDisableOnLunge)
        {
            if (c != null)
            {
                c.enabled = true;
            }
        }

        Boss.ApplyFlightVariance = true;
        //Boss.SetTarget(playerTarget);
        //AspidMode = Mode.Tactical;


        if (groundTargetOverride != null)
        {
            Boss.RemoveTargetOverride(groundTargetOverride);
            groundTargetOverride = null;
        }

        runningVariation = null;

        yield break;
    }

    /*Vector2 JumpTargeter(int time)
    {
        //return Player.Player1.transform.position;
        if (time % 2 == 0)
        {
            var rb = Player.Player1.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                return Player.Player1.transform.position + new Vector3(rb.velocity.x * jumpTime + 0.1f, 0f);
            }
            else
            {
                return Player.Player1.transform.position;
            }

            //return Player.Player1.transform.position;
        }
        else
        {
            //var area = CurrentRoomRect
            var minX = Boss.CurrentRoomRect.xMin + 6f;
            var maxX = Boss.CurrentRoomRect.xMax - 6f;

            var randValue = UnityEngine.Random.Range(minX, maxX);

            randValue = Mathf.Clamp(randValue, transform.position.x - 10f, transform.position.x + 10f);

            return new Vector2(randValue, transform.position.y);
        }
    }

    IEnumerator FireGroundLaserSweep()
    {
        float startAngle, endAngle;

        if (Boss.Orientation == AspidOrientation.Right)
        {
            startAngle = groundLaserMinMaxAngle.x;
            endAngle = groundLaserMinMaxAngle.y;
        }
        else
        {
            startAngle = 180f - groundLaserMinMaxAngle.x;
            endAngle = 180f - groundLaserMinMaxAngle.y;
        }

        return FireGroundLaser(startAngle, endAngle);
    }

    IEnumerator FireGroundLaserAtPlayer()
    {
        var playerAngle = Boss.GetAngleToPlayer();

        const float angleLimit = 45f;

        if (Boss.Orientation == AspidOrientation.Left)
        {
            if (playerAngle < 0f)
            {
                playerAngle += 360f;
            }
            playerAngle = Mathf.Clamp(playerAngle, 180 - angleLimit, 180 + angleLimit);
            if (playerAngle > 180)
            {
                playerAngle = 180f;
            }
        }
        else
        {
            if (playerAngle >= 180f)
            {
                playerAngle -= 360f;
            }
            playerAngle = Mathf.Clamp(playerAngle, -angleLimit, angleLimit);
            if (playerAngle < 0f)
            {
                playerAngle = 0f;
            }
        }

        WeaverLog.Log("PLAYER ANGLE = " + playerAngle);

        return FireGroundLaser(playerAngle, playerAngle);
    }*/

    /*IEnumerator FireGroundLaser(float startAngle, float endAngle)
    {
        var startQ = Quaternion.Euler(0f, 0f, startAngle);
        var endQ = Quaternion.Euler(0f, 0f, endAngle);
        return FireGroundLaser(startQ, endQ);
    }*/

    IEnumerator FireGroundLaser(Quaternion start, Quaternion end)
    {
        yield return new WaitForSeconds(groundLaserMaxDelay);

        yield return Boss.Head.FireGroundLaser(start, end, groundLaserFireDuration, true);
    }

    IEnumerator SwitchDirectionDuringSlide()
    {
        var oldDirection = Boss.Orientation;
        var newDirection = oldDirection == AspidOrientation.Left ? AspidOrientation.Right : AspidOrientation.Left;

        var clawsRoutine = Boss.Claws.SlideSwitchDirection(oldDirection, newDirection);
        var headRoutine = Boss.Head.SlideSwitchDirection(oldDirection, newDirection);
        var bodyRoutine = Boss.Body.SlideSwitchDirection(oldDirection, newDirection);
        var wingPlateRoutine = Boss.WingPlates.SlideSwitchDirection(oldDirection, newDirection);
        var wingsRoutine = Boss.Wings.SlideSwitchDirection(oldDirection, newDirection);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, bodyRoutine, wingPlateRoutine, wingsRoutine);

        yield return awaiter.WaitTillDone();

        Boss.Orientation = newDirection;
    }

    IEnumerator PlayLungeLandAnimation()
    {
        yield break;
    }

    /*IEnumerator DoGroundJump(int jumpTimes, Func<int, Vector2> getTarget, Action onCancel)
    {
        yield return JumpPrepare();

        for (int i = 0; i < jumpTimes; i++)
        {
            yield return JumpLaunch();

            foreach (var sound in jumpSounds)
            {
                WeaverAudio.PlayAtPoint(sound, transform.position);
            }

            CameraShaker.Instance.Shake(jumpLaunchShakeType);

            if (jumpLaunchEffectPrefab != null)
            {
                //Pooling.Instantiate(jumpLaunchEffectPrefab, transform.position + jumpLaunchEffectPrefab.transform.localPosition, jumpLaunchEffectPrefab.transform.localRotation);
            }

            var target = getTarget(i);

            if (target.y < transform.position.y + 2f && target.y > transform.position.y - 2)
            {
                target.y = transform.position.y;
            }

            var groundHits = Physics2D.RaycastNonAlloc(target, Vector2.down, rayCache, 10, LayerMask.NameToLayer("Terrain"));

            if (groundHits == 0)
            {
                if (Boss.CurrentRoomRect.BottomHit.collider != null)
                {
                    var colliderBounds = Boss.CurrentRoomRect.BottomHit.collider.bounds;

                    target.x = Mathf.Clamp(target.x, colliderBounds.min.x + 5, colliderBounds.max.x - 5);
                }
            }

            var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, jumpTime, jumpGravity);

            Boss.Rbody.gravityScale = jumpGravity;
            Boss.Rbody.velocity = velocity;

            Boss.Claws.OnGround = false;

            bool switchDirection = false;
            if (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x < target.x)
            {
                switchDirection = true;
            }

            if (Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x >= target.x)
            {
                switchDirection = true;
            }

            if (switchDirection)
            {
                yield return JumpSwitchDirectionPrepare();
                yield return JumpSwitchDirection();
                if (Boss.Orientation == AspidOrientation.Left)
                {
                    Boss.Orientation = AspidOrientation.Right;
                }
                else
                {
                    Boss.Orientation = AspidOrientation.Left;
                }
            }

            yield return new WaitUntil(() => Boss.Rbody.velocity.y <= 0f);
            var fallingAwaiter = JumpBeginFalling(switchDirection);
            yield return new WaitUntil(() => Boss.Rbody.velocity.y <= -0.5f);
            //yield return new WaitForSeconds(Time.fixedDeltaTime * 2f);
            //yield return new WaitForSeconds(jumpTime / 5f);

            bool cancel = true;

            for (float t = 0; t < 1.5f; t += Time.deltaTime)
            {
                if (Boss.Rbody.velocity.y >= -0.5f)
                {
                    cancel = false;
                    break;
                }
                yield return null;
            }

            if (cancel)
            {
                onCancel?.Invoke();
                yield break;
            }
            //yield return new WaitUntil(() => Boss.Rbody.velocity.y >= 0f);

            yield return fallingAwaiter.WaitTillDone();

            Boss.Rbody.velocity = Boss.Rbody.velocity.With(x: 0f);

            Boss.Claws.OnGround = true;

            if (jumpLandSound != null)
            {
                WeaverAudio.PlayAtPoint(jumpLandSound, transform.position);
            }

            CameraShaker.Instance.Shake(jumpLandShakeType);
            jumpLandParticles.Play();

            yield return JumpLand(i == jumpTimes - 1);
        }

        Boss.Rbody.velocity = default;
        Boss.Rbody.gravityScale = 0f;
    }*/

    /*RoutineAwaiter JumpBeginFalling(bool switchedDirection)
    {
        var clawsRoutine = Boss.Claws.GroundJumpBeginFalling(switchedDirection);
        var headRoutine = Boss.Head.GroundJumpBeginFalling(switchedDirection);
        var wingsRoutine = Boss.Wings.GroundJumpBeginFalling(switchedDirection);
        var wingPlatesRoutine = Boss.WingPlates.GroundJumpBeginFalling(switchedDirection);
        var bodyRoutine = Boss.Body.GroundJumpBeginFalling(switchedDirection);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        return awaiter;
    }

    IEnumerator JumpSwitchDirection()
    {
        var oldOrientation = Boss.Orientation;
        AspidOrientation newOrientation;
        if (oldOrientation == AspidOrientation.Left)
        {
            newOrientation = AspidOrientation.Right;
        }
        else
        {
            newOrientation = AspidOrientation.Left;
        }

        var clawsRoutine = Boss.Claws.MidJumpChangeDirection(oldOrientation, newOrientation);
        var headRoutine = Boss.Head.MidJumpChangeDirection(oldOrientation, newOrientation);
        var wingsRoutine = Boss.Wings.MidJumpChangeDirection(oldOrientation, newOrientation);
        var wingPlatesRoutine = Boss.WingPlates.MidJumpChangeDirection(oldOrientation, newOrientation);
        var bodyRoutine = Boss.Body.MidJumpChangeDirection(oldOrientation, newOrientation);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }*/

    public IEnumerator DoGroundCancel(bool onGround)
    {
        Boss.Claws.OnGround = false;
        Boss.Wings.PlayDefaultAnimation = true;

        Boss.FlightEnabled = true;
        Boss.Rbody.velocity = default;
        Boss.Rbody.gravityScale = 0f;

        foreach (var claw in Boss.Claws.claws)
        {
            claw.UnlockClaw();
        }

        var clawsRoutine = Boss.Claws.GroundMoveCancel(onGround);
        var headRoutine = Boss.Head.GroundMoveCancel(onGround);
        var wingsRoutine = Boss.Wings.GroundMoveCancel(onGround);
        var wingPlatesRoutine = Boss.WingPlates.GroundMoveCancel(onGround);
        var bodyRoutine = Boss.Body.GroundMoveCancel(onGround);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        Boss.Head.UnlockHead();

        //Boss.Recoil.ResetRecoilSpeed();

        Boss.Body.PlayDefaultAnimation = true;



        foreach (var c in collidersEnableOnLunge)
        {
            if (c != null)
            {
                c.enabled = false;
            }
        }

        foreach (var c in collidersDisableOnLunge)
        {
            if (c != null)
            {
                c.enabled = true;
            }
        }

        Boss.ApplyFlightVariance = true;
        //Boss.SetTarget(playerTarget);
        if (groundTargetOverride != null)
        {
            Boss.RemoveTargetOverride(groundTargetOverride);
            groundTargetOverride = null;
        }

        //AspidMode = Mode.Tactical;
    }

    IEnumerator JumpSwitchDirectionPrepare()
    {
        var clawsRoutine = Boss.Claws.WaitTillChangeDirectionMidJump();
        var headRoutine = Boss.Head.WaitTillChangeDirectionMidJump();
        var wingsRoutine = Boss.Wings.WaitTillChangeDirectionMidJump();
        var wingPlatesRoutine = Boss.WingPlates.WaitTillChangeDirectionMidJump();
        var bodyRoutine = Boss.Body.WaitTillChangeDirectionMidJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }

    IEnumerator JumpPrepare()
    {
        var clawsRoutine = Boss.Claws.GroundPrepareJump();
        var headRoutine = Boss.Head.GroundPrepareJump();
        var wingsRoutine = Boss.Wings.GroundPrepareJump();
        var wingPlatesRoutine = Boss.WingPlates.GroundPrepareJump();
        var bodyRoutine = Boss.Body.GroundPrepareJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    IEnumerator JumpLaunch()
    {
        var clawsRoutine = Boss.Claws.GroundLaunch();
        var headRoutine = Boss.Head.GroundLaunch();
        var wingsRoutine = Boss.Wings.GroundLaunch();
        var wingPlatesRoutine = Boss.WingPlates.GroundLaunch();
        var bodyRoutine = Boss.Body.GroundLaunch();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    IEnumerator JumpLand(bool finalLanding)
    {
        var clawsRoutine = Boss.Claws.GroundLand(finalLanding);
        var headRoutine = Boss.Head.GroundLand(finalLanding);
        var wingsRoutine = Boss.Wings.GroundLand(finalLanding);
        var wingPlatesRoutine = Boss.WingPlates.GroundLand(finalLanding);
        var bodyRoutine = Boss.Body.GroundLand(finalLanding);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(Boss, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }
}
