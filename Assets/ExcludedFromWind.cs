using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcludedFromWind : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        WindController.TouchedExclusions.Add(gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        WindController.TouchedExclusions.Remove(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        WindController.TouchedExclusions.Add(gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        WindController.TouchedExclusions.Remove(gameObject);
    }

    private void OnDisable()
    {
        WindController.TouchedExclusions.Remove(gameObject);
    }

    private void OnDestroy()
    {
        WindController.TouchedExclusions.Remove(gameObject);
    }

    public void EnableExclusion()
    {
        WindController.TouchedExclusions.Add(gameObject);
    }

    public void DisableExclusion()
    {
        WindController.TouchedExclusions.Remove(gameObject);
    }
}
