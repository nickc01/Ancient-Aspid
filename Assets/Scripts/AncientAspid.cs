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

    public AspidOrientation Orientation { get; private set; } = AspidOrientation.Left;
    public Mode AspidMode { get; private set; } = Mode.Tactical;

    private bool targetingTransform = false;

    public Vector3 TargetOffset { get; private set; }
    public Transform TargetTransform { get; private set; }
    public Rect CurrentRoomRect { get; private set; }

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

            target.x = Mathf.Clamp(target.x, CurrentRoomRect.xMin,CurrentRoomRect.xMax);
            target.y = Mathf.Clamp(target.y, CurrentRoomRect.yMin,CurrentRoomRect.yMax);

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
            float tacticalTime = 10f;
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
            yield return new WaitUntil(() => !Head.HeadLocked);

            //SWITCH TO OFFENSIVE MODE
            float offensiveTime = 10f;
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
                /*//RUN OFFENSIVE MOVES
                foreach (var move in Moves)
                {
                    if (move.MoveEnabled)
                    {
                        yield return RunMove(move);
                    }
                }

                yield return null;*/
            }

            yield return new WaitUntil(() => !Head.HeadLocked);
            yield return ExitCenterMode();
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
        var newTarget = new Vector3(Mathf.Lerp(CurrentRoomRect.xMin, CurrentRoomRect.xMax, 0.5f), CurrentRoomRect.yMin + offensiveHeight);
        SetTarget(newTarget);
        flightOffset /= 4f;
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
            if (Vector3.Distance(transform.position, newTarget) <= 0.25f)
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

    IEnumerator ExitCenterMode(AspidOrientation orientation)
    {
        SetTarget(playerTarget);
        flightOffset *= 4f;
        minimumFlightSpeed *= 4f;
        homeInOnTarget = false;
        orbitReductionAmount = origOrbitReductionAmount;
        flightSpeed *= 2f;
        yield return ChangeDirection(orientation);

        yield return new WaitForSeconds(0.25f);
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
        float distanceToTarget = Vector3.Distance(TargetPosition, transform.position);
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
        var direction = (Player.Player1.transform.position - Head.transform.position).normalized;

        return MathUtilities.CartesianToPolar(direction).x;
    }
}
