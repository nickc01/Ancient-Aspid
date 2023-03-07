using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore;
using WeaverCore.Utilities;
using System.Linq;
using WeaverCore.Assets.Components;
using System;
using WeaverCore.Enums;

public class AncientAspid : Boss
{
    public enum Mode
    {
        /// <summary>
        /// The boss is flying around nearby the player
        /// </summary>
        Tactical,

        /// <summary>
        /// The boss is locked to the center of the room
        /// </summary>
        Offensive,

        /// <summary>
        /// The boss is on the ground
        /// </summary>
        Defensive
    }


    [Header("General Config")]
    [SerializeField]
    int attunedHealth = 2000;

    [SerializeField]
    int ascendedHealth = 2000;

    [SerializeField]
    int radiantHealth = 2000;



    [Header("Flight")]

    [SerializeField]
    float flightAcceleration = 10f;

    [SerializeField]
    float flightSpeedOverDistance = 1f;

    [SerializeField]
    float leftWallBuffer = 3f;

    [SerializeField]
    float rightWallBuffer = 3f;

    [SerializeField]
    float ceilingBuffer = 3f;

    [SerializeField]
    float floorBuffer = 3f;

    [SerializeField]
    float minFlightSpeed = 0.25f;

    [SerializeField]
    float degreesSlantThreshold = 30f;

    public float orbitReductionAmount = 0.1f;

    [Tooltip("How much offset should be applied when moving towards a target?")]
    public Vector2 flightOffset = new Vector2(3f, 3f);

    [Tooltip("How long before the target offset gets reset?")]
    public float flightOffsetResetTime = 0.6f;

    [Tooltip("How fast to change to a new offset value")]
    public float flightOffsetChangeTime = 0.3f;

    [Tooltip("How fast should the boss fly towards a target?")]
    public float flightSpeed = 7f;

    [Tooltip("The minimum flight speed of the boss")]
    public float minimumFlightSpeed = 3f;

    [Tooltip("The maximum velocity of the boss")]
    public float maximumFlightSpeed = 40f;

    [field: SerializeField]
    [field: Tooltip("When checked, will cause the boss to randomly fly nearby the target, rather than just targeting it directly")]
    public bool ApplyFlightVariance { get; private set; } = true;

    [SerializeField]
    [Tooltip("The target object that is used when aiming towards the player")]
    private Transform playerTarget;

    [SerializeField]
    [Header("Offensive Mode")]
    [Tooltip("The height the boss will be at when entering offensive mode")]
    float offensiveHeight = 10f;

    bool homeInOnTarget = false;

    [SerializeField]
    float homingAmount = 0f;

    [SerializeField]
    GameObject spitTargetLeft;

    [SerializeField]
    GameObject spitTargetRight;

    [SerializeField]
    AncientAspidPrefabs prefabs;

    [SerializeField]
    TailCollider tailCollider;

    [Header("Lunge")]

    [SerializeField]
    Vector3 lungeTargetOffset;

    [SerializeField]
    float lungeAnticTime = 0.65f;

    [SerializeField]
    AudioClip lungeAnticSound;

    [SerializeField]
    AudioClip lungeSound;

    [SerializeField]
    List<Collider2D> collidersDisableOnLunge;

    [SerializeField]
    List<Collider2D> collidersEnableOnLunge;

    [SerializeField]
    float lungeSpeed = 10f;

    [SerializeField]
    AudioClip lungeLandSoundLight;

    [SerializeField]
    float lungeLandSoundLightVolume = 0.75f;

    [SerializeField]
    AudioClip lungeLandSoundHeavy;

    [SerializeField]
    AudioClip lungeSlideSound;

    [SerializeField]
    float lungeSlideSoundVolume = 0.7f;

    [SerializeField]
    float lungeSlideDeacceleration = 2f;

    [SerializeField]
    float lungeSlideSlamThreshold = 2f;

    [SerializeField]
    List<float> lungeXShiftValues = new List<float>()
    {
        -0.56f,
        -0.1f,
        0
    };

    public float lungeTurnaroundSpeed = 1.5f;

    [SerializeField]
    List<ParticleSystem> groundSkidParticles;

    [SerializeField]
    float onGroundGravity = 0.75f;

    [SerializeField]
    GameObject lungeDashEffect;

    [SerializeField]
    ParticleSystem lungeRockParticles;

    [SerializeField]
    Transform lungeDashRotationOrigin;

    [field: SerializeField]
    public float lungeDownwardsLandDelay { get; private set; } = 0.5f;



    AudioPlayer lungeSlideSoundPlayer;

    [Header("Ground Laser")]
    [SerializeField]
    Vector2 groundLaserMinMaxAngle = new Vector2(-15f,45f);

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

    [SerializeField]
    float jumpTime = 0.5f;

    [SerializeField]
    float jumpGravity = 1f;

    [SerializeField]
    List<AudioClip> jumpSounds;

    [SerializeField]
    AudioClip jumpLandSound;

    [SerializeField]
    ShakeType jumpLandShakeType;

    [SerializeField]
    GameObject jumpLaunchEffectPrefab;

    [SerializeField]
    ShakeType jumpLaunchShakeType;

    [SerializeField]
    ParticleSystem jumpLandParticles;

    public AspidOrientation Orientation { get; private set; } = AspidOrientation.Left;
    public Mode AspidMode { get; private set; } = Mode.Tactical;

