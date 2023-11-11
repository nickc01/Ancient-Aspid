using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class ArenaSweepController : ILaserController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }
    public bool Flipped { get; private set; }
    public AnimationCurve Curve { get; private set; }
    public float FireTime { get; private set; }

    bool startingOnRightSide;

    public ArenaSweepController(Quaternion from, Quaternion to, float fireTime, AnimationCurve curve = null)
    {
        Curve = curve;
        if (Curve == null)
        {
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
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
        var playerAngle = MathUtilities.ClampRotation(laserMove.Boss.GetAngleToPlayer() + 90f);

        var fromAngle = MathUtilities.ClampRotation(From.eulerAngles.z + 90f);
        var toAngle = MathUtilities.ClampRotation(To.eulerAngles.z + 90f);

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

        Quaternion angle;
        if (Flipped)
        {
            angle = Quaternion.Slerp(To, From, Curve.Evaluate(0));
        }
        else
        {
            angle = Quaternion.Slerp(From, To, Curve.Evaluate(0));
        }

        startingOnRightSide = laserMove.LaserOnRightSideOf(Player.Player1.transform.position, angle);

        return angle;
    }

    public bool CanFire(FireLaserMove laserMove)
    {
        //return laserMove.GetMinLaserDistance(Player.Player1.transform.position, GetStartAngle(laserMove, Vector2.zero)) <= 10f;
        return laserMove.IsLaserOriginVisible();
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

            Quaternion angle;

            if (Flipped)
            {
                angle = Quaternion.Slerp(To, From, Curve.Evaluate(t / FireTime));
            }
            else
            {
                angle = Quaternion.Slerp(From, To, Curve.Evaluate(t / FireTime));
            }
            setLaserAngle(angle);

            if (t > 0.25f && laserMove.LaserOnRightSideOf(Player.Player1.transform.position, angle) != startingOnRightSide)
            {
                break;
            }

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

/*public class ArenaSweepController : FireLaserMove.SweepController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }

    public bool Flipped { get; private set; }

    public AnimationCurve Curve { get; private set; }

    bool firstCalculationDone = false;

    FireLaserMove laserMove;

    public ArenaSweepController(Quaternion from, Quaternion to, float fireTime, AnimationCurve curve = null)
    {
        Curve = curve;
        if (Curve == null)
        {
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        From = from;
        To = to;
        FireTime = fireTime;
        AnticTime = 0f;
        PlayAntic = false;
        MaximumAngle = 60f;
    }

    public ArenaSweepController(float fromBottomDegrees, float toBottomDegrees, float fireTime, AnimationCurve curve = null) : this(Quaternion.Euler(0f, 0f, fromBottomDegrees - 90f), Quaternion.Euler(0f, 0f, toBottomDegrees - 90f), fireTime,curve) { }

    public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        if (!firstCalculationDone)
        {
            firstCalculationDone = true;

            var playerAngle = MathUtilities.ClampRotation(boss.GetAngleToPlayer() + 90f);

            var fromAngle = MathUtilities.ClampRotation(From.eulerAngles.z + 90f);
            var toAngle = MathUtilities.ClampRotation(To.eulerAngles.z + 90f);

            if (Flipped)
            {
                //var playerAngle = boss.GetAngleToPlayer();
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
        }
        if (Flipped)
        {
            return Quaternion.Slerp(To, From, Curve.Evaluate(timeSinceFiring / FireTime));
        }
        else
        {
            return Quaternion.Slerp(From, To, Curve.Evaluate(timeSinceFiring / FireTime));
        }
    }

    public override void Init(FireLaserMove laserMove)
    {
        this.boss = boss;
        Flipped = boss.Head.LookingDirection >= 0f;
    }
}*/

