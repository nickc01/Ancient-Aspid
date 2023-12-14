using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;
using static ShotgunController;

public class OffensiveMode : AncientAspidMode
{
    public const string OFFENSIVE_TIME = nameof(OFFENSIVE_TIME);
    public const string CHECK_TOO_FAR_AWAY_PRE = nameof(CHECK_TOO_FAR_AWAY_PRE);
    public const string AUTO_ENABLE_CAM_LOCK = nameof(AUTO_ENABLE_CAM_LOCK);
    public const string WAIT_TILL_IN_POS = nameof(WAIT_TILL_IN_POS);
    public const string OVERRIDE_CENTER_CHECK = nameof(OVERRIDE_CENTER_CHECK);
    public const string ON_CENTER_START = nameof(ON_CENTER_START);
    public const string ON_CENTER_END = nameof(ON_CENTER_END);
    public const string ON_CENTER_FAIL = nameof(ON_CENTER_FAIL);
    public const string READY_OVERRIDE = nameof(READY_OVERRIDE);
    public const string FORCE_CENTER_MODE = nameof(FORCE_CENTER_MODE);

    public IModeAreaProvider OffensiveAreaProvider { get; set; }

    public bool RanAtLeastOnce { get; private set; } = false;

    [field: Header("Offensive Mode")]
    [field: SerializeField]
    [field: Tooltip("How long does the offensive mode last. This is ignored when at the second platform")]
    public float DefaultModeDuration { get; set; } = 10f;

    [field: SerializeField]
    [field: Tooltip("The max distance the player can be from the boss in offensive mode. If the player goes beyond this distance, the boss exits offensive mode")]
    public float OffensiveMaxDistance { get; set; } = 30f;

    [field: SerializeField]
    public float OffensiveAttackDistance { get; set; } = 25f;

    [field: SerializeField]
    [field: Tooltip("The camera lock area to enable when in center mode")]
    public WeaverCameraLock CenterCamLock { get; set; }

    [field: SerializeField]
    [field: Tooltip("The final orbit reduction amount when fully in center mode")]
    public float FinalOrbitReduction { get; set; } = 50;

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

    [SerializeField]
    Vector2 centerModePlatformDest = new Vector2(220.8f, 211.5f);

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
    public float OffensiveHeight { get; private set; } = 5f;

    [SerializeField]
    List<GroupSpawner> platformGroupSpawners = new List<GroupSpawner>();


    bool stop = false;
    TargetOverride offensiveTarget;
    uint offensiveCheckRoutine;

    public bool InCenterMode { get; private set; } = false;

    bool DefaultCenterCheck()
    {
        return Boss.CanSeeTarget && Vector3.Distance(transform.position, Player.Player1.transform.position) <= OffensiveAttackDistance && Player.Player1.transform.position.y < transform.position.y + 1;
    }

