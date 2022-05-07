using UnityEngine;

public class BasicSweepController : FireLaserMove.SweepController
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

    public override void Init(AncientAspid boss)
    {
        //Flipped = boss.Head.LookingDirection >= 0f;
    }
}

