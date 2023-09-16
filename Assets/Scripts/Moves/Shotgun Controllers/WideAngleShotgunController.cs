using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore;
using WeaverCore.Utilities;

public class WideAngleShotgunController : ShotgunController
{
    //VomitGlob[] globs;
    //float aimInterpSpeed;

    //Vector3[] currentLaserTargets;

    //bool aimAtPlayer = false;

    //bool ending = false;

    AncientAspid Boss;

    //float[] laserAngles = new float[5];
    Quaternion[] startLaserAngles = new Quaternion[5];
    Quaternion[] currentLaserAngles = new Quaternion[5];
    Quaternion[] endLaserAngles = new Quaternion[5];

    float laserTime;
    float laserTimer = 0f;

    LaserShotgunMove shotgunMove;

    static float ClampAngle(float angle)
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
            var angleToPlayer = ClampAngle(Boss.GetAngleToPlayer());

            var leftSideAngle = ClampAngle(Boss.GetAngleToTarget(shotgunMove.GetFarAwayLaserTarget(0)));
            var rightSideAngle = ClampAngle(Boss.GetAngleToTarget(shotgunMove.GetFarAwayLaserTarget(4)));

            //var leftSideAngle = ClampAngle(currentLaserAngles[0].eulerAngles.z);
            //var rightSideAngle = ClampAngle(currentLaserAngles[4].eulerAngles.z);

            return angleToPlayer > leftSideAngle - 15f && angleToPlayer < rightSideAngle + 15f;
        }
    }

    public WideAngleShotgunController(AncientAspid boss, Vector3 playerTarget, float destAngle = float.NaN, float laserTime = 1.5f, float angleSeparation = 10f, float angleFromPlayer = 10f)
    {
        shotgunMove = boss.GetComponent<LaserShotgunMove>();

        Boss = boss;
        this.laserTime = laserTime;
        laserTimer = 0f;

        var angleToPlayer = Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(playerTarget).x);
        Quaternion angleToDest;

        if (float.IsNaN(destAngle))
        {
            if (Player.Player1.transform.position.x < Boss.transform.position.x)
            {
                angleToDest = angleToPlayer * Quaternion.Euler(0f, 0f, 50f);
            }
            else
            {
                angleToDest = angleToPlayer * Quaternion.Euler(0f, 0f, -50f);
            }
        }
        else
        {
            angleToDest = Quaternion.Euler(0f,0f, destAngle);
        }

        const float angleAfterDest = 10f;

        if (Player.Player1.transform.position.x < Boss.transform.position.x)
        {
            var leftAngleToPlayer = Quaternion.Euler(0f, 0f, shotgunMove.GetAngleToTargetFromLaser(2,playerTarget));
            //var rightAngleToPlayer = Quaternion.Euler(0f, 0f, shotgunMove.GetAngleToTargetFromLaser(3,playerTarget));

            Debug.Log("ON LEFT");
            startLaserAngles[0] = leftAngleToPlayer * Quaternion.Euler(0f,0f, -angleFromPlayer + (-angleSeparation * 3f));
            startLaserAngles[1] = leftAngleToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer + (-angleSeparation * 2f));
            startLaserAngles[2] = leftAngleToPlayer * Quaternion.Euler(0f,0f, -angleFromPlayer + (-angleSeparation * 1f));

            var rightStartAngle = angleToDest * Quaternion.Euler(0f, 0f, angleAfterDest);

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
            var rightAngleToPlayer = Quaternion.Euler(0f, 0f, shotgunMove.GetAngleToTargetFromLaser(2, playerTarget));
            Debug.Log("ON RIGHT");

            startLaserAngles[2] = rightAngleToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 1f));
            startLaserAngles[3] = rightAngleToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 2f));
            startLaserAngles[4] = rightAngleToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 3f));

            var leftStartAngle = angleToDest * Quaternion.Euler(0f, 0f, -angleAfterDest);

            startLaserAngles[0] = leftStartAngle * Quaternion.Euler(0f, 0f, -angleSeparation * 2f);
            startLaserAngles[1] = leftStartAngle * Quaternion.Euler(0f, 0f, -angleSeparation);

            endLaserAngles[0] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation * 2f);
            endLaserAngles[1] = angleToDest * Quaternion.Euler(0f, 0f, -angleSeparation);

            endLaserAngles[2] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 1f);
            endLaserAngles[3] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 2f);
            endLaserAngles[4] = angleToDest * Quaternion.Euler(0f, 0f, angleSeparation * 3f);
        }

        
        
        //currentLaserAngles[2] = Quaternion.Euler(0f, 0f, playerAngle);

        for (int i = 0; i < currentLaserAngles.Length; i++)
        {
            currentLaserAngles[i] = startLaserAngles[i];
        }

        /*this.globs = globs;
        currentLaserTargets = new Vector3[globs.Length];
        this.aimInterpSpeed = aimInterpSpeed;*/
    }

    public override bool DoScaleFlip() => false;

    public override bool LaserEnabled(int laserIndex)
    {
        //return laserIndex != 2;
        return true;
    }

    /*public override void ChangeMode(LaserMode mode)
    {
        if (mode == LaserMode.Firing)
        {
            aimAtPlayer = true;
        }

        if (mode == LaserMode.Ending)
        {
            ending = true;
        }
        base.ChangeMode(mode);
    }*/

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

        /*for (int i = 0; i < laserAngles.Length; i++)
        {
            laserAngles[i] = Mathf.LerpAngle()
        }*/
        /*if (aimAtPlayer && !ending)
        {
            for (int i = 0; i < currentLaserTargets.Length; i++)
            {
                currentLaserTargets[i] = Vector3.MoveTowards(currentLaserTargets[i], Player.Player1.transform.position, aimInterpSpeed * Time.deltaTime);
            }
        }*/
    }

    public override Quaternion GetLaserRotation(int laserIndex)
    {
        return currentLaserAngles[laserIndex];
        /*if (!aimAtPlayer)
        {
            currentLaserTargets[laserIndex] = globs[laserIndex].transform.position;
        }

        var rotation = AimLaserAtTarget(Lasers[laserIndex], currentLaserTargets[laserIndex]);

        return Quaternion.Euler(0f, 0f, rotation);*/
    }
}

