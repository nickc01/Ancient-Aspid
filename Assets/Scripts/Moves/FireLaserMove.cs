using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Utilities;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Contains the main laser move and other common functions and utilities for operating the laser
/// </summary>
public class FireLaserMove : AncientAspidMove
{
    public abstract class SweepController
    {
        /// <summary>
        /// How long the laser fires for
        /// </summary>
        public float FireTime;

        /// <summary>
        /// Should the laser's antic be played?
        /// </summary>
        public bool PlayAntic = false;

        /// <summary>
        /// How long the antic should play for
        /// </summary>
        public float AnticTime;

        /// <summary>
        /// The maximum possible angle for this sweep. This is mainly used to control how the head sprites should align with the laser beam
        /// </summary>
        public float MaximumAngle = 90f;

        public abstract void Init(AncientAspid boss);

        /// <summary>
        /// The function used for calculating the laser's angle. This is called every frame while the laser is firing
        /// </summary>
        public abstract Quaternion CalculateAngle(float timeSinceFiring);
    }

    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.4f;

    [SerializeField]
    LaserEmitter emitter;

    [Header("Sweep Move")]
    [SerializeField]
    float sweepTime = 5;

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
    float followPlayerTime = 5;

    [SerializeField]
    float minFollowPlayerDistance = 6f;

    [SerializeField]
    float followPlayerStartAngle = -30f;

    [SerializeField]
    float followPlayerEndAngle = 30f;

    public AnimationCurve followPlayerCurve;

    [Header("Other Settings")]
    public List<Sprite> head_Sprites;

    public List<bool> head_HorizFlip;

    public List<float> head_Degrees;

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

    bool cancelled = false;

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

    public LaserEmitter Emitter => emitter;

    Transform laserRotationOrigin;
    float maxEmitterAngle;

    AudioPlayer loopSound;

    TargetOverride target;

    //AnimationCurve defaultCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private void Awake()
    {
        laserRotationOrigin = emitter.transform.GetChild(0);
        maxEmitterAngle = head_Degrees[head_Degrees.Count - 1];
    }

    public override IEnumerator DoMove()
    {
        if (Boss.Orientation == AspidOrientation.Center)
        {
            return SweepLaser(new ArenaSweepController(sweepStartAngle, sweepEndAngle, sweepTime, sweepCurve),true,sweepGlobSpawnRate);
        }
        else
        {
            return AttackPlayer(new PlayerSweepController(followPlayerStartAngle,followPlayerEndAngle,followPlayerTime,followPlayerCurve));
        }
    }

    Vector2 GetMiddleBeamContact()
    {
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
        //var oldTarget = Boss.TargetTransform;

        //Boss.SetTarget(transform.position);
        //Boss.FreezeTarget(() => transform.position);
        target = Boss.AddTargetOverride();
        target.SetTarget(() => transform.position);

        yield return SweepLaser(controller,false,0f);

        if (target != null)
        {
            Boss.RemoveTargetOverride(target);
            target = null;
        }
        //Boss.UnfreezeTarget();
        //Boss.SetTarget(oldTarget);
        yield break;
    }

    public (float main, float extra) CalculateLaserRotation(Quaternion rotation, float divisor = 90f)
    {
        rotation *= Quaternion.Euler(0f, 0f, 90f);

        var angle = MathUtilities.ClampRotation(rotation.eulerAngles.z);

        float main = maxEmitterAngle * (angle / divisor);
        return (main, angle - main);
    }

    public void UpdateLaserRotation(Quaternion quaternion)
    {
        var startAngles = CalculateLaserRotation(quaternion);

        SetLaserRotation(startAngles.main, startAngles.extra);
    }

