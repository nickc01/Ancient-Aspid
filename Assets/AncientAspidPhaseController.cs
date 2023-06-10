using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class AncientAspidPhaseController : MonoBehaviour
{
    AncientAspid boss;

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

    private void Awake()
    {
        boss = GetComponent<AncientAspid>();
        boss.StartPhases();
    }

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

    private void Update()
    {
        if (boss.EnteringFromBottom || !boss.FullyAwake)
        {
            return;
        }

        switch (boss.Phase)
        {
            case AncientAspid.BossPhase.Phase1:
                if (Player.Player1.transform.position.y >= phase2StartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.8f)
                {
                    boss.GoToNextPhase();
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase2StartHeight, 9999f);
                }
                break;
            case AncientAspid.BossPhase.Phase2:
                if (Player.Player1.transform.position.y >= phase2AStartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.760f)
                {
                    boss.GoToNextPhase();
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase2AStartHeight, 9999f);
                }
                break;
            case AncientAspid.BossPhase.Phase2A:
                if (Player.Player1.transform.position.y >= phase2BStartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.760f)
                {
                    boss.GoToNextPhase();
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase2BStartHeight, 9999f);
                }
                break;
            case AncientAspid.BossPhase.Phase2B:
                if (Player.Player1.transform.position.y >= phase2CStartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.760f)
                {
                    boss.GoToNextPhase();
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase2CStartHeight, 9999f);
                }
                break;
            case AncientAspid.BossPhase.Phase2C:
                if (Player.Player1.transform.position.y >= phase3StartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.760f)
                {
                    boss.GoToNextPhase();
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(0f, 9999f);
                }
                break;
            case AncientAspid.BossPhase.Phase3:
                if (Player.Player1.transform.position.y >= phase4StartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.45f || Player.Player1.transform.position.y >= phase3AStartHeight) {
                    boss.GoToNextPhase();
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase3BStartHeight, 9999f);
                    goto case AncientAspid.BossPhase.Phase3A;
                }

                /*if (Player.Player1.transform.position.y >= phase4StartHeight || (boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.45f)
                {
                    boss.GoToNextPhase();
                    boss.GoToNextPhase();
                    boss.GoToNextPhase();
                    boss.GoToNextPhase();
                    boss.TargetHeightRange = new Vector2(281.7f, 9999f);
                }*/
                break;
            //(boss.HealthManager.Health / (float)boss.StartingHealth) <= 0.45f && 
            case AncientAspid.BossPhase.Phase3A:
                if (Player.Player1.transform.position.y >= phase4StartHeight || (Player.Player1.transform.position.y >= phase3BStartHeight)/* || Player.Player1.transform.position.y >= phase3CStartHeight*/)
                {
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase3CStartHeight, 9999f);
                    boss.GoToNextPhase();
                }
                break;
            case AncientAspid.BossPhase.Phase3B:
                if (Player.Player1.transform.position.y >= phase4StartHeight || (Player.Player1.transform.position.y >= phase3CStartHeight))
                {
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(phase4StartHeight, 9999f);
                    boss.GoToNextPhase();
                }
                break;
            case AncientAspid.BossPhase.Phase3C:
                if (Player.Player1.transform.position.y >= phase4StartHeight)
                {
                    boss.EnableTargetHeightRange = true;
                    boss.TargetHeightRange = new Vector2(finalHeightLimit, 9999f);
                    boss.GoToNextPhase();
                }
                break;
            default:
                break;
        }
    }
}
