﻿using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

public class VomitShotMove : AncientAspidMove
{
    public override bool MoveEnabled => Boss.CanSeeTarget && moveEnabled &&
        Boss.AspidMode == AncientAspid.Mode.Tactical &&
        Vector3.Distance(Player.Player1.transform.position, transform.position) <= 40 &&
        Vector2.Distance(Player.Player1.transform.position, Boss.Head.transform.position) >= minDistance;
    /*Mathf.Abs(Boss.Head.transform.position.x - Player.Player1.transform.position.x) >= minXDistance &&
    Boss.Head.transform.position.y - Player.Player1.transform.position.y >= minYDistance;*/

    [SerializeField]
    bool moveEnabled = true;

    /*[SerializeField]
    float minXDistance = 5f;

    [SerializeField]
    float minYDistance = 4f;*/

    [SerializeField]
    float minDistance = 10.4f;

    [SerializeField]
    int vomitSpawnAmount = 5;

    [SerializeField]
    Vector2 gapSizeDegreesRange = new Vector2(5,7);

    [SerializeField]
    float distanceMultiplier = 1f;

    [SerializeField]
    float headSpeed = 1f;

    [SerializeField]
    float angleOffset = 0f;

    [SerializeField]
    [Range(1, 2)]
    int attackVariant = 2;

    [SerializeField]
    Vector2 angleRange = new Vector2(-30f,30f);

    /*[SerializeField]
    Vector2 velocityRange = new Vector2(5f,10f);*/
    [SerializeField]
    [Tooltip("The amount of time the projectiles should take to reach their targets")]
    Vector2 timeRange = new Vector2(0.75f,1.25f);

    [SerializeField]
    float gravityScale = 0.7f;

    [SerializeField]
    AudioClip fireSound;

    [SerializeField]
    float postDelay = 0.25f;

    [SerializeField]
    float climbingPostDelay = 0.45f;

    public float ShotGravityScale => gravityScale;

    List<VomitGlob> firedShots = new List<VomitGlob>();

    /// <summary>
    /// A list of all the shots that were fired after the call to FireVomitShots
    /// </summary>
    public List<VomitGlob> FiredShots => firedShots;

    public void EnableMove(bool enabled)
    {
        moveEnabled = enabled;
    }

#if UNITY_EDITOR
    Player testPlayer;
    private void OnDrawGizmosSelected()
    {
        void DrawAngle(float angle)
        {
            var direction = MathUtilities.PolarToCartesian(angle, 2f);
            Gizmos.DrawRay(transform.position, direction);
        }

        if (testPlayer == null)
        {
            testPlayer = GameObject.FindObjectOfType<Player>();
        }
        Gizmos.color = Color.yellow;

        var direction = (testPlayer.transform.position - transform.position).normalized;

        var playerAngle = MathUtilities.CartesianToPolar(direction).x;

        DrawAngle(playerAngle + angleRange.x);
        DrawAngle(playerAngle + angleRange.y);
    }
#endif

    IEnumerable<Vector2> ConvertFunc(int shots, Func<int, Vector2> vomitShotVelocities)
    {
        for (int i = 0; i < shots; i++)
        {
            yield return vomitShotVelocities(i);
        }
    }

    public IEnumerator FireVomitShots(int shots, Func<int,Vector2> vomitShotVelocities)
    {
        if (Boss.AspidMode != AncientAspid.Mode.Defensive)
        {
            yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left, headSpeed);
        }

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");

