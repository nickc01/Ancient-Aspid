using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
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
    bool lookAtPlayer = true;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_Sprites")]
    List<Sprite> idle_Sprites;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_HorizFlip")]
    List<bool> idle_Flip;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_Degrees")]
    List<float> idle_Degrees;

    [Header("Lunge")]
    [SerializeField]
    Sprite lungeSprite;

    bool turningTowardsPlayer = false;
    float currentHeadAngle = -60f;

    [SerializeField]
    List<Sprite> landSprites;

    [SerializeField]
    List<Vector2> landPositions;

    [SerializeField]
    float landFPS;

    [Header("Ground Laser")]
    [SerializeField]
    List<Sprite> groundLaserSprites;

    //[SerializeField]
    //List<Vector3> groundLaserPositions;

    [SerializeField]
    List<Vector3> groundLaserRotations;

    [SerializeField]
    float groundLaserFPS = 12;

    [SerializeField]
    ParticleSystem groundLaserParticles;

    [SerializeField]
    LaserEmitter groundLaserEmitter;

    [SerializeField]
    Transform groundLaserOrigin;

    [SerializeField]
    AudioClip groundLaserSound;

    [SerializeField]
    GameObject spitSourceLeft;

    [SerializeField]
    GameObject spitSourceRight;

    [SerializeField]
    float groundLaserBloodSpawnRate = 0.15f;

    float laserOriginStartX;

    public float GlobalLockSpeedMultiplier { get; set; } = 1f;


    /// <summary>
    /// The current direction the head is looking in.
    /// 
    /// If the value is -60, the head is looking left
    /// If the value is 60, the head is looking right
    /// If the value is 0, the head is looking center
    /// </summary>
    public float LookingDirection => currentHeadAngle;

    public bool HeadLocked { get; private set; } = false;
    //public bool CanLockHead { get; private set; } = true;
    //public bool CanUnlockHead { get; private set; } = false;

    //public bool HeadBeingUnlocked => unlockRoutine != 0;

    public float LaserChargeUpDuration => groundLaserEmitter.ChargeUpDuration;

    uint unlockRoutine = 0;
    uint lockRoutine = 0;

    bool doUpdate = false;

    protected override void Awake()
    {
        base.Awake();
        transform.SetLocalPosition(x: 0f, y: 0f);
        MainRenderer.sprite = idle_Sprites[0];
        laserOriginStartX = groundLaserOrigin.GetXLocalPosition();
    }

    private void Update()
    {
        if (turningTowardsPlayer && doUpdate)
        {
            var angleToPlayer = GetDownwardAngleToPlayer();
            if (angleToPlayer >= currentHeadAngle)
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, 0f, 120f * Time.deltaTime);
            }
            else
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, -120f * Time.deltaTime, 0f);
            }

            //Debug.DrawRay(transform.position,MathUtilities.PolarToCartesian(new Vector2(currentHeadAngle - 90f,5f)),Color.red);

            var index = GetIdleIndexForAngle(currentHeadAngle);

            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
        }
    }

    public override float GetChangeDirectionTime(float speedMultiplier = 1f)
    {
        var time = base.GetChangeDirectionTime();

        if (lookAtPlayer)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                //yield return StartFollowingPlayer();
                time += (1f / (DEFAULT_FPS * speedMultiplier)) * DEFAULT_CENTERIZE_FRAMES;
            }
            else if (CurrentOrientation != AspidOrientation.Center && PreviousOrientation == AspidOrientation.Center)
            {
                //yield return StopFollowingPlayer();
                time += (1f / (DEFAULT_FPS * speedMultiplier)) * DEFAULT_CENTERIZE_FRAMES;
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
        if (lookAtPlayer)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                yield return StartFollowingPlayer(speedMultiplier);
                yield break;
            }
            else if (CurrentOrientation != AspidOrientation.Center && PreviousOrientation == AspidOrientation.Center)
            {
                yield return StopFollowingPlayer(speedMultiplier);
                yield break;
            }
        }

        currentHeadAngle = OrientationToAngle(CurrentOrientation);

        yield return base.ChangeDirectionRoutine(speedMultiplier);
    }

    IEnumerator StartFollowingPlayer(float speedMultiplier)
    {
        MainRenderer.flipX = false;
        StartCoroutine(UpdateColliderOffset(DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES));

        yield return InterpolateToFollowPlayer(DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * speedMultiplier);
        turningTowardsPlayer = true;
    }

    IEnumerator StopFollowingPlayer(float speedMultiplier)
    {
        StartCoroutine(UpdateColliderOffset(DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES));
        yield return InterpolateToNewAngle(currentHeadAngle, () => OrientationToAngle(CurrentOrientation), DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * speedMultiplier);

        MainRenderer.sprite = idle_Sprites[0];
        MainRenderer.flipX = CurrentOrientation == AspidOrientation.Right;

        turningTowardsPlayer = false;
    }

    IEnumerator InterpolateToFollowPlayer(float increments, float incrementsPerSecond)
    {
        return InterpolateToNewAngle(OrientationToAngle(CurrentOrientation), GetDownwardAngleToPlayer, increments, incrementsPerSecond);
    }

    IEnumerator InterpolateToNewAngle(float oldAngle, Func<float> destAngle, float increments, float incrementsPerSecond)
    {
        for (int i = 1; i <= increments; i++)
        {
            currentHeadAngle = Mathf.Lerp(oldAngle, destAngle(), i / (float)increments);
            int index = GetIdleIndexForAngle(currentHeadAngle);
            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
            if (i != increments)
            {
                yield return new WaitForSeconds(1f / incrementsPerSecond);
            }
        }
    }

    public IEnumerator LockHead(float lockSpeed = 1f)
    {
        return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left,lockSpeed);
    }

    bool switchingDirections = false;

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        switchingDirections = true;
        PreviousOrientation = oldDirection;
        CurrentOrientation = newDirection;

        //float trueNewDirection = orientation == AspidOrientation.Left ? -60f : 60f;
        //var oldIndex = GetIdleIndexForAngle(oldHeadDirection);
        //var newIndex = GetIdleIndexForAngle(trueNewDirection);
        float oldDirectionAngle = oldDirection == AspidOrientation.Left ? -60f : 60f;
        float newDirectionAngle = newDirection == AspidOrientation.Left ? -60f : 60f;


        var changeDirectionRoutine = base.ChangeDirectionRoutine(Boss.GroundMode.lungeTurnaroundSpeed);

        var interpolationRoutine = InterpolateToNewAngle(oldDirectionAngle, () => newDirectionAngle, DEFAULT_CHANGE_DIR_FRAMES, DEFAULT_FPS * Boss.GroundMode.lungeTurnaroundSpeed);

        yield return RoutineAwaiter.AwaitBoundRoutines(Boss, changeDirectionRoutine, interpolationRoutine).WaitTillDone();

        var idleIndex = GetIdleIndexForAngle(OrientationToAngle(newDirection));
        MainRenderer.sprite = idle_Sprites[idleIndex];
        MainRenderer.flipX = idle_Flip[idleIndex];
        //yield return ChangeDirection(newDirection, Boss.lungeTurnaroundSpeed);
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

    /// <summary>
    /// Locks the head so other scripts can play animations and other effects on it. Make sure to call <see cref="UnlockHead"/> when done
    /// 
    /// NOTE: It isn't guaranteed that the head will be in the direction you intend if it's not possible to do so. Always use <see cref="LookingDirection"/> after calling this
    /// 
    /// In Offensive Mode, you can pick whatever intendedDirection you want, but in defensive mode, the orientation can't be chosen, and it can be either left or right
    /// </summary>
    public IEnumerator LockHead(AspidOrientation intendedOrientation, float lockSpeed = 1f)
    {
        return LockHead(OrientationToAngle(intendedOrientation),lockSpeed);
        /*if (headLocked)
        {
            throw new Exception("The head is already locked");
        }

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            currentlyLookingAtPlayer = false;
            yield return InterpolateToNewAngle(currentHeadAngle, () => OrientationToAngle(intendedOrientation), DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS);
        }
        headLocked = true;*/
    }

    public IEnumerator QuickFlipDirection(bool faceRight, float flipTime = 1f / 12f)
    {
        if (MainRenderer.flipX != faceRight)
        {
            MainRenderer.sprite = idle_Sprites[idle_Sprites.Count / 2];
            MainRenderer.flipX = faceRight;

            currentHeadAngle = OrientationToAngle(faceRight ? AspidOrientation.Right : AspidOrientation.Left);

            yield return new WaitForSeconds(flipTime);
        }
    }

    public IEnumerator LockHead(float intendedDirection, float lockSpeed = 1f)
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
                yield return InterpolateToNewAngle(currentHeadAngle, () => intendedDirection, DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * lockSpeed * GlobalLockSpeedMultiplier);
                lockRoutine = 0;
            }

            lockRoutine = Boss.StartBoundRoutine(Lock());
            yield return new WaitUntil(() => lockRoutine == 0);
        }
    }

    /// <summary>
    /// Unlocks the head and allows its default animations to resume
    /// </summary>
    public void UnlockHead()
    {
        UnlockHead(currentHeadAngle);
    }

    /// <summary>
    /// Unlocks the head and allows its default animations to resume. 
    /// 
    /// This variant allows you to set the new direction the head should be facing
    /// 
    /// For example, if you have a move finish with the head facing to the right, set the angle to 60 degrees.
    /// 
    /// If it finishes facing left, set the angle to -60
    /// 
    /// If it finishes facing towards the center, set the angle to 0
    /// </summary>
    public void UnlockHead(float newHeadDirection)
    {
        /*if (unlockRoutine != 0)
        {
            throw new Exception("The head is already being unlocked");
        }*/
        if (!HeadLocked)
        {
            throw new Exception("The head is already unlocked");
        }

        if (lockRoutine != 0)
        {
            Boss.StopBoundRoutine(lockRoutine);
            lockRoutine = 0;
        }

        //CanUnlockHead = false;
        HeadLocked = false;
        //CanLockHead = true;

        currentHeadAngle = newHeadDirection;

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

    IEnumerator UnlockTowardsDirection(float oldHeadDirection, AspidOrientation orientation)
    {
        float trueNewDirection = orientation == AspidOrientation.Left ? -60f : 60f;
        var oldIndex = GetIdleIndexForAngle(oldHeadDirection);
        var newIndex = GetIdleIndexForAngle(trueNewDirection);

        //yield return InterpolateToNewAngle(oldHeadDirection, () => trueNewDirection, Math.Abs(newIndex - oldIndex) + 1, DEFAULT_FPS);

        var increments = Math.Abs(newIndex - oldIndex) + 1;

        currentHeadAngle = trueNewDirection;

        for (int i = 1; i <= increments; i++)
        {
            var angle = Mathf.Lerp(oldHeadDirection, trueNewDirection, i / (float)increments);
            int index = GetIdleIndexForAngle(angle);
            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
            if (i != increments)
            {
                yield return new WaitForSeconds(1f / DEFAULT_FPS);
            }
        }

        MainRenderer.sprite = idle_Sprites[0];
        MainRenderer.flipX = orientation == AspidOrientation.Right;

        unlockRoutine = 0;
        doUpdate = true;
    }

    public void UnlockHead(AspidOrientation newHeadOrientation)
    {
        UnlockHead(OrientationToAngle(newHeadOrientation));
    }

    /// <summary>
    /// Returns the downward angle to the player. 90* means the player is to the right of the boss. 0 means the player is above or below the boss. -90* means the player is to the left of the boss
    /// </summary>
    public float GetDownwardAngleToPlayer()
    {
        return GetDownwardAngleToPosition(Player.Player1.transform.position);
        //return 90f * Vector2.Dot(Vector2.right,Player.Player1.transform.position - transform.position);
    }

    public float GetDownwardAngleToPosition(Vector3 pos)
    {
        return 90f * Vector2.Dot(Vector2.right, (pos - transform.position).normalized);
    }

    float OrientationToAngle(AspidOrientation orientation)
    {
        switch (orientation)
        {
            case AspidOrientation.Left:
                return idle_Degrees[0];
            case AspidOrientation.Right:
                return idle_Degrees[idle_Degrees.Count - 1];
            default:
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
        if (angleORPosition >= 0f)
        {
            return spitSourceRight.transform.position;
        }
        else
        {
            return spitSourceLeft.transform.position;
        }
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
        ToggleLaserBubbles(false);
        transform.localEulerAngles = default;
        transform.SetLocalPosition(x: 0f, y: 0f);
        groundLaserOrigin.SetXLocalPosition(laserOriginStartX);
        base.OnStun();
    }

    public IEnumerator FireGroundLaser(Quaternion startAngle, Quaternion endAngle, float duration, bool fadeOutAtEnd = false)
    {
        //TODO TODO - PLAY HEAD ANIMATIONS
        groundLaserEmitter.ChargeUpLaser_P1();
        var fireDelay = GroundLaserFireDelay();
        var endDelay = GroundLaserEndDelay();

        ToggleLaserBubbles(false);
        //var endDuration = groundLaserEmitter.EndDuration;

        Boss.StartBoundRoutine(AimLaser(startAngle, endAngle, duration + fireDelay + endDelay));

        yield return PlayGroundLaser(0);
        //yield return new WaitForSeconds(chargeDuration);

        groundLaserEmitter.FireLaser_P2();
        var laserSound = WeaverAudio.PlayAtPoint(groundLaserSound, transform.position);
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.AverageShake);

        //yield return new WaitForSeconds(duration);
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


        var laserEndTime = groundLaserEmitter.EndLaser_P3();

        var oldTime = Time.time;
        //yield return new WaitForSeconds(endDelay);
        if (fadeOutAtEnd)
        {
            StartCoroutine(FadeOutAudio(laserSound, laserEndTime));
        }
        yield return PlayGroundLaserEnding(0);
        var dt = Time.time - oldTime;
        if (laserEndTime - dt > 0)
        {
            yield return new WaitForSeconds(laserEndTime - dt);
        }

    }

    IEnumerator FadeOutAudio(AudioPlayer audio, float time)
    {
        if (audio == null)
        {
            yield break;
        }
        var oldVolume = audio.Volume;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (audio == null)
            {
                yield break;
            }
            audio.Volume = Mathf.Lerp(oldVolume,0f,t / time);
            yield return null;
        }
    }

    IEnumerator AimLaser(Quaternion start, Quaternion end, float time)
    {
        if (CurrentOrientation == AspidOrientation.Right)
        {
            //groundLaserOrigin.Translate(0.5f,0f,0f);
            groundLaserOrigin.SetXLocalPosition(laserOriginStartX + 0.5f);
        }
        else
        {
            groundLaserOrigin.SetXLocalPosition(laserOriginStartX - 0.5f);
            //groundLaserOrigin.Translate(0.5f, 0f, 0f);
        }
        var curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        var ninety = Quaternion.Euler(0f,0f,90f);

        float bloodSpawnTimer = 0;

        var startDegrees = start.eulerAngles.z;
        var endDegrees = end.eulerAngles.z;

        /*var bloodInfo = new Blood.BloodSpawnInfo
        {
            AngleMin = Mathf.Min(startDegrees, endDegrees),
            AngleMax = Mathf.Max(startDegrees,endDegrees),
            
        }*/

        var bloodInfo = new BloodSpawnInfo(3, 5, 20f, 30f, Mathf.Min(startDegrees, endDegrees), Mathf.Max(startDegrees, endDegrees), null);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            var targetAngle = Quaternion.Slerp(start,end,curve.Evaluate(t / time));

            groundLaserOrigin.localRotation = Quaternion.Inverse(transform.rotation) * ninety * targetAngle;

            bloodSpawnTimer += Time.deltaTime;

            if (bloodSpawnTimer >= groundLaserBloodSpawnRate)
            {
                bloodSpawnTimer -= groundLaserBloodSpawnRate;

                Blood.SpawnBlood(groundLaserOrigin.position, bloodInfo);
                //Blood.SpawnDirectionalBlood(groundLaserOrigin.position, CurrentOrientation == AspidOrientation.Right ? WeaverCore.Enums.CardinalDirection.Right : WeaverCore.Enums.CardinalDirection.Left);
                //Blood.Spawn
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        groundLaserOrigin.SetXLocalPosition(laserOriginStartX);
    }

    public IEnumerator GroundPrepareJump()
    {
        var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

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
        var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;
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
        var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

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
        currentHeadAngle = newOrientation == AspidOrientation.Right ? idle_Degrees[idle_Degrees.Count - 1] : idle_Degrees[0];
    }
}

