using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;
using static WeaverCore.Blood;

public class HeadController : AspidBodyPart
{
    public struct IdleSprite
    {
        public Sprite Sprite;
        public bool XFlipped;
        public float Degrees;
    }

    [SerializeField]
    [Tooltip("When the boss is in offensive mode, the head will look towards the player if this is set to true")]
    private bool lookAtPlayer = true;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_Sprites")]
    private List<Sprite> idle_Sprites;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_HorizFlip")]
    private List<bool> idle_Flip;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_Degrees")]
    private List<float> idle_Degrees;

    [Header("Lunge")]
    [SerializeField]
    private Sprite lungeSprite;
    private bool turningTowardsPlayer = false;
    [SerializeField]
    private List<Sprite> landSprites;

    [SerializeField]
    private List<Vector2> landPositions;

    [SerializeField]
    private float landFPS;

    [Header("Ground Laser")]
    [SerializeField]
    private List<Sprite> groundLaserSprites;

    [SerializeField]
    private List<Vector3> groundLaserRotations;

    [SerializeField]
    private float groundLaserFPS = 12;

    [SerializeField]
    private ParticleSystem groundLaserParticles;

    [SerializeField]
    private LaserEmitter groundLaserEmitter;

    [SerializeField]
    private Transform groundLaserOrigin;

    [SerializeField]
    private AudioClip groundLaserSound;

    [SerializeField]
    private GameObject spitSourceLeft;

    [SerializeField]
    private GameObject spitSourceRight;

    [SerializeField]
    private float groundLaserBloodSpawnRate = 0.15f;
    private float laserOriginStartX;

    public float GlobalLockSpeedMultiplier { get; set; } = 1f;


    public float LookingDirection { get; private set; } = -60f;

    public bool HeadLocked { get; private set; } = false;
    public float LaserChargeUpDuration => groundLaserEmitter.ChargeUpDuration;

    private uint unlockRoutine = 0;
    private uint lockRoutine = 0;
    private bool doUpdate = false;

    bool changingDirection = false;

    [NonSerialized]
    ShotgunLaserManager _shotgunLasers;

    public ShotgunLaserManager ShotgunLasers
    {
        get
        {
            if (_shotgunLasers == null)
            {
                _shotgunLasers = GetComponentInChildren<ShotgunLaserManager>(true);
            }

            return _shotgunLasers;
        }
    }

    protected override void Awake()
    {
        if (!enabled)
        {
            return;
        }
        base.Awake();
         transform.SetLocalPosition(x: 0f, y: 0f);
        MainRenderer.sprite = idle_Sprites[0];
        laserOriginStartX = groundLaserOrigin.GetXLocalPosition();
    }

    private void Update()
    {
        if (turningTowardsPlayer && doUpdate)
        {
            float angleToPlayer = GetDownwardAngleToPlayer();
            if (angleToPlayer >= LookingDirection)
            {
                LookingDirection += Mathf.Clamp(angleToPlayer - LookingDirection, 0f, 120f * Time.deltaTime);
            }
            else
            {
                LookingDirection += Mathf.Clamp(angleToPlayer - LookingDirection, -120f * Time.deltaTime, 0f);
            }

            int index = GetIdleIndexForAngle(LookingDirection);

            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
        }
    }

