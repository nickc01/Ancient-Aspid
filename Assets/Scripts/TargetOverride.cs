using UnityEngine;
using System;

public class TargetOverride
{
    int mode = -1;

    Vector3 fixedPosition = default;
    Transform transform = null;
    Func<Vector3> positionFunc;

    public int Mode => mode;


    public bool SetTarget(Vector3 fixedPosition)
    {
        if (mode != 0 || this.fixedPosition != fixedPosition)
        {
            mode = 0;
            this.fixedPosition = fixedPosition;
            return true;
        }
        return false;
    }

    public bool SetTarget(Transform transform)
    {
        if (mode != 1 || this.transform != transform)
        {
            mode = 1;
            this.transform = transform;
            return true;
        }
        return false;
    }

    public bool SetTarget(Func<Vector3> positionFunc)
    {
        if (mode != 2 || this.positionFunc != positionFunc)
        {
            mode = 2;
            this.positionFunc = positionFunc;
            return true;
        }
        return false;
    }

    public Vector3 TargetPosition
    {
        get
        {
            switch (mode)
            {
                case 0:
                    return fixedPosition;
                case 1:
                    return transform.position;
                case 2:
                    return positionFunc();
                default:
                    return default;
            }
        }
    }

    public void ClearTarget()
    {
        mode = -1;
    }

    public bool HasPositionSet => mode >= 0;
}
