using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

public class WingsController : AspidBodyPart
{
    [field: SerializeField]
    public float XFlipFPS { get; private set; } = 8;

    [field: SerializeField]
    public int XFlipIncrements { get; private set; } = 5;

    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        yield return ChangeXPosition(GetDestinationLocalX(Orientation), GetDestinationLocalX(newOrientation), XFlipFPS, XFlipIncrements);
    }
}
