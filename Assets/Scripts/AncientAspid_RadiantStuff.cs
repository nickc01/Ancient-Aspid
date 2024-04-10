using System.Collections;
using UnityEngine;

public class AncientAspid_RadiantStuff : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(LockRoutine());
    }

    IEnumerator LockRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        HeroController.instance.RelinquishControl();
    }
}
