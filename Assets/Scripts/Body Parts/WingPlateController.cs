using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

public class WingPlateController : AspidBodyPart
{
    [SerializeField]
    private Sprite lungeSprite;

    [SerializeField]
    private Sprite jumpSprite;

    public override void OnStun()
    {
         transform.SetLocalPosition(x: 0f, y: 0f);
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
            yield return new WaitForSeconds(Boss.GroundMode.lungeDownwardsLandDelay);
        }
        yield return Animator.PlayAnimationTillDone("Lunge Land");
    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        Animator.StopCurrentAnimation();
        yield return ChangeDirection(newDirection, Boss.GroundMode.lungeTurnaroundSpeed);
        yield break;
    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        yield break;
    }

    public IEnumerator GroundPrepareJump()
    {
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition += Boss.GroundMode.jumpPosIncrements;
            yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
            if (i != Boss.GroundMode.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
        }

        if (jumpSprite != null)
        {
            MainRenderer.sprite = jumpSprite;
        }
        yield break;
    }

    public IEnumerator GroundLand(bool finalLanding)
    {
        transform.localPosition += Boss.GroundMode.jumpPosIncrements * Boss.GroundMode.groundJumpFrames;

        yield return new WaitForSeconds(Boss.GroundMode.groundJumpLandDelay);
        if (finalLanding)
        {
            for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
            {
                transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
            yield break;
        }
    }

    public override IEnumerator WaitTillChangeDirectionMidJump()
    {
        yield break;
    }

    public override IEnumerator MidJumpChangeDirection(AspidOrientation oldOrientation, AspidOrientation newOrientation)
    {
        Animator.PlaybackSpeed = Boss.GroundMode.MidAirSwitchSpeed;
        PreviousOrientation = CurrentOrientation;
        CurrentOrientation = newOrientation;
        MainRenderer.flipX = oldOrientation == AspidOrientation.Right;
        yield return Animator.PlayAnimationTillDone("Change Direction Quick");

        Animator.PlaybackSpeed = 1f;

        MainRenderer.flipX = newOrientation == AspidOrientation.Right;
        MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip("Change Direction", 0);
    }
}
