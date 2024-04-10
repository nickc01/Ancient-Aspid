using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class ArenaSweepController : ILaserController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }
    public bool Flipped { get; private set; }
    public AnimationCurve Curve { get; private set; }
    public float FireTime { get; private set; }

    private bool startingOnRightSide;

    public ArenaSweepController(Quaternion from, Quaternion to, float fireTime, AnimationCurve curve = null)
    {
        Curve = curve;
        Curve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);
        From = from;
        To = to;
        FireTime = fireTime;
    }

    public ArenaSweepController(float fromBottomDegrees, float toBottomDegrees, float fireTime, AnimationCurve curve = null) : this(Quaternion.Euler(0f, 0f, fromBottomDegrees - 90f), Quaternion.Euler(0f, 0f, toBottomDegrees - 90f), fireTime, curve) { }

    public void Init(FireLaserMove laserMove)
    {
        Flipped = laserMove.Boss.Head.LookingDirection >= 0f;
    }

    public Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity)
    {
        float playerAngle = MathUtilities.ClampRotation(laserMove.Boss.GetAngleToPlayer() + 90f);

        float fromAngle = MathUtilities.ClampRotation(From.eulerAngles.z + 90f);
        float toAngle = MathUtilities.ClampRotation(To.eulerAngles.z + 90f);

        if (Flipped)
        {
            if (toAngle < playerAngle + 20f)
            {
                To *= Quaternion.Euler(0f, 0f, playerAngle + 20f - toAngle);
            }
        }
        else
        {
            if (fromAngle > playerAngle - 20f)
            {
                From *= Quaternion.Euler(0f, 0f, playerAngle - 20f - fromAngle);
            }
        }

        Quaternion angle = Flipped ? Quaternion.Slerp(To, From, Curve.Evaluate(0)) : Quaternion.Slerp(From, To, Curve.Evaluate(0));
        startingOnRightSide = laserMove.LaserOnRightSideOf(Player.Player1.transform.position, angle);

        return angle;
    }

    public bool CanFire(FireLaserMove laserMove)
    {
        return true;
        //return laserMove.IsLaserOriginVisible();
    }

    public IEnumerator Fire(FireLaserMove laserMove, Func<bool> moveCancelled, Action<Quaternion> setLaserAngle, Quaternion startAngle)
    {
        for (float t = 0; t < FireTime; t += Time.deltaTime)
        {

            if (t > 0.25f && moveCancelled())
            {
                break;
            }

            if (laserMove.Boss.CurrentRunningMode is TacticalMode && t > 0.5f && !laserMove.IsLaserOriginVisible())
            {
                break;
            }

            Quaternion angle = Flipped ? Quaternion.Slerp(To, From, Curve.Evaluate(t / FireTime)) : Quaternion.Slerp(From, To, Curve.Evaluate(t / FireTime));
            setLaserAngle(angle);

            /*if (t > 0.25f && laserMove.LaserOnRightSideOf(Player.Player1.transform.position, angle) != startingOnRightSide)
            {
                break;
            }*/

            yield return null;
        }
    }

    public void OnStun(FireLaserMove laserMove)
    {

    }

    public void Uninit(FireLaserMove laserMove)
    {

    }
}

