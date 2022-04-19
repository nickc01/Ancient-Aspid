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

    WeaverAnimationData.Clip changeDirectionClip;
    WeaverAnimationData.Clip centerizeClip;
    WeaverAnimationData.Clip decenterizeClip;

    Coroutine defaultAnimationRoutine;

    float currentFrameTimer = 0;
    int currentFrame = 0;
    WeaverAnimationData.Clip currentClip;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<WeaverAnimationPlayer>();
        changeDirectionClip = animator.AnimationData.GetClip("Change Direction");
        centerizeClip = animator.AnimationData.GetClip("Centerize");
        decenterizeClip = animator.AnimationData.GetClip("Decenterize");
        defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(Orientation));
    }

    IEnumerator DefaultAnimationRoutine(AspidOrientation orientation)
    {
        if (TailRaised)
        {
            yield break;
        }
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
        float fps = changeDirectionClip.FPS;
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

    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
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
    }

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
        defaultAnimationRoutine = StartCoroutine(DefaultAnimationRoutine(Orientation));
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
