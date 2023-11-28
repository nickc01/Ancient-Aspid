using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class AncientAspidPhaseController : MonoBehaviour
{
    [SerializeField]
    float phase2StartHeight;

    [SerializeField]
    float phase2AStartHeight;

    [SerializeField]
    float phase2BStartHeight;

    [SerializeField]
    float phase2CStartHeight;

    [SerializeField]
    float phase3StartHeight;

    [SerializeField]
    float phase3AStartHeight;

    [SerializeField]
    float phase3BStartHeight;

    [SerializeField]
    float phase3CStartHeight;

    [SerializeField]
    float phase4StartHeight;

    [SerializeField]
    float finalHeightLimit;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.Lerp(Color.red,Color.yellow.With(a: 0f),0.5f);
        var orange = Gizmos.color;
        var green = Color.green.With(a: 0.5f);

        Gizmos.DrawCube(new Vector3(transform.position.x, phase2StartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));

        Gizmos.color = green;
        Gizmos.DrawCube(new Vector3(transform.position.x, phase2AStartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));
        Gizmos.DrawCube(new Vector3(transform.position.x, phase2BStartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));
        Gizmos.DrawCube(new Vector3(transform.position.x, phase2CStartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));
        Gizmos.color = orange;

        Gizmos.DrawCube(new Vector3(transform.position.x, phase3StartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));

        Gizmos.color = green;
        Gizmos.DrawCube(new Vector3(transform.position.x, phase3AStartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));
        Gizmos.DrawCube(new Vector3(transform.position.x, phase3BStartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));
        Gizmos.DrawCube(new Vector3(transform.position.x, phase3CStartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));
        Gizmos.color = orange;

        Gizmos.DrawCube(new Vector3(transform.position.x, phase4StartHeight, 0f), new Vector3(100f, 0.5f, 0.5f));

        Gizmos.color = Color.black.With(a: 0.5f);
        Gizmos.DrawCube(new Vector3(transform.position.x, finalHeightLimit, 0f), new Vector3(100f, 0.5f, 0.5f));
    }

}
