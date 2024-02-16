using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class BulletHellMove : AncientAspidMove
{
    public override bool MoveEnabled => Boss.CanSeeTarget && moveEnabled &&
        Boss.CurrentRunningMode == Boss.OffensiveMode &&
        Player.Player1.transform.position.y < Boss.Head.transform.position.y - 3f &&
        Mathf.Abs(GetPlayerAngle()) <= 60f;

    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.5f;

    FireLaserMove laserMove;

    [SerializeField]
    GameObject shotPrefab;

    [SerializeField]
    int shotsPerRow = 9;

    [SerializeField]
    int rowsToShoot = 20;

    [SerializeField]
    float shotFireVelocity = 5f;

    [SerializeField]
    int bloodSpawnAmount = 3;

    [SerializeField]
    AudioClip spitSound;

    [SerializeField]
    Vector2 spitPlaybackRateRange = new Vector2(0.25f,0.1f);

    [SerializeField]
    Vector2 spitSoundPitchRange = new Vector2(0.9f,1.4f);

    [SerializeField]
    Vector2 spitSoundPitchVariation = new Vector2(-0.1f,0.1f);

    [SerializeField]
    Vector2 rowTimeRange = new Vector2(3f,0.5f);

    [SerializeField]
    Vector2 rowAngleRange = new Vector2(-50f,50f);

    bool travelingLeft = false;

    IEnumerator onSweepDone = null;
    Action onStun = null;

    private void Awake()
    {
        laserMove = GetComponent<FireLaserMove>();
    }

    protected override IEnumerator OnExecute()
    {
        onSweepDone = null;
        onStun = null;
        yield return Boss.Head.LockHead(Boss.Orientation == AspidOrientation.Right ? AspidOrientation.Right : AspidOrientation.Left);

        travelingLeft = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        int oldIndex = -1;

        var playerAngleClamped = GetPlayerAngleClamped();
        float totalTime = 0f;

        for (int i = 0; i < rowsToShoot; i++)
        {
            totalTime += Mathf.Lerp(rowTimeRange.x, rowTimeRange.y, i / (float)(rowsToShoot - 1));
        }

        float audioTimer = 0f;

        int prevHealth = Boss.HealthComponent.Health;

        for (int i = 0; i < rowsToShoot; i++)
        {
            var rowTime = Mathf.Lerp(rowTimeRange.x, rowTimeRange.y,i / (float)(rowsToShoot - 1));

            Quaternion from = Quaternion.Euler(0f,0f, playerAngleClamped + GetTargetAngle(0f) - 90f);
            Quaternion to = Quaternion.Euler(0f,0f, playerAngleClamped + GetTargetAngle(1f) - 90f);

            float fireRate = (1f / shotsPerRow) * rowTime;

            float fireTimer = 0f;

            float audioRate = Mathf.Lerp(spitPlaybackRateRange.x,spitPlaybackRateRange.y, i / (float)(rowsToShoot - 1));
            float audioPitch = Mathf.Lerp(spitSoundPitchRange.x, spitSoundPitchRange.y, i / (float)(rowsToShoot - 1));

            float shotsFired = 0f;
            if (travelingLeft)
            {
                shotsFired = 0.5f;
            }

            for (float t = 0f; t < rowTime; t += Time.deltaTime)
            {
                var spawnLocation = laserMove.GetFireLocation();

                audioTimer += Time.deltaTime;

                if (audioTimer >= audioRate)
                {
                    audioTimer = 0f;
                    var audio = WeaverAudio.PlayAtPoint(spitSound, spawnLocation);
                    audio.AudioSource.pitch = audioPitch + spitSoundPitchVariation.RandomInRange();
                }

                //var targetRotation = Quaternion.Slerp(from, to, t / rowTime);
                //var angle = laserMove.CalculateLaserRotation(targetRotation);
                //laserMove.SetLaserRotation(0f, angle.extra);
                //Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(Quaternion.Euler(0f,0f,-90f));
                Boss.Head.ShotgunLasers.SetHeadSpriteToRotation(Quaternion.Euler(0f, 0f, -90f));
                /*var spriteIndex = laserMove.GetHeadIndexForAngle(0f);
                if (spriteIndex != oldIndex)
                {
                    oldIndex = spriteIndex;

                    Boss.Head.MainRenderer.sprite = laserMove.head_Sprites[spriteIndex];
                    Boss.Head.MainRenderer.flipX = laserMove.head_HorizFlip[spriteIndex];
                }*/

                if (fireTimer >= fireRate)
                {
                    var shotAngle = GetTargetAngle(shotsFired / shotsPerRow);
                    var shotRotation = Quaternion.Slerp(from, to, shotsFired / shotsPerRow);

                    fireTimer -= fireRate;

                    FireProjectile(spawnLocation, shotRotation, 0.1f + fireRate);

                    Blood.SpawnBlood(spawnLocation, new Blood.BloodSpawnInfo(bloodSpawnAmount, 4 + bloodSpawnAmount, 10f, 25f, shotAngle + playerAngleClamped - 90f - 50f, shotAngle + playerAngleClamped - 90f + 50f, null));

                    var wave = DeathWave.Spawn(spawnLocation + new Vector3(0f,0f,-0.1f),0.3f);
                    wave.TransparencyMultiplier = 0.2f;
                    wave.UpdateVisuals();

                    shotsFired++;
                }

                var currentPlayerAngle = GetPlayerAngle();

                if ((currentPlayerAngle < rowAngleRange.x + playerAngleClamped) || (currentPlayerAngle > rowAngleRange.y + playerAngleClamped))
                {
                    Cancelled = true;
                    onSweepDone = DoLaserAttack();
                    break;
                }

                if ((prevHealth - Boss.HealthComponent.Health) >= 70f)
                {
                    Cancelled = true;
                    onSweepDone = DoLaserAttack();
                    break;
                }

                if (Cancelled)
                {
                    break;
                }

                yield return null;
                fireTimer += Time.deltaTime;
            }
            var pAngle = GetPlayerAngle();

            if ((pAngle < rowAngleRange.x) || (pAngle > rowAngleRange.y))
            {
                Cancelled = true;
                onSweepDone = DoLaserAttack();
            }

            var camRect = Boss.CamRect;

            camRect.size += new Vector2(10f,10f);

            if (!camRect.Contains(transform.position))
            {
                Cancelled = true;
            }

            travelingLeft = !travelingLeft;

            if (Cancelled)
            {
                break;
            }
        }

        if (onSweepDone != null)
        {
            yield return onSweepDone;
        }

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Super Quick");

        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead(Boss.Head.ShotgunLasers.GetCurrentHeadAngle());
        }
        onSweepDone = null;
        onStun = null;

    }

    IEnumerator DoLaserAttack()
    {
        var laserPlayerAngle = GetPlayerAngleClamped();
        Quaternion angle2 = Quaternion.Euler(0f, 0f, 0f - 90f);

        var destAngle = laserPlayerAngle < 0f ? -120f : 120f;
        Quaternion angle1 = Quaternion.Euler(0f, 0f, destAngle - 90f);

        Boss.Head.UnlockHead(Boss.Head.ShotgunLasers.GetCurrentHeadAngle());

        onStun = laserMove.OnStun;
        yield return laserMove.SweepLaser(new BasicSweepController(
            angle1,
            angle2,
            1f,
            true,
            laserMove.followPlayerCurve
            ));
    }

    public override float GetPostDelay(int prevHealth) => postDelay;

    void FireProjectile(Vector3 position, Quaternion rotation, float extraVelocity)
    {
        var instance = Pooling.Instantiate(shotPrefab, position, Quaternion.identity);
        Vector2 targetDirection = rotation * Vector2.right;
        instance.GetComponent<Rigidbody2D>().velocity = targetDirection * shotFireVelocity * extraVelocity;
    }

    float GetTargetAngle(float t)
    {
        if (travelingLeft)
        {
            return Mathf.Lerp(rowAngleRange.y,rowAngleRange.x,t);
        }
        else
        {
            return Mathf.Lerp(rowAngleRange.x,rowAngleRange.y,t);
        }
    }

    float GetPlayerAngleClamped()
    {
        var playerAngle = MathUtilities.ClampRotation(Boss.GetAngleToPlayer() + 90f);

        if (playerAngle < rowAngleRange.x)
        {
            playerAngle = rowAngleRange.x;
        }

        if (playerAngle > rowAngleRange.y)
        {
            playerAngle = rowAngleRange.y;
        }
        return playerAngle;
    }

    float GetPlayerAngle()
    {
        return MathUtilities.ClampRotation(Boss.GetAngleToPlayer() + 90f);
    }

    public override void OnStun()
    {
        if (onStun != null)
        {
            onStun();
        }

        Boss.Head.Animator.StopCurrentAnimation();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }

}
