using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class LaserRapidFireMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;
    [SerializeField]
    [Tooltip("The position the aspid should target relative to the player")]
    Vector2 relativeTargetPos;

    [Space]
    [Header("Preparation")]
    [SerializeField]
    float preparationDelay = 0.5f;

    [SerializeField]
    LaserEmitter prepareLaserEmitter;

    [SerializeField]
    Transform prepareLaserOrigin;

    [SerializeField]
    ParticleSystem chargeUpEffects;

    [SerializeField]
    float chargeUpTime = 1f;

    [SerializeField]
    AudioClip chargeUpSound;

    [Space]
    [Header("Main Firing")]
    [SerializeField]
    float initialFireDelay = 0.2f;

    [SerializeField]
    float subsequentFireDelay = 0.1f;

    [SerializeField]
    LaserEmitter rapidFireEmitter;

    [SerializeField]
    Transform rapidFireRotationOrigin;

    [SerializeField]
    Sprite fireSprite;

    [SerializeField]
    float animationSpeed = 1f;

    [SerializeField]
    List<int> possibleShotAmounts = new List<int>
    {
        4,
        2
    };

    [SerializeField]
    float fireDuration = 0.25f;

    [SerializeField]
    ShakeType cameraShake = ShakeType.AverageShake;

    [SerializeField]
    AudioClip fireSound;

    [SerializeField]
    float volumeDecreaseDelay = 0.1f;

    [SerializeField]
    float volumeDecreaseTime = 0.5f;

    [SerializeField]
    Vector2 firePitch = new Vector2(1f,1f);

    [SerializeField]
    Vector2 angleRange = new Vector2(0,100f);

    [SerializeField]
    int bloodSpawnAmount = 10;

    [SerializeField]
    float playerPredictionAmount = 1.2f;

    [SerializeField]
    float playerAimSpeed = 5f;

    [SerializeField]
    float maximumFlightSpeed = 2f;

    [SerializeField]
    float minimumFlightSpeed = 1.5f;

    [SerializeField]
    float flightSpeed = 10f;



    float prepareLaserXOffset;
    float fireLaserXOffset;


    Transform shotGunTarget;

    FireLaserMove laserMove;

    float originalOriginDistance;

    bool doingShotgun = false;
    LaserShotgunMove shotgunMove;

    Vector3 fireTarget;
    float fireRotation;
    bool cancelled = false;

    bool freezeTarget = false;

    TargetOverride target;

    float prevMaxFlightSpeed;
    float prevMinFlightSpeed;
    float prevFlightSpeed;
    float prevOrbitReductionAmount;

    public override bool MoveEnabled
    {
        get
        {
            return Boss.CanSeeTarget && moveEnabled && Boss.CurrentRunningMode == Boss.TacticalMode;
        }
    }

    public void EnableMove(bool enabled)
    {
        moveEnabled = enabled;
    }

    private void Awake()
    {
        prepareLaserXOffset = prepareLaserOrigin.GetXLocalPosition();
        fireLaserXOffset = rapidFireRotationOrigin.GetXLocalPosition();
        laserMove = GetComponent<FireLaserMove>();
        shotgunMove = GetComponent<LaserShotgunMove>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;

        var firstAngle = MathUtilities.PolarToCartesian(-90f + angleRange.x,10f);
        var secondAngle = MathUtilities.PolarToCartesian(-90f + angleRange.y,10f);

        Gizmos.DrawRay(transform.position, firstAngle);
        Gizmos.DrawRay(transform.position, secondAngle);
    }

    void ResetTarget()
    {
        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }

    }

    void SetLaserTarget()
    {
        if (shotGunTarget == null)
        {
            var target = new GameObject("Laser Target");
            shotGunTarget = target.transform;
        }

        Vector3 targetPos;
        if (Boss.Head.LookingDirection >= 0f)
        {
            targetPos = relativeTargetPos.With(x: -relativeTargetPos.x);
        }
        else
        {
            targetPos = relativeTargetPos;
        }

        if (target == null)
        {
            target = Boss.AddTargetOverride();
        }

        freezeTarget = false;
        Vector3 lastPTarget = Player.Player1.transform.position;

        target.SetTarget(() =>
        {
            if (!freezeTarget)
            {
                lastPTarget = Player.Player1.transform.position;
            }
            return targetPos + lastPTarget;
        });

    }

    protected override IEnumerator OnExecute()
    {
        {
            cancelled = true;

            var camRect = Boss.CamRect;

            Vector2 targetPos;

            if (Boss.Head.LookingDirection >= 0f)
            {
                targetPos = relativeTargetPos.With(x: -relativeTargetPos.x);
            }
            else
            {
                targetPos = relativeTargetPos;
            }

            var pos = (Vector2)Player.Player1.transform.position + targetPos;

            var initialDirection = (pos - (Vector2)Boss.Head.transform.position).normalized;

            for (int i = 0; i <= 10; i++)
            {
                var pointToCheck = (Vector2)Boss.Head.transform.position + (initialDirection * i);
                if (camRect.Contains(pointToCheck))
                {
                    cancelled = false;
                    break;
                }
            }

            if (cancelled)
            {
                yield break;
            }
        }
        var currentPhase = Boss.CurrentPhase;

        doingShotgun = false;
        prevOrbitReductionAmount = Boss.OrbitReductionAmount;
        Boss.OrbitReductionAmount *= 3f;
        yield return Boss.Head.LockHead();

        SetLaserTarget();
        prevMaxFlightSpeed = Boss.maximumFlightSpeed;
        Boss.maximumFlightSpeed = maximumFlightSpeed;

        prevFlightSpeed = Boss.flightSpeed;
        Boss.flightSpeed = flightSpeed;

        prevMinFlightSpeed = Boss.minimumFlightSpeed;
        Boss.minimumFlightSpeed = minimumFlightSpeed;
        yield return new WaitForSeconds(preparationDelay);

        prepareLaserOrigin.SetXLocalPosition(Boss.Head.LookingDirection >= 0f ? -prepareLaserXOffset : prepareLaserXOffset);

        chargeUpEffects.Play();

        if (chargeUpSound != null)
        {
            var audio = WeaverAudio.PlayAtPoint(chargeUpSound, transform.position);
            audio.transform.SetParent(transform, true);
        }

        prepareLaserEmitter.ChargeUpDuration = chargeUpTime;

        IEnumerator ChargeUpLaser()
        {
            yield return prepareLaserEmitter.PlayChargeUpInRoutine(chargeUpTime);
        }

        Vector2 angleLimits = angleRange;

        if (Boss.Head.LookingDirection < 0f)
        {
            angleLimits = new Vector2(-angleLimits.x,-angleLimits.y);
        }

        originalOriginDistance = laserMove.Emitter.Laser.transform.GetYLocalPosition();
        uint chargeUpInRoutine = Boss.StartBoundRoutine(ChargeUpLaser());

        for (float t = 0; t < chargeUpTime; t += Time.deltaTime)
        {
            var playerAngle = ClampWithinRange(MathUtilities.ClampRotation(GetAngleToPlayer(prepareLaserOrigin) + 90f),angleLimits.x,angleLimits.y);
            var destRotation = Quaternion.Euler(0f, 0f,playerAngle - 90f);

            SetPrepareLaserAngle(playerAngle);

            chargeUpEffects.transform.localRotation = Quaternion.Euler(playerAngle - 90f, -90f, 0f);      
            if (Cancelled)
            {
                break;
            }
            yield return null;
        }

        Boss.StopBoundRoutine(chargeUpInRoutine);
        chargeUpInRoutine = Boss.StartBoundRoutine(prepareLaserEmitter.PlayChargeUpOutRoutine());


        chargeUpEffects.Stop();

        yield return new WaitForSeconds(initialFireDelay);

        if (Cancelled)
        {
            Cancel();
            yield break;
        }

        rapidFireRotationOrigin.SetXLocalPosition(Boss.Head.LookingDirection >= 0f ? -fireLaserXOffset : fireLaserXOffset);

        Boss.Head.Animator.PlaybackSpeed = animationSpeed;
        var clipDuration = Boss.Head.Animator.AnimationData.GetClipDuration("Fire Laser Antic Quick");

        var shots = possibleShotAmounts.GetRandomElement();

        var health = Boss.HealthComponent.Health;

        for (int i = 0; i < shots; i++)
        {
            if (Cancelled || Boss.CurrentPhase != currentPhase || Vector3.Distance(transform.position, Player.Player1.transform.position) > 28f)
            {
                break;
            }

            rapidFireEmitter.FireDuration = fireDuration;
            Boss.StartBoundRoutine(FireLaser(clipDuration - rapidFireEmitter.ChargeUpDuration,fireDuration));

            yield return null;
            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

            Boss.Head.MainRenderer.sprite = fireSprite;
            rapidFireEmitter.FireLaser_P2();

            if (fireSound != null)
            {
                StartCoroutine(PlayFireSound(fireSound,rapidFireRotationOrigin.position));

                PlayBloodEffects(bloodSpawnAmount, rapidFireEmitter);
            }

            CameraShaker.Instance.Shake(cameraShake);

            yield return new WaitForSeconds(fireDuration);

            rapidFireEmitter.EndLaser_P3();

            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

            yield return new WaitUntil(() => !rapidFireEmitter.FiringLaser);

            if (i >= 1 && Boss.HealthManager.Health < health || Cancelled)
            {
                break;
            }
        }

        Boss.Head.Animator.PlaybackSpeed = 1f;


        Boss.Head.Animator.PlaybackSpeed = 1f;

        freezeTarget = false;

        if (!Cancelled && currentPhase == Boss.CurrentPhase && Vector3.Distance(transform.position, Player.Player1.transform.position) <= 28f)
        {
            doingShotgun = true;

            yield return shotgunMove.DoMove();

            doingShotgun = false;
        }
        ResetTarget();

        Boss.OrbitReductionAmount = prevOrbitReductionAmount;
        Boss.maximumFlightSpeed = prevMaxFlightSpeed;
        Boss.flightSpeed = prevFlightSpeed;
        Boss.Head.UnlockHead();

    }

    void Cancel()
    {
        Boss.Head.Animator.PlaybackSpeed = 1f;
        freezeTarget = false;

        ResetTarget();

        Boss.OrbitReductionAmount = prevOrbitReductionAmount;
        Boss.maximumFlightSpeed = prevMaxFlightSpeed;
        Boss.flightSpeed = prevFlightSpeed;
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }

    IEnumerator FireLaser(float delay, float duration)
    {
        double oldRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, Player.Player1.transform.position) + 90f);
        yield return null;
        double newRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, Player.Player1.transform.position) + 90f);
        float difference = (float)((newRotation - oldRotation) / (double)Time.deltaTime);

        var predictedRotation = Mathf.LerpUnclamped((float)oldRotation,(float)oldRotation + difference,playerPredictionAmount);

        fireRotation = predictedRotation;

        yield return new WaitForSeconds(delay);



        rapidFireEmitter.ChargeUpLaser_P1();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            AimAtTarget();
            yield return null;
        }
    }

    void AimAtTarget()
    {
        var newRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, Player.Player1.transform.position) + 90f);
        fireRotation = Mathf.Lerp(fireRotation,newRotation,playerAimSpeed * Time.deltaTime);
        rapidFireRotationOrigin.transform.SetZLocalRotation(fireRotation);
    }

    IEnumerator PlayFireSound(AudioClip clip, Vector3 position)
    {
        var audio = WeaverAudio.PlayAtPoint(clip, position);
        audio.AudioSource.pitch = firePitch.RandomInRange();
        yield return new WaitForSeconds(volumeDecreaseDelay);
        for (float t = 0; t < volumeDecreaseTime; t += Time.deltaTime)
        {
            audio.AudioSource.volume = 1f - (t / volumeDecreaseTime);
            yield return null;
        }
    }

    public override float GetPostDelay(int prevHealth) => 0;

    float ClampWithinRange(float angle, float a, float b)
    {
        if (a > b)
        {
            var temp = b;
            b = a;
            a = temp;
        }
        if (angle < a)
        {
            return a;
        }
        else if (angle > b)
        {
            return b;
        }
        else
        {
            return angle;
        }
    }

    public override void OnStun()
    {
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }

        Boss.maximumFlightSpeed = prevMaxFlightSpeed;
        Boss.flightSpeed = prevFlightSpeed;
        Boss.minimumFlightSpeed = prevMinFlightSpeed;

        Boss.Head.Animator.StopCurrentAnimation();

        if (doingShotgun)
        {
            doingShotgun = false;
            shotgunMove.OnStun();

            Boss.OrbitReductionAmount /= 3f;
            ResetTarget();
        }
        else
        {
            laserMove.Emitter.Laser.transform.SetYLocalPosition(originalOriginDistance);
            chargeUpEffects.Stop();
            laserMove.Emitter.StopLaser();
            Boss.Head.Animator.PlaybackSpeed = 1f;
        }
    }

    void SetPrepareLaserAngle(float downwardAngle)
    {
        prepareLaserOrigin.SetZLocalRotation(downwardAngle);
    }

    float GetAngleToPlayer(Component relativeComponent)
    {
        return GetAngleToPlayer(relativeComponent, Player.Player1.transform.position);
    }

    float GetAngleToPlayer(Component relativeComponent, Vector3 playerPos)
    {
        return MathUtilities.CartesianToPolar(playerPos - relativeComponent.transform.position).x;
    }

    public Vector2 GetMiddleBeamContact(LaserEmitter emitter)
    {
        var contacts = emitter.Laser.ColliderContactPoints;

        var contactPoint = (Vector2)emitter.Laser.transform.TransformPoint(contacts[contacts.Count / 2]);

        var offsetVector = ((Vector2)emitter.Laser.transform.position - contactPoint).normalized;

        return contactPoint + (offsetVector * 0.3f);
    }

    public void PlayBloodEffects(int amount, LaserEmitter emitter)
    {
        PlayBloodEffects(GetMiddleBeamContact(emitter), amount);
    }

    public void PlayBloodEffects(Vector2 pos, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Blood.SpawnRandomBlood(pos);
        }
    }

    public override void StopMove()
    {
        base.StopMove();
        if (doingShotgun)
        {
            shotgunMove.StopMove();
        }
    }
}
