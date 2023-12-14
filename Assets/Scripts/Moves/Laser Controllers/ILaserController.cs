using System;
using System.Collections;
using UnityEngine;

public interface ILaserController
{
    void Init(FireLaserMove laserMove);
    bool CanFire(FireLaserMove laserMove);
    Quaternion GetStartAngle(FireLaserMove laserMove, Vector2 playerVelocity);
    IEnumerator Fire(FireLaserMove laserMove, Func<bool> moveCancelled, Action<Quaternion> setLaserAngle, Quaternion startAngle);
    void Uninit(FireLaserMove laserMove);
    void OnStun(FireLaserMove laserMove);
}


