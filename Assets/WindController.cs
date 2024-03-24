using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class WindController : MonoBehaviour
{
    public static HashSet<UnityEngine.Object> TouchedExclusions = new HashSet<Object>();

    [SerializeField]
    Vector2 windDirection = new Vector2(-1.5f, 0f);

    [SerializeField]
    float startupInterp = 0f;

    [SerializeField]
    float startupTime = 0.5f;

    Transform playerTransform;
    private void Awake()
    {
        playerTransform = Player.Player1.transform;
    }

    private void LateUpdate()
    {
        if (TouchedExclusions.Count == 0)
        {
            HeroController.instance.SetConveyorSpeed(windDirection.x * startupInterp);
            //playerTransform.position += (Vector3)(windDirection * Time.deltaTime * startupInterp);
        }
        else
        {
            HeroController.instance.SetConveyorSpeed(0f);
        }
    }

    public void StartWind()
    {
        StartCoroutine(StartWindRoutine());
    }

    IEnumerator StartWindRoutine()
    {
        for (float t = 0; t < startupTime; t += Time.deltaTime)
        {
            startupInterp = Mathf.Lerp(0f, 1f, t / startupTime);
            yield return null;
        }
    }
}
