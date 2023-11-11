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
using WeaverCore.Settings;
using WeaverCore.Implementations;
using UnityEngine.Serialization;



/*[Serializable]
public class FlightParameters
{
    public readonly FlightParameters Original;

    public float flightAcceleration;
    public float flightSpeedOverDistance;
    public float minFlightSpeed;
    public float orbitReductionAmount;
    public Vector2 flightOffset;
    public float flightOffsetResetTime;
    public float flightOffsetChangeTime;
    public float flightSpeed;
    public float minimumFlightSpeed;
    public float maximumFlightSpeed;
    public bool applyFlightVariance;
    public bool homeInOnTarget;

    public FlightParameters(float flightAcceleration, float flightSpeedOverDistance, float minFlightSpeed, float orbitReductionAmount, Vector2 flightOffset, float flightOffsetResetTime, float flightOffsetChangeTime, float flightSpeed, float minimumFlightSpeed, float maximumFlightSpeed, bool applyFlightVariance, bool homeInOnTarget)
    {
        this.flightAcceleration = flightAcceleration;
        this.flightSpeedOverDistance = flightSpeedOverDistance;
        this.minFlightSpeed = minFlightSpeed;
        this.orbitReductionAmount = orbitReductionAmount;
        this.flightOffset = flightOffset;
        this.flightOffsetResetTime = flightOffsetResetTime;
        this.flightOffsetChangeTime = flightOffsetChangeTime;
        this.flightSpeed = flightSpeed;
        this.minimumFlightSpeed = minimumFlightSpeed;
        this.maximumFlightSpeed = maximumFlightSpeed;
        this.applyFlightVariance = applyFlightVariance;
        this.homeInOnTarget = homeInOnTarget;
    }
}*/

public class AncientAspid : Boss
{
    /*public enum PathfindingMode
    {
        None,
        FollowPlayer,
        FollowTarget
    }*/

    /*public enum BossPhase
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
    }*/


    [field: Header("General Config")]
    [field: SerializeField]
    public bool TrailerMode { get; private set; } = false;

    [SerializeField]
    Phase defaultPhase;

    public Phase CurrentPhase { get; private set; } = null;

    Queue<Phase> phaseQueue = new Queue<Phase>();

    [SerializeField]
    bool godhomeMode = false;

    [SerializeField]
    MusicCue bossMusic;

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
    AncientAspidPrefabs prefabs;

    [SerializeField]
    TailCollider tailCollider;

    [field: SerializeField]
    public VomitGlob GlobPrefab { get; private set; }


    [field: Header("Sleep Mode")]
    [field: SerializeField]
    public bool StartAsleep { get; private set; } = false;

    public bool FullyAwake { get; private set; } = false;

    [SerializeField]
    List<GameObject> enableOnWakeUp;

    [SerializeField]
    SpriteRenderer sleepSprite;

    [SerializeField]
    AudioClip sleepAwakeSound;

    [SerializeField]
    float sleepAwakeSoundPitch = 1f;

    [SerializeField]
    float sleepPreJumpDelay = 0.75f;

    [SerializeField]
    float sleepShakeIntensity = 0.1f;

    [SerializeField]
    float sleepRoarDuration = 2f;

    [SerializeField]
    AudioClip sleepRoarSound;

    [SerializeField]
    float sleepRoarSoundPitch = 1f;

    [SerializeField]
    List<GroupSpawner> awakenGroupSpawners = new List<GroupSpawner>();

    [field: Header("Pathfinding")]
    [field: SerializeField]
    public bool PathfindingEnabled { get; set; } = true;

    [field: SerializeField]
    public PathfindingSystem PathFinder { get; private set; }

    [SerializeField]
    float pathRegenInterval = 0.5f;

    [SerializeField]
    CircleCollider2D aspidTerrainCollider;

    [Header("Flight")]

    [SerializeField]
    float flightAcceleration = 10f;

    [SerializeField]
    float flightSpeedOverDistance = 1f;

    [SerializeField]
    float minFlightSpeed = 0.25f;

    /// <summary>
    /// The higher this number, the less likely the aspid will orbit around it's target. This is used to make it settle at the target instead
    /// </summary>
    public float OrbitReductionAmount { get; set; } = 5f;

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
    public bool ApplyFlightVariance { get; set; } = true;

    bool homeInOnTarget = false;

    /*[SerializeField]
    [Tooltip("The target object that is used when aiming towards the player")]
    private Transform playerTarget;

    public Transform PlayerTarget => playerTarget;*/

    [field: SerializeField]
    [field: Tooltip("The target object that is used when aiming towards the player")]
    [field: FormerlySerializedAs("playerTarget")]
    public Transform PlayerTarget { get; private set; }

    //public Vector3 ExtraTargetOffset;

    public Rect FlightRange;

    [Header("Death")]
    [SerializeField]
    WeaverCameraLock deathCamLock;

    [SerializeField]
    float deathCenterRoomAxis = 219.9f;

    [SerializeField]
    Vector2 deathDropItemXRange = new Vector2(180.02f, 259.7f);

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

    [SerializeField]
    SaveSpecificSettings settings;

    [SerializeField]
    string bossDefeatedField;

    public AspidOrientation Orientation { get; set; } = AspidOrientation.Left;

    TargetOverride primaryTarget = new TargetOverride(0);

    List<TargetOverride> targetOverrides = new List<TargetOverride>();

    public Vector3 TargetOffset { get; private set; }

    [SerializeField]
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

    public Rect CurrentRoomRect => CurrentPhase.PhaseBoundaries;

    //public bool FlyingAway { get; set; } = false;

    //public bool InClimbingPhase => Phase != BossPhase.Phase1 && Phase != BossPhase.Phase3 && Phase != BossPhase.Phase4;
    //public bool InClimbingPhase => false;
    public bool InClimbingPhase => CurrentPhase.ClimbingPhase;
    public Vector3 TargetPosition
    {
        get
        {
            Vector3 target = primaryTarget.TargetPosition;
            for (int i = targetOverrides.Count - 1; i >= 0; i--)
            {
                if (targetOverrides[i].HasPositionSet)
                {
                    target = targetOverrides[i].TargetPosition;
                    break;
                }
            }
            //target = (Vector3)FlightRange.ClampWithin(target) + TargetOffset;
            return target;
        }
    }

