using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;
using WeaverCore.Utilities;

public class PlayerSweepController : ILaserController
{
    public AnimationCurve AnimCurve { get; private set; }
    public float FireTime { get; private set; }

    //bool firstAttackDone = false;

    float minAngle;
    float maxAngle;

    Quaternion from;
    Quaternion to;

    bool startingOnRightSide;

    //Vector3 playerStartPos;

    //FireLaserMove laserMove;


    public PlayerSweepController(float minAngle, float maxAngle, float fireTime, AnimationCurve animCurve)
    {
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        FireTime = fireTime;
        //AnticTime = 0f;
        //PlayAntic = false;
        //MaximumAngle = 90f;
        AnimCurve = animCurve;
    }

    /*public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        if (!firstAttackDone)
        {
            firstAttackDone = true;
            var newPlayerPos = Player.Player1.transform.position;

            //var predictedPosition = (newPlayerPos - playerStartPos);

            var angleToPlayer = MathUtilities.CartesianToPolar(playerStartPos - boss.Head.transform.position).x;

            var playerQuaternion = Quaternion.Euler(0f, 0f, angleToPlayer);

            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
            {
                from = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);
                to = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);
            }
            else
            {
                from = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);
                to = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);
            }
        }

        return Quaternion.Slerp(from, to, AnimCurve.Evaluate(timeSinceFiring / FireTime));
    }*/

    public void Init(FireLaserMove laserMove)
    {
        //this.boss = boss;
        //firstAttackDone = false;
        //playerStartPos = Player.Player1.transform.position;
    }

    public bool CanFire(FireLaserMove laserMove)
    {
        //return laserMove.GetMinLaserDistance(Player.Player1.transform.position,GetStartAngle(laserMove, Vector2.zero)) <= 10f;
        return laserMove.IsLaserOriginVisible();
    }

    public Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity)
    {
        var predictedPos = Player.Player1.transform.position + (Vector3)(playerVelocity * 0.4f);
        //var newPlayerPos = Player.Player1.transform.position;

        //var predictedPosition = (newPlayerPos - playerStartPos);

        var angleToPlayer = MathUtilities.CartesianToPolar(predictedPos - laserMove.Boss.Head.transform.position).x;
        //var angleToPlayerDirect = MathUtilities.CartesianToPolar(Player.Player1.transform.position - laserMove.Boss.Head.transform.position).x;

        var playerQuaternion = Quaternion.Euler(0f, 0f, angleToPlayer);

        var angleDiff = maxAngle - minAngle;

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

        /*var diffAngleFromPlayer = Mathf.Abs(Quaternion.Angle(from, Quaternion.Euler(0f, 0f, angleToPlayerDirect)));

        const float ANGLE_ADJUSTMENT = 15f;

        var turnAmount = ANGLE_ADJUSTMENT - diffAngleFromPlayer;


        var angleDot = Quaternion.Dot(from * Quaternion.Euler(0,0,-90), Quaternion.Euler(0f, 0f, angleToPlayerDirect));
        if (angleDot >= 0f)
        {
            from = from * Quaternion.Euler(0,0, turnAmount);
        }
        else
        {
            from = from * Quaternion.Euler(0, 0, -turnAmount);
        }

        
        */
        //WeaverLog.Log("PREV ANGLE = " + from.eulerAngles.z);
        //from = laserMove.KeepAngleDistAwayFromTarget(from, Player.Player1.transform.position, 25f);
        //WeaverLog.Log("NEW ANGLE = " + from.eulerAngles.z);

        var angle = Quaternion.Slerp(from, to, AnimCurve.Evaluate(0));

        startingOnRightSide = laserMove.LaserOnRightSideOf(Player.Player1.transform.position, angle);

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

/*public class PlayerSweepController : FireLaserMove.SweepController
{
    public AnimationCurve AnimCurve { get; private set; }


    bool firstAttackDone = false;

    float minAngle;
    float maxAngle;

    Quaternion from;
    Quaternion to;

    Vector3 playerStartPos;

    FireLaserMove laserMove;


    public PlayerSweepController(float minAngle, float maxAngle, float fireTime, AnimationCurve animCurve)
    {
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        FireTime = fireTime;
        AnticTime = 0f;
        PlayAntic = false;
        MaximumAngle = 90f;
        AnimCurve = animCurve;
    }

    public override Quaternion CalculateAngle(float timeSinceFiring)
    {
        if (!firstAttackDone)
        {
            firstAttackDone = true;
            var newPlayerPos = Player.Player1.transform.position;

            //var predictedPosition = (newPlayerPos - playerStartPos);

            var angleToPlayer = MathUtilities.CartesianToPolar(playerStartPos - boss.Head.transform.position).x;

            var playerQuaternion = Quaternion.Euler(0f,0f,angleToPlayer);

            if (UnityEngine.Random.Range(0f,1f) > 0.5f)
            {
                from = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);
                to = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);
            }
            else
            {
                from = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);
                to = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);
            }
        }

        return Quaternion.Slerp(from,to,AnimCurve.Evaluate(timeSinceFiring / FireTime));
    }

    public override void Init(FireLaserMove laserMove)
    {
        this.boss = boss;
        firstAttackDone = false;
        playerStartPos = Player.Player1.transform.position;
    }
}*/

