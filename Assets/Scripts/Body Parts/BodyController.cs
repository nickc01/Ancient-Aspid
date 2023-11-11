using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;


public class BodyController : AspidBodyPart
{
    public bool TailRaised { get; private set; } = false;
    public bool PulsingUp { get; private set; } = true;

    public bool ChangingTailState { get; private set; }
    public bool ChangingDirection { get; private set; }

    public bool AnimationStateChanging => ChangingDirection || ChangingTailState;

    WeaverAnimationPlayer animator;

    /*WeaverAnimationData.Clip changeDirectionClip;
    WeaverAnimationData.Clip centerizeClip;
    WeaverAnimationData.Clip decenterizeClip;*/

    Coroutine defaultAnimationRoutine;

    bool playDefaultAnimation = true;

    float currentFrameTimer = 0;
    int currentFrame = 0;
    WeaverAnimationData.Clip currentClip;

    public bool PlayDefaultAnimation
    {
        get => playDefaultAnimation;
        set
        {
            if (value != playDefaultAnimation)
            {
                playDefaultAnimation = value;
                if (playDefaultAnimation)
                {
                    defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(CurrentOrientation));
                }
                else
                {
                    if (defaultAnimationRoutine != null)
                    {
                        StopCoroutine(defaultAnimationRoutine);
                        defaultAnimationRoutine = null;
                    }
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<WeaverAnimationPlayer>();
        /*changeDirectionClip = animator.AnimationData.GetClip("Change Direction");
        centerizeClip = animator.AnimationData.GetClip("Centerize");
        decenterizeClip = animator.AnimationData.GetClip("Decenterize");*/
        defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(CurrentOrientation));
    }

    public override void OnStun()
    {
        transform.SetXLocalPosition(GetXForOrientation(CurrentOrientation));
        transform.SetYLocalPosition(0f);
        transform.localEulerAngles = default;
        animator.PlaybackSpeed = 1f;
        base.OnStun();
    }

    IEnumerator DefaultAnimationRoutine(AspidOrientation orientation)
    {
        /*if (TailRaised)
        {
            yield break;
        }*/
        while (true)
        {
            currentClip = GetClipForState(TailRaised, PulsingUp, orientation);

            currentFrame = animator.AnimationData.GetStartingFrame(currentClip.Name);
            animator.SpriteRenderer.sprite = currentClip.Frames[currentFrame];

            float secondsPerFrame = 1f / currentClip.FPS;

            while (currentFrame != -1)
            {
                yield return null;
                currentFrameTimer += Time.deltaTime;
                if (currentFrameTimer >= secondsPerFrame)
                {
                    currentFrameTimer -= secondsPerFrame;
                    currentFrame = animator.AnimationData.GoToNextFrame(currentClip.Name, currentFrame);
                    if (currentFrame > -1)
                    {
                        animator.SpriteRenderer.sprite = currentClip.Frames[currentFrame];
                    }
                }
            }

            PulsingUp = !PulsingUp;
        }
    }

    void StopDefaultAnimation()
    {
        if (defaultAnimationRoutine != null)
        {
            StopCoroutine(defaultAnimationRoutine);
            defaultAnimationRoutine = null;
        }
    }


    WeaverAnimationData.Clip GetClipForState(bool tailRaised, bool goingUp, AspidOrientation orientation)
    {
        if (orientation == AspidOrientation.Left || orientation == AspidOrientation.Right)
        {
            string pulseState = goingUp ? "Up" : "Down";
            string tailState = tailRaised ? "Raised" : "Lowered";

            return animator.AnimationData.GetClip($"Tail{tailState}-{pulseState}");
        }
        else
        {
            string pulseState = goingUp ? "Up" : "Down";

            return animator.AnimationData.GetClip($"TailLowered-{pulseState}-Centered");
        }
    }

    /*public float GetChangeDirectionDelay()
    {
        float fps = changeDirectionClip.FPS;
        float delay = 0;
        if (PulsingUp)
        {
            float timer = 0;
            for (int i = currentFrame; i > 0; i--)
            {
                delay += timer;
                currentFrame--;
                timer = (1f / fps);
            }
        }
        else
        {
            float timer = 0;
            for (int i = currentFrame; i < currentClip.Frames.Count; i++)
            {
                delay += timer;
                currentFrame++;
                timer = (1f / fps);
                if (currentFrame >= currentClip.Frames.Count)
                {
                    currentFrame = 0;
                    break;
                }
            }
        }
        return delay;
    }*/

    public IEnumerator PrepareChangeDirection()
    {
        VerifyState();
        ChangingDirection = true;
        StopDefaultAnimation();
        float fps = DEFAULT_FPS;
        if (PulsingUp)
        {
            float timer = 0;
            for (int i = currentFrame; i > 0; i--)
            {
                if (timer > 0)
                {
                    yield return new WaitForSeconds(timer);
                }
                currentFrame--;
                animator.SpriteRenderer.sprite = currentClip.Frames[currentFrame];
                timer = (1f / fps);
            }
        }
        else
        {
            float timer = 0;
            for (int i = currentFrame; i < currentClip.Frames.Count; i++)
            {
                if (timer > 0)
                {
                    yield return new WaitForSeconds(timer);
                }
                currentFrame++;
                timer = (1f / fps);
                if (currentFrame >= currentClip.Frames.Count)
                {
                    currentFrame = 0;
                    animator.SpriteRenderer.sprite = currentClip.Frames[currentFrame];
                    break;
                }
                animator.SpriteRenderer.sprite = currentClip.Frames[currentFrame];
            }
        }
    }

    protected override IEnumerator ChangeDirectionRoutine(float speedMultiplier = 1f)
    {
        /*if (CurrentOrientation == AspidOrientation.Center)
        {
            MainCollider.offset += new Vector2(0f, 1.17f);
        }
        else if (PreviousOrientation == AspidOrientation.Center)
        {
            MainCollider.offset -= new Vector2(0f, 1.17f);
        }*/

        if (PreviousOrientation == AspidOrientation.Center && CurrentOrientation != AspidOrientation.Center)
        {
            MainCollider.offset += new Vector2(0f, 1.17f);
        }

        string tailState = TailRaised ? "Raised" : "Lowered";

        if (PreviousOrientation == AspidOrientation.Center)
        {
            if (CurrentOrientation != AspidOrientation.Center)
            {
                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                yield return PlayChangeDirectionClip(tailState + " - Decenterize", DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES);
            }
        }
        else
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                yield return PlayChangeDirectionClip(tailState + " - Centerize", DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES);
            }
            else
            {
                Sprite initialFrame = Animator.SpriteRenderer.sprite;

                yield return PlayChangeDirectionClip(tailState + " - Change Direction", DEFAULT_FPS * speedMultiplier, DEFAULT_CHANGE_DIR_FRAMES);

                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                Animator.SpriteRenderer.sprite = initialFrame;
            }
        }
        //yield return base.ChangeDirectionRoutine();

