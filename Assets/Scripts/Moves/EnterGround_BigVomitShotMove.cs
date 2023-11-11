using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

public class EnterGround_BigVomitShotMove : AncientAspidMove
{
    public override bool MoveEnabled => false;

    [SerializeField]
    [Range(1, 2)]
    int attackVariant = 2;

    [SerializeField]
    float gravityScale = 0.7f;

    [SerializeField]
    AudioClip fireSound;

    [SerializeField]
    [Tooltip("The amount of time the projectiles should take to reach their targets")]
    Vector2 timeRange = new Vector2(0.75f, 1.25f);

    [SerializeField]
    float scale = 2;

    [SerializeField]
    AnimationCurve giantGlobCurve;

    [SerializeField]
    float giantGlobGrowTime = 0.5f;

    public VomitGlob SpawnedGlob { get; private set; } = null;
    public float SpawnedGlobEstLandTime { get; private set; } = 0;

    protected override IEnumerator OnExecute()
    {
        if (!Boss.Head.HeadLocked)
        {
            throw new Exception("This move requires the head to be locked first");
        }

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");

        float landTime = timeRange.RandomInRange();
        SpawnedGlob = FireBigShot(landTime);

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);

        SpawnedGlobEstLandTime = Mathf.Clamp(landTime - Boss.Head.Animator.AnimationData.GetClipDuration($"Fire - {attackVariant} - Attack"), 0, 5);

        yield break;
    }

    VomitGlob FireBigShot(float landTime)
    {
        Vector2 angleRange = new Vector2(360 - 90,360 - 40);
        if (Boss.Head.LookingDirection < 0)
        {
            angleRange = new Vector2(360 - 90 - 40,360 - 90);
        }

        var fireSource = Boss.Head.GetFireSource(Boss.Head.LookingDirection);
        var playerAngle = Boss.GetAngleToPlayer();
        if (playerAngle < 0)
        {
            playerAngle += 360;
        }
        var playerDistance = Vector2.Distance(Player.Player1.transform.position, Boss.Head.transform.position);

        //Debug.Log("Player Angle PRE = " + playerAngle);
        playerAngle = Mathf.Clamp(playerAngle, angleRange.x, angleRange.y);

        //Debug.Log("Player Angle POST = " + playerAngle);

        Blood.SpawnBlood(fireSource, new Blood.BloodSpawnInfo(4, 8, 10f, 25f, playerAngle - 50f, playerAngle + 50f, null));

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, fireSource);
        }

        var target = MathUtilities.PolarToCartesian(playerAngle, playerDistance - 1);

        var velocity = MathUtilities.CalculateVelocityToReachPoint(fireSource, (Vector2)fireSource + target, landTime, gravityScale);

        var glob = VomitGlob.Spawn(Boss.GlobPrefab, fireSource, velocity, gravityScale);

        glob.HasLifeTime = false;
        glob.SetScaleGradually(scale, giantGlobCurve, giantGlobGrowTime);

        return glob;
    }

    public override void OnStun()
    {
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
