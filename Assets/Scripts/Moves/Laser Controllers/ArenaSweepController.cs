using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ArenaSweepController : FireLaserMove.SweepController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }

    public bool Flipped { get; private set; }

    public ArenaSweepController(Quaternion from, Quaternion to, float fireTime)
    {
        From = from;
        To = to;
        FireTime = fireTime;
        AnticTime = 0f;
        PlayAntic = false;
        MaximumAngle = 60f;
    }

    public ArenaSweepController(float fromBottomDegrees, float toBottomDegrees, float fireTime) : this(Quaternion.Euler(0f, 0f, fromBottomDegrees - 90f), Quaternion.Euler(0f, 0f, toBottomDegrees - 90f), fireTime) { }

    public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        if (Flipped)
        {
            return Quaternion.Slerp(To, From, timeSinceFiring / FireTime);
        }
        else
        {
            return Quaternion.Slerp(From, To, timeSinceFiring / FireTime);
        }
    }

    public override void Init(AncientAspid boss)
    {
        Flipped = boss.Head.LookingDirection >= 0f;
    }
}