        if (CurrentOrientation == AspidOrientation.Center)
        {
            MainCollider.offset -= new Vector2(0f, 1.17f);
        }

        PulsingUp = true;
        currentFrame = 0;
        ChangingDirection = false;
        if (PlayDefaultAnimation)
        {
            defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(CurrentOrientation));
        }
    }

    public IEnumerator PlayLanding(bool slide)
    {
        if (!slide)
        {
            yield return new WaitForSeconds(Boss.GroundMode.lungeDownwardsLandDelay);
        }
        yield break;
    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        yield return ChangeDirection(newDirection, Boss.GroundMode.lungeTurnaroundSpeed);
        yield break;
    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        yield break;
    }

    public IEnumerator GroundPrepareJump()
    {
        var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition += Boss.GroundMode.jumpPosIncrements;
            transform.localEulerAngles += Boss.GroundMode.jumpRotIncrements * lookingDirection;
            yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpPrepareFPS);
        }
        yield break;
    }

    public IEnumerator GroundLaunch()
    {
        animator.PlaybackSpeed = Boss.GroundMode.MidAirSwitchSpeed * 1.5f;
        //StartCoroutine(PlayBodyAnim());
        var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;
        for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
        {
            transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
            transform.localEulerAngles -= Boss.GroundMode.jumpRotIncrements * lookingDirection;
            if (i == 1)
            {
                animator.PlayAnimation("Lower Tail Quick");
            }
            if (i != Boss.GroundMode.groundJumpFrames - 1)
            {
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }
        }
        TailRaised = false;
        yield break;
    }

    /*IEnumerator PlayBodyAnim()
    {
        yield return animator.PlayAnimationTillDone("Lower Tail");
        yield return animator.PlayAnimationTillDone("Raise Tail");
    }*/

    public IEnumerator GroundLand(bool finalLanding)
    {
        var lookingDirection = Boss.Orientation == AspidOrientation.Right ? -1f : 1f;

        transform.localPosition += Boss.GroundMode.jumpPosIncrements * Boss.GroundMode.groundJumpFrames;
        transform.localEulerAngles += Boss.GroundMode.jumpRotIncrements * lookingDirection * Boss.GroundMode.groundJumpFrames;

        yield return new WaitForSeconds(Boss.GroundMode.groundJumpLandDelay);

        if (finalLanding)
        {
            for (int i = 0; i < Boss.GroundMode.groundJumpFrames; i++)
            {
                transform.localPosition -= Boss.GroundMode.jumpPosIncrements;
                transform.localEulerAngles -= Boss.GroundMode.jumpRotIncrements * lookingDirection;
                yield return new WaitForSeconds(1f / Boss.GroundMode.groundJumpLaunchFPS);
            }

            yield return new WaitUntil(() => animator.PlayingGUID == default);

            animator.PlaybackSpeed = 1f;

            yield break;
        }
    }

    public override IEnumerator WaitTillChangeDirectionMidJump()
    {
        return new WaitUntil(() => animator.PlayingGUID == default);
    }

    public override IEnumerator MidJumpChangeDirection(AspidOrientation oldOrientation, AspidOrientation newOrientation)
    {
        PreviousOrientation = CurrentOrientation;
        animator.PlaybackSpeed = Boss.GroundMode.MidAirSwitchSpeed;
        CurrentOrientation = newOrientation;
        MainRenderer.flipX = oldOrientation == AspidOrientation.Right;
        yield return animator.PlayAnimationTillDone("Lowered - Change Direction Quick");

        MainRenderer.flipX = newOrientation == AspidOrientation.Right;
        MainRenderer.sprite = animator.AnimationData.GetFrameFromClip("Lowered - Change Direction", 0);

        animator.PlaybackSpeed = Boss.GroundMode.MidAirSwitchSpeed * 1.5f;
        yield return animator.PlayAnimationTillDone("Raise Tail Quick");
        TailRaised = true;
    }

    public override IEnumerator GroundMoveCancel(bool onGround)
    {
        yield return animator.PlayAnimationTillDone("Lower Tail Quick");
        TailRaised = false;
    }

    /*protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        IEnumerator ApplyClip(WeaverAnimationData.Clip clip)
        {
            StartCoroutine(ChangeXPosition(GetDestinationLocalX(Orientation), GetDestinationLocalX(newOrientation), clip));
            StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, Orientation), GetDestinationLocalX(StartingColliderOffset, newOrientation), clip));
            yield return AnimationPlayer.PlayAnimationTillDone(clip.Name);
            PulsingUp = true;
            currentFrame = 0;
            defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(newOrientation));
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
                MainCollider.offset += new Vector2(0f, 1.17f);
                yield return ApplyClip(decenterizeClip);
            }
            else
            {
                yield return ApplyClip(changeDirectionClip);
                AnimationPlayer.SpriteRenderer.flipX = newOrientation == AspidOrientation.Right;
            }
        }
        else
        {
            yield return ApplyClip(centerizeClip);
            MainCollider.offset -= new Vector2(0f, 1.17f);
        }
        ChangingDirection = false;
    }*/

    public IEnumerator RaiseTail(float speed = 1f)
    {
        VerifyState();
        if (TailRaised)
        {
            yield break;
        }
        ChangingTailState = true;
        StopDefaultAnimation();
        animator.PlaybackSpeed = speed;
        yield return animator.PlayAnimationTillDone("Raise Tail");
        animator.PlaybackSpeed = 1f;
        TailRaised = true;
        ChangingTailState = false;
    }

    public IEnumerator LowerTail(float speed = 1f)
    {
        VerifyState();
        if (!TailRaised)
        {
            yield break;
        }
        ChangingTailState = true;
        StopDefaultAnimation();
        animator.PlaybackSpeed = speed;
        yield return animator.PlayAnimationTillDone("Lower Tail");
        animator.PlaybackSpeed = 1f;
        TailRaised = false;
        ChangingTailState = false;
        if (PlayDefaultAnimation)
        {
            defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(CurrentOrientation));
        }
    }

    public override IEnumerator GroundJumpBeginFalling(bool switchedDirection)
    {
        if (!switchedDirection)
        {
            yield return animator.PlayAnimationTillDone("Raise Tail Quick");
        }
    }

    void VerifyState()
    {
        if (ChangingTailState)
        {
            throw new System.Exception("Error: The tail state is already being changed");
        }
        if (ChangingDirection)
        {
            throw new System.Exception("Error: The direction is already being changed");
        }
    }
}


