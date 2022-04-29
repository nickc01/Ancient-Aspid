using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Utilities;

[ExecuteAlways]
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

    [Header("Follow Player Move")]
    [SerializeField]
    float followPlayerTime = 5;

    [SerializeField]
    float minFollowPlayerDistance = 6f;

    //[SerializeField]
    //float followPlayerSpeed = 2f;

    //[SerializeField]
    //float followPlayerAnticTime = 0.5f;

    [SerializeField]
    float followPlayerStartAngle = -30f;

    [SerializeField]
    float followPlayerEndAngle = 30f;

    [SerializeField]
    AnimationCurve followPlayerCurve;

    [SerializeField]
    List<Sprite> head_Sprites;

    [SerializeField]
    List<bool> head_HorizFlip;

    [SerializeField]
    List<float> head_Degrees;

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

    public override bool MoveEnabled
    {
        get
        {
            if (Vector2.Distance(Player.Player1.transform.position, transform.position) >= minFollowPlayerDistance && moveEnabled)
            {
                if (Boss.Orientation == AspidOrientation.Center)
                {
                    return true;
                }
                else
                {
                    //return false;
                    return Player.Player1.transform.position.y < transform.position.y - 2f;
                }
            }

            return false;
        }
    }

    Transform laserRotationOrigin;
    float minEmitterAngle;
    float maxEmitterAngle;

    AudioPlayer loopSound;

    AnimationCurve defaultCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private void Awake()
    {
        laserRotationOrigin = emitter.transform.GetChild(0);
        minEmitterAngle = head_Degrees[0];
        maxEmitterAngle = head_Degrees[head_Degrees.Count - 1];
    }

    public override IEnumerator DoMove()
    {
        Debug.Log("Distance to Player = " + Vector2.Distance(Player.Player1.transform.position, transform.position));
        if (Boss.Orientation == AspidOrientation.Center)
        {
            //return SweepLaser(sweepTime, sweepAnticTime, sweepStartAngle, sweepEndAngle,rotationDivisor: 60f);
            return SweepLaser(new ArenaSweepController(sweepStartAngle, sweepEndAngle, sweepTime),true,sweepGlobSpawnRate);
        }
        else
        {
            //followPlayerTime,followPlayerStartAngle,followPlayerEndAngle,followPlayerCurve
            return AttackPlayer(new PlayerSweepController(followPlayerStartAngle,followPlayerEndAngle,followPlayerTime,followPlayerCurve));
        }
        /*yield return FireLaser(sweepTime,sweepAnticTime, t =>
        {
            var from = Quaternion.Euler(0f, 0f, sweepStartAngle);
            var to = Quaternion.Euler(0f, 0f, sweepEndAngle);


            //emitter.Laser.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromAngle,toAngle,currentTime / time));
            SetLaserAngle(Quaternion.Slerp(from, to, t / sweepTime).eulerAngles.z);
        },sweepStartAngle,sweepEndAngle);*/
        //return FireLaser();
        //yield break;
    }

    Vector2 GetMiddleBeamContact()
    {
        var contacts = emitter.Laser.ColliderContactPoints;

        var contactPoint = (Vector2)emitter.Laser.transform.TransformPoint(contacts[contacts.Count / 2]);

        var offsetVector = ((Vector2)emitter.Laser.transform.position - contactPoint).normalized;

        return contactPoint + (offsetVector * 0.3f);
    }

    void PlayBloodEffects(int amount)
    {
        PlayBloodEffects(GetMiddleBeamContact(), amount);
    }

    void PlayBloodEffects(Vector2 pos, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Blood.SpawnRandomBlood(pos);
        }
    }

    IEnumerator AttackPlayer(PlayerSweepController controller)
    {
        //var playerAngle = GetDownAngleToPlayer();

        var oldTarget = Boss.TargetTransform;

        Boss.SetTarget(transform.position);

        yield return SweepLaser(controller,false,0f);

        Boss.SetTarget(oldTarget);
        yield break;
    }

    /*float GetDownAngleToPlayer()
    {
        return Vector2.Dot(Vector3.right * 90f,(Player.Player1.transform.position - transform.position).normalized);
    }*/

    (float main, float extra) CalculateLaserRotation(Quaternion rotation, float divisor = 90f)
    {
        rotation *= Quaternion.Euler(0f, 0f, 90f);

        var angle = ClampRotation(rotation.eulerAngles.z);

        float main = maxEmitterAngle * (angle / divisor);
        return (main, angle - main);
    }

    /*(float main, float extra) CalculateLaserRotation(float rotation, float divisor = 90f)
    {
        float main = maxEmitterAngle * (rotation / divisor);
        return (main, rotation - main);
    }*/

    void UpdateLaserRotation(Quaternion quaternion)
    {
        var startAngles = CalculateLaserRotation(quaternion);

        SetLaserRotation(startAngles.main, startAngles.extra);
    }

    IEnumerator SweepLaser(SweepController controller, bool spawnGlobs, float globSpawnRate)
    {
        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);

        yield return null;

        controller.Init(Boss);

        float timer = 0f;

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
            //var (mainAngle, _) = GetLaserRotation();
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

                    VomitGlob.Spawn(spawnPoint, direction, playSounds: false);
                }
            }
            yield return null;
        }

        OnStun();

        yield return FinishLaserMove(headAdjustAmount, anticClip.FPS);
        //yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End");
    }

    /*IEnumerator SweepLaserOLD(float fireTime, float anticTime, float startAngle, float endAngle, AnimationCurve interpCurve = null, bool flipRotations = true, float rotationDivisor = 90f)
    {
        if (interpCurve == null)
        {
            interpCurve = defaultCurve;
        }

        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);

        //WeaverLog.Log("HEAD LOCKED LASER");
        yield return null;

        //var anticClip = Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic");
        //var clipTime = (1f / anticClip.FPS) * anticClip.Frames.Count;

        //anticTime = Mathf.Clamp(anticTime, clipTime, anticTime);


        var (startMain, startExtra) = CalculateLaserRotation(startAngle, rotationDivisor);
        var (endMain, endExtra) = CalculateLaserRotation(endAngle, rotationDivisor);

        if (flipRotations && Boss.Head.LookingDirection >= 0f)
        {
            startMain = -startMain;
            startExtra = -startExtra;

            endMain = -endMain;
            endExtra = -endExtra;

            startAngle = -startAngle;
            endAngle = -endAngle;
        }


        //emitter.Laser.transform.rotation = Quaternion.Euler(0f, 0f, startAngle);
        SetLaserRotation(startMain,startExtra);
        emitter.ChargeUpDuration = anticTime;
        emitter.FireDuration = fireTime;

        float timer = 0f;

        IEnumerator LaserUpdateRoutine()
        {
            while (true)
            {
                var mainAngle = Mathf.Lerp(startMain,endMain, interpCurve.Evaluate(timer / fireTime));
                var extraAngle = Mathf.Lerp(startExtra,endExtra, interpCurve.Evaluate(timer / fireTime));
                SetLaserRotation(mainAngle,extraAngle);
                //updateFunction(timer);
                yield return null;
            }
        }

        var updateRoutine = Boss.StartBoundRoutine(LaserUpdateRoutine());


        //yield return new WaitForSeconds(anticTime - clipTime);


        Boss.Head.MainRenderer.flipX = startAngle >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        emitter.FireLaser();

        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.RumblingSmall);
        CameraShaker.Instance.Shake(WeaverCore.Enums.ShakeType.EnemyKillShake);

        var burstSound = WeaverAudio.PlayAtPoint(LaserBurstSound, transform.position);
        burstSound.AudioSource.pitch = burstSoundPitch;
        loopSound = WeaverAudio.PlayAtPointLooped(LaserLoopSound, transform.position);
        loopSound.AudioSource.pitch = loopSoundPitch;

        yield return null;

        PlayBloodEffects(3);

        int oldIndex = -1;

        float bloodTimer = 0f;

        for (timer = 0; timer < fireTime; timer += Time.deltaTime)
        {
            bloodTimer += Time.deltaTime;
            var (mainAngle, _) = GetLaserRotation();
            var spriteIndex = GetHeadIndexForAngle(mainAngle);
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
            yield return null;
        }

        Boss.StopBoundRoutine(updateRoutine);

        Boss.Head.MainRenderer.flipX = endAngle >= 0f;

        OnStun();

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End");

        Boss.Head.UnlockHead(endAngle);
    }*/

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

    public override float PostDelay => 0.1f;

    /*(float main, float extra) GetLaserRotation()
    {
        float main = emitter.transform.GetZLocalRotation();
        float extra = laserRotationOrigin.GetZLocalRotation();

        if (main > 180f)
        {
            main -= 360f;
        }

        if (extra > 180f)
        {
            extra -= 360f;
        }

        return (main, extra);
    }*/

    void SetLaserRotation(float main, float extra)
    {
        emitter.transform.SetZLocalRotation(main);
        laserRotationOrigin.transform.SetZLocalRotation(extra);
    }

    float ClampRotation(float rotation)
    {
        rotation %= 360f;

        if (rotation > 180f)
        {
            rotation -= 360f;
        }
        return rotation;
    }

    /*float GetLaserAngle()
    {
        var result = (emitter.transform.GetZLocalRotation() + laserRotationOrigin.transform.GetZLocalRotation()) % 360f;
        if (result > 180f)
        {
            result -= 360f;
        }
        return result;
    }

    void SetLaserAngle(float angle)
    {
        var reduced = angle / 1.5f;

        emitter.transform.SetZLocalRotation(reduced);
        laserRotationOrigin.transform.SetZLocalRotation(angle - reduced);
    }*/

    /*private void Update()
    {
        Debug.Log("Laser Angle = " + emitter.Laser.transform.GetZRotation());
    }*/

    /*IEnumerator FireLaserRoutine(Action<float> updateFunction, float time, float anticTime, float startAngle)
    {
        //yield return Boss.Head.LockHead()
        //TODO TODO
        //yield return Boss.Head.DisableFollowPlayer();


        var anticClip = Boss.Head.Animator.AnimationData.GetClip("Fire Laser Antic");
        var clipTime = (1f / anticClip.FPS) * anticClip.Frames.Count;

        anticTime = Mathf.Clamp(anticTime, clipTime,anticTime);
        emitter.Laser.transform.rotation = Quaternion.Euler(0f,0f,startAngle);
        emitter.ChargeUpDuration = anticTime;
        emitter.FireDuration = time;
        emitter.FireLaser();

        float timer = 0f;

        IEnumerator LaserUpdateRoutine()
        {
            while (true)
            {
                updateFunction(timer);
                yield return null;
            }
        }

        var updateRoutine = Boss.StartBoundRoutine(LaserUpdateRoutine());

        yield return new WaitForSeconds(anticTime - clipTime);


        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        int oldIndex = -1;

        for (timer = 0; timer < time; timer += Time.deltaTime)
        {
            var spriteIndex = GetHeadSpriteIndexForAngle(emitter.Laser.transform.GetZRotation());
            if (spriteIndex != oldIndex)
            {
                oldIndex = spriteIndex;

                Boss.Head.MainRenderer.sprite = head_Sprites[spriteIndex];
                Boss.Head.MainRenderer.flipX = head_HorizFlip[spriteIndex];
            }
            yield return null;
        }

        Boss.StopBoundRoutine(updateRoutine);

        Boss.Head.UnlockHead();

        //TODO -- ENDING ANIMATION
    }*/

    /*void SetLaserRotation(float angle)
    {
        if (angle < minEmitterAngle)
        {
            emitter.transform.rotation = Quaternion.Euler(0f,0f,minEmitterAngle);
            laserRotationOrigin.rotation = Quaternion.Euler(0f,0f,-90 + (angle - minEmitterAngle));
        }
        else if (angle > maxEmitterAngle)
        {
            emitter.transform.rotation = Quaternion.Euler(0f, 0f, maxEmitterAngle);
            laserRotationOrigin.rotation = Quaternion.Euler(0f, 0f, -90 + (angle - maxEmitterAngle));
        }
        else
        {
            emitter.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            laserRotationOrigin.rotation = Quaternion.Euler(0f, 0f, -90f);
        }
    }*/

    /*float GetLaserRotation()
    {
        return emitter.transform.eulerAngles.z + 90f + laserRotationOrigin.transform.eulerAngles.z;
    }*/

    /*public IEnumerator FireLaser()
    {
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            return FireLaserAtPlayer(followPlayerTime, followPlayerSpeed);
        }
        else
        {
            return FireLaser(sweepStartAngle, sweepEndAngle, sweepTime);
        }
    }*/

    /*float GetLaserAngleToPlayer()
    {
        return MathUtilities.CartesianToPolar(Player.Player1.transform.position - emitter.Laser.transform.position).x;
    }*/

    /*public IEnumerator FireLaserAtPlayer(float time, float followSpeed)
    {
        return FireLaserRoutine(currentTime =>
        {
            var from = Quaternion.Euler(0f,0f, GetLaserRotation());
            var to = Quaternion.Euler(0f,0f, GetLaserAngleToPlayer() + 90f);

            SetLaserRotation(Quaternion.Slerp(from,to,followSpeed * Time.deltaTime).eulerAngles.z);
            //emitter.Laser.transform.rotation = Quaternion.Slerp(emitter.Laser.transform.rotation, Quaternion.Euler(0f, 0f, GetLaserAngleToPlayer()), followSpeed * Time.deltaTime);
        }, time, followPlayerAnticTime, GetLaserAngleToPlayer());
    }

    public IEnumerator FireLaser(float fromAngle, float toAngle, float time)
    {
        return FireLaserRoutine(currentTime =>
        {
            var from = Quaternion.Euler(0f, 0f, fromAngle);
            var to = Quaternion.Euler(0f, 0f, toAngle);


            //emitter.Laser.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromAngle,toAngle,currentTime / time));
            SetLaserRotation(Quaternion.Slerp(from,to,currentTime / time).eulerAngles.z);
        },time, sweepAnticTime, fromAngle);
    }*/

    public override void OnStun()
    {
        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }
        CameraShaker.Instance.SetRumble(WeaverCore.Enums.RumbleType.None);
    }

    int GetHeadIndexForSprite(Sprite sprite, bool flipped)
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

    int GetHeadIndexForAngle(float angle)
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
