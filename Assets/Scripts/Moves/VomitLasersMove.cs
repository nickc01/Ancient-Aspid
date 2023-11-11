using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

public class VomitLasersMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    Vector2[] vomitShotOffsets;

    [SerializeField]
    float vomitShotTravelTime = 0.65f;

    [SerializeField]
    float laserInterpSpeed = 5f;

    [SerializeField]
    float prepareTime = 0.75f;

    [SerializeField]
    float attackTime = 2f;

    public override bool MoveEnabled
    {
        get
        {
            var enabled = moveEnabled && 
                Boss.CanSeeTarget &&
        Boss.CurrentRunningMode == Boss.OffensiveMode &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 30f;

            //WeaverLog.LogError("RADIAL BULLET ENABLED = " + enabled);

            return enabled;
        }
    }

    VomitShotMove vomitShotMove;
    LaserShotgunMove shotgunMove;
    VomitGlob[] shots = null;

    int moveState = 0;

    private void Awake()
    {
        vomitShotMove = GetComponent<VomitShotMove>();
        shotgunMove = GetComponent<LaserShotgunMove>();
    }

    protected override IEnumerator OnExecute()
    {
        moveState = 1;

        yield return vomitShotMove.FireVomitShots(5, index =>
        {
            var target = (Vector2)Player.Player1.transform.position + vomitShotOffsets[index];

            return MathUtilities.CalculateVelocityToReachPoint(Boss.Head.transform.position, target, vomitShotTravelTime, vomitShotMove.ShotGravityScale);
        });

        moveState = 2;

        shots = vomitShotMove.FiredShots.ToArray();

        foreach (var shot in shots)
        {
            shot.ForceDisappear(prepareTime);
        }

        if (!Cancelled)
        {
            yield return shotgunMove.DoShotgunLaser(new VomitLasersShotgunController(shots, laserInterpSpeed), prepareTime, attackTime);
        }

        moveState = 0;

        shots = null;

        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }

    public override void StopMove()
    {
        base.StopMove();
        switch (moveState)
        {
            case 1:
                vomitShotMove.StopMove();
                break;
            case 2:
                shotgunMove.StopMove();
                break;
            default:
                break;
        }
    }

    public override void OnStun()
    {
        if (shots != null)
        {
            foreach (var shot in shots)
            {
                shot.ForceDisappear();
            }
            shots = null;
        }

        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }

        switch (moveState)
        {
            case 1:
                vomitShotMove.OnStun();
                break;
            case 2:
                shotgunMove.OnStun();
                break;
            default:
                break;
        }
    }
}
