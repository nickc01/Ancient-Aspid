using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseContactRegion : MonoBehaviour
{
    public bool EnteredPhaseRegion { get; private set; }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnteredPhaseRegion = true;
    }
}
