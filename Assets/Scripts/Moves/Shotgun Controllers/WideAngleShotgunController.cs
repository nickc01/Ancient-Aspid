using System;
using System.Linq;
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

    bool rightToLeft = false;
    bool flipped = false;
    //private LaserShotgunMove shotgunMove;

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
            var centerLaser = Boss.Head.ShotgunLasers.MiddleLaser;

            var toPlayer = (Vector2)(Player.Player1.transform.position - centerLaser.transform.position);

            var toPlayerAngle = MathUtilities.CartesianToPolar(toPlayer).x;
            if (toPlayerAngle > 90f)
            {
                toPlayerAngle -= 360f;
            }

            float leftSideAngle = 0f;
            float rightSideAngle = 0f;

            leftSideAngle = currentLaserAngles[0].eulerAngles.z;
            rightSideAngle = currentLaserAngles[4].eulerAngles.z;

            if (flipped)
            {
                var temp = leftSideAngle;
                leftSideAngle = rightSideAngle;
                rightSideAngle = temp;

                //currentLaserAngles[i] = Quaternion.Lerp(startLaserAngles[i], endLaserAngles[i], laserTimer / laserTime);
            }

            if (leftSideAngle > 90f)
            {
                leftSideAngle -= 360f;
            }

            if (rightSideAngle > 90f)
            {
                rightSideAngle -= 360f;
            }


            //WeaverLog.Log("Left Angle = " + leftSideAngle);
            //WeaverLog.Log("Player Angle = " + toPlayerAngle);
            //WeaverLog.Log("Right Angle = " + rightSideAngle);
            //WeaverLog.Log("Within = " + (toPlayerAngle > leftSideAngle - 15f && toPlayerAngle < rightSideAngle + 15f));

            return toPlayerAngle > leftSideAngle && toPlayerAngle < rightSideAngle;


            /*float angleToPlayer = ClampAngle(Boss.GetAngleToPlayer());

            float leftSideAngle, rightSideAngle;

            if (startingOnRight)
            {
                leftSideAngle = ClampAngle(Boss.GetAngleToTarget(Boss.Head.ShotgunLasers.GetFarAwayLaserTargetAtAngle(4, currentLaserAngles[4].eulerAngles.z)));
                rightSideAngle = ClampAngle(Boss.GetAngleToTarget(Boss.Head.ShotgunLasers.GetFarAwayLaserTargetAtAngle(0, currentLaserAngles[0].eulerAngles.z)));
            }
            else
            {
                leftSideAngle = ClampAngle(Boss.GetAngleToTarget(Boss.Head.ShotgunLasers.GetFarAwayLaserTargetAtAngle(0, currentLaserAngles[0].eulerAngles.z)));
                rightSideAngle = ClampAngle(Boss.GetAngleToTarget(Boss.Head.ShotgunLasers.GetFarAwayLaserTargetAtAngle(4, currentLaserAngles[4].eulerAngles.z)));
            }

            //WeaverLog.Log("ANGLE TO PLAYER = " + angleToPlayer);
            //WeaverLog.Log("LeftSide = " + (leftSideAngle - 15f));
            //WeaverLog.Log("RightSide = " + (rightSideAngle + 15f));

            return angleToPlayer > leftSideAngle - 15f && angleToPlayer < rightSideAngle + 15f;*/
        }
    }

    public WideAngleShotgunController(AncientAspid boss, Vector3 playerTarget, float destAmount, float destinationAngleOffset, float laserTime, float angleSeparation, float angleFromPlayer)
    {
        Boss = boss;
        var centerLaser = Boss.Head.ShotgunLasers.MiddleLaser;
        this.laserTime = laserTime;

        var toPlayer = (Vector2)(playerTarget - centerLaser.transform.position);

        var toPlayerAngle = MathUtilities.CartesianToPolar(toPlayer).x;
        if (toPlayerAngle > 90f)
        {
            toPlayerAngle -= 360f;
        }
        var dirToPlayer = Quaternion.Euler(0f, 0f, toPlayerAngle);


        Quaternion destinationAngle;

        rightToLeft = playerTarget.x >= Boss.Head.transform.position.x;

        //WeaverLog.Log("RIGHT TO LEFT MODE = " + rightToLeft);

        if (rightToLeft)
        {
            destinationAngle = Quaternion.Euler(0f, 0f, -90f - destAmount);
            startLaserAngles[0] = destinationAngle * Quaternion.Euler(0f, 0f, -(angleSeparation * 2f) + destinationAngleOffset);
            startLaserAngles[1] = destinationAngle * Quaternion.Euler(0f, 0f, -(angleSeparation * 1f) + destinationAngleOffset);
            //Player on right Side
            startLaserAngles[2] = dirToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 1f));
            startLaserAngles[3] = dirToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 2f));
            startLaserAngles[4] = dirToPlayer * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 3f));

            endLaserAngles[0] = startLaserAngles[0];
            endLaserAngles[1] = startLaserAngles[1];

            endLaserAngles[2] = destinationAngle * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 1f) - destinationAngleOffset);
            endLaserAngles[3] = destinationAngle * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 2f) - destinationAngleOffset);
            endLaserAngles[4] = destinationAngle * Quaternion.Euler(0f, 0f, angleFromPlayer + (angleSeparation * 3f) - destinationAngleOffset);
        }
        else
        {
            destinationAngle = Quaternion.Euler(0f, 0f, -90f + destAmount);

            startLaserAngles[0] = dirToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer - (angleSeparation * 3f));
            startLaserAngles[1] = dirToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer - (angleSeparation * 2f));
            startLaserAngles[2] = dirToPlayer * Quaternion.Euler(0f, 0f, -angleFromPlayer - (angleSeparation * 1f));
            //Player on left Side
            startLaserAngles[3] = destinationAngle * Quaternion.Euler(0f, 0f, (angleSeparation * 1f) - destinationAngleOffset);
            startLaserAngles[4] = destinationAngle * Quaternion.Euler(0f, 0f, (angleSeparation * 2f) - destinationAngleOffset);

            endLaserAngles[0] = destinationAngle * Quaternion.Euler(0f, 0f, -angleFromPlayer - (angleSeparation * 3f) + destinationAngleOffset);
            endLaserAngles[1] = destinationAngle * Quaternion.Euler(0f, 0f, -angleFromPlayer - (angleSeparation * 2f) + destinationAngleOffset);
            endLaserAngles[2] = destinationAngle * Quaternion.Euler(0f, 0f, -angleFromPlayer - (angleSeparation * 1f) + destinationAngleOffset);

            endLaserAngles[3] = startLaserAngles[3];
            endLaserAngles[4] = startLaserAngles[4];
        }


        flipped = rightToLeft;


        if (flipped)
        {
            Array.Reverse(startLaserAngles);
            Array.Reverse(endLaserAngles);
        }


        for (int i = 0; i < currentLaserAngles.Length; i++)
        {
            currentLaserAngles[i] = startLaserAngles[i];
        }
    }
    /*
    public void OLDCONSTRUCTOR(AncientAspid boss, Vector3 playerTarget, float destAngle = float.NaN, float laserTime = 1.5f, float angleSeparation = 10f, float angleFromPlayer = 10f)
    {
        //shotgunMove = boss.GetComponent<LaserShotgunMove>();

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

        rightToLeft = Player.Player1.transform.position.x >= Boss.transform.position.x;

        if (Player.Player1.transform.position.x < Boss.transform.position.x)
        {
            Quaternion leftAngleToPlayer = Quaternion.Euler(0f, 0f, Boss.Head.ShotgunLasers.GetAngleToTargetFromLaser(2, playerTarget));
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
            Quaternion rightAngleToPlayer = Quaternion.Euler(0f, 0f, Boss.Head.ShotgunLasers.GetAngleToTargetFromLaser(2, playerTarget));

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

    }
    */
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
                var result = Quaternion.Slerp(startLaserAngles[i], endLaserAngles[i], laserTimer / laserTime);

                currentLaserAngles[i] = result;
                /*if (rightToLeft)
                {
                    currentLaserAngles[currentLaserAngles.Length - 1 - i] = Quaternion.Lerp(startLaserAngles[i], endLaserAngles[i], laserTimer / laserTime);
                }
                else
                {
                    
                }*/
            }

            laserTimer += Time.deltaTime;
        }

    }

    public override Quaternion GetLaserRotation(int laserIndex)
    {
        return currentLaserAngles[laserIndex];
    }
}

