using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore.Utilities;
using static HeadController;

public class FireLaserMove : AncientAspidMove
{
    public abstract class SweepController
    {
        public float FireTime;

        public bool PlayAntic = false;

        public float AnticTime;

        public float MaximumAngle = 90f;

        public abstract void Init(AncientAspid boss);

        public abstract bool CanFire();

        public abstract Quaternion CalculateAngle(float timeSinceFiring);

        public virtual bool DoLaserInterrupt()
        {
            return true;
        }
    }

    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.4f;

    [SerializeField]
    float climbingPostDelay = 0.55f;

    [SerializeField]
    float headResetSpeed = 1f;

    //[SerializeField]
    //LaserEmitter emitter;

    [Header("Sweep Move")]
    [SerializeField]
    float sweepTime = 5;

    [SerializeField]
    float shadowDashSweepTime = 2.35f;

    [SerializeField]
    float sweepStartAngle = -90f - 45f;

    [SerializeField]
    float sweepEndAngle = -90f + 45f;

    [SerializeField]
    float sweepGlobSpawnRate = 0.25f;

    [SerializeField]
    Vector2 sweepGlobSizeRange = new Vector2(1.5f,2f);

    [SerializeField]
    AnimationCurve sweepCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Follow Player Move")]
    [SerializeField]
    float followPlayerTime = 1f;

    [SerializeField]
    float followPlayerShadowDashTime = 2.15f;

    [SerializeField]
    float minFollowPlayerDistance = 6f;

    [SerializeField]
    float followPlayerStartAngle = -30f;

    [SerializeField]
    float followPlayerEndAngle = 30f;

    public AnimationCurve followPlayerCurve;

    [Header("Other Settings")]

    [FormerlySerializedAs("head_Sprites")]
    public List<Sprite> head_SpritesOLD;

    [FormerlySerializedAs("head_HorizFlip")]
    public List<bool> head_HorizFlipOLD;

    [FormerlySerializedAs("head_Degrees")]
    public List<float> head_DegreesOLD;

    [SerializeField]
    AudioClip LaserBurstSound;

    [SerializeField]
    float burstSoundPitch = 1f;

    [SerializeField]
    AudioClip LaserLoopSound;

    [SerializeField]
    float loopSoundPitch = 1f;

    [SerializeField]
    float bloodSpawnRate = 0.1f;

    [SerializeField]
    float headAdjustAmount = 0.17493f;

    public override float PreDelay => 0.25f - (1f / 30f);

    public override bool MoveEnabled
    {
        get
        {
            if (!Boss.CanSeeTarget || Vector3.Distance(Player.Player1.transform.position,transform.position) >= 40)
            {
                return false;
            }
            if (Vector2.Distance(Player.Player1.transform.position, transform.position) >= minFollowPlayerDistance && moveEnabled)
            {
                if (Boss.Orientation == AspidOrientation.Center)
                {
                    return Player.Player1.transform.position.y < transform.position.y - 2f;
                }
                else
                {
                    return Player.Player1.transform.position.y < transform.position.y - 2f;
                }
            }

            return false;
        }
    }

    //public LaserEmitter Emitter => emitter;

    Transform laserRotationOrigin;
    //float maxEmitterAngle;

    AudioPlayer loopSound;

    TargetOverride target;

    public ILaserController CurrentController { get; private set; }

    private void Awake()
    {
        //laserRotationOrigin = emitter.transform.GetChild(0);
        laserRotationOrigin = Boss.Head.ShotgunLasers.MiddleLaser;
        //maxEmitterAngle = head_Degrees[head_Degrees.Count - 1];
    }

    public float GetPrimaryHeadAngle()
    {
        if (Boss.Orientation == AspidOrientation.Right)
        {
            return -45f;
        }
        else if (Boss.Orientation == AspidOrientation.Center)
        {
            return -90f;
        }
        else
        {
            return -90f - 45f;
        }
    }

    public bool IsLaserOriginVisible()
    {
        var rect = Boss.CamRect;

        rect.xMin -= 3f;
        rect.xMax += 3f;
        rect.yMin -= 3f;
        rect.yMax += 3f;

        return rect.Contains(laserRotationOrigin.transform.position);
    }

