using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;


public class FlybyTrigger : MonoBehaviour
{
    [SerializeField]
    List<AudioClip> flyAwaySounds = new List<AudioClip>();

    [SerializeField]
    Vector2 flyAwayDelayMinMax = new Vector2(0,0.25f);

    [SerializeField]
    float volume = 0.5f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<HeroController>() != null)
        {
            FlybySpitter.TriggerAllFlybys();

            foreach (var sound in flyAwaySounds)
            {
                var delay = flyAwayDelayMinMax.RandomInRange();
                StartCoroutine(PlaySoundRoutine(sound, delay));
            }

        }
    }

    IEnumerator PlaySoundRoutine(AudioClip clip, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        WeaverAudio.PlayAtPoint(clip, Player.Player1.transform.position, volume);
    }
}