    public Vector3 PathfindingTarget
    {
        get
        {
            return GetPathTarget() + new Vector3(0f, 4.5f);
        }
        /*get
        {
            if (NavMesh.FindClosestEdge(GetPathTarget() + new Vector3(0f,1f), out var hit, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                if (NavMesh.FindClosestEdge(GetPathTarget() + new Vector3(0f, -1f), out hit, NavMesh.AllAreas))
                {
                    return hit.position;
                }
                else
                {
                    return GetPathTarget();
                }
                //return Player.Player1.transform.position;
            }
        }*/
    }

    /*PathfindingMode _pathingMode = PathfindingMode.FollowPlayer;
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
    }*/

    //public bool EnteringFromBottom { get; private set; } = false;

    public Rect CamRect => new Rect { size = new Vector2(camBoxWidth, camBoxHeight), center = Player.Player1.transform.position };

    public bool CanSeeTarget => !followingPath && TargetWithinYRange && Vector3.Distance(transform.position,Player.Player1.transform.position) <= 25f;

    public bool EnableQuickEscapes { get; set; } = false;

    public bool TargetWithinYRange
    {
        get
        {
            var target = TargetPosition;

            return target.y >= (FlightRange.yMin - camBoxHeight / 2f) && target.y <= (FlightRange.yMax + camBoxHeight / 2f);
        }
    }

    public bool TargetWithinXRange
    {
        get
        {
            var target = TargetPosition;

            return target.x >= (FlightRange.xMin - camBoxWidth / 2f) && target.y <= (FlightRange.xMax + camBoxWidth / 2f);
        }
    }

    public bool IsOnScreen()
    {
        return IsOnScreen(transform.position);
    }

    public bool IsOnScreen(Vector3 pos)
    {
        var rect = CamRect;

        return rect.Contains(pos);
    }

    //public bool AllMovesDisabled => allMovesDisabled;

    //public Vector3 SpitTargetLeft => spitSourceLeft.transform.position;
    //public Vector3 SpitTargetRight => spitSourceRight.transform.position;

    public Rigidbody2D Rbody { get; private set; }

    public BodyController Body { get; private set; }
    public WingPlateController WingPlates { get; private set; }
    public HeadController Head { get; private set; }
    public WingsController Wings { get; private set; }
    public ClawController Claws { get; private set; }

    public Recoiler Recoil { get; private set; }

    /// <summary>
    /// The moves for the boss
    /// </summary>
    public List<AncientAspidMove> Moves { get; private set; }

    public AncientAspidHealth HealthManager { get; private set; }

    /// <summary>
    /// Returns true if the player is to the right of the boss.
    /// </summary>
    public bool PlayerRightOfBoss => Player.Player1.transform.position.x >= transform.position.x;

    //PlayerDamager[] damagers;
    float origOrbitReductionAmount;

    /*public int Damage
    {
        get => damagers[0].damageDealt;
        set
        {
            for (int i = damagers.GetLength(0) - 1; i >= 0; i--)
            {
                damagers[i].damageDealt = value;
            }
        }
    }*/

    /// <summary>
    /// Checks if the mode picker is running. When this is running, a random aspid mode will be selected and used to run moves.
    /// </summary>
    public bool ModePickerRunning => modePickerRoutine != 0;

    /// <summary>
    /// The currently running mode that has been selected by the mode picker
    /// </summary>
    public AncientAspidMode CurrentRunningMode { get; set; }

    uint modePickerRoutine = 0;
    int modeCounter = 0;
    bool stoppingModePicker = false;
    //bool stoppingCurrentMode = false;
    AncientAspidMode forcedMode = null;
    Dictionary<string, object> forcedModeArgs = null;

    //RaycastHit2D[] rayCache = new RaycastHit2D[4];

    NavMeshSurface navigator = null;
    NavMeshPath path = null;
    //Vector3[] cornerCache = new Vector3[100];
    List<Vector3> paths = new List<Vector3>();
    List<Vector3> pathsCache = new List<Vector3>();
    int pathCount = 0;
    int cornerIndex = 0;
    bool followingPath = false;
    Vector3 lastPathPos = default;
    RaycastHit2D[] singleHitCache = new RaycastHit2D[1];
    Vector3 lastKnownPosition = new Vector3(100000,100000);
    bool stillStuck = false;
    List<IPathfindingOverride> pathfindingOverrides = new List<IPathfindingOverride>();
    //bool riseFromCenterPlatform = false;

    public GroundMode GroundMode { get; private set; }
    public TacticalMode TacticalMode { get; private set; }
    public OffensiveMode OffensiveMode { get; private set; }

    Bounds stuckBox;
    int stuckCounter = 0;

    //FarAwayLaser farAwayMove;
    //bool forceOffensiveModeNextTime = false;

    //public bool RiseFromCenterPlatform => riseFromCenterPlatform;

    List<Renderer> renderers = new List<Renderer>();

    public int StartingHealth { get; private set; }

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

    IEnumerator<AncientAspidMove> MoveRandomizerFunc()
    {
        var moves = GetComponents<AncientAspidMove>().ToList();

        var currentMoves = new List<AncientAspidMove>();

        currentMoves.Capacity = moves.Count;

        AncientAspidMove lastMove = null;

        while (true)
        {
            currentMoves.AddRange(moves);
            currentMoves.RandomizeList();

            if (currentMoves[currentMoves.Count - 1] == lastMove)
            {
                currentMoves[currentMoves.Count - 1] = currentMoves[0];
                currentMoves[0] = lastMove;
            }

            bool moveFound = false;
            while (currentMoves.Count > 0)
            {
                var selectedMove = currentMoves[currentMoves.Count - 1];

                if (selectedMove.MoveEnabled)
                {
                    if (!moveFound)
                    {
                        moveFound = true;
                        if (selectedMove == lastMove)
                        {
                            for (int i = currentMoves.Count - 1; i >= 0; i--)
                            {
                                if (currentMoves[i] != lastMove && currentMoves[i].MoveEnabled)
                                {
                                    currentMoves[currentMoves.Count - 1] = currentMoves[i];
                                    currentMoves[i] = selectedMove;
                                    selectedMove = currentMoves[currentMoves.Count - 1];
                                    break;
                                }
                            }
                        }
                    }
                    lastMove = selectedMove;
                    Debug.Log("SELECTED MOVE = " + selectedMove);
                    yield return selectedMove;
                }
                currentMoves.Remove(selectedMove);
            }

            if (!moveFound)
            {
                yield return null;
            }
        }
    }

