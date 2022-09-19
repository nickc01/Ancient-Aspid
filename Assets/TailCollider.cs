using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailCollider : MonoBehaviour
{
    public enum Orientation
    {
        Left,
        Right,
        Center
    }


    [SerializeField]
    List<Vector3> positions;

    [SerializeField]
    List<Vector3> scales;

    public Orientation CurrentOrientation { get; private set; } = Orientation.Left;

    Coroutine changeCoroutine;


    public void ChangeOrientation(Orientation newOrientation, float time)
    {
        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
            changeCoroutine = null;
        }

        CurrentOrientation = newOrientation;

        changeCoroutine = StartCoroutine(ChangeRoutine(newOrientation, time));
    }

    IEnumerator ChangeRoutine(Orientation newOrientation, float time)
    {
        var oldPosition = transform.localPosition;
        var oldScale = transform.localScale;

        var newPosition = positions[(int)newOrientation];
        var newScale = scales[(int)newOrientation];


        for (float t = 0; t < time; t += Time.deltaTime)
        {
            transform.localPosition = Vector3.Lerp(oldPosition,newPosition,t / time);
            transform.localScale = Vector3.Lerp(oldScale,newScale,t / time);
            yield return null;
        }

        transform.localPosition = newPosition;
        transform.localScale = newScale;
    }


}