/*public class HeadControllerOLD : AspidBodyPartOLD
{
    public struct IdleSprite
    {
        public Sprite Sprite;
        public bool XFlipped;
        public float Degrees;
    }

    [SerializeField]
    [Tooltip("When the boss is in offensive mode, the head will look towards the player if this is set to true")]
    bool lookAtPlayer = true;

    public bool FollowPlayer { get; set; } = false;

    [SerializeField]
    [Tooltip("How fast the head of the boss moves when FollowPlayer is set to true")]
    float headMovementSpeed = 5f;

    bool changingSettings = false;

    WeaverAnimationData.Clip changeDirectionClip;
    WeaverAnimationData.Clip centerizeClip;
    WeaverAnimationData.Clip decenterizeClip;

    [SerializeField]
    List<Sprite> followPlayer_Sprites;

    [SerializeField]
    List<bool> followPlayer_HorizFlip;

    [SerializeField]
    List<float> followPlayer_Degrees;

    public bool HeadFacingRight
    {
        get
        {
            if (FollowPlayer)
            {
                return currentHeadAngle >= 0f;
            }
            else
            {
                return Orientation == AspidOrientation.Right;
            }
        }
    }

    public IEnumerable<IdleSprite> IdleSprites
    {
        get
        {
            for (int i = 0; i < followPlayer_Sprites.Count; i++)
            {
                yield return new IdleSprite
                {
                    Sprite = followPlayer_Sprites[i],
                    Degrees = followPlayer_Degrees[i],
                    XFlipped = followPlayer_HorizFlip[i]
                };
            }
        }
    }

    public int IdleSpriteCount => followPlayer_Sprites.Count;

    public SpriteRenderer MainRenderer => AnimationPlayer.SpriteRenderer;

    float currentHeadAngle;


    protected override void Awake()
    {
        base.Awake();
        changeDirectionClip = AnimationPlayer.AnimationData.GetClip("Change Direction");
        centerizeClip = AnimationPlayer.AnimationData.GetClip("Centerize");
        decenterizeClip = AnimationPlayer.AnimationData.GetClip("Decenterize");
    }

    private void Update()
    {
        if (FollowPlayer)
        {
            var angleToPlayer = GetDownwardAngleToPlayer();
            if (angleToPlayer >= currentHeadAngle)
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, 0f, headMovementSpeed * Time.deltaTime);
            }
            else
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, -headMovementSpeed * Time.deltaTime, 0f);
            }

            var index = GetFollowPlayerIndexForAngle(currentHeadAngle);

            AnimationPlayer.SpriteRenderer.sprite = followPlayer_Sprites[index];
            AnimationPlayer.SpriteRenderer.flipX = followPlayer_HorizFlip[index];
        }
    }

    /// <summary>
    /// Disables the head from automatically looking towards the player, and forces it to look at a specific direction
    /// 
    /// The facing angle determines where the head should look. 
    /// 
    /// Only works if the Boss's Orientation is set to Center
    /// 
    /// Example angles:
    /// 
    /// Any angle less than or equal to -60 degrees will make the head face left
    /// Any angle greater than or equal to 60 degrees will make the head face right
    /// An angle equal to 0 degrees will make the head face center
    /// </summary>
    public IEnumerator DisableFollowPlayer(float facingAngle, float speed = 1f)
    {
        if (Orientation == AspidOrientation.Center)
        {
            FollowPlayer = false;
            yield return InterpolateToNewAngle(currentHeadAngle, () => facingAngle, Mathf.RoundToInt(Mathf.Abs(facingAngle - currentHeadAngle) / 30f), 1f / (centerizeClip.FPS * speed));
            currentHeadAngle = facingAngle;
        }
    }

    float GetHeadAngle()
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

    public IEnumerator DisableFollowPlayer(float speed = 1f)
    {
        return DisableFollowPlayer(GetHeadAngle(), speed);
    }

    public IEnumerator DisableFollowPlayer(bool headFacesRight, float speed = 1f)
    {
        return DisableFollowPlayer(headFacesRight ? 60f : -60f,speed);
    }

    public void EnableFollowPlayer()
    {
        if (Orientation == AspidOrientation.Center)
        {
            FollowPlayer = true;
        }
    }



    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        if (lookAtPlayer)
        {
            if (newOrientation == AspidOrientation.Center)
            {
                yield return FollowPlayerEnable();
                yield break;
            }
            else if (newOrientation != AspidOrientation.Center && Orientation == AspidOrientation.Center)
            {
                yield return FollowPlayerDisable(newOrientation);
                yield break;
            }
        }
        if (changingSettings)
        {
            throw new System.Exception("Attempting to change head settings while it's already being changed");
        }
        changingSettings = true;
        IEnumerator ApplyClip(WeaverAnimationData.Clip clip)
        {
            StartCoroutine(ChangeXPosition(GetDestinationLocalX(Orientation), GetDestinationLocalX(newOrientation), clip));
            StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, Orientation), GetDestinationLocalX(StartingColliderOffset, newOrientation), clip));
            yield return AnimationPlayer.PlayAnimationTillDone(clip.Name);
        }

        if (newOrientation != AspidOrientation.Center)
        {
            if (Orientation == AspidOrientation.Center)
            {
                if (newOrientation == AspidOrientation.Left)
                {
                    AnimationPlayer.SpriteRenderer.flipX = false;
                }
                else
                {
                    AnimationPlayer.SpriteRenderer.flipX = true;
                }
                yield return ApplyClip(decenterizeClip);
            }
            else
            {
                Sprite initialFrame = AnimationPlayer.SpriteRenderer.sprite;
                yield return ApplyClip(changeDirectionClip);
                AnimationPlayer.SpriteRenderer.flipX = newOrientation == AspidOrientation.Right;

                AnimationPlayer.SpriteRenderer.sprite = initialFrame;
            }
        }
        else
        {
            yield return ApplyClip(centerizeClip);
        }
        changingSettings = false;
    }

    public IEnumerator FollowPlayerEnable()
    {

        if (changingSettings)
        {
            throw new System.Exception("Attempting to change head settings while it's already being changed");
        }
        changingSettings = true;

        //TODO TODo TODO 

        //ALSO MAKE SURE SPRITE LISTS GET FILLED OUT

        AnimationPlayer.SpriteRenderer.flipX = false;
        StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, Orientation), GetDestinationLocalX(StartingColliderOffset, AspidOrientation.Center), centerizeClip));
        yield return InterpolateToFollowPlayer(OrientationToAngle(Orientation), centerizeClip.Frames.Count, 1f / centerizeClip.FPS);
        currentHeadAngle = GetDownwardAngleToPlayer();

        FollowPlayer = true;
        changingSettings = false;
    }

    public IEnumerator FollowPlayerDisable(AspidOrientation newOrientation)
    {
        if (changingSettings)
        {
            throw new System.Exception("Attempting to change head settings while it's already being changed");
        }
        changingSettings = true;

        //TODO TODO TODO
        StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, AspidOrientation.Center), GetDestinationLocalX(StartingColliderOffset, newOrientation), centerizeClip));
        yield return InterpolateToNewAngle(currentHeadAngle, () => OrientationToAngle(newOrientation), centerizeClip.Frames.Count, 1f / centerizeClip.FPS);

        AnimationPlayer.SpriteRenderer.sprite = followPlayer_Sprites[0];
        AnimationPlayer.SpriteRenderer.flipX = newOrientation == AspidOrientation.Right;

        FollowPlayer = false;
        changingSettings = false;
    }

    int GetCurrentFollowPlayerIndex()
    {
        var currentSprite = AnimationPlayer.SpriteRenderer.sprite;
        var xflipped = AnimationPlayer.SpriteRenderer.flipX;
        for (int i = followPlayer_Sprites.Count - 1; i >= 0; i--)
        {
            if (followPlayer_Sprites[i].name == currentSprite.name && followPlayer_HorizFlip[i] == xflipped)
            {
                return i;
            }
        }
        return -1;
    }

    IEnumerator InterpolateToFollowPlayer(float oldAngle, float incrementCount, float secondsPerIncrement)
    {
        return InterpolateToNewAngle(oldAngle, GetDownwardAngleToPlayer, incrementCount, secondsPerIncrement);
    }

    IEnumerator InterpolateToNewAngle(float oldAngle, Func<float> destAngle, float incrementCount, float secondsPerIncrement)
    {
        for (int i = 1; i <= incrementCount; i++)
        {
            float newAngle = Mathf.Lerp(oldAngle, destAngle(), i / (float)incrementCount);
            int index = GetFollowPlayerIndexForAngle(newAngle);
            AnimationPlayer.SpriteRenderer.sprite = followPlayer_Sprites[index];
            AnimationPlayer.SpriteRenderer.flipX = followPlayer_HorizFlip[index];
            if (i != incrementCount)
            {
                yield return new WaitForSeconds(secondsPerIncrement);
            }
        }
    }

    /// <summary>
    /// Returns the downward angle to the player. 90* means the player is to the right of the boss. 0 means the player is above or below the boss. -90* means the player is to the left of the boss
    /// </summary>
    float GetDownwardAngleToPlayer()
    {
        return GetDownwardAngleToPosition(Player.Player1.transform.position);
        //return 90f * Vector2.Dot(Vector2.right,Player.Player1.transform.position - transform.position);
    }

    float GetDownwardAngleToPosition(Vector3 pos)
    {
        return 90f * Vector2.Dot(Vector2.right, (pos - transform.position).normalized);
    }

    float OrientationToAngle(AspidOrientation orientation)
    {
        switch (orientation)
        {
            case AspidOrientation.Left:
                return followPlayer_Degrees[0];
            case AspidOrientation.Right:
                return followPlayer_Degrees[followPlayer_Degrees.Count - 1];
            default:
                return 0;
        }
    }

    int GetFollowPlayerIndexForAngle(float angle)
    {
        for (int i = followPlayer_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == followPlayer_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(followPlayer_Degrees[i - 1], followPlayer_Degrees[i],0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(followPlayer_Degrees[i], followPlayer_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(followPlayer_Degrees[i], followPlayer_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(followPlayer_Degrees[i - 1], followPlayer_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public IdleSprite GetIdleSprite(int index)
    {
        return new IdleSprite
        {
            Sprite = followPlayer_Sprites[index],
            Degrees = followPlayer_Degrees[index],
            XFlipped = followPlayer_HorizFlip[index]
        };
    }
}*/
