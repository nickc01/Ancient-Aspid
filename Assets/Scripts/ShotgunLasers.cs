using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;

public class ShotgunLasers : MonoBehaviour
{
    public List<LaserEmitter> Lasers;

    [SerializeField]
    public List<float> laserTransparencies;

    [SerializeField]
    Vector3 defaultPosition;

    [SerializeField]
    Vector3 defaultRotation;

    [SerializeField]
    Vector3 attackPosition;

    [SerializeField]
    Vector3 attackRotation;

    public void PlayAntics()
    {
        for (int i = 0; i < Lasers.Count; i++)
        {
            PlayAntic(i);
        }
    }

    public void PlayAntic(LaserEmitter emitter)
    {
        emitter.ChargeUpLaser_P1();
    }

    public void PlayAntic(int laserIndex)
    {
        var emitter = Lasers[laserIndex];
        PlayAntic(emitter);
    }

    public void PlayAttack(LaserEmitter emitter)
    {
        emitter.FireLaser_P2();
    }

    public void PlayAttack(int laserIndex)
    {
        var emitter = Lasers[laserIndex];
        PlayAttack(emitter);
    }

    public void PlayAttacks()
    {
        for (int i = 0; i < Lasers.Count; i++)
        {
            PlayAttack(i);
        }
    }

    public void ApplyTransparencies()
    {
        for (int i = 0; i < Lasers.Count; i++)
        {
            var color = Lasers[i].Laser.Color;
            color.a = laserTransparencies[i];
            Lasers[i].Laser.Color = color;
        }
    }

    public void RemoveTransparencies()
    {
        for (int i = 0; i < Lasers.Count; i++)
        {
            var color = Lasers[i].Laser.Color;
            color.a = 1;
            Lasers[i].Laser.Color = color;
        }
    }

    public void SetAttackMode(bool attackMode, bool facingRight)
    {
        if (attackMode)
        {
            transform.localPosition = FlipXIfOnRight(attackPosition,facingRight);
            transform.localEulerAngles = FlipZIfOnRight(attackRotation,facingRight);
        }
        else
        {
            transform.localPosition = FlipXIfOnRight(defaultPosition,facingRight);
            transform.localEulerAngles = FlipZIfOnRight(defaultRotation,facingRight);
        }

        transform.localScale = new Vector3(facingRight ? -1 : 1,1f, 1f);
    }

    Vector3 FlipXIfOnRight(Vector3 position, bool facingRight)
    {
        if (facingRight)
        {
            position.x = -position.x;
        }
        return position;
    }

    Vector3 FlipZIfOnRight(Vector3 rotation, bool facingRight)
    {
        if (facingRight)
        {
            rotation.z = -rotation.z;
        }
        return rotation;
    }
}