    public bool StartModePicker()
    {
        if (ModePickerRunning)
        {
            return false;
        }

        modePickerRoutine = StartBoundRoutine(ModePickerRoutine(), () =>
        {
            //CurrentRunningMode = null;
            modeCounter = 0;
            modePickerRoutine = 0;
            forcedMode = null;
        });
        return true;
    }

    /// <summary>
    /// Stops the current mode, and will switch to a new mode. Doesn't do anything if the mode picker isn't running. This function will run the new mode even if <seealso cref="AncientAspidMode.ModeEnabled"/> is false, so be careful
    /// </summary>
    public IEnumerator SwitchToNewMode(AncientAspidMode newMode, Dictionary<string, object> customArgs)
    {
        if (!ModePickerRunning)
        {
            yield break;
        }

        forcedMode = newMode;
        forcedModeArgs = customArgs;

        yield return StopCurrentMode();

        yield return new WaitUntil(() => forcedMode == null);
    }

    /// <summary>
    /// Stops the currently running mode and the mode picker
    /// </summary>
    /// <returns></returns>
    public IEnumerator StopModePicker()
    {
        if (!ModePickerRunning)
        {
            yield break;
        }

        stoppingModePicker = true;
        yield return StopCurrentMode();

        if (modePickerRoutine != 0)
        {
            yield return new WaitUntil(() => modePickerRoutine == 0);
        }
    }

    /// <summary>
    /// Forcefully stops the currently running mode so a new mode can be picked by the mode picker (assuming it's running)
    /// </summary>
    public IEnumerator StopCurrentMode()
    {
        if (!ModePickerRunning)
        {
            yield break;
        }

        if (CurrentRunningMode == null)
        {
            yield break;
        }

        var lastModeCounter = modeCounter;

        CurrentRunningMode.Stop();

        yield return new WaitUntil(() => modeCounter != lastModeCounter);
    }

    IEnumerator ModePickerRoutine()
    {
        List<AncientAspidMode> modes = new List<AncientAspidMode>();
        GetComponents(modes);

        modes.Remove(TacticalMode);

        stoppingModePicker = false;
        //stoppingCurrentMode = false;

        modeCounter = 1;

        //var emptyArgs = new Dictionary<string, object>();

        IEnumerator RunMode(AncientAspidMode mode, Dictionary<string, object> args)
        {
            CurrentRunningMode = mode;
            WeaverLog.Log("Entering " + StringUtilities.Prettify(mode.GetType().Name));
            yield return mode.Execute(args);
            CurrentRunningMode = null;
            WeaverLog.Log("Exiting " + StringUtilities.Prettify(mode.GetType().Name));
            modeCounter++;
        }

        while (!stoppingModePicker)
        {
            if (forcedMode == null && TacticalMode.IsModeEnabled())
            {
                yield return RunMode(TacticalMode, null);
            }

            if (stoppingModePicker)
            {
                break;
            }

            AncientAspidMode selectedMode;
            Dictionary<string, object> args;

            if (forcedMode != null)
            {
                selectedMode = forcedMode;
                args = forcedModeArgs;
                forcedMode = null;
                forcedModeArgs = null;
            }
            else
            {
                args = null;
                modes.RandomizeList();

                selectedMode = modes.FirstOrDefault(m => m.IsModeEnabled());
            }

            if (selectedMode != null)
            {
                yield return RunMode(selectedMode, args);
            }
        }

        modeCounter = 0;
        modePickerRoutine = 0;
        forcedMode = null;
    }

    IEnumerator<AncientAspidMove> _moveRandomizer;
    public IEnumerator<AncientAspidMove> MoveRandomizer => _moveRandomizer ??= MoveRandomizerFunc();

