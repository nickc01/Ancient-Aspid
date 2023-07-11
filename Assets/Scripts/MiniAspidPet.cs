using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Attributes;

public class MiniAspidPet : SpitterPet
{
    public static bool MiniAspidPetEnabled { get; private set; } = false;

    /*[SerializeField]
    Vector2 minTargetDistanceRange = new Vector2(1.5f,3f);

    [SerializeField]
    Vector2 maxTargetDistanceRange = new Vector2(3f,5.5f);

    [SerializeField]
    Vector2 jitterAmountRange = new Vector2(0.25f,1f);

    [SerializeField]
    float jitterFrequency = 0.75f;

    Vector2 target;
    Vector2 targetJitter;

    [SerializeField]
    float collisionRadius = 12f;

    [SerializeField]
    CollisionCounter inRangeObjects;

    [SerializeField]
    float acceleration = 3f;

    [SerializeField]
    float maxVelocity = 15f;

    [SerializeField]
    Vector2 shootFrequencyMinMax = new Vector2(1.5f,2.5f);

    [SerializeField]
    AudioClip shootSound;

    [SerializeField]
    float aspidShotVelocity = 15f;

    //[SerializeField]
    //float shotAngleSeperation = 35f;

    [SerializeField]
    AspidShot aspidShotPrefab;

    bool isFlyingIn = false;

    GameObject enemyTarget = null;
    GameObject lookTarget = null;
    Rigidbody2D rb;
    WeaverAnimationPlayer animationPlayer;
    Coroutine flyingRoutine;
    Collider2D[] colliders;
    bool flyingOut = false;

    static RaycastHit2D[] enemyHitCache = new RaycastHit2D[10];


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
        animationPlayer = GetComponent<WeaverAnimationPlayer>();
        target = Player.Player1.transform.position;
        StartCoroutine(TargetUpdateRoutine());
        StartCoroutine(JitterRoutine());
        //StartCoroutine(MainRoutine());
        StartCoroutine(EnemyTargetUpdateRoutine());
        StartCoroutine(ShootRoutine());
        lookTarget = Player.Player1.gameObject;
        DontDestroyOnLoad(gameObject);

        FlyToPlayer();
    }

    private void Instance_heroInPosition(bool forceDirect)
    {
        UpdateFlyingState();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        HeroController.instance.heroInPosition += Instance_heroInPosition;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        if (HeroController.instance != null)
        {
            HeroController.instance.heroInPosition -= Instance_heroInPosition;
        }
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        if (HeroController.instance != null)
        {
            HeroController.instance.heroInPosition -= Instance_heroInPosition;
        }
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        UpdateFlyingState();
    }

    void UpdateFlyingState()
    {
        if (flyingOut)
        {
            Destroy(gameObject);
        }
        else
        {
            enemyTarget = null;
            lookTarget = Player.Player1.gameObject;
            FlyToPlayer();
        }
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            var prevTime = Time.time;

            var waitTime = shootFrequencyMinMax.RandomInRange();

            while (Time.time - prevTime < waitTime)
            {
                if (lookTarget == null)
                {
                    lookTarget = Player.Player1.gameObject;
                }

                var lookPosition = lookTarget.transform.position;

                if (enemyTarget != null)
                {
                    lookPosition = enemyTarget.transform.position;
                }

                //WeaverLog.Log("FACE RIGHT = " + (transform.position.x < lookPosition.x ? true : false));

                yield return FaceDirection(transform.position.x < lookPosition.x ? true : false);
            }

            if (enemyTarget != null && !isFlyingIn)
            {
                var enemyPos = enemyTarget.transform.position;

                yield return animationPlayer.PlayAnimationTillDone("Mini Aspid Attack Antic");

                if (enemyTarget != null)
                {
                    enemyPos = enemyTarget.transform.position;
                }

                if (shootSound != null)
                {
                    WeaverAudio.PlayAtPoint(shootSound, transform.position);
                }
                Shoot(enemyPos);

                yield return animationPlayer.PlayAnimationTillDone("Mini Aspid Attack");

                animationPlayer.PlayAnimation("Mini Aspid Idle");
            }
        }
    }

    void Shoot(Vector3 target)
    {
        var directionToTarget = (target - transform.position).normalized;

        var angle = MathUtilities.CartesianToPolar(directionToTarget).x;

        AspidShot.Spawn(aspidShotPrefab, transform.position, MathUtilities.PolarToCartesian(angle, aspidShotVelocity));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }

    IEnumerator EnemyTargetUpdateRoutine()
    {
        while (true)
        {
            //var hits = Physics2D.CircleCastNonAlloc(transform.position, collisionRadius, Vector2.right, enemyHitCache,Mathf.Infinity,LayerMask.GetMask("Enemies"));

            WeaverLog.Log("HITS = " + inRangeObjects.CollidedObjectCount);

            enemyTarget = inRangeObjects.GetNearestTarget(transform.position)?.gameObject;

            if (enemyTarget == null)
            {
                lookTarget = Player.Player1.gameObject;
            }
            else
            {
                lookTarget = enemyTarget;
            }
            yield return new WaitForSeconds(0.75f);
        }
    }

    IEnumerator JitterRoutine()
    {
        while (true)
        {
            targetJitter = UnityEngine.Random.insideUnitCircle * jitterAmountRange.RandomInRange();
            yield return new WaitForSeconds(jitterFrequency);
        }
    }

    IEnumerator TargetUpdateRoutine()
    {
        var minTargetDistance = minTargetDistanceRange.RandomInRange();
        var maxTargetDistance = maxTargetDistanceRange.RandomInRange();

        while (true)
        {
            if (!isFlyingIn)
            {
                var distanceToPlayer = Vector2.Distance(transform.position, Player.Player1.transform.position);

                Vector2 vectorToPlayer = (Vector2)Player.Player1.transform.position - (Vector2)transform.position;

                var vectorToPet = -vectorToPlayer;

                if (distanceToPlayer < minTargetDistance)
                {
                    target = (Vector2)Player.Player1.transform.position + (vectorToPet.normalized * minTargetDistance);
                }
                else if (distanceToPlayer > maxTargetDistance)
                {
                    target = (Vector2)Player.Player1.transform.position + (vectorToPet.normalized * maxTargetDistance);
                }
                else
                {
                    target = transform.position;
                }
            }
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        var petDestination = target + targetJitter;
        var distanceToDest = Vector2.Distance(petDestination, transform.position);

        var velocity = rb.velocity;

        velocity += (petDestination - (Vector2)transform.position).normalized * acceleration * Time.fixedDeltaTime;

        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0f, Mathf.Min(distanceToDest * 2f, maxVelocity));

        rb.velocity = velocity;

        if (!isFlyingIn && Vector2.Distance(transform.position, Player.Player1.transform.position) >= 25f)
        {
            FlyToPlayer();
        }

        if (!MiniAspidPetEnabled || (Initialization.Environment == WeaverCore.Enums.RunningState.Game && PlayMakerUtilities.GetFsmBool(Player.Player1.gameObject, "ProxyFSM", "No Charms")))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FaceDirection(bool faceRight)
    {
        if (faceRight && transform.localScale.x > 0)
        {
            transform.SetXLocalScale(-1f);
            yield return animationPlayer.PlayAnimationTillDone("Mini Aspid Turn");
            animationPlayer.PlayAnimation("Mini Aspid Idle");
        }
        else if (!faceRight && transform.localScale.x < 0)
        {
            transform.SetXLocalScale(1f);
            yield return animationPlayer.PlayAnimationTillDone("Mini Aspid Turn");
            animationPlayer.PlayAnimation("Mini Aspid Idle");
        }
        else
        {
            yield return null;
        }
    }

    static RaycastHit2D[] hitCache = new RaycastHit2D[1];

    IEnumerator FlyingInRoutine()
    {
        while (true)
        {
            var terrainLayer = LayerMask.GetMask("Terrain");

            float left, right, top, bottom;


            if (Physics2D.RaycastNonAlloc(Player.Player1.transform.position, Vector2.up, hitCache, 10f, terrainLayer) > 0)
            {
                top = hitCache[0].point.y;
            }
            else
            {
                top = Player.Player1.transform.position.y + 10f;
            }

            if (Physics2D.RaycastNonAlloc(Player.Player1.transform.position, Vector2.down, hitCache, 10f, terrainLayer) > 0)
            {
                bottom = hitCache[0].point.y;
            }
            else
            {
                bottom = Player.Player1.transform.position.y - 10f;
            }

            if (Physics2D.RaycastNonAlloc(Player.Player1.transform.position, Vector2.left, hitCache, 10f, terrainLayer) > 0)
            {
                left = hitCache[0].point.x;
            }
            else
            {
                left = Player.Player1.transform.position.x - 10f;
            }

            if (Physics2D.RaycastNonAlloc(Player.Player1.transform.position, Vector2.right, hitCache, 10f, terrainLayer) > 0)
            {
                right = hitCache[0].point.x;
            }
            else
            {
                right = Player.Player1.transform.position.x + 10f;
            }

            Debug.DrawLine(Player.Player1.transform.position, new Vector3(Player.Player1.transform.position.x, top), Color.red, 0.5f);
            Debug.DrawLine(Player.Player1.transform.position, new Vector3(Player.Player1.transform.position.x, bottom), Color.blue, 0.5f);
            Debug.DrawLine(Player.Player1.transform.position, new Vector3(left, Player.Player1.transform.position.y), Color.green, 0.5f);
            Debug.DrawLine(Player.Player1.transform.position, new Vector3(right, Player.Player1.transform.position.y), Color.yellow, 0.5f);

            target = Player.Player1.transform.position;

            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                var pos = transform.position;

                if (pos.x > left + 1f && pos.x < right - 1f && pos.y < top - 1f && pos.y > bottom + 1f)
                {
                    foreach (var c in colliders)
                    {
                        c.enabled = true;
                    }
                    isFlyingIn = false;
                    flyingRoutine = null;

                    yield break;
                }
                yield return null;
            }
        }
    }

    IEnumerator FlyingOutRoutine()
    {
        StartCoroutine(AudioFadeRoutine(0.5f));
        target = Player.Player1.transform.position + new Vector3(0f, 30f);

        while (true)
        {
            if (Vector3.Distance(transform.position, target) < 5f)
            {
                flyingRoutine = null;
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator AudioFadeRoutine(float time)
    {
        var audio = GetComponent<AudioSource>();
        var oldVolume = audio.volume;

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            audio.volume = Mathf.Lerp(oldVolume, 0f, t / time);
            yield return null;
        }
    }

    public void FlyToPlayer()
    {
        transform.position = Player.Player1.transform.position + new Vector3(0f, 50f, 0f);
        if (flyingRoutine != null)
        {
            StopCoroutine(flyingRoutine);
            flyingRoutine = null;
        }
        isFlyingIn = true;

        if (colliders == null)
        {
            colliders = GetComponentsInChildren<Collider2D>();
        }

        foreach (var c in colliders)
        {
            c.enabled = false;
        }
        flyingRoutine = StartCoroutine(FlyingInRoutine());
    }

    public void FlyAway()
    {
        if (flyingRoutine != null)
        {
            StopCoroutine(flyingRoutine);
            flyingRoutine = null;
        }
        isFlyingIn = true;

        if (colliders == null)
        {
            colliders = GetComponentsInChildren<Collider2D>();
        }

        foreach (var c in colliders)
        {
            c.enabled = false;
        }
        flyingRoutine = StartCoroutine(FlyingOutRoutine());
        flyingOut = true;
    }

    */