    private bool targetingTransform = false;

    public Vector3 TargetOffset { get; private set; }
    public Transform TargetTransform { get; private set; }
    public Rect CurrentRoomRect { get; private set; }

    bool flightEnabled = true;
    public bool FlightEnabled
    {
        get => flightEnabled;
        set
        {
            if (flightEnabled != value)
            {
                flightEnabled = value;
                if (!flightEnabled)
                {
                    Rbody.velocity = default;
                }
            }
        }
    }

    private Vector3 fixedTargetPos;
    public Vector3 TargetPosition
    {
        get
        {
            Vector3 target;
            if (targetingTransform && TargetTransform != null)
            {
                target = TargetTransform.position + TargetOffset;
            }
            else
            {
                target = fixedTargetPos + TargetOffset;
            }

            target.x = Mathf.Clamp(target.x, CurrentRoomRect.xMin + leftWallBuffer,CurrentRoomRect.xMax - rightWallBuffer);
            target.y = Mathf.Clamp(target.y, CurrentRoomRect.yMin + floorBuffer,CurrentRoomRect.yMax - ceilingBuffer);

            return target;
        }
    }

    public Vector3 SpitTargetLeft => spitTargetLeft.transform.position;
    public Vector3 SpitTargetRight => spitTargetRight.transform.position;

    public Rigidbody2D Rbody { get; private set; }

    public BodyController Body { get; private set; }
    public WingPlateController WingPlates { get; private set; }
    public HeadController Head { get; private set; }
    public WingsController Wings { get; private set; }
    public ClawController Claws { get; private set; }

    public Recoiler Recoil { get; private set; }

    /// <summary>
    /// The moves for the boss. Note: The order of the moves isn't guaranteed and can change anytime
    /// </summary>
    public List<AncientAspidMove> Moves { get; private set; }

    public EntityHealth HealthManager { get; private set; }

    /// <summary>
    /// Returns true if the player is to the right of the boss.
    /// </summary>
    public bool PlayerRightOfBoss => Player.Player1.transform.position.x >= transform.position.x;

    PlayerDamager[] damagers;
    float origOrbitReductionAmount;

    public int Damage
    {
        get => damagers[0].damageDealt;
        set
        {
            for (int i = damagers.GetLength(0) - 1; i >= 0; i--)
            {
                damagers[i].damageDealt = value;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        AncientAspidPrefabs.Instance = prefabs;
        Moves = GetComponents<AncientAspidMove>().ToList();
        damagers = GetComponentsInChildren<PlayerDamager>();
        CurrentRoomRect = RoomScanner.GetRoomBoundaries(transform.position);
        HealthManager = GetComponent<EntityHealth>();
        Rbody = GetComponent<Rigidbody2D>();
        Body = GetComponentInChildren<BodyController>();
        WingPlates = GetComponentInChildren<WingPlateController>();
        Wings = GetComponentInChildren<WingsController>();
        Head = GetComponentInChildren<HeadController>();
        Claws = GetComponentInChildren<ClawController>();
        Recoil = GetComponent<Recoiler>();
        SetTarget(playerTarget);
        StartCoroutine(VarianceResetter());

        switch (Difficulty)
        {
            case WeaverCore.Enums.BossDifficulty.Attuned:
                HealthManager.Health = attunedHealth;
                break;
            case WeaverCore.Enums.BossDifficulty.Ascended:
                HealthManager.Health = ascendedHealth;
                break;
            default:
                HealthManager.Health = radiantHealth;
                break;
        }
    }

    private void Start()
    {
        StartCoroutine(SlowUpdate());
        StartBoundRoutine(MainBossRoutine());
    }

    private IEnumerator SlowUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            CurrentRoomRect = RoomScanner.GetRoomBoundaries(transform.position);
        }
    }

    public IEnumerator MainBossRoutine()
    {
        while (true)
        {
            //START THE FIGHT IN TACTICAL MODE
            /*float tacticalTime = 4f;
            float tacticalStart = Time.time;

            int currentMoveIndex = 0;
            float lastMoveTime = Time.time;
            ShuffleMoves(Moves);
            while (Time.time - tacticalStart < tacticalTime)
            {
                if (currentMoveIndex == Moves.Count)
                {
                    currentMoveIndex = 0;
                    ShuffleMoves(Moves);
                }
                yield return new WaitUntil(() => !Head.HeadLocked);
                yield return CheckDirectionToPlayer();

                var move = Moves[currentMoveIndex];

                if (!move.MoveEnabled)
                {
                    currentMoveIndex++;
                    continue;
                }

                if (Time.time - lastMoveTime < move.PreDelay)
                {
                    continue;
                }

                yield return RunMove(move);

                lastMoveTime = Time.time + move.PostDelay;
                currentMoveIndex++;
            }
            yield return new WaitUntil(() => !Head.HeadLocked);*/
            //ENDING TACTICAL MODE
            var time = Time.time;
            while (Time.time <= time + 6f)
            {
                yield return CheckDirectionToPlayer(); //THIS IS A TEST
            }
            //yield return new WaitForSeconds(6f);
            yield return EnterGroundMode();
            yield return new WaitForSeconds(100f);



            //ENTERING OFFENSIVE MODE

            /*float offensiveTime = 10f;
            float offensiveStart = Time.time;

            yield return EnterCenterMode();
            AspidMode = Mode.Offensive;

            currentMoveIndex = 0;
            lastMoveTime = Time.time;
            ShuffleMoves(Moves);

            while (Time.time - offensiveStart < offensiveTime)
            {
                if (currentMoveIndex == Moves.Count)
                {
                    currentMoveIndex = 0;
                    ShuffleMoves(Moves);
                }
                yield return new WaitUntil(() => !Head.HeadLocked);
                yield return CheckDirectionToPlayer();

                var move = Moves[currentMoveIndex];

                if (!move.MoveEnabled)
                {
                    currentMoveIndex++;
                    continue;
                }

                if (Time.time - lastMoveTime < move.PreDelay)
                {
                    continue;
                }

                yield return RunMove(move);

                lastMoveTime = Time.time + move.PostDelay;
                currentMoveIndex++;
            }

            yield return new WaitUntil(() => !Head.HeadLocked);
            yield return ExitCenterMode();*/

            //ENDING OFFENSIVE MODE
            //ENTERING TACTICAL MODE
            AspidMode = Mode.Tactical;
        }
    }

