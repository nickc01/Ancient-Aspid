using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Utilities;

public class WingsController : AspidBodyPart
{
    [SerializeField]
    Sprite lungeAnticSprite;

    [SerializeField]
    Sprite lungeSprite;

    /*[field: SerializeField]
    public float XFlipFPS { get; private set; } = 8;

    [field: SerializeField]
    public int XFlipIncrements { get; private set; } = 5;

    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        yield return ChangeXPosition(GetDestinationLocalX(Orientation), GetDestinationLocalX(newOrientation), XFlipFPS, XFlipIncrements);
    }*/

    [SerializeField]
    List<Sprite> groundSwitchSprites;

    [SerializeField]
    List<bool> groundSpritesFlipped;


    bool playDefaultAnimation = true;

    float baseYPos;

    public bool PlayDefaultAnimation
    {
        get => playDefaultAnimation;
        set
        {
            if (playDefaultAnimation != value)
            {
                playDefaultAnimation = value;
                if (playDefaultAnimation)
                {
                    Animator.PlayAnimation("Default");
                }
                else
                {
                    if (Animator.PlayingClip == "Default")
                    {
                        Animator.StopCurrentAnimation();
                    }
                }
            }
        }
    }

    bool jumping = false;

    protected override IEnumerator ChangeDirectionRoutine(float speedMultiplier = 1)
    {
        if (Boss.Claws.OnGround)
        {
            int frameCount = 4;
            /*if (jumping)
            {
                frameCount = 3;
            }*/
            //Debug.Log("SWITCHING WING DIRECTION");

            //var fps = (1f / (8f * Boss.lungeTurnaroundSpeed));

            Boss.StartBoundRoutine(UpdateLocalPosition(8f * speedMultiplier, frameCount));
            Boss.StartBoundRoutine(UpdateColliderOffset(8f * speedMultiplier, frameCount));

            float frameTime = 1f / (8f * speedMultiplier);
            float timer = 0;

            for (int i = 0; i < groundSwitchSprites.Count; i++)
            {
                MainRenderer.sprite = groundSwitchSprites[i];
                var flipped = groundSpritesFlipped[i];
                if (CurrentOrientation == AspidOrientation.Left)
                {
                    flipped = !flipped;
                }
                MainRenderer.flipX = flipped;
                while (true)
                {
                    yield return null;
                    timer += Time.deltaTime;
                    if (timer >= frameTime)
                    {
                        timer -= frameTime;
                        break;
                    }
                }
                //yield return new WaitForSeconds(frameTime);
            }
        }
        else
        {
            yield return base.ChangeDirectionRoutine(speedMultiplier);
        }
    }

    public override void OnStun()
    {
        transform.SetXLocalPosition(GetXForOrientation(CurrentOrientation));
        transform.SetYLocalPosition(baseYPos);
        transform.localEulerAngles = default;
        Animator.PlaybackSpeed = 1f;
        base.OnStun();
    }

    protected override void Awake()
    {
        baseYPos = transform.GetYLocalPosition();
        base.Awake();
        Animator.PlayAnimation("Default");
    }

    public void PrepareForLunge()
    {
        PlayDefaultAnimation = false;
        MainRenderer.sprite = lungeAnticSprite;
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
        Debug.Log("WINGS SWITCH");
        Animator.StopCurrentAnimation();
        MainRenderer.flipX = false;
        return ChangeDirection(newDirection, Boss.GroundMode.lungeTurnaroundSpeed);
    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        yield break;
    }

    public IEnumerator GroundPrepareJump()
    {
        //var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition += Boss.GroundMode.jumpPosIncrements;
            //transform.localEulerAngles += Boss.jumpRotIncrements * lookingDirection;
            yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        //var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
            //transform.localEulerAngles -= Boss.jumpRotIncrements * lookingDirection;
            if (i != Boss.GroundMode.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
        }

        Animator.PlayAnimation("Default");
        yield break;
    }

    public IEnumerator GroundLand(bool finalLanding)
    {
        Animator.StopCurrentAnimation();
        MainRenderer.sprite = groundSwitchSprites[0];
        transform.localPosition += Boss.GroundMode.jumpPosIncrements * Boss.GroundMode.groundJumpFrames;

        yield return new WaitForSeconds(Boss.GroundMode.groundJumpLandDelay);

        if (finalLanding)
        {
            for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
            {
                transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
                //transform.localEulerAngles -= Boss.jumpRotIncrements * lookingDirection;
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
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
        PreviousOrientation = oldOrientation;
        CurrentOrientation = newOrientation;
        CurrentOrientation = newOrientation;
        Boss.StartBoundRoutine(UpdateColliderOffset(8f * Boss.GroundMode.MidAirSwitchSpeed, 1));
        //Boss.StartBoundRoutine();

        //float oldX = GetXForOrientation(StartingLocalX, PreviousOrientation);
        float newX = GetXForOrientation(StartingLocalX, CurrentOrientation);

        yield return UpdateLocalPosition(8f * Boss.GroundMode.MidAirSwitchSpeed, 1,0f,newX);

        MainRenderer.flipX = newOrientation == AspidOrientation.Right;

        //return ChangeDirection(newOrientation,Boss.MidAirSwitchSpeed);
        //yield break;
    }
}