    protected override void Awake()
    {
        CurrentPhase = defaultPhase;

        GroundMode = GetComponent<GroundMode>();
        TacticalMode = GetComponent<TacticalMode>();
        OffensiveMode = GetComponent<OffensiveMode>();

        navigator = GameObject.FindObjectOfType<NavMeshSurface>();
        base.Awake();
        AncientAspidPrefabs.Instance = prefabs;
        //farAwayMove = GetComponent<FarAwayLaser>();
        Moves = GetComponents<AncientAspidMove>().ToList();
        //damagers = GetComponentsInChildren<PlayerDamager>();
        //CurrentRoomRect = RoomScanner.GetRoomBoundaries(transform.position);
        HealthManager = GetComponent<AncientAspidHealth>();
        Rbody = GetComponent<Rigidbody2D>();
        Body = GetComponentInChildren<BodyController>();
        WingPlates = GetComponentInChildren<WingPlateController>();
        Wings = GetComponentInChildren<WingsController>();
        Head = GetComponentInChildren<HeadController>();
        Claws = GetComponentInChildren<ClawController>();
        Recoil = GetComponent<Recoiler>();

        Recoil.OriginalRecoilSpeed = Recoil.GetRecoilSpeed();

        if (!StartAsleep)
        {
            FullyAwake = true;
            sleepSprite.gameObject.SetActive(false);
            foreach (var obj in enableOnWakeUp)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        else
        {
            sleepSprite.transform.SetParent(null);

            foreach (var c in GroundMode.collidersEnableOnLunge)
            {
                if (c != null)
                {
                    c.enabled = true;
                }
            }

            foreach (var c in GroundMode.collidersDisableOnLunge)
            {
                if (c != null)
                {
                    c.enabled = false;
                }
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
            SetTarget(PlayerTarget);
            StartBoundRoutine(VarianceResetter());

            if (navigator != null && PathfindingEnabled)
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

        if (godhomeMode)
        {
            //OffensiveModeEnabled = false;
            //GroundModeEnabled = false;

            HealthManager.AddHealthMilestone(Mathf.RoundToInt(StartingHealth * 0.7f), () => 
            {
                //OffensiveModeEnabled = true;
                //OffensiveAreaProvider = new DefaultOffensiveAreaProvider(this);
                //OffensiveMode.OffensiveAreaProvider = new DefaultOffensiveAreaProvider(OffensiveMode.OffensiveHeight);
                GetComponent<LaserRapidFireMove>().EnableMove(true);
            });

            /*HealthManager.AddHealthMilestone(Mathf.RoundToInt(StartingHealth * 0.45f), () =>
            {
                GroundMode.GroundAreaProvider = new DefaultGroundAreaProvider(GroundMode.lungeTargetOffset);
                //GroundAreaProvider = new DefaultGroundAreaProvider(this, lungeTargetOffset);
            });*/
        }
    }

    private void HealthManager_OnHitEvent(HitInfo obj)
    {
        if (EnableQuickEscapes)
        {
            const float escapeTime = 0.5f;
            const float escapeMagnitude = 5f;
            var direction = DirectionUtilities.DegreesToDirection(GetAngleToPlayer());

            switch (direction)
            {
                case CardinalDirection.Up:
                case CardinalDirection.Down:
                    if (transform.position.x >= Player.Player1.transform.position.x && (CurrentRoomRect.xMax - transform.position.x) >= (escapeMagnitude * escapeTime))
                    {
                        QuickEscape(Vector2.right * 5f, escapeTime);
                    }
                    else if (transform.position.x < Player.Player1.transform.position.x && transform.position.x - CurrentRoomRect.xMin >= (escapeMagnitude * escapeTime))
                    {
                        QuickEscape(Vector2.left * 5f, escapeTime);
                    }
                    break;
                case CardinalDirection.Left:
                case CardinalDirection.Right:
                    if ((CurrentRoomRect.yMax - transform.position.y) >= (escapeMagnitude * escapeTime))
                    {
                        QuickEscape(Vector2.up * 5f, escapeTime);
                    }
                    else if (transform.position.y - CurrentRoomRect.yMin >= (escapeMagnitude * escapeTime))
                    {
                        QuickEscape(Vector2.down * 5f, escapeTime);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void Start()
    {
        if (StartAsleep)
        {
            //Recoil.SetRecoilSpeed(0);
            Claws.DisableSwingAttackImmediate();
            StartBoundRoutine(PreBossRoutine());
        }

        if (!TrailerMode && !StartAsleep)
        {
            //StartBoundRoutine(SlowUpdate());
            StartBoundRoutine(MainBossRoutine());
        }

        GenerateStuckBox();
        HealthManager.OnHealthChangeEvent += HealthManager_OnHealthChangeEvent;
    }

    void GenerateStuckBox()
    {
        stuckBox = new Bounds(transform.position, Vector3.one);
    }

    private void HealthManager_OnHealthChangeEvent(int previousHealth, int newHealth)
    {
        if (stuckBox.Contains(transform.position))
        {
            stuckCounter++;
            if (stuckCounter >= 5)
            {
                stuckCounter = 0;
                //StartBoundRoutine(FullyUnstuckRoutine());
            }
        }
        else
        {
            GenerateStuckBox();
            stuckCounter = 0;
        }
    }

    /*IEnumerator FullyUnstuckRoutine()
    {
        aspidTerrainCollider.enabled = false;
        yield return new WaitForSeconds(5f);
        aspidTerrainCollider.enabled = true;
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


    public void QuickEscape(Vector2 direction, float time)
    {
        StartCoroutine(QuickEscapeRoutine(direction, time));
    }

    IEnumerator QuickEscapeRoutine(Vector2 direction, float time)
    {
        var target = AddTargetOverride(100);
        target.SetTarget(() =>
        {
            var targetIndex = targetOverrides.IndexOf(target);

            if (targetIndex == 0)
            {
                return transform.position + (Vector3)(direction);
            }
            else
            {
                return targetOverrides[targetIndex - 1].TargetPosition + (Vector3)(direction);
            }
        });
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (!EnableQuickEscapes)
            {
                break;
            }
            yield return null;
        }
        RemoveTargetOverride(target);
    }

    private IEnumerator PreBossRoutine()
    {
        using var recoilOverride = Recoil.AddRecoilOverride(0);

        var enemyLayers = GetAllObjectsWithLayer("Enemies").ToList();

        var enemyLayer = LayerMask.NameToLayer("Enemies");
        var attackLayer = LayerMask.NameToLayer("Attack");

        foreach (var obj in enemyLayers)
        {
            obj.layer = attackLayer;
        }

        var hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, LayerMask.GetMask("Terrain"));

        if (hit.collider != null)
        {
            transform.position = hit.point + new Vector2(0f, 1.73f);
        }

        Body.PlayDefaultAnimation = false;
        ApplyFlightVariance = false;
        FlightEnabled = false;
        Wings.PlayDefaultAnimation = false;

        var health = HealthComponent.Health;

        //Wait until the health changes
        //yield return new WaitUntil(() => health != HealthComponent.Health || Phase >= BossPhase.Phase2);
        yield return new WaitUntil(() => health != HealthComponent.Health);

        health = HealthComponent.Health;

        foreach (var obj in enemyLayers)
        {
            obj.layer = enemyLayer;
        }

        /*if (Phase >= BossPhase.Phase2)
        {
            yield return QuickWakeRoutine();
            yield break;
        }*/


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

        /*if (!useNewSleepSprites)
        {
            Rbody.velocity = new Vector2(0f, sleepJumpVelocity);
            Rbody.gravityScale = sleepJumpGravity;
        }
        else
        {
            //Rbody.velocity = new Vector2(0f, sleepJumpVelocity / 2f);
            //Rbody.gravityScale = sleepJumpGravity / 2;
        }*/

        GroundMode.lungeDashEffect.SetActive(true);
        GroundMode.lungeDashRotationOrigin.SetZLocalRotation(90);

        float stompingRate = 0.3f;

        //float stompingPitch = 

        /*IEnumerator DoStomping()
        {
            while (true)
            {
                if (lungeLandSoundHeavy != null)
                {
                    var sound = WeaverAudio.PlayAtPoint(lungeLandSoundHeavy, transform.position);
                    sound.AudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                }
                CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
                yield return new WaitForSeconds(stompingRate);
            }
        }*/

        var sleepAnimator = sleepSprite.GetComponent<WeaverAnimationPlayer>();
        yield return sleepAnimator.PlayAnimationTillDone("Turn Around Pre");
        sleepSprite.flipX = Player.Player1.transform.position.x >= transform.position.x;
        yield return sleepAnimator.PlayAnimationTillDone("Turn Around Post");

        /*if (useNewSleepSprites)
        {
            //Coroutine stompingRoutine = StartCoroutine(DoStomping());
            var sleepAnimator = sleepSprite.GetComponent<WeaverAnimationPlayer>();
            yield return sleepAnimator.PlayAnimationTillDone("Turn Around Pre");
            sleepSprite.flipX = Player.Player1.transform.position.x >= transform.position.x;
            yield return sleepAnimator.PlayAnimationTillDone("Turn Around Post");
            //StopCoroutine(stompingRoutine);
        }
        else
        {
            foreach (var scale in sleepScalesBefore)
            {
                sleepSprite.transform.localScale = sleepSprite.transform.localScale.With(x: scale);
                yield return new WaitForSeconds(sleepScaleChangeSpeed);
            }
        }*/

        foreach (var claw in Claws.claws)
        {
            yield return claw.LockClaw();
            claw.OnGround = true;
            claw.LandImmediately();
        }

        yield return awaiter.WaitTillDone();

        /*if (!useNewSleepSprites)
        {
            sleepSprite.gameObject.SetActive(false);

            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }

            foreach (var scale in sleepScalesAfter)
            {
                transform.localScale = transform.localScale.With(x: scale);
                yield return new WaitForSeconds(sleepScaleChangeSpeed);
            }
        }*/

        var time = Time.time;

        //WeaverLog.Log("A");
        yield return new WaitUntil(() => Rbody.velocity.y <= 0f || Time.time > time + 3f);
        //WeaverLog.Log("B");

        var prevVelocity = Rbody.velocity.y;

        for (float t = 0; t < 3f; t += 0f)
        {
            var pre = Time.time;
            yield return new WaitForSeconds(1f / 30f);
            t += Time.time - pre;
            var diff = Rbody.velocity.y - prevVelocity;
            prevVelocity = Rbody.velocity.y;
            if (Mathf.Abs(diff) <= 0.001f)
            {
                break;
            }
        }

        /*if (useNewSleepSprites)
        {
            sleepSprite.gameObject.SetActive(false);

            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
        }*/

        sleepSprite.gameObject.SetActive(false);

        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }


        /*if (!useNewSleepSprites)
        {
            groundMode.lungeRockParticles.Play();

            if (groundMode.lungeLandSoundHeavy != null)
            {
                WeaverAudio.PlayAtPoint(groundMode.lungeLandSoundHeavy, transform.position);
            }
            CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);
        }
        else
        {
            //lungeRockParticles.Play();
        }*/

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

        if (health == HealthManager.Health)
        {
            yield return new WaitForSeconds(sleepPreJumpDelay);
        }

        Rbody.gravityScale = GroundMode.onGroundGravity;
        if (health == HealthManager.Health)
        {
            yield return GroundMode.ExitGroundMode();
        }
        else
        {
            StartBoundRoutine(GroundMode.ExitGroundMode());
            yield return new WaitUntil(() => Claws.OnGround == false);
        }

        //yield return new WaitUntil(() => !Head.HeadBeingUnlocked);

        if (!Head.HeadLocked)
        {
            yield return Head.LockHead(Orientation == AspidOrientation.Right ? 60f : -60f);
        }

        yield return Head.Animator.PlayAnimationTillDone("Fire - 2 - Prepare");

        var attackStartFrame = Head.Animator.AnimationData.GetFrameFromClip("Fire - 1 - Attack",0);

        Head.MainRenderer.sprite = attackStartFrame;

        var shakeRoutine = StartCoroutine(ShakeRoutine(sleepShakeIntensity));

        if (sleepRoarSound != null)
        {
            var roar = WeaverAudio.PlayAtPoint(sleepRoarSound, transform.position);
            roar.AudioSource.pitch = sleepRoarSoundPitch;
        }


        WeaverBossTitle.Spawn("Ancient", "Aspid");

        foreach (var spawner in awakenGroupSpawners)
        {
            if (spawner != null)
            {
                spawner.StartSpawning();
            }
        }

        yield return Roar(sleepRoarDuration,Head.transform.position, true);

        StopCoroutine(shakeRoutine);

        FlightEnabled = true;
        yield return Claws.EnableSwingAttack(true);

        yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Attack");

        if (Head.HeadLocked)
        {
            Head.UnlockHead();
        }

        FullyAwake = true;

        foreach (var obj in enableOnWakeUp)
        {
            obj.SetActive(true);
        }

        Music.PlayMusicCue(bossMusic);

        //Recoil.ResetRecoilSpeed();

        StartBoundRoutine(VarianceResetter());

        if (navigator != null)
        {
            StartBoundRoutine(PathFindingRoutine());
        }

        StartBoundRoutine(MainBossRoutine());

        yield break;
    }

    public IEnumerator ShiftBodyParts(bool slide, params AspidBodyPart[] bodyParts)
    {
        List<float> defaultXValues = bodyParts.Select(p => p.transform.localPosition.x).ToList();
        List<AspidBodyPart> bodyPartsList = bodyParts.ToList();

        bool firstTime = true;

        for (int i = 0; i < GroundMode.lungeXShiftValues.Count; i++)
        {
            int index = 0;
            foreach (var part in bodyPartsList)
            {
                float shiftAmount = GroundMode.lungeXShiftValues[i];
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
                    yield return new WaitForSeconds(GroundMode.lungeDownwardsLandDelay);
                    continue;
                }
            }
            yield return new WaitForSeconds(1f / 12f);

            for (int j = bodyPartsList.Count - 1; j >= 0; j--)
            {
                float shiftAmount = GroundMode.lungeXShiftValues[i];
                if (Head.LookingDirection >= 60)
                {
                    shiftAmount = -shiftAmount;
                }
                if (bodyPartsList[j].transform.localPosition.x != defaultXValues[j] + shiftAmount)
                {
                    bodyPartsList.RemoveAt(j);
                    defaultXValues.RemoveAt(j);
                }
            }
        }
    }

    /*private IEnumerator SlowUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            CurrentRoomRect = RoomScanner.GetRoomBoundaries(transform.position);
        }
    }*/

    public IEnumerator MainBossRoutine()
    {
        SetTarget(PlayerTarget);
        EnableQuickEscapes = true;
        //HealthComponent.OnHealthChangeEvent += HealthComponent_OnHealthChangeEvent;
        HealthManager.OnHitEvent += HealthManager_OnHitEvent;

        StartModePicker();

        StartBoundRoutine(PhaseCheckerRoutine());

        yield break;
    }

    IEnumerator PhaseCheckerRoutine()
    {
        yield return CurrentPhase.PhaseStart(this, null);

        StartCoroutine(PhaseRunnerRoutine());

        var currentlyCheckedPhase = CurrentPhase;

        while (true)
        {
            yield return new WaitUntil(() => currentlyCheckedPhase.CanGoToNextPhase(this));
            phaseQueue.Enqueue(currentlyCheckedPhase.NextPhase);
            currentlyCheckedPhase = currentlyCheckedPhase.NextPhase;
        }
        /*var oldPhase = Phase;

        while (true)
        {
            yield return new WaitUntil(() => Phase > oldPhase);

            var newPhase = oldPhase + 1;

            yield return ChangePhase(oldPhase, newPhase);

            oldPhase = newPhase;
        }*/
    }

    IEnumerator PhaseRunnerRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => phaseQueue.Count > 0);

            var newPhase = phaseQueue.Dequeue();
            yield return CurrentPhase.PhaseEnd(this, newPhase);
            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            yield return CurrentPhase.PhaseStart(this, oldPhase);
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

    /// <summary>
    /// Changes the flight system so that it flies and settles towards a target. Allows you to freely set the <see cref="OrbitReductionAmount"/>
    /// </summary>
    public void EnableHomeInOnTarget()
    {
        if (!homeInOnTarget)
        {
            minFlightSpeed /= 3f;
            minimumFlightSpeed /= 4f;
            homeInOnTarget = true;
            flightSpeed /= 2f;

            origOrbitReductionAmount = OrbitReductionAmount;
        }
    }

    /// <summary>
    /// Changes the flight system so that it no longer flies and settles towards a target
    /// </summary>
    public void DisableHomeInOnTarget()
    {
        if (homeInOnTarget)
        {
            minFlightSpeed *= 3f;
            minimumFlightSpeed *= 4f;
            homeInOnTarget = false;
            OrbitReductionAmount = origOrbitReductionAmount;
            flightSpeed *= 2f;
        }
    }

    /// <summary>
    /// Enables a cam lock and positions it at a certain location
    /// </summary>
    /// <param name="position">The location the camera lock should be at</param>
    /// <param name="camLock">The cam lock to enable</param>
    public void EnableCamLock(Vector3 position, WeaverCameraLock camLock)
    {
        var currentLocks = GameManager.instance.cameraCtrl.lockZoneList;

        Bounds mainBounds = default;


        if (currentLocks == null || currentLocks.Count == 0)
        {
            if (GameManager.instance.sm is WeaverSceneManager wsm)
            {
                var sceneRect = wsm.SceneDimensions;
                mainBounds = new Bounds();
                mainBounds.SetMinMax(sceneRect.min, sceneRect.max);
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
                mainBounds.SetMinMax(new Vector3(l.cameraXMin, l.cameraYMin, -1f), new Vector3(l.cameraXMax, l.cameraYMax, 1f));
                if (mainBounds.Contains(transform.position))
                {
                    break;
                }
            }
        }

        camLock.gameObject.SetActive(false);
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

    public void DisableCamLock(WeaverCameraLock camLock)
    {
        camLock.gameObject.SetActive(false);
        camLock.transform.SetParent(transform);
        camLock.transform.localPosition = default;
    }


    public AspidOrientation GetOrientationToPlayer()
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

    public IEnumerator UpdateDirection()
    {
        Vector3 targetPosition;

        if (followingPath)
        {
            targetPosition = paths[cornerIndex] - aspidTerrainCollider.transform.localPosition;
        }
        else
        {
            targetPosition = Player.Player1.transform.position;
        }


        if (transform.position.x >= targetPosition.x)
        {
            if (Orientation == AspidOrientation.Right)
            {
                yield return ChangeDirection(AspidOrientation.Left, 1.2f);
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
                yield return ChangeDirection(AspidOrientation.Right, 1.2f);
            }
            else
            {
                yield return null;
            }
        }
    }

    public IEnumerator ChangeDirection(AspidOrientation newOrientation, float speedMultiplier = 1f)
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

    /*void ShuffleMoves(List<AncientAspidMove> moves)
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
    }*/

    Vector3 GetPathTarget()
    {
        //Vector3 target = GetPathTargetUnclamped(mode);



        /*if (EnableTargetXRange)
        {
            target.x = Mathf.Clamp(target.y, TargetXRange.x, TargetXRange.y);
        }

        if (EnableTargetHeightRange)
        {
            target.y = Mathf.Clamp(target.y, TargetHeightRange.x, TargetHeightRange.y);
        }*/
        //return target;

        return FlightRange.ClampWithin(TargetPosition);
        //return TargetPosition;
    }

    /*Vector3 GetPathTargetUnclamped(PathfindingMode mode)
    {
        Vector3 target;
        switch (mode)
        {
            case PathfindingMode.FollowPlayer:
                target = Player.Player1.transform.position + new Vector3(0f, 0.5f);
                break;
            case PathfindingMode.FollowTarget:
                target = TargetPosition;
                break;
            default:
                target = transform.position;
                break;
        }

        return target;
    }*/

    bool generatingPath = false;
    bool pathUpdated = false;
    //object pathLock = new object();

    void UpdatePath()
    {
        Debug.DrawRay(PathfindingTarget, Vector3.up, Color.green, 5f);
        //Debug.Log("Updating Path = " + generatingPath);
        if (PathFinder != null && PathfindingEnabled)
        {
            if (!generatingPath)
            {
                var pathStartPos = transform.position + aspidTerrainCollider.transform.localPosition + (Vector3)(Rbody.velocity * 0.25f);

                //var lastPos = transform.position + aspidTerrainCollider.transform.localPosition;


                if (pathfindingOverrides.Count > 0)
                {
                    pathfindingOverrides.SortBy(p => -p.GetTargetPriority());
                    generatingPath = false;
                    pathUpdated = true;
                    pathsCache.Clear();
                    pathsCache.Add(pathStartPos);
                    pathsCache.Add(pathfindingOverrides[0].GetTarget());
                }
                else
                {
                    generatingPath = true;
                    PathFinder.GeneratePathAsync(pathStartPos, PathfindingTarget, pathsCache, () =>
                    {
                        /*if (pathsCache == null)
                        {
                            Debug.Log("GEN PATH = null");
                        }
                        else
                        {
                            Debug.Log("GEN PATH = " + pathsCache.Count);
                        }*/
                        if (pathsCache != null && pathsCache.Count > 2)
                        {
                            pathUpdated = true;
                        }
                        else
                        {
                            followingPath = false;
                            generatingPath = false;
                        }
                    });
                }
            }
        }
        /*Debug.DrawRay(TargetPosition, Vector3.up, Color.red, 5f);
        if (navigator != null && PathfindingEnabled)
        {
            if (path == null)
            {
                path = new NavMeshPath();
            }

            var distanceToTarget = Vector3.Distance(GetPathTarget(), transform.position);

            Debug.DrawRay(transform.position, (GetPathTarget() - transform.position).normalized * distanceToTarget,Color.red, 0.5f);

            Vector3? startPos = null;

            if (Vector3.Distance(lastKnownPosition, transform.position) <= 0.05f)
            {
                if (stillStuck)
                {
                    WeaverLog.Log("STILL STUCK");
                    StartBoundRoutine(FullyUnstuckRoutine());
                }
                startPos = Vector3.Lerp(new Vector3(CurrentRoomRect.xMin, transform.position.y + aspidTerrainCollider.transform.localPosition.y), new Vector3(CurrentRoomRect.xMax, transform.position.y + aspidTerrainCollider.transform.localPosition.y), 0.5f);
                stillStuck = true;
                WeaverLog.Log("STUCK");
            }
            else
            {
                stillStuck = false;
                lastKnownPosition = transform.position;
            }

            if (Physics2D.RaycastNonAlloc(transform.position, (GetPathTarget() - transform.position).normalized, singleHitCache, distanceToTarget, LayerMask.GetMask("Terrain")) > 0 || distanceToTarget > 25f)
            {

                if (startPos == null)
                {
                    startPos = transform.position + aspidTerrainCollider.transform.localPosition + (Vector3)(Rbody.velocity * 0.25f);
                }

                //Vector3 startPos = startPos.value;
                Vector3 pathStartPos = startPos.Value;


                Debug.DrawRay(pathStartPos, Vector3.up * 3f, Color.magenta,1f);

                Debug.DrawLine(GetPathTarget(), PathfindingTarget, Color.gray, 1f);

                NavMesh.CalculatePath(pathStartPos, PathfindingTarget, NavMesh.AllAreas, path);

                if (path.status == NavMeshPathStatus.PathInvalid)
                {
                    if (NavMesh.FindClosestEdge(pathStartPos, out var hit, NavMesh.AllAreas))
                    {

                        NavMesh.CalculatePath(hit.position, PathfindingTarget, NavMesh.AllAreas, path);

                        Debug.DrawLine(transform.position, hit.position, Color.Lerp(Color.red, Color.white, 0.5f), 1f);
                    }
                    //var midRoom = new Vector3(transform.position.x, Mathf.Lerp(CurrentRoomRect.yMax, CurrentRoomRect.yMin,0.5f));

                    //Debug.DrawLine(midRoom, new Vector3(midRoom.x, CurrentRoomRect.yMax), Color.black, 1f);
                    //Debug.DrawLine(midRoom, new Vector3(midRoom.x, CurrentRoomRect.yMin), Color.black, 1f);
                    //Debug.DrawLine(midRoom, new Vector3(CurrentRoomRect.xMin, midRoom.y), Color.black, 1f);
                    //Debug.DrawLine(midRoom, new Vector3(CurrentRoomRect.xMax, midRoom.y), Color.black, 1f);

                }

                Debug.Log("PATH STATUS = " + path.status);

                if (path.status != NavMeshPathStatus.PathInvalid)
                {
                    cornerCount = path.GetCornersNonAlloc(cornerCache);

                    lastPathPos = transform.position + aspidTerrainCollider.transform.localPosition;
                    lastKnownPosition = transform.position + aspidTerrainCollider.transform.localPosition;
                    followingPath = true;
                    Debug.Log("FOUND PATH");
                    cornerIndex = 1;
                    return;
                }
                else
                {
                    Debug.Log("NOT FOUND PATH A");
                    followingPath = false;
                }
            }
            else
            {
                Debug.Log("NOT FOUND PATH B");
                followingPath = false;
            }
        }
        else
        {
            Debug.Log("NOT FOUND PATH C");
            followingPath = false;
        }*/
    }

    IEnumerator PathFindingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pathRegenInterval);

            UpdatePath();
        }
    }


    private void Update()
    {
        if (pathUpdated)
        {
            pathUpdated = false;
            cornerIndex = 1;
            followingPath = true;

            var temp = paths;
            paths = pathsCache;
            pathsCache = temp;

            pathCount = paths.Count;

            lastPathPos = transform.position + aspidTerrainCollider.transform.localPosition;
            lastKnownPosition = transform.position + aspidTerrainCollider.transform.localPosition;
            followingPath = true;
            //Debug.Log("FOUND PATH = " + paths.Count);
            generatingPath = false;
        }

        /*if (PathFinder != null)
        {
            var lastPath = PathFinder.GeneratePath(transform.position, Player.Player1.transform.position);

            if (lastPath != null)
            {
                Debug.Log("Path = " + lastPath.Count);
                for (int i = 0; i < lastPath.Count - 1; i++)
                {
                    Debug.DrawLine(lastPath[i], lastPath[i + 1], Color.white);
                }
            }
            else
            {
                Debug.Log("Path = null");
            }
        }*/

        /*if (PathFinder != null)
        {
            if (!generating)
            {
                generating = true;
                PathFinder.GeneratePathAsync(transform.position, Player.Player1.transform.position, path =>
                {
                    lastPath = path;
                    generating = false;
                });
            }

            //Debug.Log("GENERATING = " + generating);

            //var path = PathFinder.GeneratePath(transform.position, Player.Player1.transform.position);

            if (lastPath != null)
            {
                //Debug.Log("Path = " + lastPath.Count);
                for (int i = 0; i < lastPath.Count - 1; i++)
                {
                    Debug.DrawLine(lastPath[i], lastPath[i + 1], Color.white);
                }
            }
            else
            {
                //Debug.Log("Path = null");
            }
        }
        */
        if (CurrentPhase != null)
        {
            Debug.DrawRay(primaryTarget.TargetPosition, Vector3.up * 2f, Color.red);
            Debug.DrawRay(TargetPosition, Vector3.up * 2f, Color.blue);
        }

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
                for (int i = 0; i < pathCount; i++)
                {
                    Debug.DrawLine(lastPos, paths[i] - aspidTerrainCollider.transform.localPosition, Color.blue);
                    lastPos = paths[i] - aspidTerrainCollider.transform.localPosition;
                }

                //Debug.DrawLine(lastPos, TargetPosition, Color.blue);

                var currentCorner = paths[cornerIndex] - aspidTerrainCollider.transform.localPosition;


                var directionToCorner = (currentCorner - lastPathPos).normalized;

                var directionToBoss = (transform.position - lastPathPos);

                //Project the directionToBoss Vector onto the directionToCorner line
                var distanceToCorner = Vector2.Dot(directionToBoss, directionToCorner);


                /*if (Vector3.Distance(lastPathPos, transform.position) >= Vector3.Distance(lastPathPos, currentCorner) || Vector3.Distance(currentCorner, transform.position) <= 0.5f)*/
                if (distanceToCorner >= (currentCorner - lastPathPos).magnitude - 0.25f)
                {
                    cornerIndex++;
                    if (cornerIndex >= pathCount)
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
                pathPosition = GetPathTarget();
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
                var currentCorner = paths[cornerIndex] - aspidTerrainCollider.transform.localPosition;
                float distanceToNextPoint = Vector2.Distance(transform.position, currentCorner);
                distanceToTarget = Vector2.Distance(TargetPosition, transform.position);


                float interpolationFactor = 1f;

                if (cornerIndex + 1 < pathCount)
                {
                    var nextCorner = paths[cornerIndex + 1] - aspidTerrainCollider.transform.localPosition;
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
            pathPosition = paths[cornerIndex] - aspidTerrainCollider.transform.localPosition;
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
        EnableQuickEscapes = false;
        OnDeath();
    }

    protected override void OnDeath()
    {
        if (settings.HasField<bool>(bossDefeatedField))
        {
            settings.SetFieldValue(bossDefeatedField, true);
        }

        if (CurrentRunningMode != null)
        {
            CurrentRunningMode.OnDeath();
            CurrentRunningMode = null;
        }

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
            SetTarget(PlayerTarget);
            minimumFlightSpeed *= 4f;
            homeInOnTarget = false;
            OrbitReductionAmount = origOrbitReductionAmount;
            flightSpeed *= 2f;
        }

        Claws.DisableSwingAttackImmediate();
        Body.PlayDefaultAnimation = false;
        Claws.OnGround = false;
        //Recoil.ResetRecoilSpeed();
        Recoil.ClearRecoilOverrides();
        Head.ToggleLaserBubbles(false);

        ApplyFlightVariance = false;
        SetTarget(PlayerTarget);
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

        if (Head.HeadLocked)
        {
            Head.UnlockHeadImmediate(Player.Player1.transform.position.x >= transform.position.x ? AspidOrientation.Right : AspidOrientation.Left);
        }

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
        if (!godhomeMode)
        {
            EnableCamLock(transform.position, deathCamLock);
        }

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

        if (!godhomeMode)
        {
            Player.Player1.EnterCutsceneLock(true, 2);
        }

        if (godhomeMode)
        {
            yield return new WaitForSeconds(deathSpitDelay / 2f);
        }
        else
        {
            yield return new WaitForSeconds(deathSpitDelay);
        }

        if (!godhomeMode)
        {

            yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Prepare");

            //FIRE ITEM

            /*float angleMin;
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
            }*/

            //var centerVector = new Vector2(deathCenterRoomAxis,transform.position.y);

            var startPoint = (Vector2)transform.position;

            startPoint.x = Mathf.Clamp(startPoint.x, deathDropItemXRange.x, deathDropItemXRange.y);

            Vector2 target;

            if (Physics2D.RaycastNonAlloc(startPoint, Vector2.down, singleHitCache, 10, LayerMask.GetMask("Terrain")) > 0)
            {
                target = singleHitCache[0].point;
            }
            else
            {
                target = startPoint + (Vector2.down * 10f);
            }

            Debug.DrawLine(startPoint, target, Color.red, 100f);


            //var angle = UnityEngine.Random.Range(angleMin, angleMax);

            //var velocity = MathUtilities.PolarToCartesian(angle, deathItemVelocity);
            var velocity = MathUtilities.CalculateVelocityToReachPoint(Head.transform.position, target, 0.75f);

            //velocity = velocity.normalized * deathItemVelocity;

            Blood.SpawnDirectionalBlood(Head.transform.position, Head.CurrentOrientation == AspidOrientation.Right ? CardinalDirection.Right : CardinalDirection.Left);

            var spawnedItem = GameObject.Instantiate(itemPrefab, Head.transform.position, Quaternion.identity);
            spawnedItem.RB.velocity = velocity;

            if (spitItemAudio != null)
            {
                WeaverAudio.PlayAtPoint(spitItemAudio, transform.position);
            }


            yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Attack");
        }

        yield return new WaitForSeconds(flyAwayDelay);

        aspidTerrainCollider.enabled = false;

        var camPosition = WeaverCamera.Instance.transform.position;

        while (transform.position.y < camPosition.y + 15f)
        {
            Rbody.velocity += new Vector2(0f,flyAwayAcceleration * Time.deltaTime);
            yield return null;
        }

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

        Rbody.velocity = default;

        if (!godhomeMode)
        {
            Player.Player1.ExitCutsceneLock();
        }

        if (!godhomeMode)
        {
            DisableCamLock(deathCamLock);
        }

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

        Gizmos.color = new Color(1f,0f,1f,0.3f);
        Gizmos.DrawCube(FlightRange.center, (Vector3)FlightRange.size + new Vector3(0f,0f,0.1f));
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


    /*private void SetTarget(Vector3 fixedTarget)
    {
        if (primaryTarget.SetTarget(fixedTarget))
        {
            UpdatePath();
        }
    }

    private void SetTarget(Func<Vector3> targetFunc)
    {
        if (primaryTarget.SetTarget(targetFunc))
        {
            UpdatePath();
        }
    }*/

    public TargetOverride AddTargetOverride(int priority = 0)
    {
        var target = new TargetOverride(priority);

        targetOverrides.Sort(TargetOverride.Sorter.Instance);

        targetOverrides.Add(target);
        //Debug.Log("ADDING TARGET OVERRIDE = " + target.guid);

        return target;
    }

    public bool RemoveTargetOverride(TargetOverride target)
    {
        //Debug.Log("REMOVING TARGET OVERRIDE = " + target.guid);
        if (targetOverrides.Remove(target))
        {
            targetOverrides.Sort(TargetOverride.Sorter.Instance);
            return true;
        }
        return false;
    }

    public void AddPathfindingOverride(IPathfindingOverride pathOverride)
    {
        Debug.Log("Pathfinding override added = " + pathOverride.name);
        pathfindingOverrides.Add(pathOverride);
    }

    public bool RemovePathfindingOverride(IPathfindingOverride pathOverride)
    {
        Debug.Log("Pathfinding override removed = " + pathOverride.name);
        return pathfindingOverrides.Remove(pathOverride);
    }

    public void ClearPathfindingOverrides()
    {
        pathfindingOverrides.Clear();
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
        return GetAngleToTarget(Player.Player1.transform.position + new Vector3(0f, 0.5f, 0f));
    }

    public float GetAngleToTarget(Vector3 target)
    {
        var direction = (target - Head.transform.position).normalized;

        return MathUtilities.CartesianToPolar(direction).x;
    }
}
