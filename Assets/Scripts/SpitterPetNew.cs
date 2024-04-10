using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class SpitterPetNew : MonoBehaviour
{
    //RaycastHit2D[] foundEnemies = new RaycastHit2D[5];
    int foundEnemyCount;
    int attackLayerMask;

    static RaycastHit2D[] hitCache = new RaycastHit2D[1];

    [SerializeField]
    float alertRange = 15f;

    [SerializeField]
    float maxVelocity = 8.5f;

    [SerializeField]
    float acceleration = 12f;

    [SerializeField]
    float maxTeleportDistance = 25f;

    [SerializeField]
    float maxSleepDistance = 6f;

    [SerializeField]
    Vector2 sleepTimeRange = new Vector2(9f, 11f);

    [SerializeField]
    Vector2 attackTimeRange = new Vector2(1.25f, 1.75f);

    [SerializeField]
    Vector2 preAttackDelayRange = new Vector2(0f, 0.25f);

    [SerializeField]
    float playerTargetRange = 5f;

    [SerializeField]
    float sleepFallVelocity = -10f;

    [SerializeField]
    Vector2 jitterIntensity = new Vector2(1f, 1f);

    [SerializeField]
    List<ParticleSystem> fadeInParticles;

    [SerializeField]
    float jitterRandomizeTime = 0.5f;

    [SerializeField]
    AudioClip shootSound;

    [SerializeField]
    float aspidShotVelocity = 15f;

    [SerializeField]
    float shotAngleSeperation = 35f;

    [SerializeField]
    AspidShot aspidShotPrefab;

    [SerializeField]
    int aspidShotDamage = 4;

    [SerializeField]
    bool shootTriple = true;

    [SerializeField]
    Vector2 extraOffsetXRange = default;

    [SerializeField]
    Vector2 extraOffsetYRange = default;

    [SerializeField]
    CollisionCounter collisionCounter;

    [Space]
    [Header("Animations")]
    [SerializeField]
    string attackAnticAnim = "Aspid Attack Antic";

    [SerializeField]
    string attackAnim = "Aspid Attack";

    [SerializeField]
    string idleAnim = "Aspid Idle";

    [SerializeField]
    string turnAnim = "Aspid Turn";

    [SerializeField]
    string sleepAnticAnim = "Aspid Sleep Antic";

    [SerializeField]
    string sleepAnim = "Aspid Sleep";

    SpriteRenderer _mainRenderer;
    public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

    Rigidbody2D _rigidbody;
    public Rigidbody2D Rigidbody => _rigidbody ??= GetComponent<Rigidbody2D>();

    WeaverAnimationPlayer _animator;
    public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

    AudioSource _audioPlayer;
    public AudioSource AudioPlayer => _audioPlayer ??= GetComponent<AudioSource>();

    SpriteFlasher _flasher;
    public SpriteFlasher Flasher => _flasher ??= GetComponent<SpriteFlasher>();

    float jitterTimer = 0f;
    float currentSleepTime;
    float currentAttackTime;
    float currentDelayTime;
    Vector2 jitterVector = default;

    [SerializeField]
    float sleepTimer = 0f;

    [SerializeField]
    float attackTimer = 0f;
    Bounds playerColliderBounds;
    bool colliderFound = false;
    Coroutine turnRoutine;
    Coroutine fadeRoutine;
    Coroutine attackRoutine;
    Vector2 extraOffset;

    [SerializeField]
    bool temporarilyDisabled = false;

    int terrainCollisionMask;

    public bool ShouldGoToSleep => sleepTimer >= currentSleepTime && foundEnemyCount == 0;
    public bool FacingRight => MainRenderer.flipX;
    public bool IsTurning => turnRoutine != null;
    public bool Attacking => attackRoutine != null;
    public bool Fading => fadeRoutine != null;

    public bool AimingAtPlayer { get; private set; } = true;
    public Vector3 NearestTarget { get; private set; }

    public Vector2 PlayerTarget
    {
        get
        {
            var sourcePos = transform.position;

            if (sourcePos.y <= Player.Player1.transform.position.y + 2f)
            {
                sourcePos.y = Player.Player1.transform.position.y + 2f;
            }

            var target = ((sourcePos - Player.Player1.transform.position).normalized * playerTargetRange) + Player.Player1.transform.position;

            return target;
        }
    }

    public Vector2 SleepTarget
    {
        get
        {
            var sourcePos = transform.position;

            if (sourcePos.y <= Player.Player1.transform.position.y + 2f)
            {
                sourcePos.y = Player.Player1.transform.position.y + 2f;
            }

            if (sourcePos.y >= Player.Player1.transform.position.y + 5.5f)
            {
                sourcePos.y = Player.Player1.transform.position.y + 5.5f;
            }

            var target = ((sourcePos - Player.Player1.transform.position).normalized * maxSleepDistance) + Player.Player1.transform.position;

            if (colliderFound)
            {
                if (target.x < playerColliderBounds.min.x + 1.75f)
                {
                    target.x = playerColliderBounds.min.x + 1.75f;
                }

                if (target.x > playerColliderBounds.max.x - 1.75f)
                {
                    target.x = playerColliderBounds.max.x - 1.75f;
                }
            }

            return target;
        }
    }

    private void Awake()
    {
        collisionCounter.CollisionPredicates += CollisionCounter_CollisionPredicates;

        DontDestroyOnLoad(gameObject);
        attackLayerMask = LayerMask.GetMask("Enemies");
        terrainCollisionMask = LayerMask.GetMask("Terrain");
        Animator.PlayAnimation(idleAnim);
        MainRenderer.color = MainRenderer.color.With(a: 0f);
        Flasher.FlashIntensity = 0f;
        StartCoroutine(MainRoutine());
        StartCoroutine(SleepTimerRoutine());
        StartCoroutine(EnemyScanRoutine());
        StartCoroutine(AttackTimeRange());
        StartCoroutine(PathfindingTargetSelector());

        TeleportToPlayer();
    }

    private bool CollisionCounter_CollisionPredicates(Collider2D obj)
    {
        var healthComponent = HealthUtilities.GetHealthComponent(obj.gameObject);

        if (healthComponent == null || (healthComponent != null && HealthUtilities.TryGetHealth(healthComponent, out var healthValue) && healthValue > 0))
        {
            return true;
        }

        return false;
    }

    bool hooked = false;

    private void OnEnable()
    {
        if (!hooked)
        {
            hooked = true;
            OnHook();
        }
    }

    private void OnDisable()
    {
        if (hooked)
        {
            hooked = false;
            OnUnHook();
        }
    }

    private void OnDestroy()
    {
        if (hooked)
        {
            hooked = false;
            OnUnHook();
        }

        collisionCounter.CollisionPredicates -= CollisionCounter_CollisionPredicates;
    }

    void OnHook()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        HeroController.instance.heroInPosition += Instance_heroInPosition;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        EventManager.OnEventTriggered += EventManager_OnEventTriggered;
    }

    void OnUnHook()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        if (HeroController.instance != null)
        {
            HeroController.instance.heroInPosition -= Instance_heroInPosition;
        }
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
    }

    private void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
    {
        if (eventName == "FINAL HIT")
        {
            //WeaverLog.Log("ALL CHARMS ENDING");
            TemporarilyDisable(true);
        }
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        /*WeaverLog.Log("BOOL NAME = " + name + ", val = " + orig);
        if (name == "killedFinalBoss" && orig)
        {
            TemporarilyDisable(true);
        }*/

        return orig;
    }

    private void Instance_heroInPosition(bool forceDirect)
    {
        UpdateFlyingState();
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (temporarilyDisabled)
        {
            TemporarilyDisable(false);
        }

        UpdateFlyingState();
    }


    void UpdateFlyingState()
    {
        sleepTimer = 0;
        attackTimer = 0;

        TeleportToPlayer();
        /*if (sleeping)
        {
            StopAllCoroutines();
            rb.velocity = default;
            rb.isKinematic = false;
            sleeping = false;
            var audio = GetComponent<AudioSource>();
            audio.Play();
            animationPlayer.PlayAnimation(idleLoopAnimation);
            StartCoroutine(TargetUpdateRoutine());
            StartCoroutine(JitterRoutine());
            StartCoroutine(EnemyTargetUpdateRoutine());
            StartCoroutine(ShootRoutine());
            StartCoroutine(SleepCheckRoutine());
        }

        if (flyingOut)
        {
            Destroy(gameObject);
        }
        else
        {
            enemyTarget = null;
            lookTarget = Player.Player1.gameObject;
            FlyToPlayer();
        }*/
    }

    public void FadeIn(Action onDone = null)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
        MainRenderer.color = MainRenderer.color.With(a: 0f);
        Flasher.FlashIntensity = 0f;
        fadeRoutine = StartCoroutine(FadeRoutine(true, onDone));
    }

    public void FadeOut(Action onDone = null)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
        MainRenderer.color = MainRenderer.color.With(a: 1f);
        Flasher.FlashIntensity = 0f;
        fadeRoutine = StartCoroutine(FadeRoutine(false, onDone));
    }

    IEnumerator FadeRoutine(bool fadeIn, Action onDone)
    {
        StopTurning();
        if (fadeIn)
        {
            MainRenderer.color = MainRenderer.color.With(a: 0f);
            Flasher.FlashIntensity = 0f;

            foreach (var particles in fadeInParticles)
            {
                particles.Play();
            }

            Flasher.DoFlash(0.2f, 0.5f, 1f, StayTime: 0f);

            yield return new WaitForSeconds(0.2f);
            MainRenderer.color = MainRenderer.color.With(a: 1f);
        }
        else
        {
            MainRenderer.color = MainRenderer.color.With(a: 1f);
            Flasher.FlashIntensity = 0f;

            Flasher.DoFlash(0.2f, 0.5f, 1f, StayTime: 0f);

            yield return new WaitForSeconds(0.2f);
            MainRenderer.color = MainRenderer.color.With(a: 0f);
        }

        fadeRoutine = null;
        onDone?.Invoke();
    }

    void DoFlyTick(Vector3 immediateTarget, Vector3 finalTarget, Vector2 jitter)
    {
        Debug.DrawLine(finalTarget + Vector3.up, finalTarget + Vector3.down, Color.magenta);
        Debug.DrawLine(finalTarget + Vector3.left, finalTarget + Vector3.right, Color.magenta);

        Debug.DrawLine(immediateTarget + Vector3.up, immediateTarget + Vector3.down, Color.blue);
        Debug.DrawLine(immediateTarget + Vector3.left, immediateTarget + Vector3.right, Color.blue);
        /*if (lookAtTarget == null)
        {
            lookAtTarget = () => Player.Player1.transform.position;
        }*/

        immediateTarget += (Vector3)(jitter * jitterVector);

        //while (true)
        //{
        /*if (!FlyingEnabled)
        {
            yield return new WaitUntil(() => FlyingEnabled);
        }*/

        //var target = getTarget();

        var velocity = Rigidbody.velocity;
        var oldVelocity = velocity;

        var trueMaxVelocity = maxVelocity * Mathf.Abs((Vector2.Distance(finalTarget, transform.position) / 2.5f) + 0.01f);

        if (velocity.magnitude > trueMaxVelocity)
        {
            //Start moving in the opposite direction to slow down
            velocity -= velocity.normalized * 2f * acceleration * Time.deltaTime;

            if (velocity.magnitude < trueMaxVelocity)
            {
                velocity = velocity.normalized * trueMaxVelocity;
            }
        }



        //var toTargetVector = (target - (Vector2)transform.position);

        //var ninetyRotation = Quaternion.Euler(0f,0f,90f);

        //velocity += (target - (Vector2)transform.position).normalized * acceleration * Time.deltaTime * (1f / Mathf.Clamp(0,2f,Vector2.Distance(target,transform.position) / 10f));

        velocity += (Vector2)(immediateTarget - transform.position).normalized * acceleration * Time.deltaTime;

        //velocity = Vector2.Lerp(velocity, toTargetVector.normalized * maxVelocity,);

        var sourceRotation = Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(velocity).x);
        var destRotation = Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(immediateTarget - transform.position).x);

        var newRotation = Quaternion.Lerp(sourceRotation, destRotation, Vector2.Distance(immediateTarget, transform.position) * Time.deltaTime);

        velocity = newRotation * (Vector2.right * velocity.magnitude);

        Rigidbody.velocity = velocity;

        //WeaverLog.Log($"OLD = {oldVelocity}, NEW = {velocity}");

        /*if (!Turning)
        {
            var lookAtTarget = getLookAt();

            TurnTowards(lookAtTarget);
        }*/

        //yield return null;
        //}
    }

    /*public Transform GetNearestEnemy()
    {
        float distance = float.PositiveInfinity;
        Transform nearestEnemy = null;

        var foundEnemies =

        for (int i = 0; i < foundEnemyCount; i++)
        {
            if (foundEnemies[i].transform != null)
            {
                var enemyDist = Vector2.Distance(transform.position, foundEnemies[i].transform.position);
                if (enemyDist < alertRange + 1f && enemyDist <= distance)
                {
                    distance = enemyDist;
                    nearestEnemy = foundEnemies[i].transform;
                }
            }
        }

        return nearestEnemy;
    }*/

    IEnumerator PathfindingTargetSelector()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (PlayerTracker.Instance != null && !temporarilyDisabled)
            {
                bool TestTarget(Vector3 target)
                {
                    //var center = Vector2.Lerp(transform.position, target, 0.5f);
                    var size = Vector2.Distance(transform.position, target);

                    //return Physics2D.CapsuleCastNonAlloc(center, new Vector2(size, 0.5f), CapsuleDirection2D.Horizontal, MathUtilities.CartesianToPolar((Vector2)transform.position - center).x, ((Vector2)transform.position - center).normalized, hitCache, size, terrainCollisionMask) == 0;

                    return Physics2D.RaycastNonAlloc(transform.position, (target - transform.position).normalized, hitCache, size, terrainCollisionMask) == 0;
                }

                var distToPlayer = Vector2.Distance(Player.Player1.transform.position + new Vector3(0f, 0.25f), transform.position);

                if (Physics2D.RaycastNonAlloc(transform.position, (Player.Player1.transform.position + new Vector3(0f, 0.25f) - transform.position).normalized, hitCache, distToPlayer,terrainCollisionMask) == 0)
                {
                    AimingAtPlayer = true;
                    NearestTarget = Player.Player1.transform.position;
                }
                /*if (TestTarget(Player.Player1.transform.position + new Vector3(0f,0.5f)))
                {
                    AimingAtPlayer = true;
                    NearestTarget = Player.Player1.transform.position;
                }*/
                else
                {
                    if (PlayerTracker.Instance.Points.Count == 0)
                    {
                        AimingAtPlayer = true;
                        NearestTarget = Player.Player1.transform.position;
                    }
                    else
                    {
                        var last = PlayerTracker.Instance.Points.Last;
                        bool foundTarget = false;
                        for (int i = PlayerTracker.Instance.Points.Count - 1; i >= 0; i--)
                        {
                            if (TestTarget((Vector3)(last.Value + new Vector2(0f, 0.5f))))
                            {
                                AimingAtPlayer = false;
                                NearestTarget = last.Value;
                                foundTarget = true;
                                break;
                            }
                            else
                            {
                                last = last.Previous;
                            }
                        }

                        if (!foundTarget && !temporarilyDisabled)
                        {
                            TeleportToPlayer();
                        }
                    }
                }

            }
            else
            {
                AimingAtPlayer = true;
                NearestTarget = Player.Player1.transform.position;
            }
        }
    }

    IEnumerator AttackRoutine(Transform target, Vector3 targetPos)
    {
        yield return Animator.PlayAnimationTillDone(attackAnticAnim);

        if (target != null)
        {
            targetPos = target.transform.position;
        }

        if (shootSound != null)
        {
            WeaverAudio.PlayAtPoint(shootSound, transform.position);
        }
        Shoot(targetPos);

        yield return Animator.PlayAnimationTillDone(attackAnim);

        Animator.PlayAnimation(idleAnim);

        attackRoutine = null;
    }

    void Shoot(Vector3 target)
    {
        var directionToTarget = (target - transform.position).normalized;

        var angle = MathUtilities.CartesianToPolar(directionToTarget).x;

        var shot = AspidShot.Spawn(aspidShotPrefab, transform.position, MathUtilities.PolarToCartesian(angle, aspidShotVelocity));

        shot.GetComponent<AspidShotExtraDamager>().AdditionalDamage = aspidShotDamage;
        if (shootTriple)
        {
            shot = AspidShot.Spawn(aspidShotPrefab, transform.position, MathUtilities.PolarToCartesian(angle + shotAngleSeperation, aspidShotVelocity));
            shot.GetComponent<AspidShotExtraDamager>().AdditionalDamage = aspidShotDamage;
            shot = AspidShot.Spawn(aspidShotPrefab, transform.position, MathUtilities.PolarToCartesian(angle - shotAngleSeperation, aspidShotVelocity));
            shot.GetComponent<AspidShotExtraDamager>().AdditionalDamage = aspidShotDamage;
        }
    }

    bool WithinColliderBounds()
    {
        if (colliderFound)
        {
            return transform.GetPositionX() >= playerColliderBounds.min.x + 0.75f && transform.GetPositionX() <= playerColliderBounds.max.x - 0.75f;
        }

        return true;
    }

    IEnumerator MainRoutine()
    {
        extraOffset = new Vector2(extraOffsetXRange.RandomInRange(), extraOffsetYRange.RandomInRange());

        FadeIn();

        while (true)
        {
            if (!temporarilyDisabled)
            {
                if (ShouldGoToSleep)
                {
                    var target = SleepTarget;
                    DoFlyTick(transform.position.With(x: target.x), transform.position.With(x: target.x), jitterIntensity);
                }
                else
                {
                    if (AimingAtPlayer)
                    {
                        DoFlyTick(PlayerTarget + extraOffset, PlayerTarget + extraOffset, jitterIntensity);
                    }
                    else
                    {
                        DoFlyTick(NearestTarget, PlayerTarget, jitterIntensity);
                    }
                }

                if (Vector2.Distance(Player.Player1.transform.position, transform.position) >= maxTeleportDistance)
                {
                    TeleportToPlayer();
                }

                var enemyTarget = collisionCounter.GetNearestTarget(transform.position);

                if (!Attacking && enemyTarget != null && attackTimer > currentAttackTime - currentDelayTime - 0.01f)
                {
                    currentAttackTime = attackTimeRange.RandomInRange();
                    currentDelayTime = preAttackDelayRange.RandomInRange();
                    attackTimer = 0;
                    attackRoutine = StartCoroutine(AttackRoutine(enemyTarget.transform, enemyTarget.transform.position));
                }

                if (ShouldGoToSleep)
                {
                    //WeaverLog.Log("GOING TO SLEEP");
                    StopTurning();
                    colliderFound = Physics2D.RaycastNonAlloc(Player.Player1.transform.position + new Vector3(0f, 0.25f), Vector2.down, hitCache, 3f, terrainCollisionMask) > 0;

                    //WeaverLog.Log("Collider Found = " + colliderFound);
                    if (colliderFound)
                    {
                        playerColliderBounds = hitCache[0].collider.bounds;
                        //WeaverLog.Log("XMin = " + playerColliderBounds.min.x);
                        //WeaverLog.Log("XMax = " + playerColliderBounds.max.x);
                    }

                    var target = SleepTarget;

                    var heightDiff = transform.position.y - Player.Player1.transform.position.y;

                    while (!(heightDiff < 6f && heightDiff > 0f && transform.position.x <= playerColliderBounds.max.x - 0.75f && transform.position.x >= playerColliderBounds.min.x + 0.75f))
                    //while (Vector2.Distance(Player.Player1.transform.position, transform.position) >= maxSleepDistance && Vector2.Distance(SleepTarget, transform.position) >= 0.2f && !WithinColliderBounds() && (transform.position.x > playerColliderBounds.max.x - 2f || transform.position.x < playerColliderBounds.min.x + 2f))
                    {
                        heightDiff = transform.position.y - Player.Player1.transform.position.y;
                        DoFlyTick(SleepTarget, SleepTarget, jitterIntensity);
                        if (!ShouldGoToSleep)
                        {
                            break;
                        }
                        yield return null;
                    }

                    if (ShouldGoToSleep)
                    {
                        var audio = GetComponent<AudioSource>();
                        audio.Stop();
                        Animator.PlayAnimation(sleepAnticAnim);

                        Rigidbody.velocity = Vector2.up * sleepFallVelocity;

                        while (Rigidbody.velocity.y < 0f)
                        {
                            if (!ShouldGoToSleep)
                            {
                                Animator.PlayAnimation(idleAnim);
                                break;
                            }
                            yield return null;
                        }
                        //yield return new WaitUntil(() => Rigidbody.velocity.y >= 0f);
                        Rigidbody.velocity = default;

                        if (ShouldGoToSleep)
                        {
                            Rigidbody.isKinematic = true;
                            yield return Animator.PlayAnimationTillDone(sleepAnim);

                            yield return new WaitForSeconds(0.25f);

                            var playerPos = Player.Player1.transform.position;

                            yield return new WaitUntil(() => !ShouldGoToSleep);

                            Rigidbody.isKinematic = false;

                            Animator.PlayAnimation(idleAnim);
                        }
                    }
                }
                else
                {
                    if (!Attacking)
                    {
                        if (enemyTarget != null)
                        {
                            TurnTowards(enemyTarget.transform.position);
                        }
                        else
                        {
                            TurnTowards(Player.Player1.transform.position);
                        }
                    }
                }
            }

            yield return null;
        }


        yield break;
    }

    void TemporarilyDisable(bool value)
    {
        if (temporarilyDisabled != value)
        {
            temporarilyDisabled = value;
            if (value)
            {
                if (attackRoutine != null)
                {
                    StopCoroutine(attackRoutine);
                    attackRoutine = null;
                    Animator.PlayAnimation(idleAnim);
                }

                FadeOut(() =>
                {
                    transform.position = new Vector3(9999f, 9999f, 0f);
                });
            }
            else
            {
                TeleportToPlayer();
            }
        }
    }

    void StopTurning()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
            Animator.PlayAnimation(idleAnim);
        }
    }

    void TurnTowards(Vector3 target)
    {
        if (IsTurning)
        {
            return;
        }

        if (FacingRight && target.x <= transform.position.x)
        {
            turnRoutine = StartCoroutine(TurnToDirection(false));
        }
        else if (!FacingRight && target.x >= transform.position.x)
        {
            turnRoutine = StartCoroutine(TurnToDirection(true));
        }
    }

    IEnumerator TurnToDirection(bool faceRight)
    {
        MainRenderer.flipX = faceRight;
        yield return Animator.PlayAnimationTillDone(turnAnim);
        turnRoutine = null;
        Animator.PlayAnimation(idleAnim);
    }

    IEnumerator EnemyScanRoutine()
    {
        while (true)
        {
            //var triggerHitOld = Physics2D.queriesHitTriggers;
            try
            {
                /*Physics2D.queriesHitTriggers = true;
                foundEnemyCount = Physics2D.BoxCastNonAlloc(transform.position, new Vector2(alertRange, alertRange), 0, Vector2.down, foundEnemies, alertRange, attackLayerMask,);*/
                //foundEnemyCount = Physics2D.CircleCastNonAlloc(transform.position, alertRange, Vector2.down, foundEnemies, alertRange, attackLayerMask);
                foundEnemyCount = collisionCounter.CollidedObjectCount;
            }
            finally
            {
                //Physics2D.queriesHitTriggers = triggerHitOld;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void TeleportToPlayer()
    {
        AimingAtPlayer = true;
        NearestTarget = Player.Player1.transform.position;
        MainRenderer.color = MainRenderer.color.With(a: 0f);
        Flasher.FlashIntensity = 0f;
        Rigidbody.velocity = default;
        FadeIn();

        transform.position = Player.Player1.transform.position + new Vector3(0f, 0.75f);
    }

    IEnumerator AttackTimeRange()
    {
        currentAttackTime = attackTimeRange.RandomInRange();
        currentDelayTime = preAttackDelayRange.RandomInRange();
        while (true)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= currentAttackTime)
            {
                attackTimer = currentAttackTime;
            }

            if (foundEnemyCount == 0)
            {
                //currentDelayTime = preAttackDelayRange.RandomInRange();
                if (attackTimer >= currentAttackTime - currentDelayTime)
                {
                    attackTimer = currentAttackTime - currentDelayTime;
                }
            }

            /*if (foundEnemyCount == 0)
            {
                attackTimer = 0f;
                currentAttackTime = attackTimeRange.RandomInRange();
            }*/
            /*if (Vector3.Distance(Player.Player1.transform.position, oldPlayerPos) >= 0.1f)
            {
                oldPlayerPos = Player.Player1.transform.position;
                sleepTimer = 0f;
                currentSleepTime = sleepTimeRange.RandomInRange();
            }*/
            yield return null;
        }
    }

    IEnumerator SleepTimerRoutine()
    {
        currentSleepTime = sleepTimeRange.RandomInRange();
        Vector3 oldPlayerPos = Player.Player1.transform.position;
        while (true)
        {
            if (AimingAtPlayer)
            {
                sleepTimer += Time.deltaTime;
            }
            if (Vector3.Distance(Player.Player1.transform.position, oldPlayerPos) >= 0.1f)
            {
                oldPlayerPos = Player.Player1.transform.position;
                sleepTimer = 0f;
                currentSleepTime = sleepTimeRange.RandomInRange();
            }
            yield return null;
        }
    }

    private void Update()
    {
        jitterTimer += Time.deltaTime;

        if (jitterTimer >= jitterRandomizeTime)
        {
            jitterVector = UnityEngine.Random.insideUnitCircle;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, alertRange);
    }
}