    public float GetHeadAngle()
    {
        if (Orientation == AspidOrientation.Center)
        {
            if (Player.Player1.transform.position.x < transform.position.x)
            {
                return -60f;
            }
            else
            {
                return 60f;
            }
        }
        else
        {
            if (Orientation == AspidOrientation.Left)
            {
                return -60f;
            }
            else
            {
                return 60f;
            }
        }
    }

    IEnumerator EnterCenterMode()
    {
        Recoil.SetRecoilSpeed(0f);
        //var roomBounds = RoomScanner.GetRoomBoundaries(transform.position);
        //Debug.DrawLine(new Vector3(roomBounds.xMin,roomBounds.yMin), new Vector3(roomBounds.xMax,roomBounds.yMax), Color.cyan, 5f);
        //Debug.DrawLine(transform.position, new Vector3(roomBounds.xMin, transform.position.y), Color.cyan, 5f);
        //Debug.DrawLine(transform.position, new Vector3(roomBounds.xMax, transform.position.y), Color.cyan, 5f);

        //Debug.DrawLine(transform.position, new Vector3(transform.position.x, roomBounds.yMin), Color.cyan, 5f);
        //Debug.DrawLine(transform.position, new Vector3(transform.position.x, roomBounds.yMax), Color.cyan, 5f);
        minFlightSpeed /= 3f;
        var newTarget = new Vector3(Mathf.Lerp(CurrentRoomRect.xMin, CurrentRoomRect.xMax, 0.5f), CurrentRoomRect.yMin + offensiveHeight);
        SetTarget(newTarget);
        //flightOffset /= 4f;
        minimumFlightSpeed /= 4f;
        homeInOnTarget = true;
        //orbitReductionAmount *= 4f;
        flightSpeed /= 2f;

        origOrbitReductionAmount = orbitReductionAmount;

        yield return ChangeDirection(AspidOrientation.Center);

        float timer = 0f;

        while (timer <= 0.25f/*Vector3.Distance(transform.position, newTarget) > 0.5f*/)
        {
            orbitReductionAmount += 3f * orbitReductionAmount * Time.deltaTime;
            if (Vector3.Distance(transform.position, newTarget) <= 2f)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0f;
            }
            yield return null;
        }
        Recoil.ResetRecoilSpeed();
        //yield return new WaitUntil(() => Vector3.Distance(transform.position, newTarget) <= 1f);
        //yield return new WaitForSeconds(0.25f);
    }

    IEnumerator ExitCenterMode()
    {
        return ExitCenterMode(GetOrientationToPlayer());
    }

    IEnumerator ExitCenterMode(AspidOrientation orientation,bool wait = true)
    {
        minFlightSpeed *= 3f;
        SetTarget(playerTarget);
        //flightOffset *= 4f;
        minimumFlightSpeed *= 4f;
        homeInOnTarget = false;
        orbitReductionAmount = origOrbitReductionAmount;
        flightSpeed *= 2f;
        yield return ChangeDirection(orientation);

        if (wait)
        {
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator EnterGroundMode()
    {
        if (Orientation == AspidOrientation.Center)
        {
            yield return ExitCenterMode(GetOrientationToPlayer(),false);
        }

        Body.PlayDefaultAnimation = false;

        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        ApplyFlightVariance = false;

        /*if (Head.LookingDirection >= 0f)
        {
            TargetOffset = lungeTargetOffset;
        }
        else
        {
            TargetOffset = lungeTargetOffset.With(x: -lungeTargetOffset.x);
        }*/

        if (Head.LookingDirection >= 0f)
        {
            SetTarget(Player.Player1.transform.position + lungeTargetOffset);
        }
        else
        {
            SetTarget(Player.Player1.transform.position + new Vector3(-lungeTargetOffset.x, lungeTargetOffset.y, lungeTargetOffset.z));
        }

        var headRoutine = Head.LockHead(Head.LookingDirection >= 0f ? 60f : -60f);
        var bodyRoutine = Body.RaiseTail();
        var minWaitTimeRoutine = Wait(0.5f);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, headRoutine, bodyRoutine, minWaitTimeRoutine);

        yield return awaiter.WaitTillDone();

        FlightEnabled = false;
        Recoil.SetRecoilSpeed(0f);

        yield return new WaitUntil(() => !Claws.DoingBasicAttack);

        foreach (var claw in Claws.claws)
        {
            yield return claw.LockClaw();
        }

        //yield return Body.RaiseTail();

        if (lungeAnticSound != null)
        {
            WeaverAudio.PlayAtPoint(lungeAnticSound, transform.position);
        }

        Wings.PrepareForLunge();
        Claws.PrepareForLunge();

        yield return new WaitForSeconds(lungeAnticTime);

        Wings.DoLunge();
        Claws.DoLunge();
        WingPlates.DoLunge();
        Head.DoLunge();

        if (lungeSound != null)
        {
            WeaverAudio.PlayAtPoint(lungeSound, transform.position);
        }

        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.SmallShake);

        foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = true;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = false;
        }



        bool towardsPlayer = UnityEngine.Random.Range(0f, 1f) >= 0.5f;

        Vector3 destination;

        if (UnityEngine.Random.Range(0,2) == 1 || true)
        {
            destination = Player.Player1.transform.position;
        }
        else
        {
            destination = transform.position.With(y: CurrentRoomRect.yMin);
        }

        var angleToDestination = VectorUtilities.VectorToDegrees((destination - transform.position).normalized);

        var downwardAngle = Vector3.Dot(Vector3.right, (destination - transform.position).normalized) * 90f;

        lungeDashEffect.SetActive(true);
        lungeDashRotationOrigin.SetZLocalRotation(angleToDestination);


        bool steepAngle = Mathf.Abs(downwardAngle) >= degreesSlantThreshold;

        Debug.Log("Angle = " + downwardAngle);
        Debug.Log("Steep Angle = " + steepAngle);

        Rbody.velocity = (destination - transform.position).normalized * lungeSpeed;

        yield return null;

        var yVelocity = Rbody.velocity.y;
        var xVelocity = Rbody.velocity.x;
        var halfVelocity = Mathf.Abs(Rbody.velocity.y) / 2f;

        yield return new WaitUntil(() => Mathf.Abs(Rbody.velocity.y) < halfVelocity);

        Rbody.velocity = default;

        StartBoundRoutine(PlayLungeLandAnimation());

        var headLandRoutine = Head.PlayLanding(steepAngle);
        var bodyLandRoutine = Body.PlayLanding(steepAngle);
        var wingPlateLandRoutine = WingPlates.PlayLanding(steepAngle);
        var wingsLandRoutine = Wings.PlayLanding(steepAngle);
        var clawsLandRoutine = Claws.PlayLanding(steepAngle);
        var shiftPartsRoutine = ShiftBodyParts(steepAngle, Body, WingPlates, Wings);

        List<uint> landingRoutines = new List<uint>();

        bool landingCancelled = false;

        var landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        bool slammed = false;

        uint switchDirectionRoutine = 0;
        AspidOrientation oldOrientation = Orientation;

        Rbody.gravityScale = onGroundGravity;

        void StartSlideSound()
        {
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

        void StopSlideSound()
        {
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

        if (steepAngle)
        {
            if (lungeLandSoundLight != null)
            {
                WeaverAudio.PlayAtPoint(lungeLandSoundLight, transform.position, lungeLandSoundLightVolume);
            }

            CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.AverageShake);

            StartSlideSound();

            var slideSpeed = (destination - transform.position).magnitude * lungeSpeed;

            //var horizontalSpeed = Mathf.Lerp(slideSpeed, Mathf.Abs(yVelocity), 0.65f);
            var horizontalSpeed = Mathf.Lerp(Mathf.Abs(yVelocity), Mathf.Abs(xVelocity), 0.5f);

            if (downwardAngle >= 0f)
            {
                Rbody.velocity = new Vector2(horizontalSpeed, 0f);
            }
            else
            {
                Rbody.velocity = new Vector2(-horizontalSpeed, 0f);
            }

            yield return null;

            var lastVelocityX = Rbody.velocity.x;

            do
            {
                //TODO - CALL SWITCH DIRECTION FUNCTIONS SOMEWHERE HERE
                lastVelocityX = Rbody.velocity.x;
                if (Rbody.velocity.x >= 0f)
                {
                    Rbody.velocity = Rbody.velocity.With(x: Rbody.velocity.x - (lungeSlideDeacceleration * Time.deltaTime));
                }
                else
                {
                    Rbody.velocity = Rbody.velocity.With(x: Rbody.velocity.x + (lungeSlideDeacceleration * Time.deltaTime));
                }

                if (switchDirectionRoutine == 0)
                {
                    if ((Orientation == AspidOrientation.Right && Player.Player1.transform.position.x < transform.position.x) ||
                        (Orientation == AspidOrientation.Left && Player.Player1.transform.position.x > transform.position.x))
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
                            Head.ToggleLaserBubbles(true);
                            yield return SwitchDirectionDuringSlide();
                            //yield return Head.PlayGroundLaserAntic(0);
                            if (Rbody.velocity.x != 0)
                            {
                                yield return new WaitUntil(() => Rbody.velocity.x == 0);
                            }
                            Head.ToggleLaserBubbles(false);
                            yield break;
                        }

                        switchDirectionRoutine = StartBoundRoutine(SwitchDirections());
                    }
                }

                if (Rbody.velocity.y < -0.5f)
                {
                    StopSlideSound();
                }
                else
                {
                    StartSlideSound();
                }

                yield return null;
            } while (Mathf.Abs(Rbody.velocity.x) > Mathf.Abs(lastVelocityX / 3f) && Mathf.Abs(Rbody.velocity.x) > 0.1f);

            if (Mathf.Abs(Rbody.velocity.x - lastVelocityX) >= lungeSlideSlamThreshold)
            {
                //THE BOSS SLAMMED INTO SOMETHING
                if (lungeLandSoundHeavy != null)
                {
                    WeaverAudio.PlayAtPoint(lungeLandSoundHeavy, transform.position);
                }
                CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
                slammed = true;
            }
            else
            {
                //THE BOSS STOPPED GRACEFULLY
            }

            StopSlideSound();

            if (!landingAwaiter.Done)
            {
                landingCancelled = true;
                foreach (var id in landingRoutines)
                {
                    StopBoundRoutine(id);
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

        Rbody.velocity = Rbody.velocity.With(x: 0f);

        yield return new WaitUntil(() => landingCancelled || landingAwaiter.Done);

        var headFinishRoutine = Head.FinishLanding(slammed);
        var bodyFinishRoutine = Body.FinishLanding(slammed);
        var wingPlateFinishRoutine = WingPlates.FinishLanding(slammed);
        var wingsFinishRoutine = Wings.FinishLanding(slammed);
        var clawsFinishRoutine = Claws.FinishLanding(slammed);

        var finishAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, headFinishRoutine, bodyFinishRoutine, wingPlateFinishRoutine, wingsFinishRoutine, clawsFinishRoutine);

        yield return finishAwaiter.WaitTillDone();

        if (switchDirectionRoutine != 0 && Orientation == oldOrientation)
        {
            yield return new WaitUntil(() => Orientation != oldOrientation);
        }

        if (steepAngle)
        {
            //yield return FireGroundLaserSweep();
            yield return FireGroundLaserAtPlayer();
            /*float startAngle, endAngle;

            if (Orientation == AspidOrientation.Right)
            {
                startAngle = groundLaserMinMaxAngle.x;
                endAngle = groundLaserMinMaxAngle.y;
            }
            else
            {
                startAngle = 180f - groundLaserMinMaxAngle.x;
                endAngle = 180f - groundLaserMinMaxAngle.y;
            }

            var startQ = Quaternion.Euler(0f, 0f, startAngle);
            var endQ = Quaternion.Euler(0f, 0f, endAngle);

            yield return new WaitForSeconds(groundLaserMaxDelay);

            yield return Head.FireGroundLaser(startQ,endQ, groundLaserFireDuration);*/
        }


        //TODO - AIM TOWARDS THE PLAYER OR TOWARDS THE GROUND

        //TODO - PLAY PREPARE ANIMATION. The Wings stop, the claws stop and the body lifts up

        //TODO - Either Lunge Towards the player or down towards the ground. If the enemy lunges towards the player,
        //the boss will continue to slide on the ground until he stops.

        //Make sure the claws and wings stay stopped during this state

        yield return new WaitForSeconds(0.5f);

        yield return DoGroundJump(3, JumpTargeter);

        yield break;
    }

    Vector2 JumpTargeter(int time)
    {
        if (time % 2 == 0)
        {
            return Player.Player1.transform.position;
        }
        else
        {
            //var area = CurrentRoomRect
            var minX = CurrentRoomRect.xMin + 6f;
            var maxX = CurrentRoomRect.xMax - 6f;

            var randValue = UnityEngine.Random.Range(minX, maxX);

            randValue = Mathf.Clamp(randValue, transform.position.x - 15f, transform.position.x + 15f);

            return new Vector2(randValue,transform.position.y);
        }
    }

    IEnumerator FireGroundLaserSweep()
    {
        float startAngle, endAngle;

        if (Orientation == AspidOrientation.Right)
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
        var playerAngle = GetAngleToPlayer();

        if (playerAngle < 0)
        {
            playerAngle += 360f;
        }

        //Debug.Log("Player Angle = " + playerAngle);

        //var startAngle = 0f;

        const float angleLimit = 45f;

        if (Orientation == AspidOrientation.Left)
        {
            //startAngle = 180f;
            playerAngle = Mathf.Clamp(playerAngle, 180 - angleLimit, 180 + angleLimit);
            if (playerAngle > 180)
            {
                playerAngle = 180f;
            }
        }
        else
        {
            playerAngle = Mathf.Clamp(playerAngle, angleLimit, angleLimit);
            if (playerAngle < 0f)
            {
                playerAngle = 0f;
            }
        }

        return FireGroundLaser(playerAngle, playerAngle);
    }

    IEnumerator FireGroundLaser(float startAngle, float endAngle)
    {
        /*float startAngle, endAngle;

        if (Orientation == AspidOrientation.Right)
        {
            startAngle = groundLaserMinMaxAngle.x;
            endAngle = groundLaserMinMaxAngle.y;
        }
        else
        {
            startAngle = 180f - groundLaserMinMaxAngle.x;
            endAngle = 180f - groundLaserMinMaxAngle.y;
        }*/

        var startQ = Quaternion.Euler(0f, 0f, startAngle);
        var endQ = Quaternion.Euler(0f, 0f, endAngle);

        yield return new WaitForSeconds(groundLaserMaxDelay);

        yield return Head.FireGroundLaser(startQ, endQ, groundLaserFireDuration,true);
    }

    IEnumerator SwitchDirectionDuringSlide()
    {
        var oldDirection = Orientation;
        var newDirection = oldDirection == AspidOrientation.Left ? AspidOrientation.Right : AspidOrientation.Left;

        var clawsRoutine = Claws.SlideSwitchDirection(oldDirection, newDirection);
        var headRoutine = Head.SlideSwitchDirection(oldDirection, newDirection);
        var bodyRoutine = Body.SlideSwitchDirection(oldDirection, newDirection);
        var wingPlateRoutine = WingPlates.SlideSwitchDirection(oldDirection, newDirection);
        var wingsRoutine = Wings.SlideSwitchDirection(oldDirection, newDirection);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, bodyRoutine, wingPlateRoutine, wingsRoutine);

        yield return awaiter.WaitTillDone();

        Orientation = newDirection;
    }

    IEnumerator ShiftBodyParts(bool slide, params AspidBodyPart[] bodyParts)
    {
        /*if (!slide)
        {
            yield return new WaitForSeconds(lungeDownwardsLandDelay);
        }*/

        List<float> defaultXValues = bodyParts.Select(p => p.transform.localPosition.x).ToList();
        List<AspidBodyPart> bodyPartsList = bodyParts.ToList();

        bool firstTime = true;

        for (int i = 0; i < lungeXShiftValues.Count; i++)
        {
            int index = 0;
            foreach (var part in bodyPartsList)
            {
                float shiftAmount = lungeXShiftValues[i];
                if (Head.LookingDirection >= 60)
                {
                    shiftAmount = -shiftAmount;
                }
                part.transform.SetXLocalPosition(defaultXValues[index] + shiftAmount);
                index++;
            }
            if (firstTime)
            {
                firstTime = false;
                if (!slide)
                {
                    i--;
                    yield return new WaitForSeconds(lungeDownwardsLandDelay);
                    continue;
                }
            }
            yield return new WaitForSeconds(1f / 12f);

            for (int j = bodyPartsList.Count - 1; j >= 0; j--)
            {
                float shiftAmount = lungeXShiftValues[i];
                if (Head.LookingDirection >= 60)
                {
                    shiftAmount = -shiftAmount;
                }
                if (bodyPartsList[j].transform.localPosition.x != defaultXValues[j] + shiftAmount)
                {
                    //Debug.Log("REMOVED BODY PART = " + bodyPartsList[j].GetType().FullName);
                    bodyPartsList.RemoveAt(j);
                    defaultXValues.RemoveAt(j);
                }
            }
        }
    }

    IEnumerator PlayLungeLandAnimation()
    {
        yield break;
    }

    IEnumerator ExitGroundMode()
    {
        //TODO - Play the prepare animation for lifting off of the ground

        //TODO - Play the jump animation for jumping back up into the air

        //TODO - Switch back to tactic mode and play the default claw and wing animations

        ApplyFlightVariance = true;

        foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = false;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = true;
        }

        yield break;
    }

    IEnumerator DoGroundJump(int jumpTimes, Func<int, Vector2> getTarget)
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

            var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, jumpTime, jumpGravity);

            Rbody.gravityScale = jumpGravity;
            Rbody.velocity = velocity;

            Claws.OnGround = false;

            yield return JumpSwitchDirectionPrepare();
            yield return JumpSwitchDirection();
            if (Orientation == AspidOrientation.Left)
            {
                Orientation = AspidOrientation.Right;
            }
            else
            {
                Orientation = AspidOrientation.Left;
            }

            yield return new WaitUntil(() => Rbody.velocity.y <= 0f);
            yield return new WaitForSeconds(jumpTime / 5f);
            yield return new WaitUntil(() => Rbody.velocity.y >= 0f);

            Rbody.velocity = Rbody.velocity.With(x: 0f);

            Claws.OnGround = true;

            if (jumpLandSound != null)
            {
                WeaverAudio.PlayAtPoint(jumpLandSound, transform.position);
            }

            CameraShaker.Instance.Shake(jumpLandShakeType);
            jumpLandParticles.Play();

            yield return JumpLand(i == jumpTimes - 1);
        }

        Rbody.velocity = default;
        Rbody.gravityScale = 0f;
    }

    IEnumerator JumpSwitchDirection()
    {
        var oldOrientation = Orientation;
        AspidOrientation newOrientation;
        if (oldOrientation == AspidOrientation.Left)
        {
            newOrientation = AspidOrientation.Right;
        }
        else
        {
            newOrientation = AspidOrientation.Left;
        }

        var clawsRoutine = Claws.MidJumpChangeDirection(oldOrientation,newOrientation);
        var headRoutine = Head.MidJumpChangeDirection(oldOrientation, newOrientation);
        var wingsRoutine = Wings.MidJumpChangeDirection(oldOrientation, newOrientation);
        var wingPlatesRoutine = WingPlates.MidJumpChangeDirection(oldOrientation, newOrientation);
        var bodyRoutine = Body.MidJumpChangeDirection(oldOrientation, newOrientation);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }

    IEnumerator JumpSwitchDirectionPrepare()
    {
        var clawsRoutine = Claws.WaitTillChangeDirectionMidJump();
        var headRoutine = Head.WaitTillChangeDirectionMidJump();
        var wingsRoutine = Wings.WaitTillChangeDirectionMidJump();
        var wingPlatesRoutine = WingPlates.WaitTillChangeDirectionMidJump();
        var bodyRoutine = Body.WaitTillChangeDirectionMidJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        yield return awaiter.WaitTillDone();
    }

    IEnumerator JumpPrepare()
    {
        var clawsRoutine = Claws.GroundPrepareJump();
        var headRoutine = Head.GroundPrepareJump();
        var wingsRoutine = Wings.GroundPrepareJump();
        var wingPlatesRoutine = WingPlates.GroundPrepareJump();
        var bodyRoutine = Body.GroundPrepareJump();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine,bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    IEnumerator JumpLaunch()
    {
        var clawsRoutine = Claws.GroundLaunch();
        var headRoutine = Head.GroundLaunch();
        var wingsRoutine = Wings.GroundLaunch();
        var wingPlatesRoutine = WingPlates.GroundLaunch();
        var bodyRoutine = Body.GroundLaunch();
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }

    IEnumerator JumpLand(bool finalLanding)
    {
        var clawsRoutine = Claws.GroundLand(finalLanding);
        var headRoutine = Head.GroundLand(finalLanding);
        var wingsRoutine = Wings.GroundLand(finalLanding);
        var wingPlatesRoutine = WingPlates.GroundLand(finalLanding);
        var bodyRoutine = Body.GroundLand(finalLanding);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        yield break;
    }



    private AspidOrientation GetOrientationToPlayer()
    {
        if (transform.position.x >= Player.Player1.transform.position.x)
        {
            return AspidOrientation.Left;
        }
        else
        {
            return AspidOrientation.Right;
        }
    }

    /*IEnumerator CenterizeTest()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            yield return ChangeDirection(AspidOrientation.Center);
            yield return new WaitForSeconds(1f);
            yield return ChangeDirection(AspidOrientation.Left);
            yield return new WaitForSeconds(1f);
            yield return ChangeDirection(AspidOrientation.Center);
            yield return new WaitForSeconds(1f);
            yield return ChangeDirection(AspidOrientation.Right);
        }
    }*/

    IEnumerator CheckDirectionToPlayer()
    {
        if (transform.position.x >= Player.Player1.transform.position.x)
        {
            if (Orientation == AspidOrientation.Right)
            {
                yield return ChangeDirection(AspidOrientation.Left);
            }
            else
            {
                yield return null;
            }
        }
        else
        {
            if (Orientation == AspidOrientation.Left)
            {
                yield return ChangeDirection(AspidOrientation.Right);
            }
            else
            {
                yield return null;
            }
        }
        /*while (true)
        {
            if (transform.position.x >= Player.Player1.transform.position.x)
            {
                if (Orientation == AspidOrientation.Right)
                {
                    yield return ChangeDirection(AspidOrientation.Left);
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                if (Orientation == AspidOrientation.Left)
                {
                    yield return ChangeDirection(AspidOrientation.Right);
                }
                else
                {
                    yield return null;
                }
            }
            //yield return new WaitForSeconds(4f);
            //yield return ChangeDirection();
        }*/
    }

    /*private IEnumerator ChangeMode(Mode mode)
    {
        if (mode == Mode.Offensive)
        {
            if (AspidMode == Mode.Tactical)
            {

            }
            else if (AspidMode == Mode.Defensive)
            {

            }
        }
    }*/

    private IEnumerator ChangeDirection(AspidOrientation newOrientation)
    {
        if (Orientation == newOrientation)
        {
            yield break;
        }

        WeaverLog.Log("CHANGING DIRECTION to = " + newOrientation);
        yield return Body.PrepareChangeDirection();
        WeaverLog.Log("PREP DONE");
        IEnumerator bodyRoutine = Body.ChangeDirection(newOrientation);
        IEnumerator wingPlateRoutine = WingPlates.ChangeDirection(newOrientation);
        IEnumerator wingRoutine = Wings.ChangeDirection(newOrientation);
        IEnumerator headRoutine = Head.ChangeDirection(newOrientation);
        IEnumerator clawsRoutine = Claws.ChangeDirection(newOrientation);

        var changeDirectionTime = Body.GetChangeDirectionTime();

        switch (newOrientation)
        {
            case AspidOrientation.Left:
                tailCollider.ChangeOrientation(TailCollider.Orientation.Left, changeDirectionTime);
                break;
            case AspidOrientation.Center:
                tailCollider.ChangeOrientation(TailCollider.Orientation.Center, changeDirectionTime);
                break;
            case AspidOrientation.Right:
                tailCollider.ChangeOrientation(TailCollider.Orientation.Right, changeDirectionTime);
                break;
        }

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitRoutines(this,
            bodyRoutine,
            wingRoutine,
            headRoutine,
            wingPlateRoutine,
            clawsRoutine);

        yield return new WaitUntil(() => awaiter.Done);
        WeaverLog.Log("FINISHED CHANGING DIRECTION to = " + newOrientation);
        Orientation = newOrientation;
    }

    /*static IEnumerator StartAfterDelay(IEnumerator routine, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return routine;
    }*/

    private IEnumerator VarianceResetter()
    {
        while (true)
        {
            if (ApplyFlightVariance)
            {
                Vector3 oldOffset = TargetOffset;
                //MathUtilities.PolarToCartesian(Random.Range(0f,360f),1f)
                //Random.insideUnitCircle
                Vector2 newOffset = UnityEngine.Random.insideUnitCircle * flightOffset;
                for (float t = 0; t < flightOffsetChangeTime; t += Time.deltaTime)
                {
                    TargetOffset = Vector2.Lerp(oldOffset, newOffset, Mathf.SmoothStep(0, 1, t / flightOffsetChangeTime));
                    yield return null;
                    if (!ApplyFlightVariance)
                    {
                        TargetOffset = default;
                        break;
                    }
                }
                if (ApplyFlightVariance)
                {
                    yield return new WaitForSeconds(flightOffsetResetTime);
                }
            }
            else
            {
                TargetOffset = default;
                yield return null;
            }
        }
    }

    void ShuffleMoves(List<AncientAspidMove> moves)
    {
        var lastMove = moves[moves.Count - 1];
        moves.RandomizeList();
        if (moves[0] == lastMove)
        {
            var swappedMove = moves[moves.Count - 1];
            moves[moves.Count - 1] = lastMove;
            moves[0] = swappedMove;
        }
    }

    private void Update()
    {
        if (FlightEnabled)
        {
            var distanceToTarget = Vector2.Distance(TargetPosition, transform.position);
            var directionToTarget = (Vector2)(TargetPosition - transform.position);

            Debug.DrawLine(transform.position, TargetPosition, Color.cyan);

            var maxVelocity = distanceToTarget * flightSpeedOverDistance;

            if (maxVelocity < minFlightSpeed)
            {
                maxVelocity = minFlightSpeed;
            }

            //maxVelocity = float.PositiveInfinity;

            var prevVelocity = Rbody.velocity;

            var newVelocity = prevVelocity + (directionToTarget.normalized * (flightAcceleration * Time.deltaTime));

            if (newVelocity.magnitude > maxVelocity)
            {
                newVelocity = newVelocity.normalized * maxVelocity;
            }

            Rbody.velocity = newVelocity;
        }

        /*var newVelocity = prevVelocity + flightSpeed * Time.deltaTime;
        if (newVelocity >= maxVelocity)
        {
            newVelocity = maxVelocity;
        }

        Rbody.velocity = (TargetPosition - transform.position).normalized * newVelocity;*/

        /*float distanceToTarget = Vector3.Distance(TargetPosition, transform.position);
        float distanceSquared = distanceToTarget * distanceToTarget;
        float velocity = flightSpeed + distanceSquared;

        Vector2 targetV2 = TargetPosition;

        Vector3 targetDirection = (TargetPosition - transform.position).normalized * flightSpeed * Time.deltaTime;

        Rbody.velocity = Vector3.RotateTowards(Rbody.velocity, (targetV2 - (Vector2)transform.position).normalized * Rbody.velocity.magnitude, (orbitReductionAmount / 100) * distanceSquared * Time.deltaTime, 0f);

        Rbody.velocity += (Vector2)targetDirection;

        if (Rbody.velocity.magnitude > maximumFlightSpeed + (distanceSquared))
        {
            Rbody.velocity = Rbody.velocity.normalized * (maximumFlightSpeed + (distanceSquared));
        }

        if (Rbody.velocity.magnitude < minimumFlightSpeed)
        {
            Rbody.velocity = Rbody.velocity.normalized * (minimumFlightSpeed);
        }

        if (homeInOnTarget)
        {
            Rbody.velocity *= Mathf.Clamp(distanceToTarget,0.05f + homingAmount, 1f);
            //transform.position = Vector3.Lerp(transform.position,TargetPosition,homingAmount * Time.deltaTime / 100f);
            //Rbody.velocity = Vector3.RotateTowards(Rbody.velocity, (targetV2 - (Vector2)transform.position).normalized * Rbody.velocity.magnitude, homingAmount * (1f / Mathf.Clamp(distanceToTarget,0.1f,9999f)) * Time.deltaTime, 0f);
        }*/

        if (Player.Player1.transform.position.y > transform.position.y && Mathf.Abs(Player.Player1.transform.position.x - transform.position.x) <= (Player.Player1.transform.position.y - transform.position.y))
        {
            HealthManager.Invincible = true;
            HealthManager.DeflectBlows = true;
        }
        else
        {
            HealthManager.Invincible = false;
            HealthManager.DeflectBlows = false;
        }
    }

    protected override void OnStun()
    {
        base.OnStun();
        Body.OnStun();
        Head.OnStun();
        Claws.OnStun();
        WingPlates.OnStun();
        Wings.OnStun();
        SetTarget(playerTarget);
        if (lungeSlideSoundPlayer != null)
        {
            lungeSlideSoundPlayer.Delete();
            lungeSlideSoundPlayer = null;
        }
        Rbody.gravityScale = 0f;
        Rbody.velocity = default;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(TargetPosition, Vector3.one);
    }

    public void SetTarget(Transform targetTransform)
    {
        targetingTransform = targetTransform != null;
        TargetTransform = targetTransform;
    }

    public void SetTarget(Vector3 fixedTarget)
    {
        targetingTransform = false;
        TargetTransform = null;
        fixedTargetPos = fixedTarget;
    }

    public float GetAngleToPlayer()
    {
        var direction = (Player.Player1.transform.position + new Vector3(0f,0.5f,0f) - Head.transform.position).normalized;

        return MathUtilities.CartesianToPolar(direction).x;
    }
}
