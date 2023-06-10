using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class PlayerSweepController : FireLaserMove.SweepController
{
    public AnimationCurve AnimCurve { get; private set; }


    bool firstAttackDone = false;

    float minAngle;
    float maxAngle;

    Quaternion from;
    Quaternion to;

    Vector3 playerStartPos;

    AncientAspid boss;


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
            /*if (newPlayerPos.x >= playerStartPos.x)
            {
                from = playerQuaternion * Quaternion.Euler(0f,0f,maxAngle);
                to = playerQuaternion * Quaternion.Euler(0f,0f,minAngle);
            }
            else
            {
                from = playerQuaternion * Quaternion.Euler(0f, 0f, minAngle);
                to = playerQuaternion * Quaternion.Euler(0f, 0f, maxAngle);
            }*/
        }

        return Quaternion.Slerp(from,to,AnimCurve.Evaluate(timeSinceFiring / FireTime));
    }

    public override void Init(AncientAspid boss)
    {
        this.boss = boss;
        playerStartPos = Player.Player1.transform.position;
    }
}