    static GameObject OriginalPrefab;

    public static void ReplaceHatchlingPrefab(Player player)
    {
        var charmEffects = player.transform.Find("Charm Effects");
        if (charmEffects == null)
        {
            return;
        }


        var hatchlingFSM = PlayMakerUtilities.GetPlaymakerFSMOnObject(charmEffects.gameObject, "Hatchling Spawn");

        var fsm = PlayMakerUtilities.GetFSMOnPlayMakerComponent(hatchlingFSM);

        var hatchState = PlayMakerUtilities.FindStateOnFSM(fsm, "Hatch");

        var actionData = PlayMakerUtilities.GetActionData(hatchState);

        var gameObjects = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

        foreach (var fsmGM in gameObjects)
        {
            var gm = (GameObject)fsmGM.ReflectGetProperty("Value");

            if (gm.name == "Knight Hatchling")
            {
                OriginalPrefab = gm;
                fsmGM.ReflectSetProperty("Value", WeaverAssets.LoadAssetFromBundle<GameObject, AncientAspidMod>("Knight Hatchling Aspid"));
                MiniAspidPetEnabled = true;
                return;
            }
        }
    }

    public static void RevertHatchlingPrefab(Player player)
    {
        var charmEffects = player.transform.Find("Charm Effects");
        if (charmEffects == null)
        {
            return;
        }

        var hatchlingFSM = PlayMakerUtilities.GetPlaymakerFSMOnObject(charmEffects.gameObject, "Hatchling Spawn");

        var fsm = PlayMakerUtilities.GetFSMOnPlayMakerComponent(hatchlingFSM);

        var hatchState = PlayMakerUtilities.FindStateOnFSM(fsm, "Hatch");

        var actionData = PlayMakerUtilities.GetActionData(hatchState);

        var gameObjects = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

        foreach (var fsmGM in gameObjects)
        {
            var gm = (GameObject)fsmGM.ReflectGetProperty("Value");

            if (gm.name == "Knight Hatchling Aspid")
            {
                fsmGM.ReflectSetProperty("Value", OriginalPrefab);
                MiniAspidPetEnabled = false;
                return;
            }
        }
    }
}
