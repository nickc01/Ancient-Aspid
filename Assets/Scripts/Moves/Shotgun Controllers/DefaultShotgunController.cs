using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;

public class DefaultShotgunController : ShotgunController
{
    private float prepareLerpSpeed;
    private float attackLerpSpeed;
    private List<float> laserRotations;
    private LaserEmitter centerLaser;
    private Vector3 targetPos;

    public DefaultShotgunController(float prepareLerpSpeed, float attackLerpSpeed, List<float> laserRotations)
    {
        this.prepareLerpSpeed = prepareLerpSpeed;
        this.attackLerpSpeed = attackLerpSpeed;
        this.laserRotations = laserRotations;
    }

    public override void Init(List<LaserEmitter> lasers)
    {
        base.Init(lasers);
        centerLaser = lasers[Mathf.FloorToInt(lasers.Count / 2f)];
    }

    public override void Update()
    {
        if (CurrentMode == LaserMode.Preparing)
        {
            targetPos = Vector3.Lerp(targetPos, Player.Player1.transform.position, Time.deltaTime * prepareLerpSpeed);
        }
        else if (CurrentMode == LaserMode.Firing)
        {
            targetPos = Vector3.MoveTowards(targetPos, Player.Player1.transform.position, attackLerpSpeed * Time.deltaTime);
        }
    }

    public override void ChangeMode(LaserMode mode)
    {
        if (mode == LaserMode.Preparing)
        {
            targetPos = Player.Player1.transform.position;
        }
        base.ChangeMode(mode);
    }

    public override Quaternion GetLaserRotation(int laserIndex)
    {
        float rotation = AimLaserAtTarget(centerLaser, targetPos);

        return Quaternion.Euler(0f, 0f, rotation + laserRotations[laserIndex]);
    }
}