        FireShots(shots, ConvertFunc(shots, vomitShotVelocities));

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);

        if (Boss.AspidMode != AncientAspid.Mode.Defensive)
        {
            Boss.Head.UnlockHead();
        }

        yield break;
    }

    public override IEnumerator DoMove()
    {
        List<Vector2> shots = null;

        return FireVomitShots(vomitSpawnAmount,index =>
        {
            if (shots == null)
            {
                shots = GetRandomAnglesToPlayer(vomitSpawnAmount).ToList();
            }

            return shots[index];
        });

        /*if (Boss.AspidMode != AncientAspid.Mode.Defensive)
        {
            yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left, headSpeed);
        }

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");

        //FireShots();

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);

        if (Boss.AspidMode != AncientAspid.Mode.Defensive)
        {
            Boss.Head.UnlockHead();
        }

        yield break;*/
    }

    IEnumerable<Vector2> GetRandomAnglesToPlayer(int shots)
    {
        var offset = angleOffset;
        if (Boss.Head.LookingDirection < 0f)
        {
            offset = -offset;
        }
        var gapSize = gapSizeDegreesRange.RandomInRange();

        var gapStartPosition = UnityEngine.Random.Range(angleRange.x, angleRange.y - gapSize);

        var playerAngle = Boss.GetAngleToPlayer();
        var playerDistance = Vector2.Distance(Player.Player1.transform.position, Boss.Head.transform.position);

        var fireSource = Boss.Head.GetFireSource(Boss.Head.LookingDirection);

        for (int i = 0; i < shots; i++)
        {
            var randomAngle = UnityEngine.Random.Range(angleRange.x, angleRange.y - gapSize);
            if (randomAngle > gapStartPosition)
            {
                randomAngle += gapSize;
            }

            if (Boss.Head.LookingDirection < 0f)
            {
                randomAngle = -randomAngle;
            }

            var target = MathUtilities.PolarToCartesian(playerAngle + randomAngle + offset, playerDistance);

            yield return MathUtilities.CalculateVelocityToReachPoint(fireSource, (Vector2)fireSource + target, timeRange.RandomInRange(), gravityScale);

            //FireInDirection(playerAngle + randomAngle + offset, playerDistance, fireSource);
        }

        /*Blood.SpawnBlood(fireSource, new Blood.BloodSpawnInfo(4, 8, 10f, 25f, playerAngle + offset - 50f, playerAngle + offset + 50f, null));*/

        /*if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, fireSource);
        }*/
    }

    void FireShots(int shots, IEnumerable<Vector2> velocities)
    {
        /*var offset = angleOffset;
        if (Boss.Head.LookingDirection < 0f)
        {
            offset = -offset;
        }
        var gapSize = gapSizeDegreesRange.RandomInRange();

        var gapStartPosition = UnityEngine.Random.Range(angleRange.x, angleRange.y - gapSize);

        var playerAngle = Boss.GetAngleToPlayer();
        var playerDistance = Vector2.Distance(Player.Player1.transform.position,Boss.Head.transform.position);

        var fireSource = Boss.Head.GetFireSource(Boss.Head.LookingDirection);
        */

        firedShots.Clear();

        var fireSource = Boss.Head.GetFireSource(Boss.Head.LookingDirection);

        var enumerator = velocities.GetEnumerator();

        for (int i = 0; i < shots; i++)
        {
            enumerator.MoveNext();
            /*var randomAngle = UnityEngine.Random.Range(angleRange.x, angleRange.y - gapSize);
            if (randomAngle > gapStartPosition)
            {
                randomAngle += gapSize;
            }

            if (Boss.Head.LookingDirection < 0f)
            {
                randomAngle = -randomAngle;
            }

            FireInDirection(playerAngle + randomAngle + offset, playerDistance, fireSource);*/

            var velocity = enumerator.Current;


            if (!Boss.RiseFromCenterPlatform)
            {
                firedShots.Add(VomitGlob.Spawn(fireSource, velocity, gravityScale));
            }

            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            Blood.SpawnBlood(fireSource, new Blood.BloodSpawnInfo(1, 2, 10f, 25f, angle - 50f, angle + 50f, null));
            //FireInDirection()
        }

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, fireSource);
        }
    }

    public override float PostDelay => Boss.InClimbingPhase ? climbingPostDelay : postDelay;

    /*float DistanceToPlayer()
    {
        return Vector2.Distance(Player.Player1.transform.position,Boss.Head.transform.position);
    }


    void FireInDirection(float angle, float magnitude, Vector3 position)
    {
        var target = MathUtilities.PolarToCartesian(angle, magnitude);

        var velocity = MathUtilities.CalculateVelocityToReachPoint(position, (Vector2)position + target, timeRange.RandomInRange(), gravityScale);

        if (!Boss.RiseFromCenterPlatform)
        {
            VomitGlob.Spawn(position, velocity, gravityScale);
        }
    }*/

    public override void OnStun()
    {
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
