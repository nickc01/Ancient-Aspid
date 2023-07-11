using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Enums;
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
    int shots = 4;

    [SerializeField]
    float fireDuration = 0.25f;

    [SerializeField]
    ShakeType cameraShake = ShakeType.AverageShake;

    [SerializeField]
    AudioClip fireSound;

    //[SerializeField]
    //bool doPhase1VolumeAdjust = true;

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

    /*[Space]
    [Header("Phase 2")]
    [SerializeField]
    float phase2InitialDelay = 0.25f;

    [SerializeField]
    float headRotationAmount = 20f;

    [SerializeField]
    float headRotationFPS = 12f;

    [SerializeField]
    int headRotationFrames = 3;

    [SerializeField]
    float headRotationResetDelay = 0.25f;

    [SerializeField]
    float laserOriginDistance = 0.458f;

    [SerializeField]
    float laserMainOffset = 0f;*/



    float prepareLaserXOffset;
    float fireLaserXOffset;


    Transform shotGunTarget;

    //Transform oldTarget;

    FireLaserMove laserMove;

    float originalOriginDistance;

    bool doingShotgun = false;
    LaserShotgunMove shotgunMove;

    Vector3 fireTarget;
    float fireRotation;
    bool cancelled = false;

    TargetOverride target;

    public override bool MoveEnabled
    {
        get
        {
            return Boss.CanSeeTarget && (moveEnabled || (Boss.Phase == AncientAspid.BossPhase.Default && Boss.HealthManager.Health <= 0.6f)) && Boss.AspidMode == AncientAspid.Mode.Tactical;
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

    //uint targetRoutine = 0;

    /*void StartTargeting()
    {
        SetLaserTarget();
    }*/

    void ResetTarget()
    {
        //Boss.LaserTargetOffset = default;

        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }

        //Boss.UnfreezeTarget();
        /*if (targetRoutine != 0)
        {
            Boss.StopBoundRoutine(targetRoutine);
            targetRoutine = 0;
            Boss.LaserTargetOffset = default;
            //Boss.SetTarget(oldTarget);
        }*/
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

        //Boss.LaserTargetOffset = targetPos;

        if (target == null)
        {
            target = Boss.AddTargetOverride();
        }

        target.SetTarget(() => targetPos + Player.Player1.transform.position);

        //Boss.FreezeTarget(() => targetPos + Player.Player1.transform.position);

        //oldTarget = Boss.TargetTransform;
        //Boss.SetTarget(shotGunTarget);

        /*while (true)
        {
            shotGunTarget.transform.position = Player.Player1.transform.position + targetPos;
            yield return null;
        }*/
    }

    public override IEnumerator DoMove()
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

        doingShotgun = false;
        Boss.orbitReductionAmount *= 3f;
        yield return Boss.Head.LockHead();

        SetLaserTarget();
        //StartTargeting();
        /*if (shotGunTarget == null)
        {
            shotGunTarget = Player.Player1.transform.Find("Laser Target");
            if (shotGunTarget == null)
            {
                var target = new GameObject("Laser Target");
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

        prepareLaserOrigin.SetXLocalPosition(Boss.Head.LookingDirection >= 0f ? -prepareLaserXOffset : prepareLaserXOffset);

        chargeUpEffects.Play();

        if (chargeUpSound != null)
        {
            var audio = WeaverAudio.PlayAtPoint(chargeUpSound, transform.position);
            audio.transform.SetParent(transform, true);
        }

        //laserMove.Emitter.ChargeUpDuration = chargeUpTime;
        prepareLaserEmitter.ChargeUpDuration = chargeUpTime;

        //bool chargingUp = true;

        IEnumerator ChargeUpLaser()
        {
            yield return prepareLaserEmitter.FireChargeUpOnlyRoutine();
            //chargingUp = false;
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

            var playerAngle = ClampWithinRange(MathUtilities.ClampRotation(GetAngleToPlayer(prepareLaserOrigin) + 90f),angleLimits.x,angleLimits.y);
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

        rapidFireRotationOrigin.SetXLocalPosition(Boss.Head.LookingDirection >= 0f ? -fireLaserXOffset : fireLaserXOffset);

        Boss.Head.Animator.PlaybackSpeed = animationSpeed;
        var anticClip = Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic Quick");
        var clipDuration = (1f / anticClip.FPS) * anticClip.Frames.Count;

        for (int i = 0; i < shots; i++)
        {
            if (Vector3.Distance(transform.position, Player.Player1.transform.position) > 30f)
            {
                break;
            }

            //Boss.Head.Animator.PlaybackSpeed = 20f / 12f;

            rapidFireEmitter.FireDuration = fireDuration;
            //shotgunFireEmitter.ChargeUpLaser_P1();
            //shotgunFireEmitter.FireLaserQuick();

            Boss.StartBoundRoutine(FireLaser(clipDuration - rapidFireEmitter.ChargeUpDuration,fireDuration));

            yield return null;
            //yield return new WaitForSeconds(0.1f);

            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");

            Boss.Head.MainRenderer.sprite = fireSprite;
            rapidFireEmitter.FireLaser_P2();

            if (fireSound != null)
            {
                StartCoroutine(PlayFireSound(fireSound,rapidFireRotationOrigin.position));

                PlayBloodEffects(bloodSpawnAmount, rapidFireEmitter);
            }

            CameraShaker.Instance.Shake(cameraShake);

            /*for (float t = 0; t < fireDuration; t += Time.deltaTime)
            {
                AimAtTarget();
                yield return null;
            }*/
            yield return new WaitForSeconds(fireDuration);

            rapidFireEmitter.EndLaser_P3();

            yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

            yield return new WaitUntil(() => !rapidFireEmitter.FiringLaser);
        }

        Boss.Head.Animator.PlaybackSpeed = 1f;

        /*float facingRight = Boss.Head.LookingDirection >= 0f ? 1f : -1f;

        yield return new WaitForSeconds(phase2InitialDelay);

        for (int i = 1; i <= headRotationFrames; i++)
        {
            Boss.Head.transform.SetZLocalRotation(Mathf.Lerp(0f,headRotationAmount * facingRight, i / (float)headRotationFrames));
            yield return new WaitForSeconds(1f / headRotationFPS);
        }

        yield return new WaitForSeconds(headRotationResetDelay);

        for (int i = 1; i <= headRotationFrames; i++)
        {
            Boss.Head.transform.SetZLocalRotation(Mathf.Lerp(headRotationAmount * facingRight, 0f, i / (float)headRotationFrames));
            yield return new WaitForSeconds(1f / headRotationFPS);
        }

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic Quick");*/


        //ALSO MAKE SURE THAT THE BOSS STOPS THE ATTACK PREMATURELY IF THE PLAYER IS COMPLETELY OUT OF RANGE

        //Boss.Head.UnlockHead();

        //OnStun();
        //laserMove.Emitter.Laser.transform.SetYLocalPosition(originalOriginDistance);
        Boss.Head.Animator.PlaybackSpeed = 1f;

        if (Vector3.Distance(transform.position, Player.Player1.transform.position) <= 30f)
        {
            doingShotgun = true;

            yield return shotgunMove.DoMove();

            doingShotgun = false;
        }
        Boss.orbitReductionAmount /= 3f;
        ResetTarget();

        //WeaverLog.LogError("OLD TARGET = " + oldTarget?.name);
        //Boss.SetTarget(oldTarget);

        Boss.Head.UnlockHead();

        //yield break;
    }

    IEnumerator FireLaser(float delay, float duration)
    {
        //fireTarget = Player.Player1.transform.position;

        //var oldPosition = Player.Player1.transform.position;
        double oldRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, Player.Player1.transform.position) + 90f);
        //yield return new WaitForSeconds(0.1f);
        yield return null;
        double newRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, Player.Player1.transform.position) + 90f);
        //var newPosition = Player.Player1.transform.position;

        //var directionVector = (newPosition - oldPosition).normalized;

        float difference = (float)((newRotation - oldRotation) / (double)Time.deltaTime);

        //var predictedPosition = Vector3.LerpUnclamped(newPosition, newPosition + directionVector, playerPredictionAmount);
        var predictedRotation = Mathf.LerpUnclamped((float)oldRotation,(float)oldRotation + difference,playerPredictionAmount);

        //fireTarget = predictedPosition;
        fireRotation = predictedRotation;

        yield return new WaitForSeconds(delay);

        //var playerRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, predictedPosition) + 90f);


        //var playerRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter) + 90f);

        //rapidFireRotationOrigin.transform.SetZLocalRotation(playerRotation);


        rapidFireEmitter.ChargeUpLaser_P1();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            AimAtTarget();
            yield return null;
        }
    }

    void AimAtTarget()
    {
        /*fireTarget = Vector3.Lerp(fireTarget,Player.Player1.transform.position, playerAimSpeed * Time.deltaTime);
        var playerRotation = MathUtilities.ClampRotation(GetAngleToPlayer(rapidFireEmitter, fireTarget) + 90f);
        rapidFireRotationOrigin.transform.SetZLocalRotation(playerRotation);*/

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

    public override float PostDelay => 0;

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

    /*bool AngleWithinRange(float angle, float a, float b)
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
    }*/

    public override void OnStun()
    {
        if (Boss.Head.HeadLocked && !Boss.Head.HeadBeingUnlocked)
        {
            Boss.Head.UnlockHead();
        }

        Boss.Head.Animator.StopCurrentAnimation();

        if (doingShotgun)
        {
            doingShotgun = false;
            shotgunMove.OnStun();

            Boss.orbitReductionAmount /= 3f;
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

    /*void SetShotgunLaserAngle(float downwardAngle)
    {
        rapidFireRotationOrigin.SetZLocalRotation(downwardAngle * rapidFireRotationOrigin.transform.localScale.z);
    }*/

    /*void SetShotGunLaserAngle(Quaternion rotation)
    {
        var angle = MathUtilities.ClampRotation(rotation.eulerAngles.z + 90f);
        SetShotgunLaserAngle(angle);
    }*/

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
}