    protected override IEnumerator OnExecute()
    {
        if (Boss.Orientation == AspidOrientation.Center)
        {
            return SweepLaser(new ArenaSweepController(sweepStartAngle, sweepEndAngle, !ShadowDashTracker.PlayerHasShadowDashReady ? shadowDashSweepTime : sweepTime, sweepCurve));
        }
        else
        {
            return AttackPlayer(new PlayerSweepController(followPlayerStartAngle,followPlayerEndAngle, !ShadowDashTracker.PlayerHasShadowDashReady ? followPlayerShadowDashTime : followPlayerTime, followPlayerCurve));
        }
    }

    Vector2 GetMiddleBeamContact()
    {
        var emitter = Boss.Head.ShotgunLasers.MiddleEmitter;

        var contacts = emitter.Laser.ColliderContactPoints;

        var contactPoint = (Vector2)emitter.Laser.transform.TransformPoint(contacts[contacts.Count / 2]);

        var offsetVector = ((Vector2)emitter.Laser.transform.position - contactPoint).normalized;

        return contactPoint + (offsetVector * 0.3f);
    }

    public void PlayBloodEffects(int amount)
    {
        PlayBloodEffects(GetMiddleBeamContact(), amount);
    }

    public void PlayBloodEffects(Vector2 pos, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Blood.SpawnRandomBlood(pos);
        }
    }

    public IEnumerator AttackPlayer(PlayerSweepController controller)
    {
        target = Boss.AddTargetOverride();
        target.SetTarget(() => transform.position);

        yield return SweepLaser(controller);

        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }
        yield break;
    }

    /*public (float main, float extra) CalculateLaserRotation(Quaternion rotation, float divisor = 90f)
    {
        rotation *= Quaternion.Euler(0f, 0f, 90f);

        var angle = MathUtilities.ClampRotation(rotation.eulerAngles.z);

        float main = maxEmitterAngle * (angle / divisor);
        return (main, angle - main);
    }*/

    /*public (float main, float extra) CalculateLaserRotation(Quaternion rotation, float divisor = 90f)
    {
        rotation *= Quaternion.Euler(0f, 0f, 90f);

        var angle = MathUtilities.ClampRotation(rotation.eulerAngles.z);

        float main = maxEmitterAngle * (angle / divisor);
        return (main, angle - main);
    }*/

    /*public void UpdateLaserRotation(Quaternion quaternion)
    {
        var angleDegrees = CalculateLaserRotation(quaternion);
        SetLaserRotation(angleDegrees.main, angleDegrees.extra);

        var spriteIndex = GetHeadIndexForAngle(angleDegrees.main);
        Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
        Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
    }*/

    public float GetMinLaserDistance(Vector3 target)
    {
        var main = Boss.Head.ShotgunLasers.MiddleEmitter.transform.GetZLocalRotation();
        var extra = laserRotationOrigin.GetZLocalRotation();

        var angle = extra + main;

        return GetMinLaserDistance(target, Quaternion.Euler(0f, 0f, angle));
    }

    public float GetMinLaserDistance(Vector3 target, Quaternion laserRotation)
    {
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < 20; i++)
        {
            var direction = laserRotation * Vector3.down * i;
            var point = laserRotationOrigin.transform.position + direction;

            var distance = Vector2.Distance(target, point);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
            }
        }

        return nearestDistance;
    }

    public bool LaserOnRightSideOf(Vector3 target)
    {
        var main = Boss.Head.ShotgunLasers.MiddleEmitter.transform.GetZLocalRotation();
        var extra = laserRotationOrigin.GetZLocalRotation();

        var angle = extra + main;

        return LaserOnRightSideOf(target, Quaternion.Euler(0f, 0f, angle));
    }

    public bool LaserOnRightSideOf(Vector3 target, Quaternion laserRotation)
    {
        var vectorToTarget = target - laserRotationOrigin.position;

        var vectorA = Quaternion.Euler(0f, 0f, -90f) * vectorToTarget;

        return Vector2.Dot(vectorA, (Vector2)(laserRotation * Vector3.down)) >= 0f;
    }

    public Quaternion KeepAngleDistAwayFromTarget(Quaternion rotation, Vector3 target, float angleDistance)
    {
        var angleToTarget = MathUtilities.CartesianToPolar(target - Boss.Head.transform.position).x;

        var diffAngleFromPlayer = Mathf.Abs(Quaternion.Angle(rotation, Quaternion.Euler(0f, 0f, angleToTarget)));

        if (diffAngleFromPlayer > angleDistance)
        {
            return rotation;
        }

        var turnAmount = angleDistance - diffAngleFromPlayer;


        var angleDot = Quaternion.Dot(rotation * Quaternion.Euler(0, 0, -90), Quaternion.Euler(0f, 0f, angleToTarget));
        if (angleDot >= 0f)
        {
            return rotation * Quaternion.Euler(0, 0, turnAmount);
        }
        else
        {
            return rotation * Quaternion.Euler(0, 0, -turnAmount);
        }
    }

    public IEnumerator SweepLaser(ILaserController controller)
    {
        CurrentController = controller;
        Cancelled = false;
        var currentPhase = Boss.CurrentPhase;

        controller.Init(this);

        Cancelled = !controller.CanFire(this);


        if (Cancelled)
        {
            yield break;
        }

        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);

        var anticClip = Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic");

        (double x, double y) oldPos = (Player.Player1.transform.position.x, Player.Player1.transform.position.y);
        double oldTime = Time.timeAsDouble;
        yield return new WaitForSeconds(1f / 30f);
        (double x, double y) newPos = (Player.Player1.transform.position.x, Player.Player1.transform.position.y);
        double newTime = Time.timeAsDouble;

        double dt = newTime - oldTime;

        double velocityX = (newPos.x - oldPos.x) / dt;
        double velocityY = (newPos.y - oldPos.y) / dt;

        var startAngle = controller.GetStartAngle(this, new Vector2((float)velocityX, (float)velocityY));

        Boss.Head.MainRenderer.flipX = Boss.Head.ShotgunLasers.AngleIsFacingRight(startAngle);

        //Boss.Head.Animator.PlayAnimation("Fire Laser Antic");

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        //WeaverLog.Log("ANGLE TO PLAYER = " + MathUtilities.CartesianToPolar(headToPlayer).x);

        //Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(MathUtilities.CartesianToPolar(headToPlayer).x);


        /*while (Boss.Head.Animator.PlayingClip == "Fire Laser Antic")
        {
            yield return null;
        }*/

        var emitter = Boss.Head.ShotgunLasers.MiddleEmitter;

        emitter.ChargeUpLaser_P1();
        emitter.FireLaser_P2();

        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingSmall);
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);

        var burstSound = WeaverAudio.PlayAtPoint(LaserBurstSound, transform.position);
        burstSound.AudioSource.pitch = burstSoundPitch;
        loopSound = WeaverAudio.PlayAtPointLooped(LaserLoopSound, transform.position);
        loopSound.AudioSource.pitch = loopSoundPitch;

        int oldIndex = -1;

        float bloodTimer = 0f;
        bool firing = true;

        Quaternion laserRot = default;

        Boss.Head.ShotgunLasers.ContinouslyUpdateLasers(index => laserRot);

        void UpdateLaserRotation(Quaternion quaternion)
        {
            /*var angleDegrees = CalculateLaserRotation(quaternion);
            //SetLaserRotation(angleDegrees.main, angleDegrees.extra);
            Boss.Head.ShotgunLasers.SetLaserRotation(2, quaternion);

            var spriteIndex = Boss.Head.ShotgunLasers.GetHeadIndexForAngle(angleDegrees.main);
            Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
            Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];*/
            laserRot = quaternion;
            Boss.Head.ShotgunLasers.SetLaserRotation(2, quaternion);
            Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(quaternion);
        }

        UpdateLaserRotation(startAngle);

        IEnumerator Fire()
        {
            yield return controller.Fire(this, () => Cancelled, UpdateLaserRotation, startAngle);
            firing = false;
        }

        Boss.StartBoundRoutine(Fire());

        PlayBloodEffects(3);

        while (firing)
        {
            bloodTimer += Time.deltaTime;
            if (bloodTimer >= bloodSpawnRate)
            {
                bloodTimer -= bloodSpawnRate;
                PlayBloodEffects(1);
            }

            GetMinLaserDistance(Player.Player1.transform.position);

            yield return null;
        }



        emitter.EndLaser_P3(Boss.Head.ShotgunLasers.StopContinouslyUpdating);

        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound.Delete();
            loopSound = null;
        }
        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);

        yield return FinishLaserMove(headAdjustAmount, anticClip.FPS * headResetSpeed);
        yield return new WaitUntil(() => !emitter.FiringLaser);
        controller.Uninit(this);

        CurrentController = null;
    }

    public IEnumerator FinishLaserMove()
    {
        return FinishLaserMove(headAdjustAmount, Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic").FPS * headResetSpeed);
    }

    /*public void UpdateHeadRotation(ref int oldSpriteIndex, float mainRotation)
    {
        var spriteIndex = Boss.Head.ShotgunLasers.GetHeadIndexForAngle(mainRotation);
        if (spriteIndex != oldSpriteIndex)
        {
            oldSpriteIndex = spriteIndex;

            Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
            Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
        }
    }*/


    IEnumerator FinishLaserMove(float retractAmount, float fps)
    {
        var direction = Boss.Head.ShotgunLasers.GetCurrentHeadAngle();

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End");

        Boss.Head.UnlockHead(direction < 0f ? AspidOrientation.Left : AspidOrientation.Right);
    }

    IEnumerator FinishLaserMoveOld(float retractAmount, float fps)
    {
        var headIndex = Boss.Head.ShotgunLasers.GetHeadIndexForSprite(Boss.Head.MainRenderer.sprite,Boss.Head.MainRenderer.flipX);

        var headAngle = Boss.Head.ShotgunLasers.GetCurrentHeadAngle() - 90f;

        var headVector = MathUtilities.PolarToCartesian(headAngle, retractAmount);

        Boss.Head.transform.SetLocalPosition(x: -headVector.x,y: -headVector.y);

        yield return new WaitForSeconds(1f / fps);

        var idleSprite = Boss.Head.GetIdleSprite(Boss.Head.GetIdleIndexForAngle((headAngle + 90f) * (60f / Boss.Head.ShotgunLasers.MaxHeadAngle)));

        Boss.Head.MainRenderer.sprite = idleSprite.Sprite;
        Boss.Head.MainRenderer.flipX = idleSprite.XFlipped;

        Boss.Head.transform.SetLocalPosition(x: headVector.x, y: headVector.y);

        yield return new WaitForSeconds(1f / fps);

        Boss.Head.transform.SetLocalPosition(x: 0f, y: 0f);

        yield return new WaitForSeconds(1f / fps);

        Boss.Head.UnlockHead(idleSprite.Degrees);
    }

    public override float GetPostDelay(int prevHealth) => Boss.InClimbingPhase ? climbingPostDelay : postDelay;


    /*public void SetLaserRotation(float main, float extra)
    {
        emitter.transform.SetZLocalRotation(main);
        laserRotationOrigin.SetZLocalRotation(extra);
    }

    public (float main, float extra) GetLaserRotationValues()
    {
        var main = MathUtilities.ClampRotation(emitter.transform.GetZLocalRotation());
        var extra = MathUtilities.ClampRotation(laserRotationOrigin.GetZLocalRotation());

        return (main, extra);
    }*/

    public Vector3 GetFireLocation()
    {
        return laserRotationOrigin.position;
    }

    /*public Quaternion GetLaserRotation()
    {
        return emitter.transform.rotation * laserRotationOrigin.localRotation;
    }*/

    public override void OnStun()
    {
        Boss.Head.ShotgunLasers.StopContinouslyUpdating();
        Boss.Head.ShotgunLasers.MiddleEmitter.StopLaser();
        if (CurrentController != null)
        {
            CurrentController.OnStun(this);
        }
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }

        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }

        Boss.Head.transform.SetLocalPosition(x: 0f, y: 0f);
        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }
        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
        CurrentController = null;
    }
}
