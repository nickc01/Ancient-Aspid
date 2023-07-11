using System.Collections;
using UnityEngine;
using WeaverCore;
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
        Boss.AspidMode == AncientAspid.Mode.Offensive &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;

            //WeaverLog.LogError("RADIAL BULLET ENABLED = " + enabled);

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

    Recoiler recoiler;
    FireLaserMove laserMove;

    RoarEmitter emitter;

    uint soundCoroutine;
    uint shakeCoroutine;
    uint playerAimCoroutine;

    AudioPlayer loopSound;

    private void Awake()
    {
        recoiler = GetComponent<Recoiler>();
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

    public override IEnumerator DoMove()
    {
        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);
        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        var spriteIndex = laserMove.GetHeadIndexForAngle(0f);

        Boss.Head.MainRenderer.sprite = laserMove.head_Sprites[spriteIndex];
        Boss.Head.MainRenderer.flipX = laserMove.head_HorizFlip[spriteIndex];

        recoiler.SetRecoilSpeed(0f);

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

        float secondsPerShot = 1f / shotsPerSecond;
        for (float t = 0; t < shootDuration; t += Time.deltaTime)
        {
            shotClock += Time.deltaTime;

            while (shotClock >= secondsPerShot)
            {
                shotClock -= secondsPerShot;
                Shoot();
            }

            if (!Boss.CanSeeTarget || !IsPlayerWithinArea())
            {
                break;
            }

            yield return null;
        }

        //yield return new WaitForSeconds(shootDuration);

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

        Boss.Head.UnlockHead(0f);
    }

    void Shoot()
    {
        var angle = shotAngleMinMax.RandomInRange();
        var magnitude = shotVelocityMinMax.RandomInRange();

        var velocity = MathUtilities.PolarToCartesian(angle, magnitude);

        Shoot(velocity);
        //AspidShot.Spawn(Boss.Head.transform.position + spawnOffset, velocity);
    }

    void Shoot(Vector2 velocity)
    {
        AspidShot.Spawn(Boss.Head.transform.position + spawnOffset, velocity);
    }

    public override float PostDelay => shootPostDelay;

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
            //Blood.SpawnDirectionalBlood(Boss.Head.transform.position + spawnOffset, CardinalDirection.Down);
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

        if (Boss.Head.HeadLocked && !Boss.Head.HeadBeingUnlocked)
        {
            Boss.Head.UnlockHead();
        }

        if (loopSound != null)
        {
            loopSound.StopPlaying();
            loopSound = null;
        }
    }
}