/*public class BodyAnimatorOLO : BodyPartAnimator
{
    [SerializeField]
    List<Sprite> risingFrames;

    [SerializeField]
    List<Sprite> loweringFrames;

    [SerializeField]
    float defaultFPS = 8;

    bool rising = true;
    int currentFrame = 0;


    float secondsPerFrame = 0;
    float frameTimer = 0;

    [field: SerializeField]
    public bool PlayDefaultAnimation { get; set; } = true;

    public bool ChangingDirection { get; private set; }

    public float DefaultFPS
    {
        get => defaultFPS;
        set
        {
            defaultFPS = value;
            secondsPerFrame = 1f / value;
        }
    }

    WeaverAnimationData.Clip defaultClip;
    WeaverAnimationData.Clip changeDirectionClip;


    Sprite GetCurrentFrame()
    {
        if (rising)
        {
            return risingFrames[currentFrame];
        }
        else
        {
            return loweringFrames[currentFrame];
        }
    }

    void GotoNextFrame()
    {
        currentFrame++;
        if (rising)
        {
            if (currentFrame >= risingFrames.Count)
            {
                rising = !rising;
                currentFrame = 0;
            }
        }
        else
        {
            if (currentFrame >= loweringFrames.Count)
            {
                rising = !rising;
                currentFrame = 0;
            }
        }
    }

    private void Awake()
    {
        secondsPerFrame = 1f / defaultFPS;
        defaultClip = AnimationPlayer.AnimationData.GetClip("Default");
        changeDirectionClip = AnimationPlayer.AnimationData.GetClip("Change Direction");
        AnimationPlayer.SpriteRenderer.sprite = GetCurrentFrame();
    }

    private void Update()
    {
        if (!ChangingDirection)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= secondsPerFrame)
            {
                GotoNextFrame();
                AnimationPlayer.SpriteRenderer.sprite = GetCurrentFrame();
                frameTimer -= secondsPerFrame;
            }
        }
    }

    public void SetFacingDirection(bool right)
    {
        AnimationPlayer.SpriteRenderer.flipX = right;
    }

    //Gets the delay before the body of the boss can change direction smoothly
    public float GetChangeDirectionDelay()
    {
        float fps = changeDirectionClip.FPS;
        float delay = 0;
        if (rising)
        {
            float timer = 0;
            for (int i = currentFrame; i > 0; i--)
            {
                delay += timer;
                currentFrame--;
                timer = (1f / fps);
            }
        }
        else
        {
            float timer = 0;
            for (int i = currentFrame; i < loweringFrames.Count; i++)
            {
                delay += timer;
                currentFrame++;
                timer = (1f / fps);
                if (currentFrame >= loweringFrames.Count)
                {
                    currentFrame = 0;
                    break;
                }
            }
        }
        return delay;
    }

    public float GetChangeDirectionTime()
    {
        return (1f / changeDirectionClip.FPS) * changeDirectionClip.Frames.Count;
    }

    public IEnumerator ChangeDirection()
    {
        ChangingDirection = true;
        float fps = changeDirectionClip.FPS;
        if (rising)
        {
            float timer = 0;
            for (int i = currentFrame; i > 0; i--)
            {
                if (timer > 0)
                {
                    yield return new WaitForSeconds(timer);
                }
                currentFrame--;
                timer = (1f / fps);
            }
        }
        else
        {
            float timer = 0;
            for (int i = currentFrame; i < loweringFrames.Count; i++)
            {
                if (timer > 0)
                {
                    yield return new WaitForSeconds(timer);
                }
                currentFrame++;
                timer = (1f / fps);
                if (currentFrame >= loweringFrames.Count)
                {
                    currentFrame = 0;
                    break;
                }
            }
        }
        StartCoroutine(FlipXPosition(changeDirectionClip));
        yield return AnimationPlayer.PlayAnimationTillDone(changeDirectionClip.Name);
        ChangingDirection = false;
        rising = true;
        currentFrame = 0;
        AnimationPlayer.SpriteRenderer.sprite = GetCurrentFrame();
        AnimationPlayer.SpriteRenderer.flipX = !AnimationPlayer.SpriteRenderer.flipX;
    }

}*/