    protected override IEnumerator OnExecute(Dictionary<string, object> args)
    {
        using (var recoilOverride = Boss.Recoil.AddRecoilOverride(0))
        {
            RanAtLeastOnce = true;
            stop = false;

            args.TryGetValueOfType(FORCE_CENTER_MODE, out bool forceCenterMode, false);


            args.TryGetValueOfType(OFFENSIVE_TIME, out var offensiveModeDuration, DefaultModeDuration);

            if (!forceCenterMode && args.TryGetValueOfType(CHECK_TOO_FAR_AWAY_PRE, out bool doFarAwayCheck, true) && doFarAwayCheck && Vector3.Distance(Player.Player1.transform.position, transform.position) >= OffensiveMaxDistance)
            {
                yield break;
            }

            yield return EnterCenterMode(args);

            if (!InCenterMode)
            {
                if (args.TryGetValueOfType(ON_CENTER_FAIL, out Delegate onCenterFail))
                {
                    onCenterFail?.DynamicInvoke();
                }
                yield break;
            }
            else
            {
                if (args.TryGetValueOfType(ON_CENTER_START, out Delegate onCenterStart))
                {
                    onCenterStart?.DynamicInvoke();
                }
            }

            args.TryGetValueOfType(OVERRIDE_CENTER_CHECK, out Func<bool> centerCheck, DefaultCenterCheck);

            if (args.TryGetValueOfType(READY_OVERRIDE, out Func<IEnumerator> readyCheck, null) && readyCheck != null)
            {
                yield return readyCheck();
            }

            Boss.EnableQuickEscapes = false;

            var endTime = Time.time + offensiveModeDuration;

            float lastMoveTime = Time.time;
            float lastMoveDelay = 0f;

            offensiveCheckRoutine = Boss.StartBoundRoutine(OffensiveCheck(centerCheck));

            while (!stop && Time.time <= endTime)
            {
                yield return Boss.UpdateDirection();

                if (stop)
                {
                    break;
                }

                Boss.MoveRandomizer.MoveNext();

                var move = Boss.MoveRandomizer.Current;

                if (move == null)
                {
                    continue;
                }

                var oldHealth = Boss.HealthManager.Health;

                while (Time.time - lastMoveTime < move.PreDelay + lastMoveDelay)
                {
                    if (Boss.HealthManager.Health != oldHealth)
                    {
                        oldHealth = Boss.HealthManager.Health;
                        lastMoveDelay = 0.25f;
                    }
                    yield return Boss.UpdateDirection();
                }

                oldHealth = Boss.HealthManager.Health;
                yield return RunAspidMove(move, args);

                lastMoveTime = Time.time;
                lastMoveDelay = move.GetPostDelay(oldHealth);
            }

            if (offensiveCheckRoutine != 0)
            {
                Boss.StopBoundRoutine(offensiveCheckRoutine);
                offensiveCheckRoutine = 0;
            }

            yield return ExitCenterMode();

            if (args.TryGetValueOfType(ON_CENTER_END, out Delegate onCenterEnd))
            {
                onCenterEnd?.DynamicInvoke();
            }
        }
    }

