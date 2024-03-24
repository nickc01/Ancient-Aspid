using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;

public class WideAngleShotgunMove : AncientAspidMove
{
    [SerializeField]
    Vector2 rightSidePos = new Vector2(231.62f, 204.21f);

    [SerializeField]
    Vector2 leftSidePos = new Vector2(206.05f, 204.21f);

    [SerializeField]
    float laserTime = 1.5f;

    [SerializeField]
    float angleSeparation = 10f;

    [SerializeField]
    float angleFromPlayer = 10f;

    [SerializeField]
    float bombAirTime = 0.5f;

    [SerializeField]
    float bombSize = 1.5f;

    [SerializeField]
    float bombHeadLockSpeed = 2f;

    [SerializeField]
    float destinationAngleOffset = 15f;

    [SerializeField]
    Sprite fireSprite;

    [SerializeField]
    Vector2 laserOffset = new Vector2(0f,-0.66f);

    [SerializeField]
    float destinationAngle = 55f;

    [SerializeField]
    bool enableLasers = true;

    public override bool MoveEnabled
    {
        get
        {
            var enabled = Boss.CanSeeTarget &&
                Boss.CurrentRunningMode == Boss.OffensiveMode &&
                Player.Player1.transform.position.y <= Boss.Head.transform.position.y - 1f &&
                Vector3.Distance(Player.Player1.transform.position, transform.position) <= 25f;


            var leftSide = -90f - destinationAngle + 10f;
            var rightSide = -90f + destinationAngle - 10f;

            var playerAngle = MathUtilities.CartesianToPolar(Player.Player1.transform.position - Boss.Head.transform.position).x;

            if (playerAngle > 90f)
            {
                playerAngle -= 360f;
            }

            //WeaverLog.Log("Left Side = " + leftSide);
            //WeaverLog.Log("Right Side = " + rightSide);
            //WeaverLog.Log("PlayerAngle = " + playerAngle);

            enabled = enabled && (playerAngle >= leftSide && playerAngle <= rightSide);
            //WeaverLog.Log("Wide Angle 1 = " + (Boss.CanSeeTarget));
            //WeaverLog.Log("Wide Angle 2 = " + (Boss.CurrentRunningMode == Boss.OffensiveMode));
            //WeaverLog.Log("Wide Angle 3 = " + (Player.Player1.transform.position.y <= Boss.Head.transform.position.y - 4f));
            //WeaverLog.Log("Wide Angle 4 = " + (Player.Player1.transform.position.x >= Boss.Head.transform.position.x + 2f));
            //WeaverLog.Log("Wide Angle 5 = " + (Player.Player1.transform.position.x <= Boss.Head.transform.position.x - 2f));

            //WeaverLog.Log("WIDE ANGLE ENABLED = " + enabled);
            return enabled;
            enabled = Boss.CanSeeTarget &&
        Boss.CurrentRunningMode == Boss.OffensiveMode &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f && Player.Player1.transform.position.y <= Boss.transform.position.y - 1.5f;
            return enabled;

            //return Boss.CurrentRunningMode == Boss.OffensiveMode;
        }
    }

    bool firingLaser = false;

    LaserShotgunMove shotgunMove;
    BombMove bombMove;

    private void Awake()
    {
        shotgunMove = GetComponent<LaserShotgunMove>();
        bombMove = GetComponent<BombMove>();
    }

    protected override IEnumerator OnExecute()
    {
        Vector2 destPos;

        if (Player.Player1.transform.position.x < Boss.transform.position.x)
        {
            destPos = rightSidePos;
        }
        else
        {
            destPos = leftSidePos;
        }

        /*var angleToDest = MathUtilities.CartesianToPolar(destPos - (Vector2)Boss.Head.transform.position).x;

        if (angleToDest > 180f)
        {
            angleToDest -= 360f;
        }*/

        firingLaser = true;


        float endTime = 9999f;
        float startTime = 0;
        if (enableLasers)
        {
            bool shotgunFinished = false;
            var controller = new WideAngleShotgunController(Boss, Player.Player1.transform.position, destinationAngle, destinationAngleOffset, laserTime, angleSeparation, angleFromPlayer);
            IEnumerator LaserRoutine()
            {
                if (controller.PlayerWithinLasers)
                {
                    yield return shotgunMove.DoShotgunLaser(controller, 0f, laserTime, true, fireSprite, laserOffset);
                }
                shotgunFinished = true;
            }

            startTime = Time.time;

            Boss.StartBoundRoutine(LaserRoutine());

            while (!shotgunFinished)
            {
                if (!controller.PlayerWithinLasers && Time.time > startTime + 1.1f)
                {
                    shotgunMove.CancelLaserAttack = true;
                }
                yield return null;
            }

            endTime = Time.time;
            var onRightSide = Player.Player1.transform.position.x >= Boss.transform.position.x;

            Boss.Head.MainRenderer.flipX = onRightSide;

            if (Boss.Head.HeadLocked)
            {
                Boss.Head.UnlockHead(0f);
            }

        }
        firingLaser = false;

        yield return Boss.Head.LockHead(Player.Player1.transform.position.x >= Boss.transform.position.x ? AspidOrientation.Right : AspidOrientation.Left, bombHeadLockSpeed);

        if (!enableLasers)
        {
            bool onRightSide = Player.Player1.transform.position.x >= Boss.Head.transform.position.x;

            yield return bombMove.FireBombs(new TargetBombController(bombAirTime, bombMove.BombGravityScale, bombSize, (onRightSide ? new Vector3(8.06f, -9.7035f) : new Vector3(-8.06f, -9.7035f)) + transform.position), false);
        }
        else
        {
            if (!Cancelled && endTime - startTime >= 0.5f)
            {
                yield return bombMove.FireBombs(new DirectBombController(bombAirTime, bombMove.BombGravityScale, bombSize), false);
            }
        }

        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }

    public override void OnStun()
    {
        if (firingLaser)
        {
            shotgunMove.OnStun();
        }
        else
        {
            bombMove.OnStun();
        }
    }

    public override void StopMove()
    {
        base.StopMove();
        if (firingLaser)
        {
            shotgunMove.StopMove();
        }
        else
        {
            bombMove.StopMove();
        }
    }
}
