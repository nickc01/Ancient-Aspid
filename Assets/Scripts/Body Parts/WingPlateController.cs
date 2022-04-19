using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class WingPlateController : AspidBodyPart
{
    WeaverAnimationData.Clip changeDirectionClip;
    WeaverAnimationData.Clip centerizeClip;
    WeaverAnimationData.Clip decenterizeClip;

    protected override void Awake()
    {
        base.Awake();
        changeDirectionClip = AnimationPlayer.AnimationData.GetClip("Change Direction");
        centerizeClip = AnimationPlayer.AnimationData.GetClip("Centerize");
        decenterizeClip = AnimationPlayer.AnimationData.GetClip("Decenterize");
    }

    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        IEnumerator ApplyClip(WeaverAnimationData.Clip clip)
        {
            StartCoroutine(ChangeXPosition(GetDestinationLocalX(Orientation), GetDestinationLocalX(newOrientation), clip));
            yield return AnimationPlayer.PlayAnimationTillDone(clip.Name);
        }

        if (newOrientation != AspidOrientation.Center)
        {
            if (Orientation == AspidOrientation.Center)
            {
                if (newOrientation == AspidOrientation.Left)
                {
                    AnimationPlayer.SpriteRenderer.flipX = false;
                }
                else
                {
                    AnimationPlayer.SpriteRenderer.flipX = true;
                }
                yield return ApplyClip(decenterizeClip);
            }
            else
            {
                Sprite initialFrame = AnimationPlayer.SpriteRenderer.sprite;

                yield return ApplyClip(changeDirectionClip);

                AnimationPlayer.SpriteRenderer.flipX = newOrientation == AspidOrientation.Right;
                AnimationPlayer.SpriteRenderer.sprite = initialFrame;
            }
        }
        else
        {
            yield return ApplyClip(centerizeClip);
        }
    }
}
