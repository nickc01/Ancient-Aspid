using UnityEngine;
using WeaverCore.Features;

public class RadianceLaserDisabler : MonoBehaviour
{
    [SerializeField]
    AncientAspid Boss;

    FarAwayLaser laserMove;

    private void Awake()
    {
        laserMove = Boss.GetComponent<FarAwayLaser>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        laserMove.moveEnabled = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        laserMove.moveEnabled = true;
    }
}
