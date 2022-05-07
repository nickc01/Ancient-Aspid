using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class ArenaSweepController : FireLaserMove.SweepController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }

    public bool Flipped { get; private set; }

    public AnimationCurve Curve { get; private set; }

    bool firstCalculationDone = false;

    AncientAspid boss;

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

    public override void Init(AncientAspid boss)
    {
        this.boss = boss;
        Flipped = boss.Head.LookingDirection >= 0f;
    }
}

