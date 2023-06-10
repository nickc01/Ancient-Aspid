using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore.Utilities;
using System.Linq;
using WeaverCore.Assets.Components;
using System;
using WeaverCore.Enums;
using NavMeshPlus.Components;
using UnityEngine.AI;
using UnityEngine.UIElements;
using GlobalEnums;
using System.Security.Cryptography;

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

    public enum PathfindingMode
    {
        None,
        FollowPlayer,
        FollowTarget
    }

    public enum BossPhase
    {
        Default,
        Phase1,
        Phase2,
        Phase2A,
        Phase2B,
        Phase2C,
        Phase3,
        Phase3A,
        Phase3B,
        Phase3C,
        Phase4
    }


    [field: Header("General Config")]
    [field: SerializeField]
    public bool TrailerMode { get; private set; } = false;

    public BossPhase Phase { get; set; } = AncientAspid.BossPhase.Default;

    [SerializeField]
    MusicPack bossMusic;

    [SerializeField]
    float trailerModeDestY = 0f;

    [SerializeField]
    int attunedHealth = 2000;

    [SerializeField]
    int ascendedHealth = 2000;

    [SerializeField]
    int radiantHealth = 2000;

    [SerializeField]
    float camBoxWidth = 27.8f;

    [SerializeField]
    float camBoxHeight = 15.6f;

    [SerializeField]
    bool allMovesDisabled = false;


    [field: Header("Sleep Mode")]
    [field: SerializeField]
    public bool StartAsleep { get; private set; } = false;

    public bool FullyAwake { get; private set; } = false;

    [SerializeField]
    List<float> sleepScalesBefore = new List<float>
    {
        0.1f,
        0.3f
    };

    [SerializeField]
    List<float> sleepScalesAfter = new List<float>
    {
        0.3f,
        1f
    };

    [SerializeField]
    List<GameObject> enableOnWakeUp;

    [SerializeField]
    float sleepJumpVelocity = 20f;

    [SerializeField]
    float sleepScaleChangeSpeed = 1 / 12;

    [SerializeField]
    SpriteRenderer sleepSprite;

    [SerializeField]
    AudioClip sleepAwakeSound;

    [SerializeField]
    float sleepAwakeSoundPitch = 1f;

    [SerializeField]
    float sleepPreJumpDelay = 0.75f;

    [SerializeField]
    float sleepJumpGravity = 4;

    [SerializeField]
    float sleepShakeIntensity = 0.1f;

    [SerializeField]
    float sleepRoarDuration = 2f;

    [SerializeField]
    AudioClip sleepRoarSound;

    [SerializeField]
    float sleepRoarSoundPitch = 1f;

    [SerializeField]
    AudioClip sleepRoarQuickSound;

    [SerializeField]
    float sleepRoarQuickTime = 1f;


    [Header("Pathfinding")]
    [SerializeField]
    bool pathfindingEnabled = true;

    [SerializeField]
    float pathRegenInterval = 0.5f;

    [SerializeField]
    Vector2 playerMoveBoxSize = new Vector2(5,5);



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

    public Vector3 ExtraTargetOffset;
    //public Vector3 LaserTargetOffset;

    public bool EnableTargetHeightRange = false;

    public Vector2 TargetHeightRange;

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

    [SerializeField]
    WeaverCameraLock centerCamLock;

    [SerializeField]
    Vector2 centerModePlatformDest = new Vector2(220.8f,211.5f);

    [SerializeField]
    float centerModePlatformTime = 0.75f;

    [SerializeField]
    AnimationCurve centerModePlatformCurve;

    [SerializeField]
    AudioClip centerModeRumbleSound;

    [SerializeField]
    ParticleSystem centerModeSummonGrass;

    [SerializeField]
    float centerModeRoarDelay = 0.1f;

    [field: SerializeField]
    public bool OffensiveModeEnabled { get; private set; } = true;

    [SerializeField]
    AudioClip centerModeRiseSound;

    [SerializeField]
    AudioClip centerModeRoarSound;

    [SerializeField]
    float centerModeRoarSoundPitch = 1f;

    [SerializeField]
    float centerModeRoarDuration = 0.75f;

    [SerializeField]
    CameraLockArea centerPlatformLockArea;

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

    [field: SerializeField]
    public bool GroundModeEnabled { get; private set; } = true;

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

    [SerializeField]
    ParticleSystem stompSplash;

    [SerializeField]
    ParticleSystem stompPillar;

    [SerializeField]
    Vector3 groundSplashSpawnOffset;

    [SerializeField]
    Vector2Int groundSplashBlobCount = new Vector2Int(5,10);

    [SerializeField]
    Vector2 groundSplashAngleRange = new Vector2(15, 180 - 15);

    [SerializeField]
    Vector2 groundSplashVelocityRange = new Vector2(3, 15);

    [Header("Death")]
    [SerializeField]
    WeaverCameraLock deathCamLock;

    [SerializeField]
    AudioClip deathSound;

    [SerializeField]
    float deathSpitDelay = 2f;

    [SerializeField]
    float flyAwayDelay = 0.5f;

    [SerializeField]
    float flyAwayAcceleration = 5f;

    [SerializeField]
    AudioClip flyAwayThunkSound;

    [SerializeField]
    DroppedItem itemPrefab;

    [SerializeField]
    float deathItemVelocity = 5f;

    [SerializeField]
    AudioClip spitItemAudio;

    public AspidOrientation Orientation { get; private set; } = AspidOrientation.Left;
    public Mode AspidMode { get; private set; } = Mode.Tactical;

    TargetOverride primaryTarget = new TargetOverride();

    List<TargetOverride> targetOverrides = new List<TargetOverride>();

    //private bool targetingTransform = false;

    public Vector3 TargetOffset { get; private set; }
    //public Transform TargetTransform { get; private set; }
    public RoomScanResult CurrentRoomRect { get; private set; } = new RoomScanResult();

    [SerializeField]
    bool flightEnabled = true;
    public bool FlightEnabled
    {
        get => flightEnabled;
        set
        {
            //Debug.Log($"Flight Enabled = {value}");
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

    //bool targetFrozen = false;
    //Func<Vector3> frozenTargetPosition = default;

    //private Vector3 fixedTargetPos;
    public Vector3 TargetPosition
    {
        get
        {
            Vector3 target = new Vector3(float.PositiveInfinity,float.PositiveInfinity);
            for (int i = targetOverrides.Count - 1; i >= 0; i--)
            {
                if (targetOverrides[i].HasPositionSet)
                {
                    target = targetOverrides[i].TargetPosition;
                    break;
                }
            }

            if (target.x == float.PositiveInfinity)
            {
                target = primaryTarget.TargetPosition + ExtraTargetOffset;
            }
            /*if (targetFrozen)
            {
                return frozenTargetPosition();
            }*/

            //Vector3 target;
            /*if (targetingTransform && TargetTransform != null)
            {
                target = TargetTransform.position;
            }
            else
            {
                target = fixedTargetPos;
            }*/

            if (EnableTargetHeightRange)
            {
                target.y = Mathf.Clamp(target.y, TargetHeightRange.x, TargetHeightRange.y);
            }

            target += TargetOffset;

            //target += TargetOffset + ExtraTargetOffset + LaserTargetOffset;

            return target;
        }
    }

    public Vector3 PathfindingTarget
    {
        get
        {
            if (NavMesh.FindClosestEdge(GetPathTarget(PathingMode) + new Vector3(0f,4f), out var hit, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                if (NavMesh.FindClosestEdge(GetPathTarget(PathingMode) + new Vector3(0f, -4f), out hit, NavMesh.AllAreas))
                {
                    return hit.position;
                }
                else
                {
                    return GetPathTarget(PathingMode);
                }
                //return Player.Player1.transform.position;
            }
        }
    }

    PathfindingMode _pathingMode = PathfindingMode.FollowPlayer;
    public PathfindingMode PathingMode
    {
        get => _pathingMode;
        set
        {
            if (_pathingMode != value)
            {
                _pathingMode = value;
                UpdatePath();
            }
        }
    }

    public bool EnteringFromBottom { get; private set; } = false;

    public Rect CamRect => new Rect { size = new Vector2(camBoxWidth, camBoxHeight), center = Player.Player1.transform.position };

    public bool CanSeeTarget => !followingPath && !RiseFromCenterPlatform && TargetWithinHeightRange && !AllMovesDisabled && Vector3.Distance(transform.position,Player.Player1.transform.position) <= 23f;

    public bool TargetWithinHeightRange
    {
        get
        {
            if (!EnableTargetHeightRange)
            {
                return true;
            }
            var target = GetPathTargetUnclamped(PathingMode);

            return target.y >= (TargetHeightRange.x - camBoxHeight / 2f) && target.y <= (TargetHeightRange.y + camBoxHeight / 2f);
        }
    }

    public bool AllMovesDisabled => allMovesDisabled;

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

    RaycastHit2D[] rayCache = new RaycastHit2D[4];

    NavMeshSurface navigator = null;
    NavMeshPath path = null;
    Vector3[] cornerCache = new Vector3[100];
    int cornerCount = 0;
    int cornerIndex = 0;
    bool followingPath = false;
    Vector3 lastPathPos = default;
    RaycastHit2D[] singleHitCache = new RaycastHit2D[1];
    Vector3 lastKnownPosition = new Vector3(100000,100000);
    bool stillStuck = false;
    bool riseFromCenterPlatform = false;
    //bool forceOffensiveModeNextTime = false;

    public bool RiseFromCenterPlatform => riseFromCenterPlatform;

    List<Renderer> renderers = new List<Renderer>();

    public float StartingHealth { get; private set; }

    IEnumerable<GameObject> GetAllObjectsWithLayer(string layer)
    {
        return GetAllObjectsWithLayer(LayerMask.NameToLayer(layer));
    }

    IEnumerable<GameObject> GetAllObjectsWithLayer(int layer)
    {
        return GetAllObjectsWithLayer(gameObject, layer);
    }

    IEnumerable<GameObject> GetAllObjectsWithLayer(GameObject obj, int layer)
    {
        if (obj == null)
        {
            yield break;
        }

        if (obj.layer == layer)
        {
            yield return obj;
        }

        var childCount = obj.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            foreach (var childObj in GetAllObjectsWithLayer(obj.transform.GetChild(i).gameObject, layer))
            {
                yield return childObj;
            }
        }
    }

    protected override void Awake()
    {
        navigator = GameObject.FindObjectOfType<NavMeshSurface>();
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

        if (!StartAsleep)
        {
            FullyAwake = true;
            sleepSprite.gameObject.SetActive(false);
            foreach (var obj in enableOnWakeUp)
            {
                obj.SetActive(true);
            }
        }
        else
        {
            sleepSprite.transform.SetParent(null);

            foreach (var c in collidersEnableOnLunge)
            {
                c.enabled = true;
            }

            foreach (var c in collidersDisableOnLunge)
            {
                c.enabled = false;
            }

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled)
                {
                    renderers.Add(renderer);
                }
                renderer.enabled = false;
            }

            sleepSprite.transform.SetParent(transform);
        }

        if (!TrailerMode && !StartAsleep)
        {
            SetTarget(playerTarget);
            StartBoundRoutine(VarianceResetter());

            if (navigator != null && pathfindingEnabled)
            {
                StartBoundRoutine(PathFindingRoutine());
            }
        }
        

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

        StartingHealth = HealthManager.Health;
    }

    private void Start()
    {
        if (StartAsleep)
        {
            Recoil.SetRecoilSpeed(0);
            Claws.DisableSwingAttackImmediate();
            StartBoundRoutine(PreBossRoutine());
        }

        if (!TrailerMode && !StartAsleep)
        {
            StartBoundRoutine(SlowUpdate());
            StartBoundRoutine(MainBossRoutine());
        }
    }

    /*IEnumerator SleepJumpRoutine()
    {

    }*/

    IEnumerator ShakeRoutine(float intensity, float shakeFPS = 60f)
    {
        Vector3 sourcePos = transform.position;
        while (true)
        {
            transform.position = sourcePos + (Vector3)(UnityEngine.Random.insideUnitCircle * intensity);
            Vector3 oldPos = transform.position;
            yield return new WaitForSeconds(1f / shakeFPS);
            Vector3 newPos = transform.position;

            sourcePos += newPos - oldPos;
        }
    }

    private IEnumerator QuickWakeRoutine()
    {
        var usePathfinding = pathfindingEnabled;
        SetTarget(transform.position);
        sleepSprite.gameObject.SetActive(false);
        FlightEnabled = true;
        Body.PlayDefaultAnimation = true;
        ApplyFlightVariance = true;
        Wings.PlayDefaultAnimation = true;

        if (usePathfinding)
        {
            pathfindingEnabled = false;
        }
        Recoil.ResetRecoilSpeed();

        foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = false;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = true;
        }

        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        if (sleepRoarQuickSound != null)
        {
            WeaverAudio.PlayAtPoint(sleepRoarQuickSound, transform.position);
        }

        IEnumerator RoarRoutine()
        {
            yield return Roar(sleepRoarQuickTime, Head.transform.position, false);

            Music.PlayMusicPack(bossMusic);
        }

        StartCoroutine(RoarRoutine());

        for (float t = 0; t <sleepRoarQuickTime; t += Time.deltaTime)
        {
            WeaverLog.Log("HEAD IN CAM = " + CamRect.Contains((Vector2)Head.transform.position));
            WeaverLog.Log($"XMin = {CamRect.xMin}, XMax = {CamRect.xMax}, YMin = {CamRect.yMin}, YMax = {CamRect.yMax}");
            WeaverLog.Log((Vector2)Head.transform.position);
            if (CamRect.Contains((Vector2)Head.transform.position))
            {
                WeaverLog.Log("WITHIN RANGE");
                break;
            }
            yield return null;
        }

        FullyAwake = true;

        foreach (var obj in enableOnWakeUp)
        {
            obj.SetActive(true);
        }

        SetTarget(playerTarget);

        StartBoundRoutine(VarianceResetter());

        if (navigator != null)
        {
            StartBoundRoutine(PathFindingRoutine());
        }

        StartBoundRoutine(SlowUpdate());
        StartBoundRoutine(MainBossRoutine());

        if (usePathfinding)
        {
            pathfindingEnabled = true;
        }

        yield break;
    }

    private IEnumerator PreBossRoutine()
    {
        var enemyLayers = GetAllObjectsWithLayer("Enemies").ToList();

        var enemyLayer = LayerMask.NameToLayer("Enemies");
        var attackLayer = LayerMask.NameToLayer("Attack");

        foreach (var obj in enemyLayers)
        {
            obj.layer = attackLayer;
        }

        Body.PlayDefaultAnimation = false;
        ApplyFlightVariance = false;
        FlightEnabled = false;
        Wings.PlayDefaultAnimation = false;

        var health = HealthComponent.Health;

        //Wait until the health changes
        yield return new WaitUntil(() => health != HealthComponent.Health || Phase >= BossPhase.Phase2);

        foreach (var obj in enemyLayers)
        {
            obj.layer = enemyLayer;
        }

        if (Phase >= BossPhase.Phase2)
        {
            yield return QuickWakeRoutine();
            yield break;
        }


        var flasher = sleepSprite.GetComponent<SpriteFlasher>();
        flasher.enabled = true;
        flasher.flashInfected();

        if (sleepAwakeSound != null)
        {
            var awakeSound = WeaverAudio.PlayAtPoint(sleepAwakeSound, transform.position);
            awakeSound.AudioSource.pitch = sleepAwakeSoundPitch;
        }

        var headRoutine = Head.LockHead(Head.LookingDirection >= 0f ? 60f : -60f);
        var bodyRoutine = Body.RaiseTail(1000);




        bool playerOnRight = Player.Player1.transform.position.x >= transform.position.x;

        RoutineAwaiter awaiter = null;

        if (playerOnRight)
        {
            awaiter = RoutineAwaiter.AwaitRoutine(ChangeDirection(AspidOrientation.Right, 1000));
            yield return awaiter.WaitTillDone();
        }
         

        //Claws.OnGround = true;

        awaiter = RoutineAwaiter.AwaitBoundRoutines(this, headRoutine, bodyRoutine);

        Rbody.velocity = new Vector2(0f, sleepJumpVelocity);
        Rbody.gravityScale = sleepJumpGravity;

        lungeDashEffect.SetActive(true);
        lungeDashRotationOrigin.SetZLocalRotation(90);


        foreach (var scale in sleepScalesBefore)
        {
            sleepSprite.transform.localScale = sleepSprite.transform.localScale.With(x: scale);
            yield return new WaitForSeconds(sleepScaleChangeSpeed);

            /*if (awaiter == null || awaiter.Done)
            {
                awaiter = RoutineAwaiter.AwaitRoutine(ChangeDirection(AspidOrientation.Right, 1000));
            }*/
        }

        foreach (var claw in Claws.claws)
        {
            yield return claw.LockClaw();
            claw.OnGround = true;
            claw.LandImmediately();
        }

        yield return awaiter.WaitTillDone();

        sleepSprite.gameObject.SetActive(false);

        /*foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = false;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = true;
        }*/

        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        foreach (var scale in sleepScalesAfter)
        {
            transform.localScale = transform.localScale.With(x: scale);
            yield return new WaitForSeconds(sleepScaleChangeSpeed);
        }

        yield return new WaitUntil(() => Rbody.velocity.y < 0f);
        yield return new WaitUntil(() => Rbody.velocity.y == 0f);

        lungeRockParticles.Play();

        if (lungeLandSoundHeavy != null)
        {
            WeaverAudio.PlayAtPoint(lungeLandSoundHeavy, transform.position);
        }
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);

        var headLandRoutine = Head.PlayLanding(true);
        var bodyLandRoutine = Body.PlayLanding(true);
        var wingPlateLandRoutine = WingPlates.PlayLanding(true);
        var wingsLandRoutine = Wings.PlayLanding(true);
        var clawsLandRoutine = Claws.PlayLanding(true);
        var shiftPartsRoutine = ShiftBodyParts(true, Body, WingPlates, Wings);

        List<uint> landingRoutines = new List<uint>();

        //bool landingCancelled = false;

        var landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        yield return landingAwaiter.WaitTillDone();

        var headFinishRoutine = Head.FinishLanding(false);
        var bodyFinishRoutine = Body.FinishLanding(false);
        var wingPlateFinishRoutine = WingPlates.FinishLanding(false);
        var wingsFinishRoutine = Wings.FinishLanding(false);
        var clawsFinishRoutine = Claws.FinishLanding(false);

        var finishAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, headFinishRoutine, bodyFinishRoutine, wingPlateFinishRoutine, wingsFinishRoutine, clawsFinishRoutine);

        yield return finishAwaiter.WaitTillDone();


        yield return new WaitForSeconds(sleepPreJumpDelay);

        Rbody.gravityScale = onGroundGravity;
        AspidMode = Mode.Defensive;
        yield return ExitGroundMode();

        yield return new WaitUntil(() => !Head.HeadBeingUnlocked);

        if (!Head.HeadLocked)
        {
            yield return Head.LockHead(Orientation == AspidOrientation.Right ? 60f : -60f);
        }

        yield return Head.Animator.PlayAnimationTillDone("Fire - 2 - Prepare");

        var attackAnim = Head.Animator.AnimationData.GetClip("Fire - 1 - Attack");

        Head.MainRenderer.sprite = attackAnim.Frames[0];

        var shakeRoutine = StartCoroutine(ShakeRoutine(sleepShakeIntensity));

        if (sleepRoarSound != null)
        {
            var roar = WeaverAudio.PlayAtPoint(sleepRoarSound, transform.position);
            roar.AudioSource.pitch = sleepRoarSoundPitch;
        }


        AreaTitle.Spawn("Ancient", "Aspid");

        yield return Roar(sleepRoarDuration,Head.transform.position, true);

        StopCoroutine(shakeRoutine);

        FlightEnabled = true;
        yield return Claws.EnableSwingAttack(true);

        yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Attack");

        Head.UnlockHead();

        yield return new WaitUntil(() => !Head.HeadBeingUnlocked);

        FullyAwake = true;

        foreach (var obj in enableOnWakeUp)
        {
            obj.SetActive(true);
        }

        Music.PlayMusicPack(bossMusic);

        StartBoundRoutine(VarianceResetter());

        if (navigator != null)
        {
            StartBoundRoutine(PathFindingRoutine());
        }

        StartBoundRoutine(SlowUpdate());
        StartBoundRoutine(MainBossRoutine());

        yield break;
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
        var validModes = new List<Mode>
        {
            Mode.Offensive,
            Mode.Tactical,
            Mode.Defensive
        };

        var currentMode = (Mode)(-1);

        while (true)
        {
            if (!riseFromCenterPlatform && !CanSeeTarget)
            {
                AspidMode = Mode.Tactical;
                while (!CanSeeTarget)
                {
                    yield return UpdateDirection();

                    if (riseFromCenterPlatform)
                    {
                        break;
                    }
                    //yield return TacticalModeRoutine(1f);
                }
            }

            if (riseFromCenterPlatform)
            {
                WeaverLog.LogError("ENTERING OFFENSIVE MODE PRE PRE");
                currentMode = Mode.Offensive;
            }
            else if (AllMovesDisabled)
            {
                currentMode = Mode.Tactical;
            }
            /*else if (forceOffensiveModeNextTime)
            {
                forceOffensiveModeNextTime = false;
                currentMode = Mode.Offensive;
            }*/
            else
            {
                if (currentMode != (Mode)(-1))
                {
                    var lastMove = currentMode;
                    validModes.Remove(lastMove);

                    int startIndex = OffensiveModeEnabled ? 0 : 1;
                    int endIndex = GroundModeEnabled ? validModes.Count : validModes.Count - 1;

                    currentMode = validModes.GetRandomElement(startIndex, endIndex);

                    validModes.Add(lastMove);
                }
                else
                {
                    currentMode = Mode.Tactical;
                }
            }

            switch (currentMode)
            {
                case Mode.Tactical:
                    yield return TacticalModeRoutine(6f);
                    break;
                case Mode.Offensive:
                    WeaverLog.LogError("ENTERING OFFENSIVE MODE PRE");
                    yield return OffensiveModeRoutine();
                    break;
                case Mode.Defensive:
                    yield return DefensiveModeRoutine();
                    break;
                default:
                    yield return TacticalModeRoutine(2f);
                    break;
            }




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
            /*var time = Time.time;
            while (Time.time <= time + 6f)
            {
                yield return CheckDirectionToPlayer(); //THIS IS A TEST
            }*/
            //yield return EnterGroundMode();
            //yield return ExitGroundMode();



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

            yield return TacticalModeRoutine(2f);
        }
    }

    IEnumerator TacticalModeRoutine(float time)
    {
        AspidMode = Mode.Tactical;
        float tacticalTime = time;
        float tacticalStart = Time.time;

        int currentMoveIndex = 0;
        float lastMoveTime = Time.time;
        ShuffleMoves(Moves);
        /*for (int i = 0; i < Moves.Count; i++)
        {
            Debug.Log($"Move {i} = {Moves[i].GetType().FullName}");
        }*/
        yield return new WaitForSeconds(0.5f);
        while (!riseFromCenterPlatform && Time.time - tacticalStart < tacticalTime && CanSeeTarget)
        {
            if (currentMoveIndex == Moves.Count)
            {
                currentMoveIndex = 0;
                ShuffleMoves(Moves);
            }
            yield return new WaitUntil(() => !Head.HeadLocked && !Head.HeadBeingUnlocked);
            yield return UpdateDirection();

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

            WeaverLog.Log("Running Move = " + move.GetType().FullName);
            if (!AllMovesDisabled)
            {
                yield return RunMove(move);
            }
            else
            {
                yield return null;
            }

            lastMoveTime = Time.time + move.PostDelay;
            currentMoveIndex++;
        }
        yield return new WaitUntil(() => !Head.HeadLocked);
    }

    IEnumerator OffensiveModeRoutine()
    {
        float offensiveTime = 10f;

        yield return EnterCenterMode();
        //while (true)
        //{
        WeaverLog.LogError("BEFORE FORCE");
        yield return new WaitUntil(() => !riseFromCenterPlatform);
        WeaverLog.LogError("AFTER FORCE");

        float offensiveStart = Time.time;

        if (Orientation != AspidOrientation.Center)
        {
            yield break;
        }
        AspidMode = Mode.Offensive;

        int currentMoveIndex = 0;
        float lastMoveTime = Time.time;
        ShuffleMoves(Moves);

        var modeEnabled = (Phase == BossPhase.Default && OffensiveModeEnabled) || (Phase == BossPhase.Phase3 || Phase == BossPhase.Phase4);

        while (Time.time - offensiveStart < offensiveTime && modeEnabled && !AllMovesDisabled && CanSeeTarget && Vector3.Distance(transform.position, Player.Player1.transform.position) <= 25)
        {
            WeaverLog.LogError("OFFENSIVE LOOP");
            if (currentMoveIndex == Moves.Count)
            {
                currentMoveIndex = 0;
                ShuffleMoves(Moves);
            }
            yield return new WaitUntil(() => !Head.HeadLocked && !Head.HeadBeingUnlocked);
            yield return UpdateDirection();

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

            WeaverLog.Log("Running Move = " + move.GetType().FullName);
            yield return RunMove(move);

            lastMoveTime = Time.time + move.PostDelay;
            currentMoveIndex++;
        }

        /*if (forceOffensiveModeNextTime)
        {
            forceOffensiveModeNextTime = false;
            continue;
        }
        else
        {
            break;
        }*/
        //}

        yield return new WaitUntil(() => !Head.HeadLocked && !Head.HeadBeingUnlocked);
        yield return ExitCenterMode();

        AspidMode = Mode.Tactical;
    }

    IEnumerator DefensiveModeRoutine()
    {
        if (GroundModeEnabled && Vector3.Distance(Player.Player1.transform.position,transform.position) <= 35f && CanSeeTarget)
        {
            yield return EnterGroundMode();
            yield return ExitGroundMode();
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

    void EnableCenterLock(Vector3 position, WeaverCameraLock camLock)
    {
        var currentLocks = GameManager.instance.cameraCtrl.lockZoneList;

        Bounds mainBounds = default;


        if (currentLocks == null || currentLocks.Count == 0)
        {
            if (GameManager.instance.sm is WeaverSceneManager wsm)
            {
                var sceneRect = wsm.SceneDimensions;
                mainBounds = new Bounds();
                mainBounds.SetMinMax(sceneRect.min,sceneRect.max);
            }
            else
            {
                mainBounds = new Bounds();
                mainBounds.SetMinMax(Vector3.zero, new Vector3(GameManager.instance.sceneWidth, GameManager.instance.sceneHeight));
            }
        }
        else
        {

            for (int i = currentLocks.Count - 1; i >= 0; i--)
            {
                var l = currentLocks[i];
                mainBounds = new Bounds();
                mainBounds.SetMinMax(new Vector3(l.cameraXMin, l.cameraYMin,-1f), new Vector3(l.cameraXMax, l.cameraYMax,1f));
                if (mainBounds.Contains(transform.position))
                {
                    break;
                }
            }

            /*var mainLock = currentLocks[0];
            mainBounds = new Bounds();
            mainBounds.SetMinMax(new Vector3(mainLock.cameraXMin, mainLock.cameraYMin),new Vector3(mainLock.cameraXMax, mainLock.cameraYMax));*/
        }


        camLock.transform.SetParent(null);
        camLock.transform.position = position;
        camLock.RefreshCamBounds();

        float xOffset = 0;
        float yOffset = 0;

        if (camLock.cameraXMax > mainBounds.max.x)
        {
            xOffset += -(camLock.cameraXMax - mainBounds.max.x);
        }

        if (camLock.cameraXMin < mainBounds.min.x)
        {
            xOffset += (mainBounds.min.x - camLock.cameraXMin);
        }

        if (camLock.cameraYMax > mainBounds.max.y)
        {
            yOffset += -(camLock.cameraYMax - mainBounds.max.y);
        }

        if (camLock.cameraYMin < mainBounds.min.y)
        {
            yOffset += (mainBounds.min.y - camLock.cameraYMin);
        }

        camLock.transform.position += new Vector3(xOffset, yOffset);

        camLock.gameObject.SetActive(true);
    }

    void DisableCenterLock(WeaverCameraLock camLock)
    {
        camLock.gameObject.SetActive(false);
        camLock.transform.SetParent(transform);
        camLock.transform.localPosition = default;
    }

    IEnumerator EnterCenterMode()
    {
        WeaverLog.LogError("ENTERING CENTER MODE");
        if (!riseFromCenterPlatform && Vector3.Distance(Player.Player1.transform.position, transform.position) >= 30)
        {
            yield break;
        }

        Recoil.SetRecoilSpeed(0f);
        //var roomBounds = RoomScanner.GetRoomBoundaries(transform.position);
        //Debug.DrawLine(new Vector3(roomBounds.xMin,roomBounds.yMin), new Vector3(roomBounds.xMax,roomBounds.yMax), Color.cyan, 5f);
        //Debug.DrawLine(transform.position, new Vector3(roomBounds.xMin, transform.position.y), Color.cyan, 5f);
        //Debug.DrawLine(transform.position, new Vector3(roomBounds.xMax, transform.position.y), Color.cyan, 5f);

        //Debug.DrawLine(transform.position, new Vector3(transform.position.x, roomBounds.yMin), Color.cyan, 5f);
        //Debug.DrawLine(transform.position, new Vector3(transform.position.x, roomBounds.yMax), Color.cyan, 5f);
        minFlightSpeed /= 3f;

        CurrentRoomRect = RoomScanner.GetRoomBoundaries(Player.Player1.transform.position);

        var newTarget = new Vector3(Mathf.Lerp(CurrentRoomRect.Rect.xMin, CurrentRoomRect.Rect.xMax, 0.5f), CurrentRoomRect.Rect.yMin + offensiveHeight);

        newTarget.x = Mathf.Clamp(newTarget.x,Player.Player1.transform.position.x - 10f,Player.Player1.transform.position.x + 10f);
        newTarget.y = Mathf.Clamp(newTarget.y, Player.Player1.transform.position.y + 4f, Player.Player1.transform.position.y + 20f);


        //var newTarget
        //newTarget = transform.position;
        SetTarget(newTarget);
        //flightOffset /= 4f;
        minimumFlightSpeed /= 4f;
        homeInOnTarget = true;
        //orbitReductionAmount *= 4f;
        flightSpeed /= 2f;

        origOrbitReductionAmount = orbitReductionAmount;

        //yield return ChangeDirection(AspidOrientation.Center);
        yield return ChangeDirection(AspidOrientation.Center);

        float timer = 0f;
        float maxTimeToCenter = 4f;
        float centeringTimer = 0;

        bool centered = false;
        //bool centeringDone = false;

        while (!riseFromCenterPlatform && timer <= 0.25f && centeringTimer < maxTimeToCenter)
        {
            if (Vector3.Distance(transform.position, newTarget) <= 2f && !centered)
            {
                centered = true;

                /*IEnumerator CenteringRoutine()
                {
                    EnableCenterLock(newTarget);
                    //centeringDone = true;
                }*/
                //StartBoundRoutine(CenteringRoutine());
                EnableCenterLock(newTarget, centerCamLock);
            }

            if (Vector3.Distance(Player.Player1.transform.position, newTarget) > 25f)
            {
                centeringTimer = maxTimeToCenter;
                break;
            }

            centeringTimer += Time.deltaTime;
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

        /*if (centered)
        {
            yield return new WaitUntil(() => centeringDone);
        }*/

        //forceCentralMode = false;

        if (centeringTimer >= maxTimeToCenter)
        {
            yield return ExitCenterMode();
        }
        //yield return new WaitUntil(() => Vector3.Distance(transform.position, newTarget) <= 1f);
        //yield return new WaitForSeconds(0.25f);
    }

    IEnumerator ExitCenterMode()
    {
        return ExitCenterMode(GetOrientationToPlayer());
    }

    IEnumerator ExitCenterMode(AspidOrientation orientation,bool wait = true)
    {
        DisableCenterLock(centerCamLock);
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


    IEnumerator ShootGiantBlob(EnterGround_BigVomitShotMove move, float preDelay = 0.25f)
    {
        yield return new WaitForSeconds(preDelay);
        WeaverLog.Log("Running Move = " + move.GetType().FullName);
        yield return RunMove(move);


        for (float t = move.SpawnedGlobEstLandTime; t >= 0.5f; t -= Time.deltaTime)
        {
            yield return null;
        }
    }

    IEnumerator EnterGroundMode()
    {
        bool globMode = UnityEngine.Random.Range(0f, 1f) > 0.5f;

        //If we are already in center, then exit out of it
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

        //Move into position
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

        var bigBlobMove = GetComponent<EnterGround_BigVomitShotMove>();

        if (globMode)
        {
            //Debug.Log("GLOB START");
            yield return ShootGiantBlob(bigBlobMove);
            //Debug.Log("GLOB END");
        }

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

        Vector3 destination;

        if (!globMode)
        {
            bool towardsPlayer = UnityEngine.Random.Range(0f, 1f) >= 0.5f;

            if (UnityEngine.Random.Range(0, 2) == 1 || true)
            {
                destination = Player.Player1.transform.position;
            }
            else
            {
                destination = transform.position.With(y: CurrentRoomRect.Rect.yMin);
            }
        }
        else
        {
            destination = bigBlobMove.SpawnedGlob.transform.position + new Vector3(0,2,0);
        }

        var angleToDestination = VectorUtilities.VectorToDegrees((destination - transform.position).normalized);

        var downwardAngle = Vector3.Dot(Vector3.right, (destination - transform.position).normalized) * 90f;

        lungeDashEffect.SetActive(true);
        lungeDashRotationOrigin.SetZLocalRotation(angleToDestination);


        bool steepAngle = Mathf.Abs(downwardAngle) >= degreesSlantThreshold;

        if (globMode)
        {
            steepAngle = false;
        }

        Rbody.velocity = (destination - transform.position).normalized * lungeSpeed;

        yield return null;

        var yVelocity = Rbody.velocity.y;
        var xVelocity = Rbody.velocity.x;
        var halfVelocity = Mathf.Abs(Rbody.velocity.y) / 2f;


        //
        //Wait until landing, or cancel if needed
        //

        bool landed = false;

        for (float t = 0; t < 2; t += Time.deltaTime)
        {
            if (Mathf.Abs(Rbody.velocity.y) < halfVelocity)
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
            yield return JumpCancel(false);

            if (globMode)
            {
                bigBlobMove.SpawnedGlob.ForceDisappear();
            }
            yield break;
        }
        //yield return new WaitUntil(() => Mathf.Abs(Rbody.velocity.y) < halfVelocity);

        Rbody.velocity = default;

        if (globMode)
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

                    VomitGlob.Spawn(transform.position + groundSplashSpawnOffset, velocity);
                }
            }
        }

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

                if (Rbody.velocity.y < -0.5f && Vector3.Distance(transform.position, Player.Player1.transform.position) >= 30)
                {
                    yield return JumpCancel(false);
                    yield break;
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

        if (Rbody.velocity.y < -2)
        {
            yield return JumpCancel(false);
            yield break;
        }

        if (steepAngle && CanSeeTarget)
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

        if (Rbody.velocity.y < -2)
        {
            yield return JumpCancel(false);
            yield break;
        }


        //TODO - AIM TOWARDS THE PLAYER OR TOWARDS THE GROUND

        //TODO - PLAY PREPARE ANIMATION. The Wings stop, the claws stop and the body lifts up

        //TODO - Either Lunge Towards the player or down towards the ground. If the enemy lunges towards the player,
        //the boss will continue to slide on the ground until he stops.

        //Make sure the claws and wings stay stopped during this state

        AspidMode = Mode.Defensive;

        bool jumpsCancelled = false;

        void onCancel()
        {
            jumpsCancelled = true;
        }

        yield return new WaitForSeconds(0.5f);

        if (CanSeeTarget)
        {
            yield return DoGroundJump(2, JumpTargeter, onCancel);
        }

        if (jumpsCancelled || !CanSeeTarget)
        {
            yield return JumpCancel(false);
            yield break;
        }

        //SHOOT MORE BLOBS
        WeaverLog.Log("Running Move = " + typeof(VomitShotMove).FullName);
        yield return RunMove(GetComponent<VomitShotMove>());

        if (CanSeeTarget)
        {
            yield return DoGroundJump(3, JumpTargeter, onCancel);
        }

        if (jumpsCancelled || !CanSeeTarget)
        {
            yield return JumpCancel(false);
            yield break;
        }

        yield return new WaitForSeconds(0.25f);

        yield break;
    }

    IEnumerator ExitGroundMode()
    {
        if (AspidMode != Mode.Defensive)
        {
            yield break;
        }
        //TODO - Play the prepare animation for lifting off of the ground

        //TODO - Play the jump animation for jumping back up into the air

        //TODO - Switch back to tactic mode and play the default claw and wing animations

        //bool onGround = Claws.OnGround;

        yield return JumpPrepare();
        yield return JumpLaunch();

        foreach (var sound in jumpSounds)
        {
            WeaverAudio.PlayAtPoint(sound, transform.position);
        }

        Claws.OnGround = false;
        Wings.PlayDefaultAnimation = true;

        foreach (var claw in Claws.claws)
        {
            claw.UnlockClaw();
        }

        CameraShaker.Instance.Shake(jumpLaunchShakeType);

        var target = transform.position + new Vector3(0f, 20f, 0f);

        if (target.y < transform.position.y + 2f && target.y > transform.position.y - 2)
        {
            target.y = transform.position.y;
        }

        var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, jumpTime, jumpGravity / 2);

        Rbody.gravityScale = jumpGravity;
        Rbody.velocity = velocity;

        yield return null;
        yield return null;


        yield return new WaitUntil(() => Rbody.velocity.y <= 0);

        FlightEnabled = true;
        Rbody.velocity = default;
        Rbody.gravityScale = 0f;
        



        //var headRoutine = Head.LockHead(Head.LookingDirection >= 0f ? 60f : -60f);
        //var bodyRoutine = Body.RaiseTail();
        //var minWaitTimeRoutine = Wait(0.5f);

        //RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, headRoutine, bodyRoutine, minWaitTimeRoutine);
        Head.UnlockHead();

        Recoil.ResetRecoilSpeed();

        Body.PlayDefaultAnimation = true;



        foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = false;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = true;
        }

        ApplyFlightVariance = true;
        SetTarget(playerTarget);
        AspidMode = Mode.Tactical;

        yield break;
    }

    Vector2 JumpTargeter(int time)
    {
        //return Player.Player1.transform.position;
        if (time % 2 == 0)
        {
            return Player.Player1.transform.position;
        }
        else
        {
            //var area = CurrentRoomRect
            var minX = CurrentRoomRect.Rect.xMin + 6f;
            var maxX = CurrentRoomRect.Rect.xMax - 6f;

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

        const float angleLimit = 45f;

        if (Orientation == AspidOrientation.Left)
        {
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

    IEnumerator DoGroundJump(int jumpTimes, Func<int, Vector2> getTarget, Action onCancel)
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
                if (CurrentRoomRect.BottomHit.collider != null)
                {
                    var colliderBounds = CurrentRoomRect.BottomHit.collider.bounds;

                    target.x = Mathf.Clamp(target.x, colliderBounds.min.x + 5,colliderBounds.max.x - 5);
                }
            }

            var velocity = MathUtilities.CalculateVelocityToReachPoint(transform.position, target, jumpTime, jumpGravity);

            Rbody.gravityScale = jumpGravity;
            Rbody.velocity = velocity;

            Claws.OnGround = false;

            bool switchDirection = false;
            if (Orientation == AspidOrientation.Right && Player.Player1.transform.position.x < target.x)
            {
                switchDirection = true;
            }

            if (Orientation == AspidOrientation.Left && Player.Player1.transform.position.x >= target.x)
            {
                switchDirection = true;
            }

            if (switchDirection)
            {
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
            }

            yield return new WaitUntil(() => Rbody.velocity.y <= 0f);
            var fallingAwaiter = JumpBeginFalling(switchDirection);
            yield return new WaitUntil(() => Rbody.velocity.y <= -0.5f);
            //yield return new WaitForSeconds(Time.fixedDeltaTime * 2f);
            //yield return new WaitForSeconds(jumpTime / 5f);

            bool cancel = true;

            for (float t = 0; t < 1.5f; t += Time.deltaTime)
            {
                if (Rbody.velocity.y >= -0.5f)
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
            //yield return new WaitUntil(() => Rbody.velocity.y >= 0f);

            yield return fallingAwaiter.WaitTillDone();

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

    RoutineAwaiter JumpBeginFalling(bool switchedDirection)
    {
        var clawsRoutine = Claws.GroundJumpBeginFalling(switchedDirection);
        var headRoutine = Head.GroundJumpBeginFalling(switchedDirection);
        var wingsRoutine = Wings.GroundJumpBeginFalling(switchedDirection);
        var wingPlatesRoutine = WingPlates.GroundJumpBeginFalling(switchedDirection);
        var bodyRoutine = Body.GroundJumpBeginFalling(switchedDirection);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);
        return awaiter;
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

    IEnumerator JumpCancel(bool onGround)
    {
        Claws.OnGround = false;
        Wings.PlayDefaultAnimation = true;

        FlightEnabled = true;
        Rbody.velocity = default;
        Rbody.gravityScale = 0f;

        foreach (var claw in Claws.claws)
        {
            claw.UnlockClaw();
        }

        var clawsRoutine = Claws.GroundMoveCancel(onGround);
        var headRoutine = Head.GroundMoveCancel(onGround);
        var wingsRoutine = Wings.GroundMoveCancel(onGround);
        var wingPlatesRoutine = WingPlates.GroundMoveCancel(onGround);
        var bodyRoutine = Body.GroundMoveCancel(onGround);
        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, clawsRoutine, headRoutine, wingsRoutine, wingPlatesRoutine, bodyRoutine);

        yield return awaiter.WaitTillDone();

        Head.UnlockHead();

        Recoil.ResetRecoilSpeed();

        Body.PlayDefaultAnimation = true;



        foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = false;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = true;
        }

        ApplyFlightVariance = true;
        SetTarget(playerTarget);

        AspidMode = Mode.Tactical;
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

    IEnumerator UpdateDirection()
    {
        Vector3 targetPosition;

        if (followingPath)
        {
            targetPosition = cornerCache[cornerIndex];
        }
        else
        {
            targetPosition = Player.Player1.transform.position;
        }


        if (transform.position.x >= targetPosition.x)
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

    private IEnumerator ChangeDirection(AspidOrientation newOrientation, float speedMultiplier = 1f)
    {
        if (Orientation == newOrientation)
        {
            yield break;
        }

        //WeaverLog.Log("CHANGING DIRECTION to = " + newOrientation);
        yield return Body.PrepareChangeDirection();
        //WeaverLog.Log("PREP DONE");
        IEnumerator bodyRoutine = Body.ChangeDirection(newOrientation, speedMultiplier);
        IEnumerator wingPlateRoutine = WingPlates.ChangeDirection(newOrientation, speedMultiplier);
        IEnumerator wingRoutine = Wings.ChangeDirection(newOrientation, speedMultiplier);
        IEnumerator headRoutine = Head.ChangeDirection(newOrientation, speedMultiplier);
        IEnumerator clawsRoutine = Claws.ChangeDirection(newOrientation, speedMultiplier);

        var changeDirectionTime = Body.GetChangeDirectionTime(speedMultiplier);

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

    bool WithinBox(Vector3 targetPos, Vector3 boxPos, Vector2 boxSize)
    {
        var bounds = new Bounds(boxPos, boxSize);

        return targetPos.x > bounds.min.x && targetPos.x < bounds.max.x && targetPos.y > bounds.min.y && targetPos.y < bounds.max.y;
    }

    Vector3 GetPathTarget(PathfindingMode mode)
    {
        Vector3 target = GetPathTargetUnclamped(mode);

        if (EnableTargetHeightRange)
        {
            target.y = Mathf.Clamp(target.y, TargetHeightRange.x, TargetHeightRange.y);
        }
        return target;
    }

    Vector3 GetPathTargetUnclamped(PathfindingMode mode)
    {
        Vector3 target;
        switch (mode)
        {
            case PathfindingMode.FollowPlayer:
                target = Player.Player1.transform.position;
                break;
            case PathfindingMode.FollowTarget:
                target = TargetPosition;
                break;
            default:
                target = transform.position;
                break;
        }

        return target;
    }

    void UpdatePath()
    {
        if (navigator != null && pathfindingEnabled && PathingMode != PathfindingMode.None)
        {
            if (path == null)
            {
                path = new NavMeshPath();
            }

            var distanceToTarget = Vector3.Distance(GetPathTarget(PathingMode), transform.position);

            Debug.DrawRay(transform.position, (GetPathTarget(PathingMode) - transform.position).normalized * distanceToTarget,Color.red, 0.5f);

            Vector3? startPos = null;

            if (Vector3.Distance(lastKnownPosition, transform.position) <= 0.05f)
            {
                if (stillStuck)
                {
                    WeaverLog.Log("STILL STUCK");
                    if (!riseFromCenterPlatform)
                    {
                        Rbody.velocity = UnityEngine.Random.insideUnitCircle * 50f;
                    }
                }
                startPos = Vector3.Lerp(new Vector3(CurrentRoomRect.Rect.xMin, transform.position.y), new Vector3(CurrentRoomRect.Rect.xMax, transform.position.y), 0.5f);
                stillStuck = true;
                WeaverLog.Log("STUCK");
            }
            else
            {
                stillStuck = false;
                lastKnownPosition = transform.position;
            }

            if (Physics2D.RaycastNonAlloc(transform.position, (GetPathTarget(PathingMode) - transform.position).normalized, singleHitCache, distanceToTarget, LayerMask.GetMask("Terrain")) == 1)
            {
                //Debug.Log("")
                /*if (followingPath && WithinBox(Player.Player1.transform.position,lastKnownPlayerPos,playerMoveBoxSize))
                {
                    return;
                }*/

                //Debug.Log("VELOCITY = " + (Rbody.velocity * 0.25f));
                //Debug.Log("MAGNITUDE = " + (Rbody.velocity.magnitude * 0.25f));

                //Debug.DrawLine(transform.position, new Vector3(transform.position.x, CurrentRoomRect.Rect.yMax), Color.magenta, 1f);
                //Debug.DrawLine(transform.position, new Vector3(transform.position.x, CurrentRoomRect.Rect.yMin), Color.magenta, 1f);
                //Debug.DrawLine(transform.position, new Vector3(CurrentRoomRect.Rect.xMin, transform.position.y), Color.magenta, 1f);
                //Debug.DrawLine(transform.position, new Vector3(CurrentRoomRect.Rect.xMax, transform.position.y), Color.magenta, 1f);

                //Debug.DrawRay(transform.position, Rbody.velocity * 0.25f, Color.red, 1f);

                if (startPos == null)
                {
                    var castAheadCount = Physics2D.RaycastNonAlloc(transform.position, Rbody.velocity.normalized, singleHitCache, Rbody.velocity.magnitude * 0.25f, LayerMask.GetMask("Terrain"));

                    if (castAheadCount > 0)
                    {
                        startPos = singleHitCache[0].point;
                        Debug.DrawLine(transform.position, startPos.Value, Color.grey, 1f);
                    }
                    else
                    {
                        startPos = transform.position + (Vector3)(Rbody.velocity * 0.25f);
                    }
                }

                //Vector3 startPos = startPos.value;
                Vector3 pathStartPos = startPos.Value;

                if (pathStartPos.x < CurrentRoomRect.Rect.xMin + 4.21f)
                {
                    pathStartPos.x = CurrentRoomRect.Rect.xMin + 4.21f;
                }

                if (pathStartPos.x > CurrentRoomRect.Rect.xMax - 4.21f)
                {
                    pathStartPos.x = CurrentRoomRect.Rect.xMax - 4.21f;
                }

                if (pathStartPos.y < CurrentRoomRect.Rect.yMin + 4.21f)
                {
                    pathStartPos.y = CurrentRoomRect.Rect.yMin + 4.21f;
                }

                if (pathStartPos.y > CurrentRoomRect.Rect.yMax - 4.21f)
                {
                    pathStartPos.y = CurrentRoomRect.Rect.yMax - 4.21f;
                }


                Debug.DrawLine(pathStartPos, new Vector3(pathStartPos.x, CurrentRoomRect.Rect.yMax), Color.green, 1f);
                Debug.DrawLine(pathStartPos, new Vector3(pathStartPos.x, CurrentRoomRect.Rect.yMin), Color.green, 1f);
                Debug.DrawLine(pathStartPos, new Vector3(CurrentRoomRect.Rect.xMin, pathStartPos.y), Color.green, 1f);
                Debug.DrawLine(pathStartPos, new Vector3(CurrentRoomRect.Rect.xMax, pathStartPos.y), Color.green, 1f);

                Debug.DrawLine(GetPathTarget(PathingMode), PathfindingTarget, Color.yellow, 1f);

                NavMesh.CalculatePath(pathStartPos, PathfindingTarget, NavMesh.AllAreas, path);

                if (path.status == NavMeshPathStatus.PathInvalid)
                {
                    if (NavMesh.FindClosestEdge(pathStartPos, out var hit, NavMesh.AllAreas))
                    {

                        NavMesh.CalculatePath(hit.position, PathfindingTarget, NavMesh.AllAreas, path);

                        Debug.DrawLine(transform.position, hit.position, Color.Lerp(Color.red, Color.white, 0.5f), 1f);
                    }
                    //var midRoom = new Vector3(transform.position.x, Mathf.Lerp(CurrentRoomRect.Rect.yMax, CurrentRoomRect.Rect.yMin,0.5f));

                    //Debug.DrawLine(midRoom, new Vector3(midRoom.x, CurrentRoomRect.Rect.yMax), Color.black, 1f);
                    //Debug.DrawLine(midRoom, new Vector3(midRoom.x, CurrentRoomRect.Rect.yMin), Color.black, 1f);
                    //Debug.DrawLine(midRoom, new Vector3(CurrentRoomRect.Rect.xMin, midRoom.y), Color.black, 1f);
                    //Debug.DrawLine(midRoom, new Vector3(CurrentRoomRect.Rect.xMax, midRoom.y), Color.black, 1f);

                }

                if (path.status != NavMeshPathStatus.PathInvalid)
                {
                    cornerCount = path.GetCornersNonAlloc(cornerCache);

                    lastPathPos = transform.position;
                    lastKnownPosition = transform.position;
                    followingPath = true;
                    //Debug.Log("FOUND PATH");
                    cornerIndex = 1;
                    return;
                    /*if (cornerCount > 2)
                    {

                    }
                    else
                    {
                        Debug.Log("NOT FOUND PATH B");
                    }*/
                }
                else
                {
                    //Debug.Log("NOT FOUND PATH A");
                    followingPath = false;
                }
            }
            else
            {
                //Debug.Log("NOT FOUND PATH B");
                followingPath = false;
            }
        }
        else
        {
            followingPath = false;
        }
    }

    IEnumerator PathFindingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pathRegenInterval);

            UpdatePath();
        }
    }

    public IEnumerator TriggerTrailerLandingRoutine()
    {
        Body.PlayDefaultAnimation = false;

        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        ApplyFlightVariance = false;

        //Move into position
        /*if (Head.LookingDirection >= 0f)
        {
            SetTarget(Player.Player1.transform.position + lungeTargetOffset);
        }
        else
        {
            SetTarget(Player.Player1.transform.position + new Vector3(-lungeTargetOffset.x, lungeTargetOffset.y, lungeTargetOffset.z));
        }*/

        var headRoutine = Head.LockHead(Head.LookingDirection >= 0f ? 60f : -60f);
        var bodyRoutine = Body.RaiseTail();
        var minWaitTimeRoutine = Wait(0.5f);

        RoutineAwaiter awaiter = RoutineAwaiter.AwaitBoundRoutines(this, headRoutine, bodyRoutine, minWaitTimeRoutine);

        yield return awaiter.WaitTillDone();

        FlightEnabled = false;
        Recoil.SetRecoilSpeed(0f);

        var bigBlobMove = GetComponent<EnterGround_BigVomitShotMove>();

        /*if (globMode)
        {
            yield return ShootGiantBlob(bigBlobMove);
        }*/

        yield return new WaitUntil(() => !Claws.DoingBasicAttack);

        foreach (var claw in Claws.claws)
        {
            yield return claw.LockClaw();
        }

        //yield return Body.RaiseTail();

        /*if (lungeAnticSound != null)
        {
            WeaverAudio.PlayAtPoint(lungeAnticSound, transform.position);
        }*/

        Wings.PrepareForLunge();
        Claws.PrepareForLunge();

        //yield return new WaitForSeconds(lungeAnticTime);

        Wings.DoLunge();
        Claws.DoLunge();
        WingPlates.DoLunge();
        Head.DoLunge();

        if (lungeSound != null)
        {
            WeaverAudio.PlayAtPoint(lungeSound, transform.position);
        }

        //CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.SmallShake);

        /*foreach (var c in collidersEnableOnLunge)
        {
            c.enabled = true;
        }

        foreach (var c in collidersDisableOnLunge)
        {
            c.enabled = false;
        }*/

        Vector3 destination = transform.position.With(y: transform.position.y - 100f);

        /*if (!globMode)
        {
            bool towardsPlayer = UnityEngine.Random.Range(0f, 1f) >= 0.5f;

            if (UnityEngine.Random.Range(0, 2) == 1 || true)
            {
                destination = Player.Player1.transform.position;
            }
            else
            {
                destination = transform.position.With(y: CurrentRoomRect.Rect.yMin);
            }
        }
        else
        {
            destination = bigBlobMove.SpawnedGlob.transform.position + new Vector3(0, 2, 0);
        }*/

        var angleToDestination = VectorUtilities.VectorToDegrees((destination - transform.position).normalized);

        var downwardAngle = Vector3.Dot(Vector3.right, (destination - transform.position).normalized) * 90f;

        lungeDashEffect.SetActive(true);
        lungeDashRotationOrigin.SetZLocalRotation(angleToDestination);


        bool steepAngle = true;

        /*if (globMode)
        {
            steepAngle = false;
        }*/

        Rbody.velocity = (destination - transform.position).normalized * lungeSpeed;

        yield return null;

        var yVelocity = Rbody.velocity.y;
        var xVelocity = Rbody.velocity.x;
        var halfVelocity = Mathf.Abs(Rbody.velocity.y) / 2f;


        //
        //Wait until landing, or cancel if needed
        //

        //bool landed = false;

        yield return new WaitUntil(() => transform.position.y <= trailerModeDestY);

        /*for (float t = 0; t < 2; t += Time.deltaTime)
        {
            if (Mathf.Abs(Rbody.velocity.y) < halfVelocity)
            {
                landed = true;
                break;
            }

            if (t > 0.5f && Vector3.Distance(transform.position, Player.Player1.transform.position) >= 30)
            {
                break;
            }

            yield return null;
        }*/

        transform.SetPositionY(trailerModeDestY);

        /*if (!landed)
        {
            yield return JumpCancel(false);

            if (globMode)
            {
                bigBlobMove.SpawnedGlob.ForceDisappear();
            }
            yield break;
        }*/
        //yield return new WaitUntil(() => Mathf.Abs(Rbody.velocity.y) < halfVelocity);

        Rbody.velocity = default;

        /*if (globMode)
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

                    VomitGlob.Spawn(transform.position + groundSplashSpawnOffset, velocity);
                }
            }
        }*/

        StartBoundRoutine(PlayLungeLandAnimation());

        var headLandRoutine = Head.PlayLanding(steepAngle);
        var bodyLandRoutine = Body.PlayLanding(steepAngle);
        var wingPlateLandRoutine = WingPlates.PlayLanding(steepAngle);
        var wingsLandRoutine = Wings.PlayLanding(steepAngle);
        var clawsLandRoutine = Claws.PlayLanding(steepAngle);
        var shiftPartsRoutine = ShiftBodyParts(steepAngle, Body, WingPlates, Wings);

        List<uint> landingRoutines = new List<uint>();

        //bool landingCancelled = false;

        var landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        bool slammed = false;

        uint switchDirectionRoutine = 0;
        AspidOrientation oldOrientation = Orientation;

        //Rbody.gravityScale = onGroundGravity;

        /*void StartSlideSound()
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
        }*/

        lungeRockParticles.Play();

        /*if (steepAngle)
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

                if (Rbody.velocity.y < -0.5f && Vector3.Distance(transform.position, Player.Player1.transform.position) >= 30)
                {
                    yield return JumpCancel(false);
                    yield break;
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
        else*/
        {
            if (lungeLandSoundHeavy != null)
            {
                WeaverAudio.PlayAtPoint(lungeLandSoundHeavy, transform.position);
            }
            CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
        }

        Rbody.velocity = Rbody.velocity.With(x: 0f);

        //yield return new WaitUntil(() => landingCancelled || landingAwaiter.Done);

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

        /*if (Rbody.velocity.y < -2)
        {
            yield return JumpCancel(false);
            yield break;
        }*/

        yield break;
    }

    private void Update()
    {
        if (TrailerMode || !FullyAwake)
        {
            return;
        }
        if (FlightEnabled)
        {
            //navigator.

            Vector3 pathPosition;

            if (followingPath)
            {
                Vector3 lastPos = transform.position;
                for (int i = 0; i < cornerCount; i++)
                {
                    Debug.DrawLine(lastPos, cornerCache[i], Color.blue);
                    lastPos = cornerCache[i];
                }

                Debug.DrawLine(lastPos, TargetPosition, Color.blue);

                var currentCorner = cornerCache[cornerIndex];


                var directionToCorner = (currentCorner - lastPathPos).normalized;

                var directionToBoss = (transform.position - lastPathPos);

                //Project the directionToBoss Vector onto the directionToCorner line
                var distanceToCorner = Vector2.Dot(directionToBoss, directionToCorner);


                /*if (Vector3.Distance(lastPathPos, transform.position) >= Vector3.Distance(lastPathPos, currentCorner) || Vector3.Distance(currentCorner, transform.position) <= 0.5f)*/
                if (distanceToCorner >= (currentCorner - lastPathPos).magnitude - 0.25f)
                {
                    cornerIndex++;
                    if (cornerIndex >= cornerCount)
                    {
                        followingPath = false;
                        cornerIndex = 0;
                    }
                    else
                    {
                        lastPathPos = currentCorner;
                    }
                }

                pathPosition = currentCorner;
            }
            else
            {
                pathPosition = TargetPosition;
            }
            /*if (NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, path))
            {
                //Debug.Log("PATH GENERATED");
                int count = path.GetCornersNonAlloc(cornerCache);
                Debug.Log("PATH COUNT = " + count);


                Vector3 lastPos = transform.position;
                for (int i = 0; i < count; i++)
                {
                    Debug.DrawLine(lastPos, cornerCache[i], Color.blue);
                    lastPos = cornerCache[i];
                }

                Debug.DrawLine(lastPos, TargetPosition, Color.blue);

            }*/

            float distanceToTarget;

            if (followingPath)
            {
                var currentCorner = cornerCache[cornerIndex];
                float distanceToNextPoint = Vector2.Distance(transform.position, currentCorner);
                distanceToTarget = Vector2.Distance(TargetPosition, transform.position);


                float interpolationFactor = 1f;

                if (cornerIndex + 1 < cornerCount)
                {
                    var nextCorner = cornerCache[cornerIndex + 1];
                    var currentCornerVector = (currentCorner - transform.position).normalized;
                    var nextCornerVector = (nextCorner - currentCorner).normalized;

                    //Debug.Log("DOT = " + Vector2.Dot(currentCorner, nextCornerVector));

                    //interpolationFactor = Mathf.Abs(Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(Vector2.Dot(currentCornerVector, nextCornerVector))));
                    interpolationFactor = Mathf.Clamp01(Vector2.Dot(currentCornerVector, nextCornerVector));

                    //Debug.Log("TURN FACTOR = " + interpolationFactor);
                }

                distanceToTarget = Mathf.Lerp(distanceToNextPoint, distanceToTarget, interpolationFactor);
            }
            else
            {
                distanceToTarget = Vector2.Distance(TargetPosition, transform.position);
            }

            //var distanceToTarget = Vector2.Distance(TargetPosition, transform.position);
            var directionToTarget = (Vector2)(pathPosition - transform.position);

            Debug.DrawLine(transform.position, pathPosition, Color.cyan);

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

    private void FixedUpdate()
    {
        Vector3 pathPosition;

        if (followingPath)
        {
            pathPosition = cornerCache[cornerIndex];
        }
        else
        {
            pathPosition = TargetPosition;
        }

        var directionToTarget = (Vector2)(pathPosition - transform.position);

        if (Vector3.Distance(transform.position, Player.Player1.transform.position) >= 40)
        {
            Rbody.velocity = Vector2.Lerp(Rbody.velocity, directionToTarget.normalized * Rbody.velocity.magnitude, 5f * Time.fixedDeltaTime);
        }
    }

    protected override void OnStun()
    {
        OnDeath();
    }

    protected override void OnDeath()
    {
        Debug.Log("ON DEATH START");
        base.OnStun();
        Body.OnStun();
        Head.OnStun();
        Claws.OnStun();
        WingPlates.OnStun();
        Wings.OnStun();

        sleepSprite.gameObject.SetActive(false);
        transform.localScale = Vector3.one;

        if (homeInOnTarget)
        {
            minFlightSpeed *= 3f;
            SetTarget(playerTarget);
            minimumFlightSpeed *= 4f;
            homeInOnTarget = false;
            orbitReductionAmount = origOrbitReductionAmount;
            flightSpeed *= 2f;
        }

        DisableCenterLock(centerCamLock);
        Claws.DisableSwingAttackImmediate();
        Body.PlayDefaultAnimation = false;
        Claws.OnGround = false;
        Recoil.ResetRecoilSpeed();
        Head.ToggleLaserBubbles(false);

        ApplyFlightVariance = false;
        var groundGlob = GetComponent<EnterGround_BigVomitShotMove>().SpawnedGlob;
        if (groundGlob != null)
        {
            groundGlob.ForceDisappear();
        }
        SetTarget(playerTarget);
        if (lungeSlideSoundPlayer != null)
        {
            lungeSlideSoundPlayer.Delete();
            lungeSlideSoundPlayer = null;
        }
        Rbody.gravityScale = 0f;
        Rbody.velocity = default;

        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
        {
            if (collider.GetComponent<WeaverCameraLock>() == null)
            {
                collider.enabled = false;
            }
        }

        Debug.Log("ON DEATH END");

        FlightEnabled = false;

        Head.UnlockHeadImmediate(Player.Player1.transform.position.x >= transform.position.x ? AspidOrientation.Right : AspidOrientation.Left);

        foreach (var laser in GetComponentsInChildren<LaserEmitter>())
        {
            laser.gameObject.SetActive(false);
        }
        Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, 0.5f);

        if (deathSound != null)
        {
            WeaverAudio.PlayAtPoint(deathSound,transform.position);
        }

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        //TODO - FIGURE OUT WHY THE DEATH CAM LOCK ISN"T WORKING"
        EnableCenterLock(transform.position, deathCamLock);
        centerCamLock.GetComponent<Collider2D>().enabled = true;

        var targetOrientation = Player.Player1.transform.position.x >= transform.position.x ? AspidOrientation.Right : AspidOrientation.Left;

        IEnumerator[] routines = new IEnumerator[]
        {
            Body.ChangeDirection(targetOrientation, 100000),
            Head.ChangeDirection(targetOrientation, 100000),
            Claws.ChangeDirection(targetOrientation, 100000),
            WingPlates.ChangeDirection(targetOrientation, 100000),
            Wings.ChangeDirection(targetOrientation, 100000)
        };

        yield return RoutineAwaiter.AwaitRoutines(this, routines);

        foreach (var flasher in GetComponentsInChildren<SpriteFlasher>())
        {
            flasher.flashInfectedLong();
        }

        yield return Head.LockHead(Player.Player1.transform.position.x >= transform.position.x ? AspidOrientation.Right : AspidOrientation.Left, 1000);

        foreach (var claw in Claws.claws)
        {
            if (!claw.ClawLocked)
            {
                yield return claw.LockClaw();
            }
            claw.Animator.PlaybackSpeed = 1000;
            claw.Animator.PlayAnimation("Lunge Antic", () =>
            {
                claw.Animator.PlaybackSpeed = 1f;
            });
        }

        StartCoroutine(ShakeRoutine(0.15f, 60f));

        Player.Player1.EnterCutsceneLock(true, 2);

        yield return new WaitForSeconds(deathSpitDelay);

        yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Prepare");

        //FIRE ITEM

        float angleMin;
        float angleMax;

        if (Head.CurrentOrientation == AspidOrientation.Right)
        {
            angleMin = -90;
            angleMax = 0f;
        }
        else
        {
            angleMin = -180;
            angleMax = -90;
        }

        var angle = UnityEngine.Random.Range(angleMin, angleMax);

        var velocity = MathUtilities.PolarToCartesian(angle, deathItemVelocity);

        Blood.SpawnDirectionalBlood(Head.transform.position, Head.CurrentOrientation == AspidOrientation.Right ? CardinalDirection.Right : CardinalDirection.Left);

        var spawnedItem = GameObject.Instantiate(itemPrefab, Head.transform.position, Quaternion.identity);
        spawnedItem.RB.velocity = velocity;

        if (spitItemAudio != null)
        {
            WeaverAudio.PlayAtPoint(spitItemAudio, transform.position);
        }


        yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Attack");

        yield return new WaitForSeconds(flyAwayDelay);

        var camPosition = WeaverCamera.Instance.transform.position;

        while (transform.position.y < camPosition.y + 15f)
        {
            Rbody.velocity += new Vector2(0f,flyAwayAcceleration * Time.deltaTime);
            yield return null;
        }

        Rbody.velocity = default;

        if (flyAwayThunkSound != null)
        {
            WeaverAudio.PlayAtPoint(flyAwayThunkSound, transform.position);
        }

        CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        Player.Player1.ExitCutsceneLock();

        DisableCenterLock(deathCamLock);

        gameObject.SetActive(false);

        yield break;
    }

#if UNITY_EDITOR

    static Player GizmoPlayer;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(TargetPosition, Vector3.one);
        Gizmos.color = Color.Lerp(Color.red, Color.blue,0.5f);
        Gizmos.color = Gizmos.color.With(a: 0.5f);

        if (GizmoPlayer == null)
        {
            GizmoPlayer = GameObject.FindObjectOfType<Player>();
        }

        if (GizmoPlayer != null)
        {
            Gizmos.DrawCube(GizmoPlayer.transform.position, new Vector3(camBoxWidth, camBoxHeight, 0.1f));
        }
    }

    public bool PointVisibleToPlayer(Vector2 point)
    {
        var rect = new Rect(Player.Player1.transform.position, new Vector2(camBoxWidth, camBoxHeight));

        return rect.Contains(point);
    }

#endif

    private void SetTarget(Transform targetTransform)
    {
        if (primaryTarget.SetTarget(targetTransform))
        {
            UpdatePath();
        }
        /*if ((targetTransform != null) != targetingTransform || targetTransform != TargetTransform)
        {
            targetingTransform = targetTransform != null;
            TargetTransform = targetTransform;
            UpdatePath();
        }*/
    }

    private void SetTarget(Vector3 fixedTarget)
    {
        if (primaryTarget.SetTarget(fixedTarget))
        {
            UpdatePath();
        }
        /*if (targetingTransform == true || fixedTarget != fixedTargetPos)
        {
            targetingTransform = false;
            TargetTransform = null;
            fixedTargetPos = fixedTarget;
            UpdatePath();
        }*/
    }

    private void SetTarget(Func<Vector3> targetFunc)
    {
        if (primaryTarget.SetTarget(targetFunc))
        {
            UpdatePath();
        }
    }

    public TargetOverride AddTargetOverride()
    {
        var target = new TargetOverride();

        targetOverrides.Add(target);

        return target;
    }

    public bool RemoveTargetOverride(TargetOverride target)
    {
        return targetOverrides.Remove(target);
    }

    /*public void FreezeTarget(Func<Vector3> frozenTargetOffset)
    {
        if (!targetFrozen)
        {
            targetFrozen = true;
        }

        this.frozenTargetPosition = frozenTargetOffset;
    }

    public void UnfreezeTarget()
    {
        if (targetFrozen)
        {
            frozenTargetPosition = default;
            targetFrozen = false;
        }
    }*/

    public float GetAngleToPlayer()
    {
        var direction = (Player.Player1.transform.position + new Vector3(0f,0.5f,0f) - Head.transform.position).normalized;

        return MathUtilities.CartesianToPolar(direction).x;
    }

    public void StartPhases()
    {
        Phase = AncientAspid.BossPhase.Phase1;
        OffensiveModeEnabled = false;
        GroundModeEnabled = false;

        GetComponent<VomitShotMove>().EnableMove(false);
    }

    public void GoToNextPhase()
    {
        switch (Phase)
        {
            case BossPhase.Phase1:
                Phase = BossPhase.Phase2;
                ExtraTargetOffset = new Vector3(0f,0.35f,0f);
                GetComponent<VomitShotMove>().EnableMove(true);
                break;
            case BossPhase.Phase2:
                Phase = BossPhase.Phase2A;
                break;
            case BossPhase.Phase2A:
                Phase = BossPhase.Phase2B;
                break;
            case BossPhase.Phase2B:
                Phase = BossPhase.Phase2C;
                HealthManager.AddModifier<InvincibleHealthModifier>();
                break;
            case BossPhase.Phase2C:
                Phase = BossPhase.Phase3;
                ExtraTargetOffset = default;
                HealthManager.RemoveModifier<InvincibleHealthModifier>();

                EnteringFromBottom = true;
                //WeaverLog.LogError("BEGINNING BOTTOM ENTRY PRE");
                StartBoundRoutine(EnterFromBottomRoutine());

                /*PathingMode = PathfindingMode.FollowTarget;
                if (transform.position.x >= 223.7f)
                {
                    SetTarget(new Vector3(258.68f, 203.49f));
                }
                else
                {
                    SetTarget(new Vector3(184.3f, 203.49f));
                }*/

                /*if (Player.Player1.transform.position.y >= 200 && transform.position.y < 200)
                {
                    
                }
                else
                {
                    WeaverLog.LogError("NOT BEGINNING BOTTOM ENTRY PRE");
                    OffensiveModeEnabled = true;
                }*/
                break;
            case BossPhase.Phase3:
                HealthManager.AddModifier<InvincibleHealthModifier>();
                ExtraTargetOffset = new Vector3(0f, 10f, 0f);
                PathingMode = PathfindingMode.FollowPlayer;
                SetTarget(Player.Player1.transform);
                Phase = BossPhase.Phase3A;
                OffensiveModeEnabled = false;
                break;
            case BossPhase.Phase3A:
                OffensiveModeEnabled = false;
                Phase = BossPhase.Phase3B;

                ExtraTargetOffset = default;
                PathingMode = PathfindingMode.FollowTarget;
                if (transform.position.x >= 223.7f)
                {
                    SetTarget(new Vector3(292.8f, 287.4f));
                }
                else
                {
                    SetTarget(new Vector3(142.4f, 287.4f));
                }

                break;
            case BossPhase.Phase3B:
                Phase = BossPhase.Phase3C;
                OffensiveModeEnabled = false;
                //PathingMode = PathfindingMode.FollowPlayer;
                //SetTarget(playerTarget);

                break;
            case BossPhase.Phase3C:
                HealthManager.RemoveModifier<InvincibleHealthModifier>();
                Phase = BossPhase.Phase4;
                GroundModeEnabled = true;
                ExtraTargetOffset = default;
                OffensiveModeEnabled = true;

                PathingMode = PathfindingMode.FollowPlayer;
                SetTarget(playerTarget);

                StartBoundRoutine(CowardiceRoutine(2));
                break;
            default:
                break;
        }

        WeaverLog.LogError("NOW IN PHASE = " + Phase);
    }

    IEnumerator CowardiceRoutine(int timesBeforeFlyAway)
    {
        int timesCounted = 0;
        while (true)
        {
            yield return new WaitUntil(() => Player.Player1.transform.position.y >= 271.3f);
            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(() => Player.Player1.transform.position.y < 271.3f);
            if (timesCounted++ >= timesBeforeFlyAway)
            {
                break;
            }
        }

        SetTarget(new Vector3(223.7f, 445.2f));
        PathingMode = PathfindingMode.FollowTarget;
        WeaverLog.LogWarning("FLYING AWAY");

        allMovesDisabled = true;
        yield return new WaitUntil(() => AspidMode != Mode.Defensive);
        yield return new WaitUntil(() => transform.position.y >= Player.Player1.transform.position.y + 20f);

        FlightEnabled = false;
        ApplyFlightVariance = false;

        /*if (transform.position.y >= Player.Player1.transform.position.y + 20f)
        {
            
        }*/

        Music.ApplyMusicSnapshot(Music.SnapshotType.Silent,0f,0.5f);

        while (true)
        {
            Rbody.velocity = Mathf.Max(Rbody.velocity.magnitude, 20f) * Vector2.up;
            yield return null;
        }

        //TODO - FLY AWAY
    }

    enum CentralRiseSpeed
    {
        Default,
        Quick,
        Instant
    }

    IEnumerator CentralRiseRoutine()
    {
        FlightEnabled = false;
        ApplyFlightVariance = false;
        transform.SetPosition2D(220.8f, 188.95f);
        SetTarget(transform.position);
        Rbody.velocity = default;
        Rbody.simulated = false;

        var riseSpeed = CentralRiseSpeed.Default;

        riseFromCenterPlatform = true;

        if (centerPlatformLockArea != null)
        {
            //TODO - ALSO ACCOUNT FOR THE FACT THAT THE PLAYER CAN GO UNDERNEATH THE ARENA. Maybe make the boss immediately rise up in that case
            //yield return new WaitUntil(() => GameManager.instance.cameraCtrl.lockZoneList.Contains(centerPlatformLockArea));

            //242.3

            while (true)
            {
                if (GameManager.instance.cameraCtrl.lockZoneList.Contains(centerPlatformLockArea))
                {
                    break;
                }

                if (Player.Player1.transform.position.y < 200)
                {
                    if (Player.Player1.transform.position.x <= 242.3f && Player.Player1.transform.position.x >= 201f)
                    {
                        riseSpeed = CentralRiseSpeed.Instant;
                        break;
                    }
                }

                if (Player.Player1.transform.position.y >= 220f)
                {
                    riseSpeed = CentralRiseSpeed.Quick;
                    break;
                }
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }


        AudioPlayer rumbleSound = null;

        if (riseSpeed == CentralRiseSpeed.Default)
        {
            if (centerModeSummonGrass != null)
            {
                centerModeSummonGrass.Play();
            }

            CameraShaker.Instance.SetRumble(RumbleType.RumblingMed);

            rumbleSound = WeaverAudio.PlayAtPointLooped(centerModeRumbleSound, transform.position);
        }

        IEnumerator VisualsRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (centerModeSummonGrass != null)
            {
                centerModeSummonGrass.Stop();
            }

            rumbleSound?.StopPlaying();

            CameraShaker.Instance.SetRumble(RumbleType.None);
            CameraShaker.Instance.Shake(ShakeType.BigShake);

            if (centerModeRiseSound != null)
            {
                WeaverAudio.PlayAtPoint(centerModeRiseSound, transform.position);
            }

            if (riseSpeed == CentralRiseSpeed.Default)
            {
                if (centerModeRoarSound != null)
                {
                    var instance = WeaverAudio.PlayAtPoint(centerModeRoarSound, transform.position);
                    instance.AudioSource.pitch = centerModeRoarSoundPitch;
                }

                var roar = RoarEmitter.Spawn(transform.position);
                roar.StopRoaringAfter(centerModeRoarDuration);

                var prevPosition = transform.position;

                for (float t = 0; t < centerModeRoarDuration; t += Time.deltaTime)
                {
                    var newPosition = transform.position;
                    roar.transform.localPosition += newPosition - prevPosition;
                    prevPosition = newPosition;
                    yield return null;
                }
            }
        }

        //OffensiveModeEnabled = true;
        //riseFromCenterPlatform = true;

        if (riseSpeed == CentralRiseSpeed.Default)
        {
            EnableCenterLock(centerModePlatformDest, centerCamLock);
        }

        if (riseSpeed == CentralRiseSpeed.Default)
        {
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitUntil(() => Orientation == AspidOrientation.Center);

        if (riseSpeed != CentralRiseSpeed.Instant)
        {
            StartCoroutine(VisualsRoutine(centerModeRoarDelay));
        }

        SetTarget(centerModePlatformDest);

        var oldPos = transform.position;

        if (riseSpeed != CentralRiseSpeed.Instant)
        {
            for (float t = 0f; t < centerModePlatformTime; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(oldPos, centerModePlatformDest, centerModePlatformCurve.Evaluate(t / centerModePlatformTime));

                yield return null;
            }
        }

        transform.position = centerModePlatformDest;

        if (riseSpeed == CentralRiseSpeed.Instant || riseSpeed == CentralRiseSpeed.Quick)
        {
            OffensiveModeEnabled = false;
        }
        else
        {
            OffensiveModeEnabled = true;
        }

        ExtraTargetOffset = default;

        Rbody.velocity = default;
        Rbody.simulated = true;

        FlightEnabled = true;
        ApplyFlightVariance = true;
        riseFromCenterPlatform = false;
        PathingMode = PathfindingMode.FollowPlayer;

        if (riseSpeed == CentralRiseSpeed.Instant || riseSpeed == CentralRiseSpeed.Quick)
        {
            yield return new WaitUntil(() => Orientation != AspidOrientation.Center);
            OffensiveModeEnabled = true;
        }

        GetComponent<LaserRapidFireMove>().EnableMove(true);

        EnableTargetHeightRange = true;
        TargetHeightRange.x = 201f;
    }

    enum CenterBottomTargetType
    {
        None,
        Bottom,
        Left,
        Right
    }

    IEnumerator EnterFromBottomRoutine()
    {

        yield return new WaitUntil(() => FullyAwake);
        //SetTarget(transform.position.With(y: 0f));

        //bool falling = false;
        //var currentTargetType = CenterBottomTargetType.None;



        while (true)
        {
            var playerAboveArena = Player.Player1.transform.position.y >= 200;
            var bossAboveArena = transform.position.y >= 200;

            if ((playerAboveArena && bossAboveArena)/* || currentTargetType == CenterBottomTargetType.Left || currentTargetType != CenterBottomTargetType.Right*/)
            {
                TargetHeightRange.x = 0f;
                ExtraTargetOffset = default;

                //TODO - IF BOTH ARE ABOVE. TRY FLYING TO THE OTHER SIDE and TELEPORT TO THE BOTTOM.

                if (Player.Player1.transform.position.x >= 197.62f)
                {
                    //if (currentTargetType != CenterBottomTargetType.Left)
                    //{
                        SetTarget(new Vector3(167.4f,216.7f));
                        PathingMode = PathfindingMode.FollowTarget;
                        WeaverLog.LogWarning("FOLLOW LEFT TARGET");
                        //currentTargetType = CenterBottomTargetType.Left;
                    //}
                    if (transform.position.x <= 179.7f || transform.position.x <= Player.Player1.transform.position.x - 20f)
                    {
                        yield return CentralRiseRoutine();
                        break;
                        /*if (!riseFromCenterPlatform)
                        {
                            yield break;
                        }
                        else
                        {
                            currentTargetType = CenterBottomTargetType.None;
                        }*/
                    }
                }
                else
                {
                    //if (currentTargetType != CenterBottomTargetType.Right)
                    //{
                        SetTarget(new Vector3(262.5f, 216.7f));
                        PathingMode = PathfindingMode.FollowTarget;
                        WeaverLog.LogWarning("FOLLOW RIGHT TARGET");
                        //currentTargetType = CenterBottomTargetType.Right;
                    //}

                    if (transform.position.x >= 251.1f || transform.position.x >= Player.Player1.transform.position.x + 20f)
                    {
                        yield return CentralRiseRoutine();
                        break;
                        /*if (!riseFromCenterPlatform)
                        {
                            yield break;
                        }
                        else
                        {
                            riseFromCenterPlatform = false;
                            currentTargetType = CenterBottomTargetType.None;
                        }*/
                    }
                }
                //break;
            }
            else if (playerAboveArena && !bossAboveArena)
            {
                TargetHeightRange.x = 0f;
                ExtraTargetOffset = default;
                //if (currentTargetType != CenterBottomTargetType.Bottom)
                //{
                    PathingMode = PathfindingMode.FollowTarget;
                    WeaverLog.LogWarning("FOLLOW BOTTOM TARGET");
                    SetTarget(new Vector3(220.8f, 85.29f));
                    //currentTargetType = CenterBottomTargetType.Bottom;
                //}

                if (transform.position.y <= Player.Player1.transform.position.y - 15f)
                {
                    yield return CentralRiseRoutine();
                    break;
                }
            }
            /*else if (!playerAboveArena && bossAboveArena)
            {
                TargetHeightRange.x = 0f;
                ExtraTargetOffset = new Vector3(0f, 6.5f, 0f);
                //if (currentTargetType != CenterBottomTargetType.None)
                //{

                    PathingMode = PathfindingMode.FollowTarget;
                    SetTarget(Player.Player1.transform);
                    WeaverLog.LogWarning("FOLLOW PLAYER A");
                    //currentTargetType = CenterBottomTargetType.None;
                //}
            }*/
            else if ((!playerAboveArena && !bossAboveArena) || (!playerAboveArena && bossAboveArena))
            {
                if (Player.Player1.transform.position.y <= transform.position.y)
                {
                    ExtraTargetOffset = new Vector3(0f, 0f, 0f);
                    if (transform.position.y >= 180f)
                    {
                        TargetHeightRange.x = 0f;
                        PathingMode = PathfindingMode.FollowTarget;
                        if (transform.position.x >= 223.7f)
                        {
                            //SET TO RIGHT TARGET
                            SetTarget(new Vector3(250.46f, 213.86f));
                        }
                        else
                        {
                            //SET TO LEFT TARGET
                            SetTarget(new Vector3(185.6f, 213.86f));
                        }
                    }
                    else
                    {
                        PathingMode = PathfindingMode.FollowPlayer;
                        SetTarget(playerTarget);

                        TargetHeightRange.x = Mathf.Max(TargetHeightRange.x,Player.Player1.transform.position.y + 14f);
                    }
                }
                else
                {
                    TargetHeightRange.x = 0f;
                    PathingMode = PathfindingMode.FollowTarget;
                    if (transform.position.x >= 223.7f)
                    {
                        //SET TO LEFT TARGET
                        SetTarget(new Vector3(185.6f, 213.86f));
                    }
                    else
                    {
                        //SET TO RIGHT TARGET
                        SetTarget(new Vector3(250.46f, 213.86f));
                    }
                }

                /*if (Player.Player1.transform.position.y <= 180f)
                {
                    ExtraTargetOffset = new Vector3(0f, 6.5f, 0f);
                    //if (currentTargetType != CenterBottomTargetType.None)
                    //{
                        PathingMode = PathfindingMode.FollowTarget;
                        SetTarget(new Vector3(249.2f, 187.6f));
                        WeaverLog.LogWarning("FOLLOW PLAYER B");
                        //currentTargetType = CenterBottomTargetType.None;
                    //}
                }
                else
                {
                    ExtraTargetOffset = default;
                    //if (currentTargetType != CenterBottomTargetType.Bottom)
                    //{
                        PathingMode = PathfindingMode.FollowTarget;
                        WeaverLog.LogWarning("FOLLOW BOTTOM TARGET");
                        SetTarget(new Vector3(220.8f, 85.29f));
                        //currentTargetType = CenterBottomTargetType.Bottom;
                    //}

                    if (transform.position.y <= Player.Player1.transform.position.y - 15f)
                    {
                        yield return CentralRiseRoutine();
                        break;
                    }
                }*/
            }

            yield return null;
            /*if (Player.Player1.transform.position.y >= 200 && transform.position.y < 200 && Player.Player1.transform.position.y <= 217f)
            {

            }*/
        }

        EnteringFromBottom = false;

        /*while (true)
        {
            if (Player.Player1.transform.position.y >= 200 && transform.position.y < 200 && Player.Player1.transform.position.y <= 217f)
            {
                WeaverLog.Log("BEGINNING BOTTOM ENTRY");
                if (transform.position.y <= 170.0f)
                {
                    FlightEnabled = false;
                    ApplyFlightVariance = false;
                    transform.SetPosition2D(220.8f, 188.95f);
                    SetTarget(transform.position);
                    Rbody.velocity = default;
                    Rbody.simulated = false;

                    //TODO - PLAY RUMBLE

                    //OffensiveModeEnabled = true;
                    forceCentralMode = true;

                    EnableCenterLock(centerModePlatformDest, centerCamLock);

                    yield return new WaitForSeconds(1f);

                    yield return new WaitUntil(() => Orientation == AspidOrientation.Center);

                    SetTarget(centerModePlatformDest);

                    var oldPos = transform.position;

                    for (float t = 0f; t < centerModePlatformTime; t += Time.deltaTime)
                    {
                        transform.position = Vector3.Lerp(oldPos, centerModePlatformDest,centerModePlatformCurve.Evaluate(t /  centerModePlatformTime));

                        yield return null;
                    }

                    ExtraTargetOffset = default;

                    Rbody.velocity = default;
                    Rbody.simulated = true;

                    FlightEnabled = true;
                    ApplyFlightVariance = true;
                    forceCentralMode = false;


                    WeaverLog.LogError("FORCED CENTERAL");
                    yield break;
                }
            }
            else
            {
                SetTarget(playerTarget);
                yield break;
            }

            yield return null;
        }*/
    }
}
