using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

public class GodHomeIntroController : MonoBehaviour
{
    [SerializeField]
    float beginDelay = 0.5f;

    [SerializeField]
    float lockDelay = 0.5f;

    [SerializeField]
    float meteorActivateDelay = 1f;

    [SerializeField]
    float meteorDisappearDelay = 0.25f;

    [SerializeField]
    float platformDisappearDelay = 0.8f;

    [SerializeField]
    AncientAspidMeteor meteor;

    [SerializeField]
    CameraLockArea lockArea;

    [SerializeField]
    float lockDuration;

    [SerializeField]
    float impactDelay = 10f;

    [SerializeField]
    float playerActivateHeight = 67.61f;

    [Space]
    [Header("Extras")]
    [SerializeField]
    AncientAspidMusicController musicPlayer;

    [SerializeField]
    List<AudioClip> impactSounds;

    [SerializeField]
    ShakeType impactShakeType = ShakeType.BigShake;

    [SerializeField]
    List<ParticleSystem> impactParticles = new List<ParticleSystem>();

    [SerializeField]
    ColorFader impactFader;

    [SerializeField]
    Collider2D mainPlatformCollider;

    [SerializeField]
    SpriteRenderer mainPlatformRenderer;

    [SerializeField]
    Collider2D ceilingCollider;

    [SerializeField]
    Vector3 mainPlatformCrumblesOffset = default;

    [SerializeField]
    Vector2 mainPlatformScaleOffset = new Vector2(2f, 2f);

    [SerializeField]
    WindController wind;

    [SerializeField]
    AncientAspid boss;

    [SerializeField]
    CameraLockArea arenaArea;

    [SerializeField]
    MusicCue blankMusicCue;


    Bounds ceilingBounds;

    bool lockPlayerVelocity = false;

    Rigidbody2D playerRB;

    GameObject vignette;

    private void Awake()
    {
        if (blankMusicCue != null && !musicPlayer.UsingAlt)
        {
            Music.PlayMusicCue(blankMusicCue, 0f, 1f, false);
        }
        vignette = GameObject.Find("Vignette");
        ceilingBounds = ceilingCollider.bounds;
        ceilingCollider.enabled = false;
        arenaArea.gameObject.SetActive(false);

        if (vignette != null)
        {
            vignette.SetActive(false);
        }

        StartCoroutine(StarterRoutine());
    }

    IEnumerator StarterRoutine()
    {
        yield return new WaitForSeconds(beginDelay);
        StartCoroutine(ImpactRoutine());
        StartCoroutine(LockRoutine());
        StartCoroutine(MeteorRoutine());
    }

    IEnumerator ImpactRoutine()
    {
        yield return new WaitForSeconds(impactDelay);

        //Player.Player1.EnterRoarLock();

        HeroUtilities.PlayPlayerClip("Roar Lock");

        foreach (var clip in impactSounds)
        {
            WeaverAudio.PlayAtPoint(clip, transform.position);
        }

        CameraShaker.Instance.Shake(impactShakeType);

        foreach (var particle in impactParticles)
        {
            particle.Play();
        }

        impactFader.Fade(true);

        yield return new WaitForSeconds(meteorDisappearDelay);

        foreach (var particle in meteor.GetComponentsInChildren<ParticleSystem>())
        {
            particle.Stop();
        }

        foreach (var rend in meteor.GetComponentsInChildren<SpriteRenderer>())
        {
            rend.enabled = false;
        }
        yield return new WaitForSeconds(platformDisappearDelay);
        meteor.gameObject.SetActive(false);

        mainPlatformCollider.enabled = false;
        mainPlatformRenderer.enabled = false;

        Player.Player1.ExitCutsceneLock();

        //Player.Player1.ExitRoarLock();
        HeroController.instance.RelinquishControl();

        if (vignette != null)
        {
            vignette.SetActive(true);
        }

        mainPlatformRenderer.transform.position += mainPlatformCrumblesOffset;

        var originalScale = mainPlatformRenderer.transform.localScale;

        mainPlatformRenderer.transform.localScale = ((Vector3)(mainPlatformRenderer.transform.localScale * new Vector2(mainPlatformScaleOffset.x, mainPlatformScaleOffset.y))).With(z: 1f);

        for (int i = 0; i < mainPlatformRenderer.transform.childCount; i++)
        {
            var crumble = mainPlatformRenderer.transform.GetChild(i);

            crumble.gameObject.SetActive(true);
            crumble.transform.SetParent(null);
            crumble.transform.localScale = originalScale;
        }

        playerRB = Player.Player1.GetComponent<Rigidbody2D>();
        lockPlayerVelocity = true;

        while (Player.Player1.transform.position.y >= playerActivateHeight)
        {
            yield return null;
        }

        lockPlayerVelocity = false;

        HeroController.instance.RegainControl();
        wind.StartWind();
        boss.gameObject.SetActive(true);
        //yield return new WaitUntil(() => Player.Player1.transform.position.y <= ceilingBounds.min.y - 5f);

        ceilingCollider.enabled = true;
        arenaArea.gameObject.SetActive(true);

        yield break;
    }

    private void FixedUpdate()
    {
        if (lockPlayerVelocity)
        {
            if (playerRB.velocity.y < -21)
            {
                playerRB.velocity = playerRB.velocity.With(y: -21f);
            }
        }
    }

    IEnumerator LockRoutine()
    {
        yield return new WaitForSeconds(lockDelay);

        musicPlayer.Play(AncientAspidMusicController.MusicPhase.AR1);

        Player.Player1.EnterCutsceneLock(true);

        HeroController.instance.StopAnimationControl();

        if (!HeroController.instance.cState.facingRight)
        {
            HeroController.instance.FaceRight();
            HeroUtilities.PlayPlayerClip("Turn");
        }

        lockArea.gameObject.SetActive(true);
        yield return new WaitForSeconds(lockDuration);
        HeroUtilities.PlayPlayerClip("LookUp");
        lockArea.gameObject.SetActive(false);
        yield break;
    }

    IEnumerator MeteorRoutine()
    {
        yield return new WaitForSeconds(meteorActivateDelay);
        meteor.gameObject.SetActive(true);
        yield break;
    }
}
