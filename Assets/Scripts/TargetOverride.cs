using UnityEngine;
using System;
using System.Collections.Generic;

public class TargetOverride
{
    int mode = -1;

    Vector3 fixedPosition = default;
    Transform transform = null;
    Func<Vector3> positionFunc;
    public Guid guid;

    public int Mode => mode;

    public readonly int Priority;


    public TargetOverride(int priority)
    {
        guid = Guid.NewGuid();
        Priority = priority;
    }

    public bool SetTarget(Vector3 fixedPosition)
    {
        if (mode != 0 || this.fixedPosition != fixedPosition)
        {
            mode = 0;
            this.fixedPosition = fixedPosition;
            Debug.Log("SETTING TARGET OVERRIDE = (Vector3)" + fixedPosition);
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
            Debug.Log("SETTING TARGET OVERRIDE = (Transform)" + transform);
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
            Debug.Log("SETTING TARGET OVERRIDE = (Func)" + positionFunc());
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

    public class Sorter : IComparer<TargetOverride>
    {
        static Sorter _instance;
        public static Sorter Instance = _instance ??= new Sorter();

        Comparer<int> numComparer;

        public Sorter()
        {
            numComparer = Comparer<int>.Default;
        }

        public int Compare(TargetOverride x, TargetOverride y)
        {
            return numComparer.Compare(x.Priority, y.Priority);
        }
    }
}
