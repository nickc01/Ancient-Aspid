using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;

public abstract class ShotgunController
{
    public List<LaserEmitter> Lasers;

    public LaserMode CurrentMode { get; protected set; }

    public enum LaserMode
    {
        None,
        Preparing,
        PostPrepare,
        Prefire,
        Firing,
        Ending
    }

    public virtual void Init(List<LaserEmitter> lasers)
    {
        Lasers = lasers;
    }

    public virtual void Update()
    {

    }

    public virtual void OnStun()
    {

    }

    public virtual void OnDone()
    {

    }

    public virtual void ChangeMode(LaserMode mode)
    {
        CurrentMode = mode;
    }

    public virtual bool DoScaleFlip() => true;

    public abstract Quaternion GetLaserRotation(int laserIndex);

    public virtual bool LaserEnabled(int laserIndex)
    {
        return true;
    }

    public float AimLaserAtTarget(LaserEmitter source, Vector3 target)
    {
        var origin = GetOrigin(source);

        var difference = new Vector2(target.x, target.y) - new Vector2(origin.position.x, origin.position.y);

        difference = origin.transform.parent.InverseTransformVector(difference);

        var angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        //origin.transform.SetRotationZ(angle + 90f);

        return angle;
    }

    Transform GetOrigin(LaserEmitter emitter)
    {
        return emitter.Laser.transform.parent;
    }
}