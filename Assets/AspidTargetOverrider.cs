using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspidTargetOverrider : MonoBehaviour
{
    [SerializeField]
    Vector2 flyTarget;

    [SerializeField]
    int priority = 10000;

    [SerializeField]
    AncientAspid Boss;

    TargetOverride targetOverride;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetOverride != null)
        {
            Boss.RemoveTargetOverride(targetOverride);
        }
        targetOverride = Boss.AddTargetOverride(priority);

        targetOverride.SetTarget(flyTarget);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (targetOverride != null)
        {
            Boss.RemoveTargetOverride(targetOverride);
            targetOverride = null;
        }
    }
}
