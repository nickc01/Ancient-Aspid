using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class FlybySpitter : MonoBehaviour
{
    [SerializeField]
    Vector2 angleMinMax = new Vector2(35,90 - 15);

    [SerializeField]
    Vector2 speedMinMax = new Vector2(5,15);

    [SerializeField]
    Vector2 delayMinMax = new Vector2(0,1f);

    [SerializeField]
    bool triggerOnAwake = false;

    [SerializeField]
    Vector2 minMaxDistanceToPlayer = new Vector2(1,10);


    private void Awake()
    {
        if (triggerOnAwake)
        {
            TriggerFlyBy();
        }
    }

    public void TriggerFlyBy()
    {
        StartCoroutine(DoFlyByRoutine());
    }

    public static void TriggerAllFlybys()
    {
        var spitters = GameObject.FindObjectsOfType<FlybySpitter>(true);

        foreach (var spitter in spitters)
        {
            spitter.TriggerFlyBy();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        var minDirection = MathUtilities.PolarToCartesian(angleMinMax.x,1f);
        Gizmos.DrawRay(transform.position, minDirection);

        var maxDirection = MathUtilities.PolarToCartesian(angleMinMax.y, 1f);
        Gizmos.DrawRay(transform.position, maxDirection);
    }

    IEnumerator DoFlyByRoutine()
    {
        yield return new WaitForSeconds(delayMinMax.RandomInRange());

        var audioPlayer = GetComponent<AudioPlayer>();

        audioPlayer.Play();


        var angle = angleMinMax.RandomInRange();
        var speed = speedMinMax.RandomInRange();

        var direction = MathUtilities.PolarToCartesian(angle, speed);

        var oldVolume = audioPlayer.Volume;

        while (true)
        {
            transform.position += (Vector3)direction * Time.deltaTime;

            var distanceToPlayer = Vector3.Distance(transform.position, Player.Player1.transform.position);

            distanceToPlayer = Mathf.Clamp(distanceToPlayer, minMaxDistanceToPlayer.x, minMaxDistanceToPlayer.y);

            audioPlayer.Volume = (1 - Mathf.InverseLerp(minMaxDistanceToPlayer.x, minMaxDistanceToPlayer.y, distanceToPlayer)) * oldVolume;

            yield return null;
        }
    }
}
