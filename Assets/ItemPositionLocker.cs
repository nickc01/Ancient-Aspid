using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

public class ItemPositionLocker : MonoBehaviour
{
    [SerializeField]
    float minX;

    [SerializeField]
    float maxX;

    [SerializeField]
    float minY;

    [SerializeField]
    float maxY;

    private void LateUpdate()
    {
        if (transform.GetXPosition() < minX)
        {
            transform.SetXPosition(minX);
        }

        if (transform.GetXPosition() > maxX)
        {
            transform.SetXPosition(maxX);
        }

        if (transform.GetYPosition() < minY)
        {
            transform.SetYPosition(minY);
        }

        if (transform.GetYPosition() > maxY)
        {
            transform.SetYPosition(maxY);
        }
    }
}
