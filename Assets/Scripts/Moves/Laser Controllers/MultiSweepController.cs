using UnityEngine;

public class MultiSweepController : FireLaserMove.SweepController
{
    public readonly float LaserMovementAcceleration;
    public readonly float MaxAngleVelocity;

    float angle;
    float angleVelocity;

    AncientAspid boss;

    public MultiSweepController(float fireDuration, float laserMovementAcceleration, float startAngle, float startAngleVelocity, float maxAngleVelocity)
    {
        FireTime = fireDuration;
        AnticTime = 0f;
        PlayAntic = false;
        LaserMovementAcceleration = laserMovementAcceleration;
        angle = startAngle;
        angleVelocity = startAngleVelocity;
        MaxAngleVelocity = maxAngleVelocity;
    }

    public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        var direction = Mathf.Sign(Mathf.LerpAngle(angle, boss.GetAngleToPlayer(), Time.deltaTime) - angle);

        angleVelocity += direction * LaserMovementAcceleration * Time.deltaTime;

        if (angleVelocity > MaxAngleVelocity)
        {
            angleVelocity = MaxAngleVelocity;
        }
        else if (angleVelocity < -MaxAngleVelocity)
        {
            angleVelocity = -MaxAngleVelocity;
        }

        angle += angleVelocity * Time.deltaTime;

        return Quaternion.Euler(0f, 0f, angle);
    }

    public override void Init(AncientAspid boss)
    {
        this.boss = boss;
    }

    public override bool DoLaserInterrupt()
    {
        return false;
    }
}

