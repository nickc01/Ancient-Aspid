using System.Collections;
using UnityEngine;
using WeaverCore;

public class AncientAspid_AscendedStuff : MonoBehaviour
{
    [SerializeField]
    AncientAspid Boss;

    [SerializeField]
    float vomitPercentageEnable = 0.8f;

    [SerializeField]
    float rapidFireEnable = 0.5f;

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        yield return new WaitUntil(() => Boss.isActiveAndEnabled);
        yield return null;

        var health = Boss.HealthManager.Health;

        Boss.HealthManager.AddHealthMilestone(Mathf.RoundToInt(health * vomitPercentageEnable), () =>
        {
            Boss.GetComponent<VomitShotMove>().EnableMove(true);
        });

        Boss.HealthManager.AddHealthMilestone(Mathf.RoundToInt(health * rapidFireEnable), () =>
        {
            Boss.GetComponent<LaserRapidFireMove>().EnableMove(true);
        });
        //WeaverLog.Log("HEALTH = " + Boss.HealthManager.Health);
    }
}
