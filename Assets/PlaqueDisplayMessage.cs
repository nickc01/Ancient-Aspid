using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using static BossStatueTrophyPlaque;

public class PlaqueDisplayMessage : Conversation
{
    [OnHarmonyPatch]
    static void OnPatch(HarmonyPatcher patch)
    {
        {
            var orig = typeof(BossStatueTrophyPlaque).GetMethod(nameof(BossStatueTrophyPlaque.DoTierCompleteEffect), BindingFlags.Public | BindingFlags.Instance);
            var postfix = typeof(PlaqueDisplayMessage).GetMethod(nameof(DoTierCompleteEffectPostfix), BindingFlags.NonPublic | BindingFlags.Static);

            patch.Patch(orig, null, postfix);
        }
    }

    static void DoTierCompleteEffectPostfix(BossStatueTrophyPlaque __instance, DisplayType type)
    {
        var displayMessage = GameObject.FindObjectOfType<PlaqueDisplayMessage>();

        if (displayMessage != null && displayMessage.enabled)
        {
            displayMessage.OnAward(type);
        }
    }

    void OnAward(DisplayType type)
    {
        if (type == DisplayType.Tier2 || type == DisplayType.Tier3)
        {
            if (!HeroController.instance.isHeroInPosition)
            {
                HeroController.instance.heroInPosition += Instance_heroInPosition;
            }
            else
            {
                DisplayConvo();
            }
        }
    }

    private void Instance_heroInPosition(bool forceDirect)
    {
        HeroController.instance.heroInPosition -= Instance_heroInPosition;
        DisplayConvo();
    }

    void DisplayConvo()
    {
        HeroController.instance.RelinquishControl();
        StartConversation(false, () =>
        {
            HideConversationBox();
            HeroController.instance.RegainControl();
        });
    }

    protected override IEnumerator DoConversation()
    {
        yield return SpeakImmediate("Congrats on beating the boss on Ascended mode. Radiant difficulty is currently not avaiable yet, but should be in an update soon");
    }

    private void OnEnable()
    {
        
    }
}
