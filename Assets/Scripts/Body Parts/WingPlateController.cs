using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;
using static WeaverCore.Utilities.WeaverAnimationData;

public class WingPlateController : AspidBodyPart
{
    [SerializeField]
    Sprite lungeSprite;

    /*WeaverAnimationData.Clip changeDirectionClip;
    WeaverAnimationData.Clip centerizeClip;
    WeaverAnimationData.Clip decenterizeClip;*/

    /*protected override void Awake()
    {
        base.Awake();
        changeDirectionClip = AnimationPlayer.AnimationData.GetClip("Change Direction");
        centerizeClip = AnimationPlayer.AnimationData.GetClip("Centerize");
        decenterizeClip = AnimationPlayer.AnimationData.GetClip("Decenterize");
    }*/

    /*protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
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
    }*/

    public override void OnStun()
    {
        transform.SetLocalPosition(x: 0f,y: 0f);
        Animator.PlaybackSpeed = 1f;
        transform.localEulerAngles = default;
        base.OnStun();
    }

    public void DoLunge()
    {
        MainRenderer.sprite = lungeSprite;
    }

    public IEnumerator PlayLanding(bool slide)
    {
        if (!slide)
        {
            MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land", 0);
            yield return new WaitForSeconds(Boss.lungeDownwardsLandDelay);
        }
        yield return Animator.PlayAnimationTillDone("Lunge Land");
    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        Animator.StopCurrentAnimation();
        yield return ChangeDirection(newDirection, Boss.lungeTurnaroundSpeed);
        yield break;
    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        yield break;
    }

    public IEnumerator GroundPrepareJump()
    {
        //var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

        for (int i = 0; i < Boss.groundJumpFrames; i++)
        {
            transform.localPosition += Boss.jumpPosIncrements;
            //transform.localEulerAngles += Boss.jumpRotIncrements * lookingDirection;
            yield return new WaitForSeconds(1f / Boss.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        //var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;
        for (int i = 0; i < Boss.groundJumpFrames; i++)
        {
            transform.localPosition -= Boss.jumpPosIncrements;
            //transform.localEulerAngles -= Boss.jumpRotIncrements * lookingDirection;
            if (i != Boss.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.groundJumpLaunchFPS);
            }
        }
        yield break;
    }

    public IEnumerator GroundLand(bool finalLanding)
    {
        transform.localPosition += Boss.jumpPosIncrements * Boss.groundJumpFrames;

        yield return new WaitForSeconds(Boss.groundJumpLandDelay);
        if (finalLanding)
        {
            for (int i = 0; i < Boss.groundJumpFrames; i++)
            {
                transform.localPosition -= Boss.jumpPosIncrements;
                //transform.localEulerAngles -= Boss.jumpRotIncrements * lookingDirection;
                yield return new WaitForSeconds(1f / Boss.groundJumpLaunchFPS);
            }
            yield break;
        }
        //var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;
    }

    public override IEnumerator WaitTillChangeDirectionMidJump()
    {
        yield break;
    }

    public override IEnumerator MidJumpChangeDirection(AspidOrientation oldOrientation, AspidOrientation newOrientation)
    {
        Animator.PlaybackSpeed = Boss.MidAirSwitchSpeed;
        PreviousOrientation = CurrentOrientation;
        CurrentOrientation = newOrientation;
        MainRenderer.flipX = oldOrientation == AspidOrientation.Right;
        yield return Animator.PlayAnimationTillDone("Change Direction Quick");

        Animator.PlaybackSpeed = 1f;

        MainRenderer.flipX = newOrientation == AspidOrientation.Right;
        MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip("Change Direction", 0);
    }
}
