using System;
using System.Collections;
using UnityEngine;
using WeaverCore;

public class MultiSweepController : ILaserController
{
    public readonly float LaserMovementAcceleration;
    public readonly float MaxAngleVelocity;
    public float FireTime { get; private set; }

    float angle;
    float angleVelocity;

    public MultiSweepController(float fireDuration, float laserMovementAcceleration, float startAngle, float startAngleVelocity, float maxAngleVelocity)
    {
        FireTime = fireDuration;
        LaserMovementAcceleration = laserMovementAcceleration;
        angle = startAngle;
        angleVelocity = startAngleVelocity;
        MaxAngleVelocity = maxAngleVelocity;
    }

    public void Init(FireLaserMove laserMove)
    {
        
    }

    public bool CanFire(FireLaserMove laserMove)
    {
        //return laserMove.GetMinLaserDistance(Player.Player1.transform.position, GetStartAngle(laserMove, Vector2.zero)) <= 10f;
        return laserMove.IsLaserOriginVisible();
    }

    public Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity)
    {
        //var direction = Mathf.Sign(Mathf.LerpAngle(angle, laserMove.Boss.GetAngleToPlayer(), Time.deltaTime) - angle);

        /*angleVelocity += direction * LaserMovementAcceleration * Time.deltaTime;

        if (angleVelocity > MaxAngleVelocity)
        {
            angleVelocity = MaxAngleVelocity;
        }
        else if (angleVelocity < -MaxAngleVelocity)
        {
            angleVelocity = -MaxAngleVelocity;
        }

        angle += angleVelocity * Time.deltaTime;*/




        return Quaternion.Euler(0f, 0f, angle);
    }

    public IEnumerator Fire(FireLaserMove laserMove, Func<bool> moveCancelled, Action<Quaternion> setLaserAngle, Quaternion startAngle)
    {
        for (float t = 0; t < FireTime; t += Time.deltaTime)
        {
            if (t > 0.25f && moveCancelled())
            {
                break;
            }

            /*if (t > 0.25f && !laserMove.IsLaserOriginVisible())
            {
                break;
            }*/

            var direction = Mathf.Sign(Mathf.LerpAngle(angle, laserMove.Boss.GetAngleToPlayer(), Time.deltaTime) - angle);

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

            setLaserAngle(Quaternion.Euler(0f, 0f, angle));
            yield return null;
        }
    }

    public void Uninit(FireLaserMove laserMove)
    {
        
    }

    public void OnStun(FireLaserMove laserMove)
    {
        
    }
}

/*public class MultiSweepController : FireLaserMove.SweepController
{
    public readonly float LaserMovementAcceleration;
    public readonly float MaxAngleVelocity;

    float angle;
    float angleVelocity;

    FireLaserMove laserMove;

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

    public override void Init(FireLaserMove laserMove)
    {
        this.boss = boss;
    }

    public override bool DoLaserInterrupt()
    {
        return false;
    }
}*/

