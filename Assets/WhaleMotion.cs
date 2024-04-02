using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Utilities;

public class WhaleMotion : MonoBehaviour
{
    [SerializeField]
    float curveXOffset = 0f;

    [SerializeField]
    float curveYScale = 1f;

    [SerializeField]
    float curveXScale = 1f;

    [SerializeField]
    float curveYOffset = 0f;

    [SerializeField]
    float curveMovementSpeed = 0.01f;

    [SerializeField]
    float startT = -1f;

    [SerializeField]
    float endT = 1f;

    float timer = -1f;

    [SerializeField]
    UnityEvent onDone;

    private void Awake()
    {
        timer = startT;
    }

    private void Update()
    {
        timer += Time.deltaTime * curveMovementSpeed;

        if (timer >= endT)
        {
            timer = startT;
            onDone?.Invoke();
        }


        var posSample = SampleCurve(timer);
        var nextSample = SampleCurve(timer + 0.01f);

        transform.SetPosition2D(new Vector2(timer, posSample));

        var direction = MathUtilities.CartesianToPolar(new Vector2(timer + 0.01f, nextSample) - new Vector2(timer, posSample)).x;

        transform.SetZLocalRotation(direction);
    }

    float SampleCurve(float t)
    {
        t = (t - curveXOffset) / curveXScale;

        return (-(t * t) * curveYScale) + (1 + curveYOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < 200; i++)
        {
            var sample = Mathf.Lerp(startT, endT, i / 200f);

            var nextSample = Mathf.Lerp(startT, endT, (i + 1) / 200f);

            var s = SampleCurve(sample);
            var e = SampleCurve(nextSample);

            Gizmos.DrawLine(new Vector3(sample, s), new Vector3(nextSample, e));
        }
    }
}
