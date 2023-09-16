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
    Sprite fireSprite;

    [SerializeField]
    Vector2 laserOffset = new Vector2(0f,-0.66f);

    public override bool MoveEnabled
    {
        get
        {
            var enabled = Boss.CanSeeTarget &&
        Boss.AspidMode == AncientAspid.Mode.Offensive &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;
            return enabled;
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

    public override IEnumerator DoMove()
    {
        //yield return shotgunMove.DoShotgunLaser(new WideAngleShotgunController(Boss,Boss.GetAngleToPlayer()), 0f, 1.5f);

        var angleToPlayer = Boss.GetAngleToPlayer();

        if (angleToPlayer > 180f)
        {
            angleToPlayer -= 360f;
        }


        Vector2 destPos;

        if (Player.Player1.transform.position.x < Boss.transform.position.x)
        {
            destPos = rightSidePos;
        }
        else
        {
            destPos = leftSidePos;
        }

        var angleToDest = MathUtilities.CartesianToPolar(destPos - (Vector2)Boss.Head.transform.position).x;

        if (angleToDest > 180f)
        {
            angleToDest -= 360f;
        }

        firingLaser = true;

        bool shotgunFinished = false;

        var controller = new WideAngleShotgunController(Boss, Player.Player1.transform.position, angleToDest, laserTime, angleSeparation, angleFromPlayer);

        IEnumerator LaserRoutine()
        {
            yield return shotgunMove.DoShotgunLaser(controller, 0f, laserTime, true, fireSprite, laserOffset);
            shotgunFinished = true;
        }

        var startTime = Time.time;

        Boss.StartBoundRoutine(LaserRoutine());

        while (!shotgunFinished)
        {
            if (!controller.PlayerWithinLasers && Time.time > startTime + 1.1f)
            {
                shotgunMove.CancelLaserAttack = true;
            }
            yield return null;
        }

        var endTime = Time.time;
        /*Boss.Head.UnlockHeadImmediate(Player.Player1.transform.position.x >= Boss.transform.position.x ? AspidOrientation.Right : AspidOrientation.Left);*/

        var onRightSide = Player.Player1.transform.position.x >= Boss.transform.position.x;

        Boss.Head.MainRenderer.flipX = onRightSide;

        //yield return Boss.Head.Animator.PlayAnimationTillDone("Fire Laser End Quick");

        //Boss.Head.UnlockHeadImmediate(AspidOrientation.Center);
        Boss.Head.UnlockHead(0f);

        //Boss.Head.Animator.PlayAnimation("Fire Laser End Quick");

        yield return Boss.Head.LockHead(Player.Player1.transform.position.x >= Boss.transform.position.x ? AspidOrientation.Right : AspidOrientation.Left, bombHeadLockSpeed);

        firingLaser = false;

        if (endTime - startTime >= 0.5f)
        {
            yield return bombMove.FireBombs(new DirectBombController(bombAirTime, bombMove.BombGravityScale, bombSize), false);
        }

        if (Boss.Head.HeadLocked && !Boss.Head.HeadBeingUnlocked)
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
}
