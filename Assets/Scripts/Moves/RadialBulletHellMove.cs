using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class RadialBulletHellMove : AncientAspidMove
{
    public override bool MoveEnabled
    {
        get
        {
            var enabled = Boss.CanSeeTarget &&
        Boss.CurrentRunningMode == Boss.OffensiveMode &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;

            return enabled;
        }
    }

    [SerializeField]
    AudioClip roarSound;

    [SerializeField]
    float roarEmitterDuration = 1f;

    [SerializeField]
    float shootDelay;

    [SerializeField]
    RumbleType cameraRumbleType;

    [SerializeField]
    float shakeIntensity = 0.1f;

    [SerializeField]
    float shotsPerSecond = 50f;

    [SerializeField]
    Vector2 shotAngleMinMax = new Vector2(-180f + 25f, 0f - 25f);

    [SerializeField]
    Vector2 shotVelocityMinMax = new Vector2(10f,15f);

    [SerializeField]
    float shootDuration = 7f;

    [SerializeField]
    float playerAimFrequency = 0.5f;

    [SerializeField]
    float shootPostDelay = 0.5f;

    [SerializeField]
    AudioClip shootSound;

    [SerializeField]
    float shootSoundsPerSecond = 15f;

    [SerializeField]
    Vector2 shootPitchMinMax = new Vector2(1f - 0.15f, 1f + 0.15f);

    [SerializeField]
    Vector3 spawnOffset;

    [SerializeField]
    AudioClip rumbleLoopSound;

    FireLaserMove laserMove;

    RoarEmitter emitter;

    uint soundCoroutine;
    uint shakeCoroutine;
    uint playerAimCoroutine;

    AudioPlayer loopSound;

    bool firingLaser = false;

    private void Awake()
    {
        laserMove = GetComponent<FireLaserMove>();
    }

    bool IsPlayerWithinArea()
    {
        var angleToPlayer = (Boss.GetAngleToPlayer() + 360f) % 360f;

        var minAngle = (shotAngleMinMax.x + 360f) % 360f;
        var maxAngle = (shotAngleMinMax.y + 360f) % 360f;

        var downVector = Vector2.down;

        var minVector = MathUtilities.PolarToCartesian(shotAngleMinMax.x, 1f);
        var maxVector = MathUtilities.PolarToCartesian(shotAngleMinMax.y, 1f);

        var playerVector = MathUtilities.PolarToCartesian(Boss.GetAngleToPlayer(), 1f);

        var playerDotProduct = Vector2.Dot(downVector, playerVector);

        if (playerVector.x >= 0f)
        {
            return playerDotProduct >= Vector2.Dot(downVector, maxVector);
        }
        else
        {
            return playerDotProduct >= Vector2.Dot(downVector, minVector);
        }
    }

    protected override IEnumerator OnExecute()
    {
        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);
        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        //var spriteIndex = Boss.Head.ShotgunLasers.GetHeadIndexForAngle(0f);

        //Boss.Head.MainRenderer.sprite = laserMove.head_Sprites[spriteIndex];
        //Boss.Head.MainRenderer.flipX = laserMove.head_HorizFlip[spriteIndex];
        Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(Quaternion.Euler(0f, 0f, -90f));

        emitter = RoarEmitter.Spawn(Boss.Head.transform.position);

        emitter.StopRoaringAfter(roarEmitterDuration);

        if (roarSound != null)
        {
            WeaverAudio.PlayAtPoint(roarSound, Boss.Head.transform.position);
        }

        if (rumbleLoopSound != null)
        {
            loopSound = WeaverAudio.PlayAtPointLooped(rumbleLoopSound, transform.position);
        }

        shakeCoroutine = Boss.StartBoundRoutine(ShakeRoutine(shakeIntensity));

        CameraShaker.Instance.SetRumble(cameraRumbleType);

        yield return new WaitForSeconds(shootDelay);

        soundCoroutine = Boss.StartBoundRoutine(PlayShootSoundRoutine());

        playerAimCoroutine = Boss.StartBoundRoutine(PlayerAimRoutine());


        float shotClock = 0;

        int prevHealth = Boss.HealthComponent.Health;


        for (float t = 0; t < shootDuration - ((prevHealth - Boss.HealthComponent.Health) / 64f); t += Time.deltaTime)
        {
            float shootDelayTime = 1f / (shotsPerSecond + ((prevHealth - Boss.HealthComponent.Health) / 5f));
            shotClock += Time.deltaTime;

            while (shotClock >= shootDelayTime)
            {
                shotClock -= shootDelayTime;
                Shoot();
            }

            if (Cancelled || Vector3.Distance(Boss.transform.position,Player.Player1.transform.position) >= 28f || !IsPlayerWithinArea())
            {
                break;
            }

            yield return null;
        }

        if (shakeCoroutine != 0)
        {
            Boss.StopBoundRoutine(shakeCoroutine);
            shakeCoroutine = 0;
        }

        if (soundCoroutine != 0)
        {
            Boss.StopBoundRoutine(soundCoroutine);
            soundCoroutine = 0;
        }

        if (playerAimCoroutine != 0)
        {
            Boss.StopBoundRoutine(playerAimCoroutine);
            playerAimCoroutine = 0;
        }

        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }

        CameraShaker.Instance.SetRumble(RumbleType.None);

        if (!Cancelled && Vector3.Distance(Boss.transform.position, Player.Player1.transform.position) < 28f && !IsPlayerWithinArea())
        {
            var laserPlayerAngle = GetPlayerAngleClamped();

            Quaternion start = Quaternion.Euler(0f, 0f, laserPlayerAngle - 90f);

            var destAngle = laserPlayerAngle < 0f ? -120f : 120f;
            Quaternion destination = Quaternion.Euler(0f, 0f, destAngle - 90f);

            Boss.Head.UnlockHead(Boss.Head.ShotgunLasers.GetCurrentHeadAngle());
            firingLaser = true;
            yield return laserMove.SweepLaser(new BasicSweepController(
                start,
                destination,
                1f,
                true,
                laserMove.followPlayerCurve
                ));
            firingLaser = false;

        }
        else
        {
            Boss.Head.UnlockHead(0f);
        }

    }

    float GetPlayerAngleClamped()
    {
        var playerAngle = MathUtilities.ClampRotation(Boss.GetAngleToPlayer() + 90f);

        if (playerAngle < shotAngleMinMax.x + 90f)
        {
            playerAngle = shotAngleMinMax.x + 90f;
        }

        if (playerAngle > shotAngleMinMax.y + 90f)
        {
            playerAngle = shotAngleMinMax.y + 90f;
        }
        return playerAngle;
    }

    void Shoot()
    {
        var angle = shotAngleMinMax.RandomInRange();
        var magnitude = shotVelocityMinMax.RandomInRange();

        var velocity = MathUtilities.PolarToCartesian(angle, magnitude);

        Shoot(velocity);
    }

    void Shoot(Vector2 velocity)
    {
        AspidShot.Spawn(Boss.Head.transform.position + spawnOffset, velocity);
    }

    public override float GetPostDelay(int prevHealth) => shootPostDelay;

    IEnumerator PlayShootSoundRoutine()
    {
        while (true)
        {
            if (shootSound != null)
            {
                var sound = WeaverAudio.PlayAtPoint(shootSound, Boss.Head.transform.position);
                sound.AudioSource.pitch = shootPitchMinMax.RandomInRange();

                yield return new WaitForSeconds(1f / shootSoundsPerSecond);
            }

            Blood.SpawnRandomBlood(Boss.Head.transform.position + spawnOffset);
        }
    }

    IEnumerator PlayerAimRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(playerAimFrequency);

            var velocity = MathUtilities.CalculateVelocityToReachPoint(Boss.Head.transform.position + spawnOffset, Player.Player1.transform.position, Vector3.Distance(Boss.Head.transform.position + spawnOffset, Player.Player1.transform.position) / shotVelocityMinMax.RandomInRange(), 0.05);

            Shoot(velocity);

            yield return null;
        }
    }

    IEnumerator ShakeRoutine(float intensity, float shakeFPS = 60f)
    {
        Vector3 sourcePos = transform.position;
        while (true)
        {
            transform.position = sourcePos + (Vector3)(UnityEngine.Random.insideUnitCircle * intensity);
            Vector3 oldPos = transform.position;
            yield return new WaitForSeconds(1f / shakeFPS);
            Vector3 newPos = transform.position;

            sourcePos += newPos - oldPos;
        }
    }

    public override void StopMove()
    {
        base.StopMove();
        if (firingLaser)
        {
            laserMove.StopMove();
        }
    }

    public override void OnStun()
    {
        CameraShaker.Instance.SetRumble(RumbleType.None);

        if (emitter != null)
        {
            emitter.StopRoaring();
            emitter = null;
        }

        if (shakeCoroutine != 0)
        {
            Boss.StopBoundRoutine(shakeCoroutine);
            shakeCoroutine = 0;
        }

        if (soundCoroutine != 0)
        {
            Boss.StopBoundRoutine(soundCoroutine);
            soundCoroutine = 0;
        }

        if (playerAimCoroutine != 0)
        {
            Boss.StopBoundRoutine(playerAimCoroutine);
            playerAimCoroutine = 0;
        }

        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }

        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }

        if (firingLaser)
        {
            laserMove.OnStun();
        }
    }
}
