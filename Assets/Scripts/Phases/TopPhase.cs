using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using WeaverCore;

public class TopPhase : Phase
{
    [SerializeField]
    int timesBeforeFlyAway = 3;

    [SerializeField]
    List<SpriteRenderer> enableOnCowardiceFlyAway;

    [SerializeField]
    List<SpriteRenderer> disableOnCowardiceFlyAway;

    [SerializeField]
    UpperGroundAreaProvider groundAreaProvider;

    [SerializeField]
    UpperOffensiveAreaProvider offensiveAreaProvider;

    AncientAspid boss;

    [SerializeField]
    Vector2 flightRangeX;

    [SerializeField]
    float cowardiceZAxis;

    [SerializeField]
    Vector2 cowardiceSpawnOffset;

    [SerializeField]
    Rect cowardiceSpawnLimits;

    [SerializeField]
    bool debug = false;

    [SerializeField]
    float verticalRiseVelocity = 2f;

    [SerializeField]
    float upperRangeHeightTrigger;

    [SerializeField]
    Rect upperRangeSpawnRect;

    [SerializeField]
    float upperRangeVelocity = 0.25f;

    [SerializeField]
    Vector2 upperRangeOffset;


    /*public override bool CanGoToNextPhase(AncientAspid boss)
    {
        return base.CanGoToNextPhase(boss);
    }*/

    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
        if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            boss.MusicPlayer.TransitionToPhase(AncientAspidMusicController.MusicPhase.AR3);
        }
        boss.GetComponent<LaserRapidFireMove>().EnableMove(true);
        this.boss = boss;

        var flightRange = boss.FlightRange;
        flightRange.xMin = flightRangeX.x;
        flightRange.xMax = flightRangeX.y;
        boss.FlightRange = flightRange;

        boss.GroundMode.GroundAreaProvider = groundAreaProvider;
        boss.OffensiveMode.OffensiveAreaProvider = offensiveAreaProvider;
        boss.StartBoundRoutine(CowardiceRoutine(timesBeforeFlyAway));
        yield break;
    }

    protected override IEnumerator OnPhaseEnd(AncientAspid boss, Phase nextPhase)
    {
        yield break;
    }

    IEnumerator CowardiceRoutine(int timesBeforeFlyAway)
    {
        int timesCounted = 0;
        while (true)
        {
            yield return new WaitUntil(() => Player.Player1.transform.position.y >= 271.3f);
            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(() => Player.Player1.transform.position.y < 271.3f);
            if (timesCounted++ >= timesBeforeFlyAway)
            {
                break;
            }
        }

        boss.SwitchToNewMode(boss.TacticalMode, new Dictionary<string, object>
        {
            {TacticalMode.TACTICAL_TIME, float.PositiveInfinity},
            {TacticalMode.DO_FLY_AWAY, true}
        });

        boss.AddTargetOverride(int.MaxValue).SetTarget(new Vector3(223.7f, 445.2f));

        WeaverLog.LogWarning("FLYING AWAY");

        boss.FlightEnabled = false;
        boss.ApplyFlightVariance = false;

        foreach (var sprite in enableOnCowardiceFlyAway)
        {
            sprite.enabled = true;
        }

        foreach (var sprite in disableOnCowardiceFlyAway)
        {
            sprite.enabled = false;
        }

        foreach (var collider in boss.GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }

        var clawController = boss.GetComponentInChildren<ClawController>(true);

        clawController.EnableSwingAttack(false);
        clawController.enabled = false;
        boss.GetComponentInChildren<FarAwayLaser>(true).moveEnabled = false;

        Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, 0.5f);

        if (boss.MusicPlayer != null && !WeaverCore.Features.Boss.InPantheon)
        {
            boss.MusicPlayer.Stop();
        }

        yield return new WaitForSeconds(1f);

        boss.transform.position = Player.Player1.transform.position + (Vector3)cowardiceSpawnOffset;

        if (boss.transform.position.x > cowardiceSpawnLimits.max.x)
        {
            boss.transform.SetPositionX(cowardiceSpawnLimits.max.x);
        }

        if (boss.transform.position.x < cowardiceSpawnLimits.min.x)
        {
            boss.transform.SetPositionX(cowardiceSpawnLimits.min.x);
        }

        if (boss.transform.position.y > cowardiceSpawnLimits.max.y)
        {
            boss.transform.SetPositionY(cowardiceSpawnLimits.max.y);
        }

        if (boss.transform.position.y < cowardiceSpawnLimits.min.y)
        {
            boss.transform.SetPositionY(cowardiceSpawnLimits.min.y);
        }

        boss.transform.SetPositionZ(cowardiceZAxis);

        while (Player.Player1.transform.position.y < upperRangeHeightTrigger)
        {
            //boss.Rbody.velocity = Mathf.Max(boss.Rbody.velocity.magnitude, 20f) * Vector2.up;
            boss.Rbody.velocity = new Vector2(0f, verticalRiseVelocity);
            yield return null;
        }

        boss.transform.position = Player.Player1.transform.position + (Vector3)upperRangeOffset;

        if (boss.transform.position.x > upperRangeSpawnRect.max.x)
        {
            boss.transform.SetPositionX(upperRangeSpawnRect.max.x);
        }

        if (boss.transform.position.x < upperRangeSpawnRect.min.x)
        {
            boss.transform.SetPositionX(upperRangeSpawnRect.min.x);
        }

        if (boss.transform.position.y > upperRangeSpawnRect.max.y)
        {
            boss.transform.SetPositionY(upperRangeSpawnRect.max.y);
        }

        if (boss.transform.position.y < upperRangeSpawnRect.min.y)
        {
            boss.transform.SetPositionY(upperRangeSpawnRect.min.y);
        }

        boss.transform.SetPositionZ(cowardiceZAxis);

        while (true)
        {
            //boss.Rbody.velocity = Mathf.Max(boss.Rbody.velocity.magnitude, 20f) * Vector2.up;
            boss.Rbody.velocity = new Vector2(0f, upperRangeVelocity);
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (debug)
        {
            Gizmos.color = new Color(0, 0, 1f, 0.35f);
            Gizmos.DrawCube(cowardiceSpawnLimits.center, cowardiceSpawnLimits.size);
        }

        if (debug)
        {
            Gizmos.color = new Color(0, 1f, 0f, 0.35f);
            Gizmos.DrawCube(upperRangeSpawnRect.center, upperRangeSpawnRect.size);
        }
    }
}
