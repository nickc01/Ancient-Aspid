using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class PlayerSweepController : ILaserController
{
    public AnimationCurve AnimCurve { get; private set; }
    public float FireTime { get; private set; }

    private float minAngle;
    private float maxAngle;
    private Quaternion from;
    private Quaternion to;


    public PlayerSweepController(float minAngle, float maxAngle, float fireTime, AnimationCurve animCurve)
    {
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        FireTime = fireTime;
        AnimCurve = animCurve;
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
        Vector3 predictedPos = Player.Player1.transform.position + (Vector3)(playerVelocity * 0.4f);
        float angleToPlayer = MathUtilities.CartesianToPolar(predictedPos - laserMove.Boss.Head.transform.position).x;
        Quaternion playerQuaternion = Quaternion.Euler(0f, 0f, angleToPlayer);

        float angleDiff = maxAngle - minAngle;

        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
        {
            from = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);

            from = laserMove.KeepAngleDistAwayFromTarget(from, Player.Player1.transform.position, 25f);

            to = from * Quaternion.Euler(0f, 0f, -angleDiff);
        }
        else
        {
            from = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);

            from = laserMove.KeepAngleDistAwayFromTarget(from, Player.Player1.transform.position, 25f);

            to = from * Quaternion.Euler(0f, 0f, angleDiff);
        }

        Quaternion angle = Quaternion.Slerp(from, to, AnimCurve.Evaluate(0));

        return angle;
    }

    public IEnumerator Fire(FireLaserMove laserMove, Func<bool> moveCancelled, Action<Quaternion> setLaserAngle, Quaternion startAngle)
    {
        for (float t = 0; t < FireTime; t += Time.deltaTime)
        {
            if (t > 0.25f && moveCancelled())
            {
                break;
            }

            if (t > 0.5f && !laserMove.IsLaserOriginVisible())
            {
                break;
            }

            setLaserAngle(Quaternion.Slerp(from, to, AnimCurve.Evaluate(t / FireTime)));

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