    public override float GetChangeDirectionTime(float speedMultiplier = 1f)
    {
        float time = base.GetChangeDirectionTime();

        if (lookAtPlayer)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                time += 1f / (DEFAULT_FPS * speedMultiplier) * DEFAULT_CENTERIZE_FRAMES;
            }
            else if (CurrentOrientation != AspidOrientation.Center && PreviousOrientation == AspidOrientation.Center)
            {
                time += 1f / (DEFAULT_FPS * speedMultiplier) * DEFAULT_CENTERIZE_FRAMES;
            }
        }

        return time;
    }

    protected override IEnumerator ChangeDirectionRoutine(float speedMultiplier = 1f)
    {
        if (HeadLocked)
        {
            throw new Exception("The head wasn't unlocked when the last move ended");
        }

        changingDirection = true;

        void StopUnlocker()
        {
            if (unlockRoutine != 0)
            {
                Boss.StopBoundRoutine(unlockRoutine);
                unlockRoutine = 0;
                HeadLocked = false;
                doUpdate = true;
            }
        }

        if (lookAtPlayer)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                StopUnlocker();
                yield return StartFollowingPlayer(speedMultiplier);
                yield break;
            }
            else if (CurrentOrientation != AspidOrientation.Center && PreviousOrientation == AspidOrientation.Center)
            {
                StopUnlocker();
                yield return StopFollowingPlayer(speedMultiplier);
                yield break;
            }
        }

        //var oldUnlockRoutine = unlockRoutine;

        /*if (unlockRoutine != 0)
        {
            turningTowardsPlayer = true;
            doUpdate = true;
            Boss.StopBoundRoutine(unlockRoutine);
            unlockRoutine = 0;
        }*/

        StopUnlocker();

        //LookingDirection = OrientationToAngle(CurrentOrientation);

        //yield return base.ChangeDirectionRoutine(speedMultiplier);
        yield return Internal_ChangeDirectionHead(speedMultiplier);

        //if (PreviousOrientation != AspidOrientation.Center && CurrentOrientation != AspidOrientation.Center)
        if (CurrentOrientation != AspidOrientation.Center)
        {
            if (CurrentOrientation == AspidOrientation.Left)
            {
                MainRenderer.sprite = idle_Sprites[0];
                MainRenderer.flipX = idle_Flip[0];
            }
            else if (CurrentOrientation == AspidOrientation.Right)
            {
                MainRenderer.sprite = idle_Sprites[idle_Degrees.Count - 1];
                MainRenderer.flipX = idle_Flip[idle_Degrees.Count - 1];
            }
        }

        changingDirection = false;
    }

    float GetClipDuration(string name)
    {
        return Boss.Head.Animator.AnimationData.GetClipDuration(name);
    }

    IEnumerator Internal_ChangeDirectionHead(float speedMultiplier)
    {
        if (PreviousOrientation == AspidOrientation.Center)
        {
            if (CurrentOrientation != AspidOrientation.Center)
            {
                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;

                var newAngle = CurrentOrientation == AspidOrientation.Right ? 60f : -60f;
                //yield return PlayChangeDirectionClip("Decenterize", DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES);
                yield return InterpolateToNewAngleSmooth(LookingDirection, () => newAngle, GetClipDuration("Decenterize") / speedMultiplier);
            }
        }
        else
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                yield return InterpolateToNewAngleSmooth(LookingDirection, () => 0f, GetClipDuration("Centerize") / speedMultiplier);
                //yield return PlayChangeDirectionClip("Centerize", DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES);
            }
            else
            {
                Animator.SpriteRenderer.flipX = CurrentOrientation != AspidOrientation.Right;
                Sprite initialFrame = Animator.SpriteRenderer.sprite;

                var newAngle = CurrentOrientation == AspidOrientation.Right ? 60f : -60f;

                yield return InterpolateToNewAngleSmooth(LookingDirection, () => newAngle, GetClipDuration("Change Direction") / speedMultiplier);
                //yield return PlayChangeDirectionClip("Change Direction", DEFAULT_FPS * speedMultiplier, DEFAULT_CHANGE_DIR_FRAMES);

                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                Animator.SpriteRenderer.sprite = initialFrame;
            }
        }
    }

    private IEnumerator StartFollowingPlayer(float speedMultiplier)
    {
        MainRenderer.flipX = false;
         StartCoroutine(UpdateColliderOffset(DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES));

        yield return InterpolateToFollowPlayer(DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * speedMultiplier);
        turningTowardsPlayer = true;
    }

    private IEnumerator StopFollowingPlayer(float speedMultiplier)
    {
         StartCoroutine(UpdateColliderOffset(DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES));
        yield return InterpolateToNewAngle(LookingDirection, () => OrientationToAngle(CurrentOrientation), DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * speedMultiplier);

        MainRenderer.sprite = idle_Sprites[0];
        MainRenderer.flipX = CurrentOrientation == AspidOrientation.Right;

        turningTowardsPlayer = false;
    }

    private IEnumerator InterpolateToFollowPlayer(float increments, float incrementsPerSecond)
    {
        return InterpolateToNewAngle(OrientationToAngle(CurrentOrientation), GetDownwardAngleToPlayer, increments, incrementsPerSecond);
    }

    private IEnumerator InterpolateToNewAngleSmooth(float oldAngle, Func<float> destAngle, float duration)
    {
        int index;
        //for (int i = 1; i <= increments; i++)
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            LookingDirection = Mathf.Lerp(oldAngle, destAngle(), t / duration);
            index = GetIdleIndexForAngle(LookingDirection);
            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
            yield return null;
        }

        LookingDirection = destAngle();
        index = GetIdleIndexForAngle(LookingDirection);
        MainRenderer.sprite = idle_Sprites[index];
        MainRenderer.flipX = idle_Flip[index];
    }

    private IEnumerator InterpolateToNewAngle(float oldAngle, Func<float> destAngle, float increments, float incrementsPerSecond)
    {
        int index;
        for (int i = 1; i <= increments; i++)
        {
            LookingDirection = Mathf.Lerp(oldAngle, destAngle(), i / (float)increments);
            index = GetIdleIndexForAngle(LookingDirection);
            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
            if (i != increments)
            {
                yield return new WaitForSeconds(1f / incrementsPerSecond);
            }
        }

        LookingDirection = destAngle();
        index = GetIdleIndexForAngle(LookingDirection);
        MainRenderer.sprite = idle_Sprites[index];
        MainRenderer.flipX = idle_Flip[index];
    }

    public IEnumerator LockHead(float lockSpeed = 1f)
    {
        if (Boss.CurrentRunningMode == Boss.OffensiveMode)
        {
            return LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left, lockSpeed);
        }
        else
        {
            return LockHead(Boss.Head.CurrentOrientation, lockSpeed);
        }
    }

    private bool switchingDirections = false;

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        switchingDirections = true;
        PreviousOrientation = oldDirection;
        CurrentOrientation = newDirection;

        float oldDirectionAngle = oldDirection == AspidOrientation.Left ? -60f : 60f;
        float newDirectionAngle = newDirection == AspidOrientation.Left ? -60f : 60f;


        IEnumerator changeDirectionRoutine = base.ChangeDirectionRoutine(Boss.GroundMode.lungeTurnaroundSpeed);

        IEnumerator interpolationRoutine = InterpolateToNewAngle(oldDirectionAngle, () => newDirectionAngle, DEFAULT_CHANGE_DIR_FRAMES, DEFAULT_FPS * Boss.GroundMode.lungeTurnaroundSpeed);

        yield return RoutineAwaiter.AwaitBoundRoutines(Boss, changeDirectionRoutine, interpolationRoutine).WaitTillDone();

        int idleIndex = GetIdleIndexForAngle(OrientationToAngle(newDirection));
        MainRenderer.sprite = idle_Sprites[idleIndex];
        MainRenderer.flipX = idle_Flip[idleIndex];
        yield break;
    }

    public IEnumerator PlayLanding(bool slide)
    {
        if (!slide)
        {
            if (switchingDirections == false)
            {
                MainRenderer.sprite = landSprites[0];
            }
            if (CurrentOrientation == AspidOrientation.Right)
            {
                 transform.SetLocalPosition(x: -landPositions[0].x, y: landPositions[0].y);
            }
            else
            {
                 transform.SetLocalPosition(x: landPositions[0].x, y: landPositions[0].y);
            }
            yield return new WaitForSeconds(Boss.GroundMode.lungeDownwardsLandDelay);
        }

        switchingDirections = false;
        for (int i = 0; i < landSprites.Count; i++)
        {
            if (switchingDirections == false)
            {
                MainRenderer.sprite = landSprites[i];
            }
            if (CurrentOrientation == AspidOrientation.Right)
            {
                 transform.SetLocalPosition(x: -landPositions[i].x, y: landPositions[i].y);
            }
            else
            {
                 transform.SetLocalPosition(x: landPositions[i].x, y: landPositions[i].y);
            }
            yield return new WaitForSeconds(1f / landFPS);
        }
    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        yield break;
    }

    public IEnumerator LockHead(AspidOrientation intendedOrientation, float lockSpeed = 1f)
    {
        return LockHead(OrientationToAngle(intendedOrientation), lockSpeed);
    }

    public IEnumerator QuickFlipDirection(bool faceRight, float flipTime = 1f / 12f)
    {
        if (MainRenderer.flipX != faceRight)
        {
            MainRenderer.sprite = idle_Sprites[idle_Sprites.Count / 2];
            MainRenderer.flipX = faceRight;

            LookingDirection = OrientationToAngle(faceRight ? AspidOrientation.Right : AspidOrientation.Left);

            var index = GetIdleIndexForAngle(LookingDirection);
            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];

            yield return new WaitForSeconds(flipTime);
        }
    }

    public IEnumerator LockHead(float intendedDirection, float lockSpeed)
    {
        if (HeadLocked)
        {
            throw new Exception("The head is already locked");
        }

        if (unlockRoutine != 0)
        {
            Boss.StopBoundRoutine(unlockRoutine);
            unlockRoutine = 0;
        }

        doUpdate = false;
        HeadLocked = true;

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            turningTowardsPlayer = false;

            IEnumerator Lock()
            {
                yield return InterpolateToNewAngle(LookingDirection, () => intendedDirection, DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * lockSpeed * GlobalLockSpeedMultiplier);
                lockRoutine = 0;
            }

            lockRoutine = Boss.StartBoundRoutine(Lock());
            yield return new WaitUntil(() => lockRoutine == 0);
        }
        else
        {
            var oldIndex = GetIdleIndexForAngle(LookingDirection);
            var newIndex = GetIdleIndexForAngle(intendedDirection);
            IEnumerator Lock()
            {
                yield return InterpolateToNewAngle(LookingDirection, () => intendedDirection, Mathf.Abs(newIndex - oldIndex), DEFAULT_FPS * lockSpeed * GlobalLockSpeedMultiplier);
                lockRoutine = 0;
            }

            if (Mathf.Abs(newIndex - oldIndex) != 0)
            {
                lockRoutine = Boss.StartBoundRoutine(Lock());
                yield return new WaitUntil(() => lockRoutine == 0);
            }
        }
    }

    public void UnlockHead()
    {
        UnlockHead(LookingDirection);
    }

    public void UnlockHead(float newHeadDirection)
    {
        if (!HeadLocked)
        {
            throw new Exception("The head is already unlocked");
        }

        if (lockRoutine != 0)
        {
            Boss.StopBoundRoutine(lockRoutine);
            lockRoutine = 0;
        }

        HeadLocked = false;
        LookingDirection = newHeadDirection;

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            turningTowardsPlayer = true;
            doUpdate = true;
        }
        else if (CurrentOrientation != AspidOrientation.Center)
        {
            unlockRoutine = Boss.StartBoundRoutine(UnlockTowardsDirection(newHeadDirection, CurrentOrientation));
        }
    }

    public void UnlockHeadImmediate(AspidOrientation orientation)
    {
        if (!HeadLocked)
        {
            throw new Exception("The head is already unlocked");
        }

        if (lockRoutine != 0)
        {
            Boss.StopBoundRoutine(lockRoutine);
            lockRoutine = 0;
        }

        HeadLocked = false;
        doUpdate = true;
        LookingDirection = OrientationToAngle(orientation);

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            turningTowardsPlayer = true;
        }
        else if (CurrentOrientation != AspidOrientation.Center)
        {
            MainRenderer.sprite = idle_Sprites[0];
            MainRenderer.flipX = orientation == AspidOrientation.Right;
        }

        if (unlockRoutine != 0)
        {
            Boss.StopBoundRoutine(unlockRoutine);
            unlockRoutine = 0;
        }
    }

    private IEnumerator UnlockTowardsDirection(float oldHeadDirection, AspidOrientation orientation)
    {
        float trueNewDirection = orientation == AspidOrientation.Left ? -60f : 60f;
        int oldIndex = GetIdleIndexForAngle(oldHeadDirection);
        int newIndex = GetIdleIndexForAngle(trueNewDirection);

        int increments = Math.Abs(newIndex - oldIndex) + 1;

        //LookingDirection = trueNewDirection;

        for (int i = 1; i <= increments; i++)
        {
            float angle = Mathf.Lerp(oldHeadDirection, trueNewDirection, i / (float)increments);
            int index = GetIdleIndexForAngle(angle);
            if (!changingDirection)
            {
                LookingDirection = angle;
                MainRenderer.sprite = idle_Sprites[index];
                //WeaverLog.Log("Setting Sprite to = " + idle_Sprites[index]);
                MainRenderer.flipX = idle_Flip[index];

                //WeaverLog.Log("Unlock Setting Sprite To = " + MainRenderer.sprite);
            }
            if (i != increments)
            {
                yield return new WaitForSeconds(1f / DEFAULT_FPS);
            }
        }

        if (!changingDirection)
        {
            LookingDirection = trueNewDirection;
            //WeaverLog.Log("Unlock Setting Sprite To = " + idle_Sprites[0]);
            MainRenderer.sprite = idle_Sprites[0];
            //WeaverLog.Log("Setting Sprite to = " + idle_Sprites[0]);
            MainRenderer.flipX = orientation == AspidOrientation.Right;
        }

        unlockRoutine = 0;
        doUpdate = true;
    }

    public void UnlockHead(AspidOrientation newHeadOrientation)
    {
        UnlockHead(OrientationToAngle(newHeadOrientation));
    }

    public float GetDownwardAngleToPlayer()
    {
        return GetDownwardAngleToPosition(Player.Player1.transform.position);
    }

    public float GetDownwardAngleToPosition(Vector3 pos)
    {
        return 90f * Vector2.Dot(Vector2.right, (pos - transform.position).normalized);
    }

    private float OrientationToAngle(AspidOrientation orientation)
    {
        if (orientation == AspidOrientation.Left)
        {
            return idle_Degrees[0];
        }
        else if (orientation == AspidOrientation.Right)
        {
            return idle_Degrees[idle_Degrees.Count - 1];
        }
        else
        {
            return 0;
        }
    }

    public int GetIdleIndexForAngle(float angle)
    {
        for (int i = idle_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == idle_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(idle_Degrees[i - 1], idle_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(idle_Degrees[i], idle_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(idle_Degrees[i], idle_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(idle_Degrees[i - 1], idle_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public Vector3 GetFireSource(float angleORPosition)
    {
        return angleORPosition >= 0f ? spitSourceRight.transform.position : spitSourceLeft.transform.position;
    }

    public IdleSprite GetIdleSprite(int index)
    {
        return new IdleSprite
        {
            Sprite = idle_Sprites[index],
            Degrees = idle_Degrees[index],
            XFlipped = idle_Flip[index]
        };
    }

    public void DoLunge()
    {
        MainRenderer.sprite = lungeSprite;
    }

    public void ToggleLaserBubbles(bool enable)
    {
        if (enable)
        {
            groundLaserParticles.Play();
        }
        else
        {
            groundLaserParticles.Stop();
        }
    }

    public IEnumerator PlayGroundLaserAntic(float delay)
    {

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        MainRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
        for (int i = 0; i < 3; i++)
        {
            MainRenderer.sprite = groundLaserSprites[i];
            transform.localEulerAngles = groundLaserRotations[i];

            if (CurrentOrientation == AspidOrientation.Right)
            {
                 transform.SetZLocalRotation(-transform.GetZLocalRotation());
            }
            yield return new WaitForSeconds(1f / groundLaserFPS);
        }
    }

    public float GroundLaserFireDelay()
    {
        return 3f / groundLaserFPS;
    }

    public float GroundLaserEndDelay()
    {
        return (groundLaserSprites.Count - 6) / groundLaserFPS;
    }

    public IEnumerator PlayGroundLaser(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        MainRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
        for (int i = 3; i < 6; i++)
        {
            MainRenderer.sprite = groundLaserSprites[i];
            transform.localEulerAngles = groundLaserRotations[i];
            if (CurrentOrientation == AspidOrientation.Right)
            {
                 transform.SetZLocalRotation(-transform.GetZLocalRotation());
            }
            yield return new WaitForSeconds(1f / groundLaserFPS);
        }
    }

    public IEnumerator PlayGroundLaserEnding(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        MainRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
        for (int i = 6; i < groundLaserSprites.Count; i++)
        {
            MainRenderer.sprite = groundLaserSprites[i];
            transform.localEulerAngles = groundLaserRotations[i];
            if (CurrentOrientation == AspidOrientation.Right)
            {
                 transform.SetZLocalRotation(-transform.GetZLocalRotation());
            }
            yield return new WaitForSeconds(1f / groundLaserFPS);
        }
    }

    public IEnumerator FireLaser()
    {
        yield break;
    }

    public override void OnStun()
    {
        changingDirection = false;
        ToggleLaserBubbles(false);
        transform.localEulerAngles = default;
         transform.SetLocalPosition(x: 0f, y: 0f);
         groundLaserOrigin.SetXLocalPosition(laserOriginStartX);
        base.OnStun();
    }

    public IEnumerator FireGroundLaser(Quaternion startAngle, Quaternion endAngle, float duration, bool fadeOutAtEnd = false)
    {
         groundLaserEmitter.ChargeUpLaser_P1();
        float fireDelay = GroundLaserFireDelay();
        float endDelay = GroundLaserEndDelay();

        ToggleLaserBubbles(false);
         Boss.StartBoundRoutine(AimLaser(startAngle, endAngle, duration + fireDelay + endDelay));

        yield return PlayGroundLaser(0);
         groundLaserEmitter.FireLaser_P2();
        AudioPlayer laserSound = WeaverAudio.PlayAtPoint(groundLaserSound, transform.position);
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.AverageShake);

        float timer = 0f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            timer += Time.deltaTime;
            if (timer > 0.15f)
            {
                 Blood.SpawnDirectionalBlood(groundLaserOrigin.transform.position, DirectionUtilities.DegreesToDirection(startAngle.eulerAngles.z));
                timer -= 0.15f;
            }
            yield return null;
        }


        float laserEndTime = groundLaserEmitter.EndLaser_P3();

        float oldTime = Time.time;
        if (fadeOutAtEnd)
        {
             StartCoroutine(FadeOutAudio(laserSound, laserEndTime));
        }
        yield return PlayGroundLaserEnding(0);
        float dt = Time.time - oldTime;
        if (laserEndTime - dt > 0)
        {
            yield return new WaitForSeconds(laserEndTime - dt);
        }

    }

    private IEnumerator FadeOutAudio(AudioPlayer audio, float time)
    {
        if (audio == null)
        {
            yield break;
        }
        float oldVolume = audio.Volume;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (audio == null)
            {
                yield break;
            }
            audio.Volume = Mathf.Lerp(oldVolume, 0f, t / time);
            yield return null;
        }
    }

    private IEnumerator AimLaser(Quaternion start, Quaternion end, float time)
    {
        if (CurrentOrientation == AspidOrientation.Right)
        {
             groundLaserOrigin.SetXLocalPosition(laserOriginStartX + 0.5f);
        }
        else
        {
             groundLaserOrigin.SetXLocalPosition(laserOriginStartX - 0.5f);
        }
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        Quaternion ninety = Quaternion.Euler(0f, 0f, 90f);

        float bloodSpawnTimer = 0;

        float startDegrees = start.eulerAngles.z;
        float endDegrees = end.eulerAngles.z;

        BloodSpawnInfo bloodInfo = new BloodSpawnInfo(3, 5, 20f, 30f, Mathf.Min(startDegrees, endDegrees), Mathf.Max(startDegrees, endDegrees), null);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            Quaternion targetAngle = Quaternion.Slerp(start, end, curve.Evaluate(t / time));

            groundLaserOrigin.localRotation = Quaternion.Inverse(transform.rotation) * ninety * targetAngle;

            bloodSpawnTimer += Time.deltaTime;

            if (bloodSpawnTimer >= groundLaserBloodSpawnRate)
            {
                bloodSpawnTimer -= groundLaserBloodSpawnRate;

                 Blood.SpawnBlood(groundLaserOrigin.position, bloodInfo);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
         groundLaserOrigin.SetXLocalPosition(laserOriginStartX);
    }

    public IEnumerator GroundPrepareJump()
    {
        float lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition += Boss.GroundMode.jumpPosIncrements;
            transform.localEulerAngles += Boss.GroundMode.jumpRotIncrements * lookingDirection;
            yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        float lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
            transform.localEulerAngles -= Boss.GroundMode.jumpRotIncrements * lookingDirection;
            if (i != Boss.GroundMode.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
        }
        yield break;
    }

    public IEnumerator GroundLand(bool finalLanding)
    {
        float lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

        transform.localPosition += Boss.GroundMode.jumpPosIncrements * Boss.GroundMode.groundJumpFrames;
        transform.localEulerAngles += Boss.GroundMode.jumpRotIncrements * lookingDirection * Boss.GroundMode.groundJumpFrames;

        yield return new WaitForSeconds(Boss.GroundMode.groundJumpLandDelay);

        if (finalLanding)
        {
            for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
            {
                transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
                transform.localEulerAngles -= Boss.GroundMode.jumpRotIncrements * lookingDirection;
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
            yield break;
        }
    }

    public override IEnumerator WaitTillChangeDirectionMidJump()
    {
        yield break;
    }

    public override IEnumerator MidJumpChangeDirection(AspidOrientation oldOrientation, AspidOrientation newOrientation)
    {
        Animator.PlaybackSpeed = Boss.GroundMode.MidAirSwitchSpeed;
        PreviousOrientation = CurrentOrientation;
        CurrentOrientation = newOrientation;
        MainRenderer.flipX = oldOrientation == AspidOrientation.Right;
        yield return Animator.PlayAnimationTillDone("Change Direction Quick");

        MainRenderer.flipX = newOrientation == AspidOrientation.Right;
        MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip("Change Direction", 0);
        Animator.PlaybackSpeed = 1f;
        LookingDirection = newOrientation == AspidOrientation.Right ? idle_Degrees[idle_Degrees.Count - 1] : idle_Degrees[0];
    }
}

