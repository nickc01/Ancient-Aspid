using System;
using System.Collections;
using UnityEngine;

public class MultiSweepController : ILaserController
{
    public float LaserMovementAcceleration;
    public float MaxAngleVelocity;
    public float FireTime { get; private set; }

    private float angle;
    private float angleVelocity;

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
        return laserMove.IsLaserOriginVisible();
    }

    public Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity)
    {
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

            float direction = Mathf.Sign(Mathf.LerpAngle(angle, laserMove.Boss.GetAngleToPlayer(), Time.deltaTime) - angle);

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