    public IEnumerator SweepLaser(SweepController controller, bool spawnGlobs, float globSpawnRate)
    {
        controller.Init(Boss);

        var initialAngle = controller.CalculateAngle(0f);

        var initialDirection = initialAngle * Vector3.right;

        Debug.DrawRay(Boss.Head.transform.position, initialDirection * 10f, Color.yellow, 10f);

        var camRect = Boss.CamRect;

        cancelled = true;

        for (int i = 0; i <= 10; i++)
        {
            var pointToCheck = Boss.Head.transform.position + (initialDirection * i);
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

        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);

        float timer = 0f;

        yield return null;

        emitter.ChargeUpDuration = controller.PlayAntic ? controller.AnticTime : 0f;
        emitter.FireDuration = controller.FireTime;

        var anticClip = Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic");

        if (controller.PlayAntic)
        {
            emitter.FireLaser();
            var clipTime = (1f / anticClip.FPS) * anticClip.Frames.Count;

            for (float t = 0; t < controller.AnticTime - clipTime; t += Time.deltaTime)
            {
                UpdateLaserRotation(controller.CalculateAngle(timer));
                yield return null;
            }
            //yield return new WaitForSeconds(controller.AnticTime - clipTime);
        }

        Boss.Head.MainRenderer.flipX = CalculateLaserRotation(controller.CalculateAngle(timer)).main >= 0f;

        Boss.Head.Animator.PlayAnimation("Fire Laser Antic");

        while (Boss.Head.Animator.PlayingClip == "Fire Laser Antic")
        {
            UpdateLaserRotation(controller.CalculateAngle(timer));
            yield return null;
        }

        if (!controller.PlayAntic)
        {
            emitter.FireLaser();
        }

        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingSmall);
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);

        var burstSound = WeaverAudio.PlayAtPoint(LaserBurstSound, transform.position);
        burstSound.AudioSource.pitch = burstSoundPitch;
        loopSound = WeaverAudio.PlayAtPointLooped(LaserLoopSound, transform.position);
        loopSound.AudioSource.pitch = loopSoundPitch;

        PlayBloodEffects(3);

        int oldIndex = -1;

        float bloodTimer = 0f;
        float globTimer = 0f;

        for (timer = 0; timer < controller.FireTime; timer += Time.deltaTime)
        {
            var angle = CalculateLaserRotation(controller.CalculateAngle(timer));
            bloodTimer += Time.deltaTime;
            SetLaserRotation(angle.main, angle.extra);
            var spriteIndex = GetHeadIndexForAngle(angle.main);
            if (spriteIndex != oldIndex)
            {
                oldIndex = spriteIndex;

                Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
                Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
            }

            if (bloodTimer >= bloodSpawnRate)
            {
                bloodTimer -= bloodSpawnRate;
                PlayBloodEffects(1);
            }

            if (spawnGlobs)
            {
                globTimer += Time.deltaTime;
                if (globTimer >= globSpawnRate)
                {
                    globTimer -= globSpawnRate;
                    var middle = GetMiddleBeamContact();
                    var spawnPoint = Vector2.Lerp(transform.position,middle,0.5f);
                    var direction = (middle - (Vector2)transform.position).normalized * 70f;

                    var glob = VomitGlob.Spawn(spawnPoint, direction, playSounds: false);
                    glob.SetScale(sweepGlobSizeRange.RandomInRange());
                }
            }

            if (Boss.RiseFromCenterPlatform)
            {
                emitter.StopLaser();
                break;
            }

            yield return null;
        }

        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }
        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);

        yield return FinishLaserMove(headAdjustAmount, anticClip.FPS);
    }

    public IEnumerator FinishLaserMove()
    {
        return FinishLaserMove(headAdjustAmount, Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic").FPS);
    }

    public void UpdateHeadRotation(ref int oldSpriteIndex, float mainRotation)
    {
        var spriteIndex = GetHeadIndexForAngle(mainRotation);
        if (spriteIndex != oldSpriteIndex)
        {
            oldSpriteIndex = spriteIndex;

            Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
            Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
        }
    }

    IEnumerator FinishLaserMove(float retractAmount, float fps)
    {
        var headIndex = GetHeadIndexForSprite(Boss.Head.MainRenderer.sprite,Boss.Head.MainRenderer.flipX);
        var headAngle = head_Degrees[headIndex] - 90f;

        var headVector = MathUtilities.PolarToCartesian(headAngle, retractAmount);

        Boss.Head.transform.SetLocalPosition(x: -headVector.x,y: -headVector.y);

        yield return new WaitForSeconds(1f / fps);

        var idleSprite = Boss.Head.GetIdleSprite(Boss.Head.GetIdleIndexForAngle((headAngle + 90f) * (60f / maxEmitterAngle)));

        Boss.Head.MainRenderer.sprite = idleSprite.Sprite;
        Boss.Head.MainRenderer.flipX = idleSprite.XFlipped;

        Boss.Head.transform.SetLocalPosition(x: headVector.x, y: headVector.y);

        yield return new WaitForSeconds(1f / fps);

        Boss.Head.transform.SetLocalPosition(x: 0f, y: 0f);

        yield return new WaitForSeconds(1f / fps);

        Boss.Head.UnlockHead(idleSprite.Degrees);
    }

    public override float PostDelay => cancelled ? 0f : postDelay;


    public void SetLaserRotation(float main, float extra)
    {
        emitter.transform.SetZLocalRotation(main);
        laserRotationOrigin.SetZLocalRotation(extra);
    }

    public (float main, float extra) GetLaserRotationValues()
    {
        var main = MathUtilities.ClampRotation(emitter.transform.GetZLocalRotation());
        var extra = MathUtilities.ClampRotation(laserRotationOrigin.GetZLocalRotation());

        return (main, extra);
    }

    public Vector3 GetFireLocation()
    {
        return laserRotationOrigin.position;
    }

    public Quaternion GetLaserRotation()
    {
        return emitter.transform.rotation * laserRotationOrigin.localRotation;
    }

    public override void OnStun()
    {
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
    }

    public int GetHeadIndexForSprite(Sprite sprite, bool flipped)
    {
        for (int i = head_Degrees.Count - 1; i >= 0; i--)
        {
            if (head_Sprites[i] == sprite && head_HorizFlip[i] == flipped)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetHeadIndexForAngle(float angle)
    {
        for (int i = head_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == head_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(head_Degrees[i - 1], head_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(head_Degrees[i], head_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(head_Degrees[i], head_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(head_Degrees[i - 1], head_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }
}
