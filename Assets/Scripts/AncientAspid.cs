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
using WeaverCore.Attributes;

public class AncientAspid : Boss
{
    [field: Header("General Config")]
    [field: SerializeField]
    public bool TrailerMode { get; private set; } = false;

    [SerializeField]
    Phase defaultPhase;

    public Phase CurrentPhase { get; private set; } = null;

    Queue<Phase> phaseQueue = new Queue<Phase>();

    [SerializeField]
    bool godhomeMode = false;

    public bool GodhomeMode => godhomeMode;

    [SerializeField]
    MusicCue preAwakeCue;

    [SerializeField]
    MusicCue musicOff;

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

    [SerializeField]
    [FormerlySerializedAs("musicPlayer")]
    public AncientAspidMusicController MusicPlayer;

    [SerializeField]
    MusicCue blankCue;

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

    [field: SerializeField]
    [field: Tooltip("The target object that is used when aiming towards the player")]
    [field: FormerlySerializedAs("playerTarget")]
    public Transform PlayerTarget { get; private set; }

    public Rect FlightRange;

    [Header("Death")]
    [SerializeField]
    WeaverCameraLock deathCamLock;

    [SerializeField]
    [Tooltip("The journal entry to unlock upon death")]
    HunterJournalEntry journalEntry;

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
    AudioClip spitItemAudio;

    [SerializeField]
    SaveSpecificSettings settings;

