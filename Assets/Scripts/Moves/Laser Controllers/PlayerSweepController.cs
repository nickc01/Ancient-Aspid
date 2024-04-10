using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class PlayerSweepController : ILaserController
{
    //[SerializeField]
    //Vector2 angleRange = new Vector2(45f, -180f - 45f);

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
        //return Vector2.Distance(laserMove.GetNearestLaserPoint(Player.Player1.transform.position), Player.Player1.transform.position) <= 20f;
        return laserMove.IsLaserOriginVisible();
    }

    static Vector3 PredictPlayerPos(Vector3 startPlayerPos, float time)
    {
        var rb = Player.Player1.transform.GetComponent<Rigidbody2D>();

        //return startPlayerPos;
        return startPlayerPos + ((Vector3)rb.velocity * time) + (Vector3)((1f / 2f) * Physics2D.gravity * rb.gravityScale * time * time);
    }

    public Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity)
    {
        //List<float> startAngles = new List<float>();
        //List<float> endAngles = new List<float>();


        var predictedPlayerPos = PredictPlayerPos(Player.Player1.transform.position, 0.4f);

        float angleToPlayer = MathUtilities.CartesianToPolar(predictedPlayerPos - laserMove.Boss.Head.transform.position).x;
        float angleDiff = maxAngle - minAngle;

        //WeaverLog.Log("ANGLE TO PLAYER PREPROCESS = " + angleToPlayer);
        //Debug.DrawRay(laserMove.Boss.Head.transform.position, MathUtilities.CartesianToPolar(angleToPlayer, 100), Color.black, 10f);

        if (angleToPlayer > 90f)
        {
            angleToPlayer -= 360f;
        }

        //If on left side
        if (angleToPlayer > -270f)
        {
            if (angleToPlayer < -270f + angleDiff + 10f)
            {
                angleToPlayer = -270f + angleDiff + 10f;
            }
        }
        else
        {
            if (angleToPlayer < 90 - angleDiff - 10f)
            {
                angleToPlayer = 90 - angleDiff - 10f;
            }
        }

        angleToPlayer += 180f;

        /*WeaverLog.Log("ANGLE TO PLAYER = " + angleToPlayer);
        Debug.DrawRay(laserMove.Boss.Head.transform.position, MathUtilities.CartesianToPolar(angleToPlayer, 100), Color.gray, 10f);

        WeaverLog.Log("-DIFF = " + (angleToPlayer - angleDiff));
        Debug.DrawRay(laserMove.Boss.Head.transform.position, MathUtilities.CartesianToPolar((angleToPlayer - angleDiff), 100), Color.red, 10f);

        WeaverLog.Log("DIFF = " + (angleToPlayer + angleDiff));
        Debug.DrawRay(laserMove.Boss.Head.transform.position, MathUtilities.CartesianToPolar((angleToPlayer + angleDiff), 100), Color.blue, 10f);*/

        //startAngles.Add(angleToPlayer - angleDiff);
        //endAngles.Add(angleToPlayer + angleDiff);

        //startAngles.Add(angleToPlayer + angleDiff);
        //endAngles.Add(angleToPlayer - angleDiff);




        //TODO - FIX THIS SO THAT THE START AND END ANGLES ARE RESTRICTED TO A RANGE, SO THEY DON"T GO TOO HIGH AND LOOP AROUND THE TOP. ALSO MAKE SURE THIS DOESN"T HIT DIRECTLY AT THE PLAYER
        //Vector3 predictedPos = Player.Player1.transform.position + (Vector3)(playerVelocity * 0.4f);
        //float angleToPlayer = MathUtilities.CartesianToPolar(predictedPos - laserMove.Boss.Head.transform.position).x;
        Quaternion playerQuaternion = Quaternion.Euler(0f, 0f, angleToPlayer);

        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
        {
            from = playerQuaternion * Quaternion.Euler(0f, 0f, -angleDiff);
            to = playerQuaternion *= Quaternion.Euler(0f, 0f, angleDiff);

            /*from = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);

            from = laserMove.KeepAngleDistAwayFromTarget(from, Player.Player1.transform.position, 25f);

            to = from * Quaternion.Euler(0f, 0f, -angleDiff);*/
        }
        else
        {
            from = playerQuaternion * Quaternion.Euler(0f, 0f, angleDiff);
            to = playerQuaternion *= Quaternion.Euler(0f, 0f, -angleDiff);

            /*from = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);

            from = laserMove.KeepAngleDistAwayFromTarget(from, Player.Player1.transform.position, 25f);

            to = from * Quaternion.Euler(0f, 0f, angleDiff);*/
        }

        //from = Quaternion.Euler(0f, 0f, -180f);
        //to = Quaternion.Euler(0f, 0f, 0);

        /*if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
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
        }*/

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

            var laserVisible = Vector2.Distance(laserMove.GetNearestLaserPoint(Player.Player1.transform.position), Player.Player1.transform.position) <= 20f;

            if (t > 0.5f && !laserVisible)
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

