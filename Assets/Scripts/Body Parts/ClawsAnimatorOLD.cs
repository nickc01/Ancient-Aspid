/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class ClawsAnimator : BodyPartAnimator
{
    [field: SerializeField]
    public float XFlipFPS { get; private set; } = 8;

    [field: SerializeField]
    public int XFlipIncrements { get; private set; } = 5;


    public IEnumerator ChangeDirection()
    {
        return FlipXPosition(XFlipFPS, XFlipIncrements);
    }
}*/