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


/*public abstract class LaserControllerNEW : MonoBehaviour
{
    public AncientAspid Boss { get; private set; }

    public event Action<Quaternion> OnAngleSet;

    public void Init(AncientAspid boss, Action<Quaternion> onAngleSet)
    {
        Boss = boss;
        OnAngleSet += onAngleSet;
        OnLaserStart();
    }

    public IEnumerator FireLaser(Func<bool> cancel, Quaternion startAngle)
    {
        //OnFireStart();

        yield return FireRoutine(cancel, startAngle);

        //OnFireEnd();
    }

    public void Uninit()
    {
        OnLaserEnd();
        Boss = null;
        OnAngleSet = null;
    }

    public abstract Quaternion GetStartAngle();

    /// <summary>
    /// Called when the laser move begins
    /// </summary>
    protected virtual void OnLaserStart() { }

    /// <summary>
    /// Can this laser be fired? If this returns false, then the laser move will get cancelled
    /// </summary>
    public abstract bool CanFire();

    //protected virtual void OnFireStart() { }

    protected abstract IEnumerator AnticRoutine();

    protected abstract IEnumerator FireRoutine(Func<bool> cancel, Quaternion startAngle);

    //protected virtual void OnFireEnd() { }

    /// <summary>
    /// Called when the laser move ends
    /// </summary>
    protected virtual void OnLaserEnd() { }

    public abstract void OnStun();
}*/