    [SerializeField]
    [SaveSpecificFieldName(typeof(bool), nameof(settings))]
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
            return target;
        }
    }

    public Vector3 PathfindingTarget
    {
        get
        {
            return GetPathTarget() + new Vector3(0f, 4.5f);
        }
    }

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

    [NonSerialized]
    Rigidbody2D _rbody;
    public Rigidbody2D Rbody => _rbody ??= GetComponent<Rigidbody2D>();

    [NonSerialized]
    BodyController _body;
    public BodyController Body => _body ??= GetComponentInChildren<BodyController>();

    [NonSerialized]
    WingPlateController _wingPlates;
    public WingPlateController WingPlates => _wingPlates ??= GetComponentInChildren<WingPlateController>();

    [NonSerialized]
    HeadController _head;
    public HeadController Head => _head ??= GetComponentInChildren<HeadController>();

    [NonSerialized]
    WingsController _wings;
    public WingsController Wings => _wings ??= GetComponentInChildren<WingsController>();

    [NonSerialized]
    ClawController _claws;
    public ClawController Claws => _claws ??= GetComponentInChildren<ClawController>();

    [NonSerialized]
    Recoiler _recoiler;
    public Recoiler Recoil => _recoiler ??= GetComponent<Recoiler>();

    [NonSerialized]
    GroundMode _groundMode;
    public GroundMode GroundMode => _groundMode ??= GetComponent<GroundMode>();

    [NonSerialized]
    TacticalMode _tacticalMode;
    public TacticalMode TacticalMode => _tacticalMode ??= GetComponent<TacticalMode>();

    [NonSerialized]
    OffensiveMode _offensiveMode;
    public OffensiveMode OffensiveMode => _offensiveMode ??= GetComponent<OffensiveMode>();

    public List<AncientAspidMove> Moves { get; private set; }


    [NonSerialized]
    AncientAspidHealth _healthManager;
    public AncientAspidHealth HealthManager => _healthManager ??= GetComponent<AncientAspidHealth>();

    public bool PlayerRightOfBoss => Player.Player1.transform.position.x >= transform.position.x;

    float origOrbitReductionAmount;

    public bool ModePickerRunning => modePickerRoutine != 0;

    public AncientAspidMode CurrentRunningMode { get; set; }

    uint modePickerRoutine = 0;
    int modeCounter = 0;
    bool stoppingModePicker = false;
    AncientAspidMode forcedMode = null;
    Dictionary<string, object> forcedModeArgs = null;

    NavMeshSurface navigator = null;
    List<Vector3> paths = new List<Vector3>();
    List<Vector3> pathsCache = new List<Vector3>();
    int pathCount = 0;
    int cornerIndex = 0;
    bool followingPath = false;
    Vector3 lastPathPos = default;
    RaycastHit2D[] singleHitCache = new RaycastHit2D[1];
    List<IPathfindingOverride> pathfindingOverrides = new List<IPathfindingOverride>();

    Bounds stuckBox;
    int stuckCounter = 0;

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
            modeCounter = 0;
            modePickerRoutine = 0;
            forcedMode = null;
        });
        return true;
    }

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
        modeCounter = 1;

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
        if (!enabled)
        {
            return;
        }

        CurrentPhase = defaultPhase;

        //GroundMode = GetComponent<GroundMode>();
        //TacticalMode = GetComponent<TacticalMode>();
        //OffensiveMode = GetComponent<OffensiveMode>();

        navigator = GameObject.FindObjectOfType<NavMeshSurface>();
        base.Awake();
        AncientAspidPrefabs.Instance = prefabs;
        Moves = GetComponents<AncientAspidMove>().ToList();
        //HealthManager = GetComponent<AncientAspidHealth>();
        //Rbody = GetComponent<Rigidbody2D>();
        //Body = GetComponentInChildren<BodyController>();
        //WingPlates = GetComponentInChildren<WingPlateController>();
        //Wings = GetComponentInChildren<WingsController>();
        //Head = GetComponentInChildren<HeadController>();
        //Claws = GetComponentInChildren<ClawController>();
        //Recoil = GetComponent<Recoiler>();

        Recoil.OriginalRecoilSpeed = Recoil.GetRecoilSpeed();

        if (!StartAsleep)
        {
            FullyAwake = true;
            if (sleepSprite != null)
            {
                sleepSprite.gameObject.SetActive(false);
            }
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
            HealthManager.AddHealthMilestone(Mathf.RoundToInt(StartingHealth * 0.7f), () => 
            {
                GetComponent<LaserRapidFireMove>().EnableMove(true);
            });

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
            Claws.DisableSwingAttackImmediate();
            StartBoundRoutine(PreBossRoutine());
        }

        if (!TrailerMode && !StartAsleep)
        {
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
            }
        }
        else
        {
            GenerateStuckBox();
            stuckCounter = 0;
        }
    }

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
        sleepSprite.gameObject.SetActive(true);

        var health = HealthComponent.Health;

        //Initialization.PerformanceLog("WAKEUP START");

        yield return new WaitUntil(() => health != HealthComponent.Health);

        if (MusicPlayer != null)
        {
            Music.PlayMusicCue(blankCue, Mathf.Epsilon, Mathf.Epsilon, true);
            Music.ApplyMusicSnapshot(Music.SnapshotType.Normal, 0f, 0f);
            MusicPlayer.Play(AncientAspidMusicController.MusicPhase.AR1);
        }

        double timeStart = Time.timeAsDouble;

        if (preAwakeCue != null)
        {
            Music.PlayMusicCue(preAwakeCue, 1f, 1f, true);
        }

        health = HealthComponent.Health;

        foreach (var obj in enemyLayers)
        {
            obj.layer = enemyLayer;
        }


        var flasher = sleepSprite.GetComponent<SpriteFlasher>();
        flasher.enabled = true;
        flasher.flashInfected();

        if (sleepAwakeSound != null)
        {
            var awakeSound = WeaverAudio.PlayAtPoint(sleepAwakeSound, transform.position);
            awakeSound.AudioSource.pitch = sleepAwakeSoundPitch;
        }

        var headRoutine = Head.LockHead(Head.LookingDirection >= 0f ? 60f : -60f, 1f);
        var bodyRoutine = Body.RaiseTail(1000);




        bool playerOnRight = Player.Player1.transform.position.x >= transform.position.x;

        RoutineAwaiter awaiter = null;

        if (playerOnRight)
        {
            //Initialization.PerformanceLog("CHANGE DIRCTION");
            awaiter = RoutineAwaiter.AwaitRoutine(ChangeDirection(AspidOrientation.Right, 1000));
            yield return awaiter.WaitTillDone();
        }

        //Initialization.PerformanceLog("STARTING JUMP");
        awaiter = RoutineAwaiter.AwaitBoundRoutines(this, headRoutine, bodyRoutine);

        GroundMode.lungeDashEffect.SetActive(true);
        GroundMode.lungeDashRotationOrigin.SetZLocalRotation(90);

        //float stompingRate = 0.3f;

        var sleepAnimator = sleepSprite.GetComponent<WeaverAnimationPlayer>();
        //Initialization.PerformanceLog("STARTING TURN AROUND PRE");
        yield return sleepAnimator.PlayAnimationTillDone("Turn Around Pre");
        //Initialization.PerformanceLog("STOPPING TURN AROUND PRE");
        sleepSprite.flipX = Player.Player1.transform.position.x >= transform.position.x;
        //Initialization.PerformanceLog("STARTING TURN AROUND POST");
        yield return sleepAnimator.PlayAnimationTillDone("Turn Around Post");
        //Initialization.PerformanceLog("STOPPING TURN AROUND POST");

        foreach (var claw in Claws.claws)
        {
            yield return claw.LockClaw();
            claw.OnGround = true;
            claw.LandImmediately();
        }

        //Initialization.PerformanceLog("CLAWS LOCKED");

        yield return awaiter.WaitTillDone();

        //Initialization.PerformanceLog("AWAITER DONE");

        var time = Time.time;

        yield return new WaitUntil(() => Rbody.velocity.y <= 0f || Time.time > time + 3f);
        //Initialization.PerformanceLog("BEGINNING FALL");
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

        sleepSprite.gameObject.SetActive(false);

        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }


        var headLandRoutine = Head.PlayLanding(true);
        var bodyLandRoutine = Body.PlayLanding(true);
        var wingPlateLandRoutine = WingPlates.PlayLanding(true);
        var wingsLandRoutine = Wings.PlayLanding(true);
        var clawsLandRoutine = Claws.PlayLanding(true);
        var shiftPartsRoutine = ShiftBodyParts(true, Body, WingPlates, Wings);

        List<uint> landingRoutines = new List<uint>();

        var landingAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, landingRoutines, headLandRoutine, bodyLandRoutine, wingPlateLandRoutine, wingsLandRoutine, clawsLandRoutine, shiftPartsRoutine);

        //Initialization.PerformanceLog("BEGINNING LANDING AWAITE");
        yield return landingAwaiter.WaitTillDone();
        //Initialization.PerformanceLog("ENDING LANDING AWAITE");

        var headFinishRoutine = Head.FinishLanding(false);
        var bodyFinishRoutine = Body.FinishLanding(false);
        var wingPlateFinishRoutine = WingPlates.FinishLanding(false);
        var wingsFinishRoutine = Wings.FinishLanding(false);
        var clawsFinishRoutine = Claws.FinishLanding(false);

        var finishAwaiter = RoutineAwaiter.AwaitBoundRoutines(this, headFinishRoutine, bodyFinishRoutine, wingPlateFinishRoutine, wingsFinishRoutine, clawsFinishRoutine);
        //Initialization.PerformanceLog("BEGINNING FINISHE AWAITE");
        yield return finishAwaiter.WaitTillDone();
        //Initialization.PerformanceLog("ENDING FINISHE AWAITE");

        //WeaverLog.Log("TURN AROUND FULL TIME = " + (Time.timeAsDouble - timeStart));

        if (health == HealthManager.Health)
        {
            yield return new WaitForSeconds(sleepPreJumpDelay);
            //Initialization.PerformanceLog("PRE JUMP DELAY");
        }

        Rbody.gravityScale = GroundMode.onGroundGravity;
        if (health == HealthManager.Health)
        {
            //Initialization.PerformanceLog("BEGIN - EXITING GROUND MODE SLOW");
            yield return GroundMode.ExitGroundMode();
            //Initialization.PerformanceLog("END - EXITING GROUND MODE SLOW");
        }
        else
        {
            StartBoundRoutine(GroundMode.ExitGroundMode());
            //Initialization.PerformanceLog("BEGIN - EXITING GROUND MODE FAST");
            yield return new WaitUntil(() => Claws.OnGround == false);
            //Initialization.PerformanceLog("END - EXITING GROUND MODE FAST");
        }

        if (!Head.HeadLocked)
        {
            //Initialization.PerformanceLog("BEGIN - LOCK HEAD");
            yield return Head.LockHead(Orientation == AspidOrientation.Right ? 60f : -60f, 1f);
            //Initialization.PerformanceLog("END - LOCK HEAD");
        }

        yield return Head.Animator.PlayAnimationTillDone("Fire - 2 - Prepare");
        //Initialization.PerformanceLog("Fire - 2 - Prepare DONE");

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

        //Initialization.PerformanceLog("BEGIN - ROAR");

        //Music.PlayMusicCue(bossMusic, 0f, 0f, true);

        yield return Roar(sleepRoarDuration,Head.transform.position, true);
        //Initialization.PerformanceLog("END - ROAR");

        StopCoroutine(shakeRoutine);

        FlightEnabled = true;
        //Initialization.PerformanceLog("BEGIN - SWING ATTACK ENABLE");
        yield return Claws.EnableSwingAttack(true);
        //Initialization.PerformanceLog("END - SWING ATTACK ENABLE");

        //Initialization.PerformanceLog("BEGIN - Fire - 1 - Attack");
        yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Attack");
        //Initialization.PerformanceLog("END - Fire - 1 - Attack");

        if (Head.HeadLocked)
        {
            Head.UnlockHead();
        }

        FullyAwake = true;

        foreach (var obj in enableOnWakeUp)
        {
            obj.SetActive(true);
        }
        if (preAwakeCue == null)
        {
            Music.PlayMusicCue(musicOff);
        }
        yield return null;

        if (MusicPlayer == null)
        {
            Music.PlayMusicCue(bossMusic, 0f, 0f, true);
        }
        //Music.PlayMusicCue(bossMusic, 0f, 0f, true);

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

    public IEnumerator MainBossRoutine()
    {
        if (godhomeMode && MusicPlayer != null)
        {
            MusicPlayer.Play(AncientAspidMusicController.MusicPhase.AR1);
        }
        SetTarget(PlayerTarget);
        EnableQuickEscapes = true;
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

    public void EnableCamLock(Vector3 position, WeaverCameraLock camLock, bool clampWithinArea = true)
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

        if (clampWithinArea)
        {
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
        }

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

        yield return Body.PrepareChangeDirection();
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

    private IEnumerator VarianceResetter()
    {
        while (true)
        {
            if (ApplyFlightVariance)
            {
                Vector3 oldOffset = TargetOffset;
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

    Vector3 GetPathTarget()
    {


        return FlightRange.ClampWithin(TargetPosition);
    }

    bool generatingPath = false;
    bool pathUpdated = false;
    void UpdatePath()
    {
        Debug.DrawRay(PathfindingTarget, Vector3.up, Color.green, 5f);
        if (PathFinder != null && PathfindingEnabled)
        {
            if (!generatingPath)
            {
                var pathStartPos = transform.position + aspidTerrainCollider.transform.localPosition + (Vector3)(Rbody.velocity * 0.25f);


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
            followingPath = true;
            generatingPath = false;
        }

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
            Vector3 pathPosition;

            if (followingPath)
            {
                Vector3 lastPos = transform.position;
                for (int i = 0; i < pathCount; i++)
                {
                    Debug.DrawLine(lastPos, paths[i] - aspidTerrainCollider.transform.localPosition, Color.blue);
                    lastPos = paths[i] - aspidTerrainCollider.transform.localPosition;
                }

                var currentCorner = paths[cornerIndex] - aspidTerrainCollider.transform.localPosition;


                var directionToCorner = (currentCorner - lastPathPos).normalized;

                var directionToBoss = (transform.position - lastPathPos);

                var distanceToCorner = Vector2.Dot(directionToBoss, directionToCorner);


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

                    interpolationFactor = Mathf.Clamp01(Vector2.Dot(currentCornerVector, nextCornerVector));

                }

                distanceToTarget = Mathf.Lerp(distanceToNextPoint, distanceToTarget, interpolationFactor);
            }
            else
            {
                distanceToTarget = Vector2.Distance(TargetPosition, transform.position);
            }

            var directionToTarget = (Vector2)(pathPosition - transform.position);

            Debug.DrawLine(transform.position, pathPosition, Color.cyan);

            var maxVelocity = distanceToTarget * flightSpeedOverDistance;

            if (maxVelocity < minFlightSpeed)
            {
                maxVelocity = minFlightSpeed;
            }

            var prevVelocity = Rbody.velocity;

            var newVelocity = prevVelocity + (directionToTarget.normalized * (flightAcceleration * Time.deltaTime));

            if (newVelocity.magnitude > maxVelocity)
            {
                newVelocity = newVelocity.normalized * maxVelocity;
            }

            Rbody.velocity = newVelocity;
        }

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
        /*if (settings.HasField<bool>(bossDefeatedField))
        {
            settings.SetFieldValue(bossDefeatedField, true);
        }*/
        settings.TrySetFieldValue(bossDefeatedField, true);

        if (CurrentRunningMode != null)
        {
            CurrentRunningMode.OnDeath();
            CurrentRunningMode = null;
        }

        if (journalEntry != null)
        {
            HunterJournal.RecordKillFor(journalEntry.EntryName);
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

        var targetOrientation = Player.Player1.transform.position.x >= transform.position.x ? AspidOrientation.Right : AspidOrientation.Left;

        if (Head.HeadLocked)
        {
            Head.UnlockHeadImmediate(targetOrientation);
        }

        foreach (var laser in GetComponentsInChildren<LaserEmitter>())
        {
            laser.gameObject.SetActive(false);
        }
        if (MusicPlayer != null)
        {
            MusicPlayer.Stop();
        }
        else
        {
            Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, 0.5f);
        }

        if (deathSound != null)
        {
            WeaverAudio.PlayAtPoint(deathSound,transform.position);
        }

        StartCoroutine(DeathRoutine(targetOrientation));
    }

    IEnumerator DeathRoutine(AspidOrientation targetOrientation)
    {
        var targetPos = transform.position;

        if (targetPos.y <= Player.Player1.transform.position.y + 5f)
        {
            targetPos.y = Player.Player1.transform.position.y + 5f;
        }

        if (!godhomeMode)
        {
            EnableCamLock(targetPos, deathCamLock);
        }

        //var targetOrientation = Player.Player1.transform.position.x >= transform.position.x ? AspidOrientation.Right : AspidOrientation.Left;

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

        yield return Head.LockHead(targetOrientation, 1000);

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
            Player.Player1.EnterCutsceneLock(true);
        }

        if (godhomeMode)
        {
            yield return new WaitForSeconds(deathSpitDelay / 2f);
        }
        else
        {
            for (float t = 0; t < deathSpitDelay; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 5 * Time.deltaTime);
                yield return null;
            }
            //yield return new WaitForSeconds(deathSpitDelay);
        }

        if (!godhomeMode)
        {
            yield return Head.Animator.PlayAnimationTillDone("Fire - 1 - Prepare");

            var startPoint = (Vector2)transform.position;

            //startPoint.x = Mathf.Clamp(startPoint.x, deathDropItemXRange.x, deathDropItemXRange.y);

            Vector2 target = Player.Player1.transform.position;

            target.x = Mathf.Clamp(target.x, deathDropItemXRange.x, deathDropItemXRange.y);

            /*if (Physics2D.RaycastNonAlloc(startPoint, Vector2.down, singleHitCache, 10, LayerMask.GetMask("Terrain")) > 0)
            {
                target = singleHitCache[0].point;
            }
            else
            {
                target = startPoint + (Vector2.down * 10f);
            }*/

            Debug.DrawLine(startPoint, target, Color.red, 100f);


            var velocity = MathUtilities.CalculateVelocityToReachPoint(Head.transform.position, target, 0.75f);

            if (velocity.y > 0f)
            {
                velocity.y = 0f;
            }

            Blood.SpawnDirectionalBlood(Head.transform.position, Head.CurrentOrientation == AspidOrientation.Right ? CardinalDirection.Right : CardinalDirection.Left);

            settings.TrySetFieldValue(bossDefeatedField, true);
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
    }


    public TargetOverride AddTargetOverride(int priority = 0)
    {
        var target = new TargetOverride(priority);

        targetOverrides.Sort(TargetOverride.Sorter.Instance);

        targetOverrides.Add(target);
        return target;
    }

    public bool RemoveTargetOverride(TargetOverride target)
    {
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
