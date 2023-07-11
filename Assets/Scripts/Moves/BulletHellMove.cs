using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class BulletHellMove : AncientAspidMove
{
    public override bool MoveEnabled => Boss.CanSeeTarget && moveEnabled &&
        Boss.AspidMode == AncientAspid.Mode.Offensive &&
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

    bool firingLaser = false;

    Recoiler recoiler;

    float oldRecoilSpeed = 0f;

    private void Awake()
    {
        recoiler = GetComponent<Recoiler>();
        laserMove = GetComponent<FireLaserMove>();
    }

    public override IEnumerator DoMove()
    {
        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left);

        travelingLeft = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser Antic");

        oldRecoilSpeed = recoiler.GetRecoilSpeed();

        recoiler.SetRecoilSpeed(0f);

        int oldIndex = -1;

        var playerAngleClamped = GetPlayerAngleClamped();
        float totalTime = 0f;

        for (int i = 0; i < rowsToShoot; i++)
        {
            totalTime += Mathf.Lerp(rowTimeRange.x, rowTimeRange.y, i / (float)(rowsToShoot - 1));
        }

        float audioTimer = 0f;

        for (int i = 0; i < rowsToShoot; i++)
        {
            var rowTime = Mathf.Lerp(rowTimeRange.x, rowTimeRange.y,i / (float)(rowsToShoot - 1));

            Quaternion from = Quaternion.Euler(0f,0f, playerAngleClamped + GetTargetAngle(0f) - 90f);
            Quaternion to = Quaternion.Euler(0f,0f, playerAngleClamped + GetTargetAngle(1f) - 90f);

            float fireRate = (1f / shotsPerRow) * rowTime;

            float fireTimer = 0f;

            float audioRate = Mathf.Lerp(spitPlaybackRateRange.x,spitPlaybackRateRange.y, i / (float)(rowsToShoot - 1));
            float audioPitch = Mathf.Lerp(spitSoundPitchRange.x, spitSoundPitchRange.y, i / (float)(rowsToShoot - 1));

            bool doLaserAttack = false;

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

                var targetRotation = Quaternion.Slerp(from, to, t / rowTime);
                var angle = laserMove.CalculateLaserRotation(targetRotation);
                laserMove.SetLaserRotation(0f, angle.extra);
                var spriteIndex = laserMove.GetHeadIndexForAngle(0f);
                if (spriteIndex != oldIndex)
                {
                    oldIndex = spriteIndex;

                    Boss.Head.MainRenderer.sprite = laserMove.head_Sprites[spriteIndex];
                    Boss.Head.MainRenderer.flipX = laserMove.head_HorizFlip[spriteIndex];
                }

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

                if ((currentPlayerAngle < rowAngleRange.x && !travelingLeft) || (currentPlayerAngle > rowAngleRange.y && travelingLeft))
                {
                    doLaserAttack = true;
                    break;
                }

                yield return null;
                fireTimer += Time.deltaTime;
            }

            var currentAngle = GetTargetAngle(1f);
            var playerAngle = GetPlayerAngle();

            //Debug.Log("PLAYER ANGLE + " + playerAngle);
            //Debug.Log("CURRENT ANGLE = " + currentAngle);
            //Debug.Log("ROW X = " + rowAngleRange.x);
            //Debug.Log("ROW Y = " + rowAngleRange.y);

            bool cancelAttack = false;

            //TODO : FIRE LASER IF PLAYER IS OUTSIDE OF RAPID FIRE
            if (playerAngle < playerAngleClamped + rowAngleRange.x && currentAngle < rowAngleRange.x + 0.2f)
            {
                doLaserAttack = true;
                cancelAttack = true;
            }
            else if (playerAngle > playerAngleClamped + rowAngleRange.y && currentAngle > rowAngleRange.y - 0.2f)
            {
                doLaserAttack = true;
                cancelAttack = true;
            }

            if (Vector3.Distance(transform.position, Player.Player1.transform.position) >= 25)
            {
                cancelAttack = true;
            }

            if (doLaserAttack)
            {
                recoiler.SetRecoilSpeed(oldRecoilSpeed);
                //float directionScalar = playerAngle >= 0f ? 1f : -1f;

                var startAngle = playerAngle < 0f ? rowAngleRange.x : rowAngleRange.y;

                Quaternion start = Quaternion.Euler(0f,0f, playerAngleClamped + startAngle - 90f);

                //var playerTarget = Boss.GetAngleToPlayer() + (directionScalar * 30f);

                //Quaternion destination = Quaternion.Euler(0f, 0f, playerTarget);

                var destAngle = playerAngle < 0f ? -120f : 120f;
                Quaternion destination = Quaternion.Euler(0f,0f,destAngle - 90f);

                Boss.Head.UnlockHead(laserMove.GetLaserRotationValues().main);
                firingLaser = true;
                yield return laserMove.SweepLaser(new BasicSweepController(
                    start,
                    destination,
                    1f,
                    laserMove.followPlayerCurve
                    ), false, 0f);
                firingLaser = false;
                cancelAttack = true;
            }

            travelingLeft = !travelingLeft;

            if (cancelAttack || !Boss.CanSeeTarget || !Boss.OffensiveModeEnabled)
            {
                if (Boss.Head.HeadLocked)
                {
                    Boss.Head.UnlockHead(laserMove.GetLaserRotationValues().main);
                }
                recoiler.SetRecoilSpeed(oldRecoilSpeed);
                yield break;
            }
        }
        recoiler.SetRecoilSpeed(oldRecoilSpeed);

        yield return laserMove.FinishLaserMove();

        //Boss.Head.UnlockHead(laserMove.GetLaserRotationValues().main);
    }

    public override float PostDelay => postDelay;

    void FireProjectile(Vector3 position, Quaternion rotation, float extraVelocity)
    {
        var instance = Pooling.Instantiate(shotPrefab, position, Quaternion.identity);
        Vector2 targetDirection = rotation * Vector2.right;
        //Vector2 targetDirection = (rotationReferencePos + (rotation * toPlayerVector)) - position;
        //Vector2 direction = rotation * Vector2.right;
        instance.GetComponent<Rigidbody2D>().velocity = targetDirection * shotFireVelocity * extraVelocity;
    }

    /*void FireProjectileOLD(Vector3 position, Quaternion rotation, Vector3 toPlayerVector, Vector3 rotationReferencePos, float extraVelocity)
    {
        var instance = Pooling.Instantiate(shotPrefab, position, Quaternion.identity);
        Vector2 targetDirection = rotation * Vector2.right;
        //Vector2 targetDirection = (rotationReferencePos + (rotation * toPlayerVector)) - position;
        //Vector2 direction = rotation * Vector2.right;
        instance.GetComponent<Rigidbody2D>().velocity = targetDirection * shotFireVelocity * extraVelocity;
    }*/

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

        if (playerAngle + rowAngleRange.x < -90f)
        {
            playerAngle = -90f - rowAngleRange.x;
        }
        else if (playerAngle + rowAngleRange.y > 90f)
        {
            playerAngle = 90f - rowAngleRange.y;
        }
        return playerAngle;
    }

    float GetPlayerAngle()
    {
        return MathUtilities.ClampRotation(Boss.GetAngleToPlayer() + 90f);
    }

    public override void OnStun()
    {
        if (firingLaser)
        {
            laserMove.OnStun();
        }
        else
        {
            recoiler.SetRecoilSpeed(oldRecoilSpeed);
        }
        Boss.Head.Animator.StopCurrentAnimation();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
