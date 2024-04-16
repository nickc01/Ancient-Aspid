using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;

public static class HeroController_OnDeath_Patch
{
    [OnHarmonyPatch]
    static void OnHarmonyPatch(HarmonyPatcher patcher)
    {
        var orig = typeof(HeroController).GetMethod("Die", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefix = typeof(HeroController_OnDeath_Patch).GetMethod(nameof(DiePrefix), BindingFlags.NonPublic | BindingFlags.Instance);

        //Patch the HeroController.Die() method to run some code right before Die() gets called.
        patcher.Patch(orig, prefix, null);
    }

    static bool DiePrefix(HeroController __instance)
    {
        //INSERT CUSTOM CODE HERE THATS CALLED RIGHT BEFORE THE Die() FUNCTION GETS CALLED

        //If this returns true, then the code originally used in the Die() method gets run after this Prefix
        return true;
    }
}

public class DeepnestSceneUpdater : MonoBehaviour
{
    void Start()
    {
        var boneSprite = GameObject.Find("bone_deep_0122_b (1)");

        if (boneSprite != null)
        {
            boneSprite.transform.position -= new Vector3(0f,0.25f);
        }
    }
}
