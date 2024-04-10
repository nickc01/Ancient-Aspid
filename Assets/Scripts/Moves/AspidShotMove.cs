using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;



public class AspidShotMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.6f;

    [SerializeField]
    float climbingPostDelay = 0.8f;

    [SerializeField]
    float minDistance = 5f;

    [SerializeField]
    float headSpeed = 1.5f;

    [SerializeField]
    Vector2 shotAngleRandomness = new Vector2(-10f, 10f);

    [SerializeField]
    [Range(1, 2)]
    int attackVariant = 1;

    [SerializeField]
    GameObject ShotPrefab;

    [SerializeField]
    AudioClip fireSound;

    List<IAspidComboProvider> comboProviders;

    AspidOrientation startOrientation;

    private void Awake()
    {
        comboProviders = new List<IAspidComboProvider>();
        GetComponents(comboProviders);
    }

    public override bool MoveEnabled => Boss.CanSeeTarget && moveEnabled && Vector2.Distance(Player.Player1.transform.position,Boss.Head.transform.position) >= minDistance;       

    public override float GetPostDelay(int prevHealth) => Boss.InClimbingPhase ? climbingPostDelay : postDelay;

    protected override IEnumerator OnExecute()
    {
        startOrientation = Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left;

        yield return Boss.Head.LockHead(startOrientation, headSpeed);

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        var comboProvider = comboProviders.GetRandomElement();
        comboProvider.Init(out var comboCount);

        for (int i = 0; i < comboCount; i++)
        {
            var enumerator = comboProvider.DoShots(i);

            yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare{(i == 0 ? "" : " - Quick")}");

            while (enumerator.MoveNext())
            {
                var info = enumerator.Current;

                Fire(Boss.Head.MainRenderer.flipX ? 45f : -45f, info.ShotAngleOffset, info.Shots, info.ShotSpeed, info.ShotAngleSeparation, info.ShotScale);
            }
            yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack{(i == comboCount - 1 ? "" : " - Quick")}");

            Boss.Head.MainRenderer.Restore(oldState);

            if (Cancelled)
            {
                break;
            }

            yield return Boss.Head.QuickFlipDirection(Boss.PlayerRightOfBoss);
        }

        Boss.Head.UnlockHead();
    }

    void Fire(float baseAngle, float angleOffset, int shots, float speed, float angleSeparation, float scale)    
    {
        if (speed <= 0)
        {
            speed = 0.01f;
        }

        var sourcePos = Boss.Head.GetFireSource(baseAngle);

        Blood.SpawnBlood(sourcePos, new Blood.BloodSpawnInfo(3, 7, 10f, 25f, baseAngle - 90f - 40f, baseAngle - 90f + 40f, null));
        float gravityScale = 1;
        if (ShotPrefab.TryGetComponent(out Rigidbody2D rb))
        {
            gravityScale = rb.gravityScale;
        }

        var velocityToPlayer = MathUtilities.CalculateVelocityToReachPoint(sourcePos, Player.Player1.transform.position, Vector3.Distance(sourcePos, Player.Player1.transform.position) / speed, gravityScale);

        var polarCoordsToPlayer = MathUtilities.CartesianToPolar(velocityToPlayer);

        shots -= 1;

        float lowerShots = -shots / 2;

        for (float i = lowerShots; i <= lowerShots + shots; i++)
        {
            FireShot(polarCoordsToPlayer.x, (angleSeparation * i) + angleOffset,sourcePos, polarCoordsToPlayer.y, scale);
        }

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, sourcePos);
        }
    }

    void FireShot(float playerAngle, float angle, Vector3 sourcePos, float velocity, float scale)
    {
        var instance = Pooling.Instantiate(ShotPrefab, sourcePos, Quaternion.identity);
        if (instance.TryGetComponent(out Rigidbody2D rb))
        {
            rb.velocity = MathUtilities.PolarToCartesian(playerAngle + angle + shotAngleRandomness.RandomInRange(), velocity);
        }
        if (instance.TryGetComponent(out AspidShotBase aspidShot))
        {
            aspidShot.ScaleFactor = scale;
        }
        else
        {
            instance.transform.SetLocalScaleXY(scale, scale);
        }
    }

    public override void OnStun()
    {
        Boss.Head.Animator.StopCurrentAnimation();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead(startOrientation);
        }
    }

}
