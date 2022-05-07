using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class LaserShotgunMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.5f;

    [SerializeField]
    Vector2 relativeTargetPos;

    [Space]
    [Header("Preparation")]
    [SerializeField]
    float preparationDelay = 0.5f;

    [SerializeField]
    LaserEmitter prepareShotGunEmitter;

    [SerializeField]
    Transform prepareShotgunOrigin;

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
    LaserEmitter shotgunFireEmitter;

    [SerializeField]
    Transform shotgunFireRotationOrigin;

    [SerializeField]
    Sprite fireSprite;

    [SerializeField]
    float phase1AnimationSpeed = 1f;

    [SerializeField]
    int phase1Shots = 4;

    [SerializeField]
    float phase1FireDuration = 0.25f;

    [SerializeField]
    ShakeType phase1CameraShake = ShakeType.AverageShake;

    [SerializeField]
    AudioClip phase1FireSound;

    [SerializeField]
    bool doPhase1VolumeAdjust = true;

    [SerializeField]
    float volumeDecreaseDelay = 0.1f;

    [SerializeField]
    float volumeDecreaseTime = 0.5f;

    [SerializeField]
    Vector2 phase1FirePitch = new Vector2(1f,1f);

    [SerializeField]
    Vector2 angleRange = new Vector2(0,100f);

    [SerializeField]
    float headRotationAmount = 20f;

    [SerializeField]
    float headRotationFPS = 12f;

    [SerializeField]
    int headRotationFrames = 3;

    [SerializeField]
    float laserOriginDistance = 0.458f;

    [SerializeField]
    float laserMainOffset = 0f;



    float prepareShotgunXOffset;
    float fireShotgunXOffset;


    Transform shotGunTarget;

    Transform oldTarget;

    FireLaserMove laserMove;

    float originalOriginDistance;

    public override bool MoveEnabled => moveEnabled && Boss.AspidMode == AncientAspid.Mode.Tactical;

    private void Awake()
    {
        prepareShotgunXOffset = prepareShotgunOrigin.GetXLocalPosition();
        fireShotgunXOffset = shotgunFireRotationOrigin.GetXLocalPosition();
        laserMove = GetComponent<FireLaserMove>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;

        var firstAngle = MathUtilities.PolarToCartesian(-90f + angleRange.x,10f);
        var secondAngle = MathUtilities.PolarToCartesian(-90f + angleRange.y,10f);

        Gizmos.DrawRay(transform.position, firstAngle);
        Gizmos.DrawRay(transform.position, secondAngle);
    }

    uint targetRoutine = 0;

    void StartTargetRoutine()
    {
        targetRoutine = Boss.StartBoundRoutine(TargetRoutine());
    }

    void EndTargetRoutine()
    {
        if (targetRoutine != 0)
        {
            Boss.StopBoundRoutine(targetRoutine);
            targetRoutine = 0;
            Boss.SetTarget(oldTarget);
        }
    }

    IEnumerator TargetRoutine()
    {
        if (shotGunTarget == null)
        {
            var target = new GameObject("Shotgun Target");
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

        oldTarget = Boss.TargetTransform;
        Boss.SetTarget(shotGunTarget);

        while (true)
        {
            shotGunTarget.transform.position = Player.Player1.transform.position + targetPos;
            yield return null;
        }
    }

    public override IEnumerator DoMove()
    {
        Boss.orbitReductionAmount *= 3f;
        yield return Boss.Head.LockHead();

        StartTargetRoutine();
        /*if (shotGunTarget == null)
        {
            shotGunTarget = Player.Player1.transform.Find("Shotgun Target");
            if (shotGunTarget == null)
            {
                var target = new GameObject("Shotgun Target");
                target.transform.SetParent(Player.Player1.transform);
                shotGunTarget = target.transform;
            }
        }

        if (Boss.Head.LookingDirection >= 0f)
        {
            shotGunTarget.localPosition = relativeTargetPos.With(x: -relativeTargetPos.x);
        }
        else
        {
            shotGunTarget.localPosition = relativeTargetPos;
        }


        oldTarget = Boss.TargetTransform;
        Boss.SetTarget(shotGunTarget);*/

        yield return new WaitForSeconds(preparationDelay);

        prepareShotgunOrigin.SetXLocalPosition(Boss.Head.LookingDirection >= 0f ? -prepareShotgunXOffset : prepareShotgunXOffset);

        chargeUpEffects.Play();

        if (chargeUpSound != null)
        {
            var audio = WeaverAudio.PlayAtPoint(chargeUpSound, transform.position);
            audio.transform.SetParent(transform, true);
        }

        //laserMove.Emitter.ChargeUpDuration = chargeUpTime;
        prepareShotGunEmitter.ChargeUpDuration = chargeUpTime;

        bool chargingUp = true;

        IEnumerator ChargeUpLaser()
        {
            yield return prepareShotGunEmitter.FireChargeUpOnlyRoutine();
            chargingUp = false;
        }

        Vector2 angleLimits = angleRange;

        if (Boss.Head.LookingDirection < 0f)
        {
            angleLimits = new Vector2(-angleLimits.x,-angleLimits.y);
        }

        originalOriginDistance = laserMove.Emitter.Laser.transform.GetYLocalPosition();
        //laserMove.Emitter.Laser.transform.SetYLocalPosition(laserOriginDistance);
        Boss.StartBoundRoutine(ChargeUpLaser());

        //Debug.Log("PLAYER ANGLE = " + MathUtilities.ClampRotation(GetAngleToPlayer(prepareShotGunEmitter) + 90f));
        //Debug.Log("MIN ANGLE = " + angleLimits.x);
        //Debug.Log("MAX ANGLE = " + angleLimits.y);
        //int oldIndex = -1;

        //while (chargingUp)
        for (float t = 0; t < chargeUpTime; t += Time.deltaTime)
        {
            //Debug.Log("TRUE ANGLE TO PLAYER = " + GetAngleToPlayer(prepareShotgunOrigin));

            //Debug.DrawRay(prepareShotgunOrigin.position,MathUtilities.PolarToCartesian(GetAngleToPlayer(prepareShotgunOrigin),10f),Color.cyan);

            //Debug.DrawLine(prepareShotgunOrigin.position, Player.Player1.transform.position, Color.Lerp(Color.red,Color.blue,0.5f));

            var playerAngle = ClampWithinRange(MathUtilities.ClampRotation(GetAngleToPlayer(prepareShotgunOrigin) + 90f),angleLimits.x,angleLimits.y);
            var destRotation = Quaternion.Euler(0f, 0f,playerAngle - 90f);

            //Debug.Log("FINAL PLAYER ANGLE = " + playerAngle);
            //var (main, extra) = laserMove.CalculateLaserRotation(destRotation,100f);

            //laserMove.SetLaserRotation(laserMainOffset, main + extra - laserMainOffset);

            SetPrepareLaserAngle(playerAngle);

            chargeUpEffects.transform.localRotation = Quaternion.Euler(playerAngle - 90f, -90f, 0f);//destRotation * Quaternion.Euler(0f,-90f,0f);//Quaternion.Euler(-90f + main + extra,-90f,0f);
            //laserMove.UpdateHeadRotation(ref oldIndex, main);
            yield return null;
        }

        chargeUpEffects.Stop();

        yield return new WaitForSeconds(initialFireDelay);

        shotgunFireRotationOrigin.SetXLocalPosition(Boss.Head.LookingDirection >= 0f ? -fireShotgunXOffset : fireShotgunXOffset);

        Boss.Head.Animator.PlaybackSpeed = phase1AnimationSpeed;
        var anticClip = Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic Quick");
        var clipDuration = (1f / anticClip.FPS) * anticClip.Frames.Count;

        for (int i = 0; i < phase1Shots; i++)
        {
            //Boss.Head.Animator.PlaybackSpeed = 20f / 12f;

            shotgunFireEmitter.FireDuration = phase1FireDuration;
            //shotgunFireEmitter.ChargeUpLaser_P1();
            //shotgunFireEmitter.FireLaserQuick();

            var playerRotation = MathUtilities.ClampRotation(GetAngleToPlayer(shotgunFireEmitter) + 90f);

            shotgunFireRotationOrigin.transform.SetZLocalRotation(playerRotation);

            Boss.StartBoundRoutine(FireLaser(clipDuration - shotgunFireEmitter.ChargeUpDuration));
            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

            Boss.Head.MainRenderer.sprite = fireSprite;
            shotgunFireEmitter.FireLaser_P2();

            if (phase1FireSound != null)
            {
                StartCoroutine(PlayFireSound(phase1FireSound,shotgunFireRotationOrigin.position));
            }

            CameraShaker.Instance.Shake(phase1CameraShake);

            for (float t = 0; t < phase1FireDuration; t += Time.deltaTime)
            {

                yield return null;
            }

            shotgunFireEmitter.EndLaser_P3();

            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

            yield return new WaitUntil(() => !shotgunFireEmitter.FiringLaser);
        }

        Boss.Head.Animator.PlaybackSpeed = 1f;

        //ALSO MAKE SURE THAT THE BOSS STOPS THE ATTACK PREMATURELY IF THE PLAYER IS COMPLETELY OUT OF RANGE

        Boss.Head.UnlockHead();

        OnStun();

        yield break;
    }

    IEnumerator FireLaser(float delay)
    {
        yield return new WaitForSeconds(delay);
        shotgunFireEmitter.ChargeUpLaser_P1();
    }

    IEnumerator PlayFireSound(AudioClip clip, Vector3 position)
    {
        var audio = WeaverAudio.PlayAtPoint(clip, position);
        audio.AudioSource.pitch = phase1FirePitch.RandomInRange();
        yield return new WaitForSeconds(volumeDecreaseDelay);
        for (float t = 0; t < volumeDecreaseTime; t += Time.deltaTime)
        {
            audio.AudioSource.volume = 1f - (t / volumeDecreaseTime);
            yield return null;
        }
    }

    public override float PostDelay => postDelay;

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

    bool AngleWithinRange(float angle, float a, float b)
    {
        if (a > b)
        {
            var temp = b;
            b = a;
            a = temp;
        }
        return angle >= a && angle <= b;
    }

    void UpdateLaserRotation()
    {
        //CalculateLaserRotation();
    }

    public override void OnStun()
    {
        Boss.orbitReductionAmount /= 3f;
        laserMove.Emitter.Laser.transform.SetYLocalPosition(originalOriginDistance);
        EndTargetRoutine();
        chargeUpEffects.Stop();
        laserMove.Emitter.StopLaser();
        Boss.SetTarget(oldTarget);
        Boss.Head.Animator.PlaybackSpeed = 1f;
    }

    void SetShotgunLaserAngle(float downwardAngle)
    {
        shotgunFireRotationOrigin.SetZLocalRotation(downwardAngle * shotgunFireRotationOrigin.transform.localScale.z);
    }

    void SetShotGunLaserAngle(Quaternion rotation)
    {
        var angle = MathUtilities.ClampRotation(rotation.eulerAngles.z + 90f);
        SetShotgunLaserAngle(angle);
    }

    void SetPrepareLaserAngle(float downwardAngle)
    {
        prepareShotgunOrigin.SetZLocalRotation(downwardAngle);
    }

    float GetAngleToPlayer(Component relativeComponent)
    {
        return MathUtilities.CartesianToPolar(Player.Player1.transform.position - relativeComponent.transform.position).x;
    }
}
