
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

public static class ShadowDashTracker
{
    public static bool PlayerHasShadowDashReady => !dashing;

    static bool dashing = false;

    static float dashTimer = 0f;

    [OnRuntimeInit]
    static void Init()
    {
        UnboundCoroutine.Start(CheckerRoutine());
    }

    static IEnumerator CheckerRoutine()
    {
        yield return new WaitUntil(() => Player.Player1Raw != null);

        while (Player.Player1Raw != null)
        {
            if (dashing)
            {
                dashTimer -= Time.deltaTime;

                if (dashTimer <= 0)
                {
                    dashTimer = 0;
                    dashing = false;
                }
            }
            else
            {
                if (HeroController.instance.cState.shadowDashing)
                {
                    dashing = true;
                    dashTimer = HeroController.instance.SHADOW_DASH_COOLDOWN;
                }
            }
            yield return null;
        }
    }
}