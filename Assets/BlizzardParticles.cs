using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class BlizzardParticles : MonoBehaviour
{
    [SerializeField]
    Vector2 heightMinMax = new Vector2();

    [SerializeField]
    float heightOffset = 0;

    ParticleSystem[] particles;

    bool particlesEnabled = false;

    [SerializeField]
    bool followPlayerX = false;

    [SerializeField]
    Vector2 widthMinMax = new Vector2();

    [SerializeField]
    float widthOffset = 0f;

    new AudioSource audio;

    [SerializeField]
    float fadeAudioTime = 2.5f;

    [SerializeField]
    float audioMaxVolume = 0.584f;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.volume = 0f;
        }
        particles = GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particles)
        {
            particle.Stop();
        }
    }

    private void Update()
    {
        if (Player.Player1.transform.position.y >= heightMinMax.x && Player.Player1.transform.position.y <= heightMinMax.y)
        {

            if (followPlayerX && Player.Player1.transform.position.x >= widthMinMax.x && Player.Player1.transform.position.x <= widthMinMax.y)
            {
                EnableParticles(true);
                transform.SetPositionY(Player.Player1.transform.position.y + heightOffset);
                transform.SetPositionX(Player.Player1.transform.position.x + widthOffset);
            }
            else
            {
                EnableParticles(true);
                transform.SetPositionY(Player.Player1.transform.position.y + heightOffset);
            }
        }
        else
        {
            EnableParticles(false);
        }
    }

    IEnumerator FadeAudioRoutine(float speed, float to)
    {
        if (audio == null)
        {
            yield break;
        }

        var oldVolume = audio.volume;

        for (float t = 0; t < speed; t += Time.deltaTime)
        {
            audio.volume = Mathf.Lerp(oldVolume, to, t / speed);
            yield return null;
        }
    }


    void EnableParticles(bool enable)
    {
        if (particlesEnabled != enable)
        {
            particlesEnabled = enable;

            if (enable)
            {
                foreach (var particle in particles)
                {
                    particle.Play();
                }
                StopAllCoroutines();
                StartCoroutine(FadeAudioRoutine(fadeAudioTime, audioMaxVolume));
            }
            else
            {
                foreach (var particle in particles)
                {
                    particle.Stop();
                }

                StopAllCoroutines();
                StartCoroutine(FadeAudioRoutine(fadeAudioTime, 0f));
            }
        }
    }
}