    IEnumerator OffensiveCheck(Func<bool> check)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.15f);
            if (!check())
            {
                Stop();
                offensiveCheckRoutine = 0;
                yield break;
            }
        }
    }

    public override void Stop()
    {
        stop = true;
        StopCurrentMove();
    }

    IEnumerator EnterCenterMode(Dictionary<string, object> args)
    {


        InCenterMode = false;
        Boss.EnableHomeInOnTarget();


        var newTarget = OffensiveAreaProvider.GetModeTarget(Boss);

        Debug.DrawLine(newTarget, newTarget + Vector2.up, Color.blue,2f);
        Debug.DrawLine(newTarget, newTarget + Vector2.down, Color.red, 2f);
        Debug.DrawLine(newTarget, newTarget + Vector2.left, Color.green, 2f);
        Debug.DrawLine(newTarget, newTarget + Vector2.right, Color.cyan, 2f);

        offensiveTarget = Boss.AddTargetOverride(-10);
        offensiveTarget.SetTarget(newTarget);
        yield return Boss.ChangeDirection(AspidOrientation.Center);

        float timer = 0f;
        float maxTimeToCenter = 4f;
        float centeringTimer = 0;

        bool centered = false;
        args.TryGetValueOfType(CHECK_TOO_FAR_AWAY_PRE, out bool doFarAwayCheck, true);

        args.TryGetValueOfType(AUTO_ENABLE_CAM_LOCK, out bool autoEnableCamLock, true);

        if (args.TryGetValueOfType(WAIT_TILL_IN_POS, out bool doWait, true) && doWait)
        {
            while (timer <= 0.25f && centeringTimer < maxTimeToCenter)
            {
                if (Vector3.Distance(transform.position, newTarget) <= 2f && !centered)
                {
                    centered = true;
                    if (autoEnableCamLock)
                    {
                        Boss.EnableCamLock(newTarget, CenterCamLock);
                    }
                }

                if (doFarAwayCheck && Vector3.Distance(Player.Player1.transform.position, newTarget) > 25f)
                {
                    centeringTimer = maxTimeToCenter;
                    break;
                }

                centeringTimer += Time.deltaTime;
                Boss.OrbitReductionAmount += 3f * Boss.OrbitReductionAmount * Time.deltaTime;
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
        }
        else
        {
            if (autoEnableCamLock)
            {
                Boss.EnableCamLock(newTarget, CenterCamLock);
            }
        }

        if (centeringTimer >= maxTimeToCenter)
        {
            yield return ExitCenterMode();
        }
        else
        {
            InCenterMode = true;
        }
    }

    public IEnumerator ExitCenterMode()
    {
        return ExitCenterMode(Boss.GetOrientationToPlayer());
    }

    public IEnumerator ExitCenterMode(AspidOrientation orientation, bool wait = true)
    {
        if (offensiveTarget != null)
        {
            Boss.RemoveTargetOverride(offensiveTarget);
            offensiveTarget = null;
        }
        Boss.DisableCamLock(CenterCamLock);
        Boss.DisableHomeInOnTarget();
        yield return Boss.ChangeDirection(orientation);

        Boss.EnableQuickEscapes = true;
        if (wait)
        {
            yield return new WaitForSeconds(0.25f);
        }
    }

    public override void OnDeath()
    {
        if (offensiveTarget != null)
        {
            Boss.RemoveTargetOverride(offensiveTarget);
            offensiveTarget = null;
        }
        Boss.DisableCamLock(CenterCamLock);
        Boss.DisableHomeInOnTarget();

        CenterCamLock.GetComponent<Collider2D>().enabled = true;
    }

    protected override bool ModeEnabled(Dictionary<string, object> args)
    {
        return OffensiveAreaProvider != null && OffensiveAreaProvider.IsTargetActive(Boss);
    }

    enum CenterBottomTargetType
    {
        None,
        Bottom,
        Left,
        Right
    }

    public IEnumerator EnterFromBottomRoutine()
    {
        yield return new WaitUntil(() => Boss.FullyAwake);
        var enterBottomTarget = Boss.AddTargetOverride(int.MaxValue / 2);

        while (true)
        {
            var playerAboveArena = Player.Player1.transform.position.y >= 200;
            var bossAboveArena = transform.position.y >= 200;

            if ((playerAboveArena && bossAboveArena)        )
            {
                Boss.FlightRange.yMin = 0f;
                if (Player.Player1.transform.position.x >= 197.62f)
                {
                    enterBottomTarget.SetTarget(new Vector3(167.4f, 216.7f));
                    WeaverLog.LogWarning("FOLLOW LEFT TARGET");
                    if (transform.position.x <= 179.7f || transform.position.x <= Player.Player1.transform.position.x - 20f)
                    {
                        yield return CentralRiseRoutine();
                        break;
                    }
                }
                else
                {
                    enterBottomTarget.SetTarget(new Vector3(262.5f, 216.7f));
                    WeaverLog.LogWarning("FOLLOW RIGHT TARGET");
                    if (transform.position.x >= 251.1f || transform.position.x >= Player.Player1.transform.position.x + 20f)
                    {
                        yield return CentralRiseRoutine();
                        break;
                    }
                }
            }
            else if (playerAboveArena && !bossAboveArena)
            {
                Boss.FlightRange.yMin = 0f;
                WeaverLog.LogWarning("FOLLOW BOTTOM TARGET");
                enterBottomTarget.SetTarget(new Vector3(220.8f, 85.29f));
                if (transform.position.y <= Player.Player1.transform.position.y - 15f)
                {
                    yield return CentralRiseRoutine();
                    break;
                }
            }
            else if ((!playerAboveArena && !bossAboveArena) || (!playerAboveArena && bossAboveArena))
            {
                if (Player.Player1.transform.position.y <= transform.position.y)
                {
                    if (transform.position.y >= 180f)
                    {
                        Boss.FlightRange.yMin = 0f;
                        if (transform.position.x >= 223.7f)
                        {
                            enterBottomTarget.SetTarget(new Vector3(250.46f, 213.86f));
                        }
                        else
                        {
                            enterBottomTarget.SetTarget(new Vector3(185.6f, 213.86f));
                        }
                    }
                    else
                    {
                        enterBottomTarget.SetTarget(Boss.PlayerTarget);

                        Boss.FlightRange.yMin = Mathf.Max(Boss.FlightRange.yMin, Player.Player1.transform.position.y + 14f);
                    }
                }
                else
                {
                    Boss.FlightRange.yMin = 0f;
                    if (transform.position.x >= 223.7f)
                    {
                        enterBottomTarget.SetTarget(new Vector3(185.6f, 213.86f));
                    }
                    else
                    {
                        enterBottomTarget.SetTarget(new Vector3(250.46f, 213.86f));
                    }
                }

            }

            yield return null;
        }

        Boss.RemoveTargetOverride(enterBottomTarget);

    }

    enum CentralRiseSpeed
    {
        Default,
        Quick,
        Instant
    }


    IEnumerator CentralRiseRoutine()
    {
        Boss.FlightEnabled = false;
        Boss.ApplyFlightVariance = false;
        transform.SetPosition2D(220.8f, 188.95f);
        Boss.Rbody.velocity = default;
        Boss.Rbody.simulated = false;

        var riseSpeed = CentralRiseSpeed.Default;

        bool fullyRisen = false;

        var platformProvider = GameObject.FindObjectOfType<PlatformOffensiveAreaProvider>();

        IEnumerator ReadyToDoMovesRoutine()
        {
            yield return new WaitUntil(() => fullyRisen);
        }

        bool retry = false;

        bool CenterCheck()
        {
            if (retry)
            {
                return false;
            }
            else
            {
                return DefaultCenterCheck();
            }
        }

        OffensiveAreaProvider = platformProvider;

        yield return Boss.SwitchToNewMode(this, new Dictionary<string, object>
        {
            {OFFENSIVE_TIME, float.PositiveInfinity},
            {FORCE_CENTER_MODE, true},
            {CHECK_TOO_FAR_AWAY_PRE, false},
            {READY_OVERRIDE, new Func<IEnumerator>(ReadyToDoMovesRoutine)},
            {OVERRIDE_CENTER_CHECK, new Func<bool>(CenterCheck)}
        });

        if (centerPlatformLockArea != null)
        {
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
                        retry = true;
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

            if (rumbleSound != null)
            {
                rumbleSound.StopPlaying();
            }

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

                foreach (var spawner in platformGroupSpawners)
                {
                    if (spawner != null)
                    {
                        spawner.StartSpawning();
                    }
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

        if (riseSpeed == CentralRiseSpeed.Default)
        {
            Boss.EnableCamLock(centerModePlatformDest, CenterCamLock);
        }

        if (riseSpeed == CentralRiseSpeed.Default)
        {
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitUntil(() => Boss.Orientation == AspidOrientation.Center);

        if (riseSpeed != CentralRiseSpeed.Instant)
        {
            StartCoroutine(VisualsRoutine(centerModeRoarDelay));
        }

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

        Boss.Rbody.velocity = default;
        Boss.Rbody.simulated = true;

        Boss.FlightEnabled = true;
        Boss.ApplyFlightVariance = true;
        fullyRisen = true;
        if (riseSpeed == CentralRiseSpeed.Instant || riseSpeed == CentralRiseSpeed.Quick)
        {
            yield return new WaitUntil(() => Boss.Orientation != AspidOrientation.Center);
        }

        if (retry)
        {
            Boss.StartBoundRoutine(EnterFromBottomRoutine());
        }
        else
        {
            Boss.FlightRange.yMin = 201f;
        }
    }
}
