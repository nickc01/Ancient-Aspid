using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class WideAngleShotgunController : ShotgunController
{
    private AncientAspid Boss;
    private Quaternion[] startLaserAngles = new Quaternion[5];
    private Quaternion[] currentLaserAngles = new Quaternion[5];
    private Quaternion[] endLaserAngles = new Quaternion[5];
    private float laserTime;
    private float laserTimer = 0f;
    private LaserShotgunMove shotgunMove;

    private static float ClampAngle(float angle)
    {
        if (angle > 90f)
        {
            angle -= 360f;
        }
        return angle;
    }

    public bool PlayerWithinLasers
    {
        get
        {
            float angleToPlayer = ClampAngle(Boss.GetAngleToPlayer());

            float leftSideAngle = ClampAngle(Boss.GetAngleToTarget(shotgunMove.GetFarAwayLaserTarget(0)));
            float rightSideAngle = ClampAngle(Boss.GetAngleToTarget(shotgunMove.GetFarAwayLaserTarget(4)));

            return angleToPlayer > leftSideAngle - 15f && angleToPlayer < rightSideAngle + 15f;
        }
    }

    public WideAngleShotgunController(AncientAspid boss, Vector3 playerTarget, float destAngle = float.NaN, float laserTime = 1.5f, float angleSeparation = 10f, float angleFromPlayer = 10f)
    {
        shotgunMove = boss.GetComponent<LaserShotgunMove>();

        Boss = boss;
        this.laserTime = laserTime;
        laserTimer = 0f;

        Quaternion angleToPlayer = Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(playerTarget).x);
        Quaternion angleToDest;

        if (float.IsNaN(destAngle))
        {
            angleToDest = Player.Player1.transform.position.x < Boss.transform.position.x
                ? angleToPlayer * Quaternion.Euler(0f, 0f, 50f)
                : angleToPlayer * Quaternion.Euler(0f, 0f, -50f);
        }
        else
        {
            angleToDest = Quaternion.Euler(0f, 0f, destAngle);
        }

        const float angleAfterDest = 10f;

        if (Player.Player1.transform.position.x < Boss.transform.position.x)
        {
            Quaternion leftAngleToPlayer = Quaternion.Euler(0f, 0f, shotgunMove.GetAngleToTargetFromLaser(2, playerTarget));
            startLaserAngles[0] = leftAngleToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer + (-angleSeparation * 3f));
            startLaserAngles[1] = leftAngleToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer + (-angleSeparation * 2f));
            startLaserAngles[2] = leftAngleToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer + (-angleSeparation * 1f));

            Quaternion rightStartAngle = angleToDest * Quaternion.Euler(0f, 0f, angleAfterDest);

            startLaserAngles[3] = rightStartAngle * Quaternion.Euler(0f, 0f, angleSeparation);
            startLaserAngles[4] = rightStartAngle * Quaternion.Euler(0f, 0f, angleSeparation * 2f);

            endLaserAngles[0] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation * 3f);
            endLaserAngles[1] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation * 2f);
            endLaserAngles[2] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation * 1f);

            endLaserAngles[3] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation);
            endLaserAngles[4] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 2f);
        }
        else
        {
            Quaternion rightAngleToPlayer = Quaternion.Euler(0f, 0f, shotgunMove.GetAngleToTargetFromLaser(2, playerTarget));

            startLaserAngles[2] = rightAngleToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 1f));
            startLaserAngles[3] = rightAngleToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 2f));
            startLaserAngles[4] = rightAngleToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 3f));

            Quaternion leftStartAngle = angleToDest * Quaternion.Euler(0f, 0f, -angleAfterDest);

            startLaserAngles[0] = leftStartAngle * Quaternion.Euler(0f, 0f, -angleSeparation * 2f);
            startLaserAngles[1] = leftStartAngle * Quaternion.Euler(0f, 0f, -angleSeparation);

            endLaserAngles[0] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation * 2f);
            endLaserAngles[1] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation);

            endLaserAngles[2] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 1f);
            endLaserAngles[3] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 2f);
            endLaserAngles[4] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 3f);
        }



        for (int i = 0; i < currentLaserAngles.Length; i++)
        {
            currentLaserAngles[i] = startLaserAngles[i];
        }

    }

    public override bool DoScaleFlip()
    {
        return false;
    }

    public override bool LaserEnabled(int laserIndex)
    {
        return true;
    }

    public override void Update()
    {
        if (CurrentMode == LaserMode.Firing)
        {
            for (int i = 0; i < currentLaserAngles.Length; i++)
            {
                currentLaserAngles[i] = Quaternion.Lerp(startLaserAngles[i], endLaserAngles[i], laserTimer / laserTime);
            }

            laserTimer += Time.deltaTime;
        }

    }

    public override Quaternion GetLaserRotation(int laserIndex)
    {
        return currentLaserAngles[laserIndex];
    }
}

