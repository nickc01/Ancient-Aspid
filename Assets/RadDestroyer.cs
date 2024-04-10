using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadDestroyer : MonoBehaviour
{
    [SerializeField]
    float delay = 10;

    [NonSerialized]
    new Collider2D collider2D;

    private void Awake()
    {
        collider2D = GetComponent<Collider2D>();
        collider2D.enabled = false;
        StartCoroutine(DelayRoutine());
    }

    IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(delay);
        collider2D.enabled = true;
    }
}
