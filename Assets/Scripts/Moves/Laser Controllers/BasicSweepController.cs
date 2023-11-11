using System;
using System.Collections;
using UnityEngine;
using WeaverCore;

public class BasicSweepController : ILaserController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }
    public AnimationCurve Curve { get; private set; }
    public float FireTime { get; private set; }
    public bool DoAntiDirectAdjustment { get; private set; }

    public BasicSweepController(Quaternion from, Quaternion to, float fireTime, bool doAntiDirectAdjustment = true, AnimationCurve curve = null)
    {
        Curve = curve;
        if (Curve == null)
        {
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        From = from;
        To = to;
        FireTime = fireTime;
        DoAntiDirectAdjustment = doAntiDirectAdjustment;
        /*FireTime = fireTime;
        AnticTime = 0f;
        PlayAntic = false;
        MaximumAngle = 90f;*/
    }

    /*public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        return Quaternion.Slerp(From, To, Curve.Evaluate(timeSinceFiring / FireTime));
    }*/

    /*public override void Init(FireLaserMove laserMove)
    {
        //Flipped = boss.Head.LookingDirection >= 0f;
    }*/

    public bool CanFire(FireLaserMove laserMove)
    {
        //return laserMove.GetMinLaserDistance(Player.Player1.transform.position, GetStartAngle(laserMove, Vector2.zero)) <= 10f;
        return laserMove.IsLaserOriginVisible();
    }

    public Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity)
    {
        if (DoAntiDirectAdjustment)
        {
            From = laserMove.KeepAngleDistAwayFromTarget(From, Player.Player1.transform.position, 25f);
        }

        return Quaternion.Slerp(From, To, Curve.Evaluate(0));
    }

    public IEnumerator Fire(FireLaserMove laserMove, Func<bool> moveCancelled, Action<Quaternion> setLaserAngle, Quaternion startAngle)
    {
        for (float t = 0; t < FireTime; t += Time.deltaTime)
        {
            if (t > 0.25f && moveCancelled())
            {
                break;
            }

            /*if (t > 0.5f && !laserMove.IsLaserOriginVisible())
            {
                break;
            }*/

            setLaserAngle(Quaternion.Slerp(From, To, Curve.Evaluate(t / FireTime)));
            yield return null;
        }
    }

    public void Uninit(FireLaserMove laserMove)
    {
        
    }

    public void OnStun(FireLaserMove laserMove)
    {
        
    }

    public void Init(FireLaserMove laserMove)
    {
        
    }
}


/*public class BasicSweepController : FireLaserMove.SweepController
{
    public Quaternion From { get; private set; }
    public Quaternion To { get; private set; }
    public AnimationCurve Curve { get; private set; }

    public BasicSweepController(Quaternion from, Quaternion to, float fireTime, AnimationCurve curve = null)
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
        MaximumAngle = 90f;
    }

    public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        return Quaternion.Slerp(From, To, Curve.Evaluate(timeSinceFiring / FireTime));
    }

    public override void Init(FireLaserMove laserMove)
    {
        //Flipped = boss.Head.LookingDirection >= 0f;
    }
}*/

