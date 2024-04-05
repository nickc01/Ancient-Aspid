using GlobalEnums;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore;
using System.Linq;
using Modding;
using WeaverCore.Utilities;

public class GodhomeRespawnManager : MonoBehaviour
{
    [SerializeField]
    List<MovingPlatform> alternativePlatforms = new List<MovingPlatform>();

    [SerializeField]
    Vector2 alternativePlatformsOffset = new Vector2();

    [SerializeField]
    MovingPlatform mainPlatform;

    [SerializeField]
    Vector2 mainPlatformOffset = new Vector2();

    [SerializeField]
    Vector2 validPositionRange;


    static HashSet<GodhomeRespawnManager> hookedObjects = new HashSet<GodhomeRespawnManager>();

    [OnHarmonyPatch]
    static void OnHarmonyPatch(HarmonyPatcher patcher)
    {
        {
            var orig = typeof(HeroController).GetMethod("DieFromHazard", BindingFlags.NonPublic | BindingFlags.Instance);

            var prefix = typeof(TemporaryPlatform).GetMethod("DieFromHazardPrefix", BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }
    }

    [OnRuntimeInit]
    static void OnRuntimeInit()
    {
        ModHooks.TakeDamageHook += ModHooks_TakeDamageHook;
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
    }

    private static int ModHooks_TakeDamageHook(ref int hazardType, int damage)
    {
        DieFromHazardPrefix();

        return damage;
    }

    private static int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        DieFromHazardPrefix();

        return damageAmount;
    }

    static bool DieFromHazardPrefix()
    {
        foreach (var obj in hookedObjects)
        {
            obj.AdjustSpawnLocation();
        }

        return true;
    }

    private void Awake()
    {
        alternativePlatforms.ShuffleInPlace();
        if (enabled)
        {
            hookedObjects.Add(this);
        }
        AdjustSpawnLocation();
    }

    private void OnEnable()
    {
        hookedObjects.Add(this);
    }

    private void OnDisable()
    {
        hookedObjects.Remove(this);
    }

    private void OnDestroy()
    {
        hookedObjects.Remove(this);
    }

    void AdjustSpawnLocation()
    {
        if (TryGoToPlatform(mainPlatform, mainPlatformOffset))
        {
            return;
        }

        for (int i = 0; i < alternativePlatforms.Count; i++)
        {
            if (TryGoToPlatform(alternativePlatforms[i], alternativePlatformsOffset))
            {
                return;
            }
        }
    }

    bool TryGoToPlatform(MovingPlatform platform, Vector2 offset)
    {
        if (platform != null && platform.transform.position.x >= validPositionRange.x && platform.transform.position.x <= validPositionRange.y)
        {
            transform.SetParent(platform.transform);
            transform.localPosition = offset;

            PlayerData.instance.SetHazardRespawn(transform.position, false);
            return true;
        }

        return false;
    }
}
