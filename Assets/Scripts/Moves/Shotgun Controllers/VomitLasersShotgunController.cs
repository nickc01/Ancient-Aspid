using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;

public class VomitLasersShotgunController : ShotgunController
{
    private VomitGlob[] globs;
    private float aimInterpSpeed;
    private Vector3[] currentLaserTargets;
    private bool aimAtPlayer = false;
    private bool ending = false;

    public VomitLasersShotgunController(VomitGlob[] globs, float aimInterpSpeed)
    {
        this.globs = globs;
        currentLaserTargets = new Vector3[globs.Length];
        this.aimInterpSpeed = aimInterpSpeed;
    }

    public override void ChangeMode(LaserMode mode)
    {
        if (mode == LaserMode.Firing)
        {
            aimAtPlayer = true;
        }

        if (mode == LaserMode.Ending)
        {
            ending = true;
        }
        base.ChangeMode(mode);
    }

    public override void Update()
    {
        if (aimAtPlayer && !ending)
        {
            for (int i = 0; i < currentLaserTargets.Length; i++)
            {
                currentLaserTargets[i] = Vector3.MoveTowards(currentLaserTargets[i], Player.Player1.transform.position, aimInterpSpeed * Time.deltaTime);
            }
        }
    }


    public override Quaternion GetLaserRotation(int laserIndex)
    {
        if (!aimAtPlayer)
        {
            currentLaserTargets[laserIndex] = globs[laserIndex].transform.position;
        }

        float rotation = AimLaserAtTarget(Lasers[laserIndex], currentLaserTargets[laserIndex]);

        return Quaternion.Euler(0f, 0f, rotation);
    }
}
