using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class MultiSweepLaserMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;

    public override bool MoveEnabled => moveEnabled &&
                Boss.CanSeeTarget &&
        Boss.CurrentRunningMode == Boss.OffensiveMode &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;

    [SerializeField]
    bool shootVomitShots = true;

    [SerializeField]
    Vector2[] vomitShotOffsets;

    [SerializeField]
    float vomitShotTravelTime = 0.65f;

    VomitShotMove vomitShotMove;
    FireLaserMove laserMove;

    MoveState currentMoveState = MoveState.None;

    [SerializeField]
    float laserFireDuration = 5f;

    [SerializeField]
    float laserAcceleration = 60f;

    [SerializeField]
    float laserInitialVelocity = 0f;

    [SerializeField]
    float maximumLaserVelocity = 90f;

    [SerializeField]
    float startAngleFromPlayer = -40f;

    enum MoveState
    {
        None,
        VomitShots,
        Laser
    }

    private void Awake()
    {
        vomitShotMove = GetComponent<VomitShotMove>();
        laserMove = GetComponent<FireLaserMove>();
    }


    protected override IEnumerator OnExecute()
    {
        if (shootVomitShots)
        {
            currentMoveState = MoveState.VomitShots;
            yield return vomitShotMove.FireVomitShots(5, index =>
            {
                var target = (Vector2)Player.Player1.transform.position + vomitShotOffsets[index];

                return MathUtilities.CalculateVelocityToReachPoint(Boss.Head.transform.position, target, vomitShotTravelTime, vomitShotMove.ShotGravityScale);
            });
        }


        currentMoveState = MoveState.Laser;

        float startAngle;

        if (Player.Player1.transform.position.x >= transform.position.x)
        {
            startAngle = Boss.GetAngleToPlayer() + startAngleFromPlayer;
        }
        else
        {
            startAngle = Boss.GetAngleToPlayer() - startAngleFromPlayer;
        }

        if (!Cancelled)
        {
            yield return laserMove.SweepLaser(new MultiSweepController(laserFireDuration, laserAcceleration, startAngle, laserInitialVelocity, maximumLaserVelocity));
        }

        currentMoveState = MoveState.None;

    }

    public override void OnStun()
    {
        switch (currentMoveState)
        {
            case MoveState.None:
                break;
            case MoveState.VomitShots:
                vomitShotMove.OnStun();
                break;
            case MoveState.Laser:
                laserMove.OnStun();
                break;
            default:
                break;
        }

        currentMoveState = MoveState.None;
    }

    public override void StopMove()
    {
        switch (currentMoveState)
        {
            case MoveState.None:
                break;
            case MoveState.VomitShots:
                vomitShotMove.StopMove();
                break;
            case MoveState.Laser:
                laserMove.StopMove();
                break;
        }
        base.StopMove();
    }
}
