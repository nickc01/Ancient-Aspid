using UnityEngine;

public class BossTrailerLungeTrigger : MonoBehaviour 
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<HeroController>() != null)
        {
            var aspid = GameObject.FindObjectOfType<AncientAspid>();
            StartCoroutine(aspid.TriggerTrailerLandingRoutine());
        }
    }
}
