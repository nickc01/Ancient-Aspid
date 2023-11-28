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

    protected override IEnumerator OnPhaseStart(AncientAspid boss, Phase prevPhase)
    {
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

        Music.ApplyMusicSnapshot(Music.SnapshotType.Silent, 0f, 0.5f);

        while (true)
        {
            boss.Rbody.velocity = Mathf.Max(boss.Rbody.velocity.magnitude, 20f) * Vector2.up;
            yield return null;
        }

    }
}
