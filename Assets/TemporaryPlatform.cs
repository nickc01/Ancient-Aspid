using GlobalEnums;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;

public class TemporaryPlatform : MonoBehaviour
{
    static HashSet<TemporaryPlatform> hookedObjects = new HashSet<TemporaryPlatform>();

    SpriteFlasher flasher;
    SpriteRenderer mainRenderer;
    Collider2D mainCollider;
    WeaverAnimationPlayer mainAnimator;
    ExcludedFromWind excluder;

    [SerializeField]
    ParticleSystem disappearParticles;

    [SerializeField]
    ParticleSystem activeParticles;

    [SerializeField]
    string platType = "R Plat Wide";

    [SerializeField]
    float beginHideDelay = 3.5f;

    [SerializeField]
    float flashTime = 0.3f;

    public bool Visible { get; private set; } = true;

    bool hooked = false;

    [OnHarmonyPatch]
    static void OnHarmonyPatch(HarmonyPatcher patcher)
    {
        {
            var orig = typeof(HeroController).GetMethod("DieFromHazard", BindingFlags.NonPublic | BindingFlags.Instance);

            var prefix = typeof(TemporaryPlatform).GetMethod("DieFromHazardPrefix", BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }
    }

    static bool DieFromHazardPrefix()
    {
        foreach (var obj in hookedObjects)
        {
            obj.Show();
        }

        return true;
    }

    private void Awake()
    {
        if (enabled)
        {
            hookedObjects.Add(this);
        }

        excluder = GetComponent<ExcludedFromWind>();
        mainAnimator = GetComponent<WeaverAnimationPlayer>();
        mainRenderer = GetComponent<SpriteRenderer>();
        flasher = GetComponent<SpriteFlasher>();
        mainCollider = GetComponent<Collider2D>();
        Show();
    }

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        var hazard = (HazardType)hazardType;

        if (hazard == HazardType.PIT || hazard == HazardType.LAVA || hazard == HazardType.SPIKES || hazard == HazardType.ACID)
        {
            Show();
        }

        return damageAmount;
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

    void Show()
    {
        excluder.EnableExclusion();
        Visible = true;

        if (!mainRenderer.enabled)
        {
            mainAnimator.PlayAnimation($"{platType} Appear");
        }
        else
        {
            mainAnimator.PlayAnimation($"{platType} Idle");
        }

        activeParticles.Stop();
        disappearParticles.Stop();

        mainRenderer.enabled = true;
        mainCollider.enabled = true;
        excluder.DisableExclusion();

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        StartCoroutine(FadeFlash(flasher.FlashIntensity, 0f, flashTime));
        yield return Fade(mainRenderer.color, Color.white, flashTime);
        yield return new WaitForSeconds(beginHideDelay);

        activeParticles.Play();
        yield return FadeFlash(flasher.FlashIntensity, 1f, flashTime * 6);
        activeParticles.Stop();
        disappearParticles.Play();
        mainCollider.enabled = false;
        
        yield return new WaitForSeconds(0.1f);

        yield return mainAnimator.PlayAnimationTillDone($"{platType} Disappear");
        //yield return Fade(mainRenderer.color, default, flashTime);
        mainRenderer.enabled = false;
        flasher.FlashIntensity = 0f;

        yield break;
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            mainRenderer.color = Color.Lerp(from, to, t / time);
            yield return null;
        }

        mainRenderer.color = to;
    }

    IEnumerator FadeFlash(float from, float to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            flasher.FlashIntensity = Mathf.Lerp(from, to, t / time);
            yield return null;
        }

        flasher.FlashIntensity = to;
    }
}
