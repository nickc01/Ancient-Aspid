using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : AspidBodyPart
{
    [field: SerializeField]
    public float XFlipFPS { get; private set; } = 8;

    [field: SerializeField]
    public int XFlipIncrements { get; private set; } = 5;


    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        return ChangeXPosition(GetDestinationLocalX(Orientation),GetDestinationLocalX(newOrientation), XFlipFPS, XFlipIncrements);
    }
}
