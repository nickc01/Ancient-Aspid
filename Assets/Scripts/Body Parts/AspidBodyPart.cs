using System.Collections;
using System.Linq;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;


[RequireComponent(typeof(WeaverAnimationPlayer))]
public abstract class AspidBodyPart : MonoBehaviour
{
    protected const float DEFAULT_FPS = 8;
    protected const int DEFAULT_CHANGE_DIR_FRAMES = 4;
    protected const int DEFAULT_CENTERIZE_FRAMES = 3;

    public float StartingLocalX { get; private set; }
    float startingColliderOffsetX;

    AncientAspid _boss;
    public AncientAspid Boss => _boss ??= GetComponentInParent<AncientAspid>();

    Collider2D _mainCollider;
    public Collider2D MainCollider => _mainCollider ??= GetComponent<Collider2D>();

    WeaverAnimationPlayer _animator;
    public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

    SpriteRenderer _mainRenderer;
    public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

    public AspidOrientation CurrentOrientation { get; protected set; } = AspidOrientation.Left;
    public AspidOrientation PreviousOrientation { get; protected set; } = AspidOrientation.Left;

    protected virtual void Awake()
    {
        StartingLocalX = transform.GetXLocalPosition();
        if (MainCollider != null)
        {
            startingColliderOffsetX = MainCollider.offset.x;
        }
    }

    public IEnumerator ChangeDirection(AspidOrientation newDirection, float speedMultplier = 1f)
    {
        if (newDirection == CurrentOrientation)
        {
            //yield break;
            //Debug.Log("NOT CHANGING DIRECTION");
            return Enumerable.Empty<byte>().GetEnumerator();
        }
        PreviousOrientation = CurrentOrientation;
        CurrentOrientation = newDirection;

        return ChangeDirectionRoutine(speedMultplier);
    }

    protected IEnumerator PlayChangeDirectionClip(string clipName, float fps, int frameCount)
    {
        //Debug.Log($"PLAYING FPS {fps} on {GetType().FullName}");
        if (Animator.HasAnimationClip(clipName))
        {
            var clip = Animator.AnimationData.GetClip(clipName);
            Boss.StartBoundRoutine(UpdateLocalPosition(clip.FPS,clip.Frames.Count));
            Boss.StartBoundRoutine(UpdateColliderOffset(clip.FPS, clip.Frames.Count));
            Animator.PlaybackSpeed = fps / clip.FPS;
            yield return Animator.PlayAnimationTillDone(clipName);
            Animator.PlaybackSpeed = 1f;
        }
        else
        {
            Boss.StartBoundRoutine(UpdateLocalPosition(fps,frameCount));
            yield return UpdateColliderOffset(fps,frameCount);
        }
    }

    public virtual float GetChangeDirectionTime(float speedMultiplier = 1f)
    {
        if (PreviousOrientation == AspidOrientation.Center)
        {
            if (CurrentOrientation != AspidOrientation.Center)
            {
                //Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                //yield return PlayChangeDirectionClip("Decenterize", DEFAULT_FPS, DEFAULT_CENTERIZE_FRAMES);
                return (1f / (DEFAULT_FPS * speedMultiplier)) * DEFAULT_CENTERIZE_FRAMES;
            }
        }
        else
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                //yield return PlayChangeDirectionClip("Centerize", DEFAULT_FPS, DEFAULT_CENTERIZE_FRAMES);
                return (1f / (DEFAULT_FPS * speedMultiplier)) * DEFAULT_CENTERIZE_FRAMES;
            }
            else
            {
                //Sprite initialFrame = Animator.SpriteRenderer.sprite;

                //yield return PlayChangeDirectionClip("Change Direction", DEFAULT_FPS, DEFAULT_CHANGE_DIR_FRAMES);

                //Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                //Animator.SpriteRenderer.sprite = initialFrame;

                return (1f / (DEFAULT_FPS * speedMultiplier)) * DEFAULT_CHANGE_DIR_FRAMES;
            }
        }

        return 0;
    }

    protected virtual IEnumerator ChangeDirectionRoutine(float speedMultiplier = 1f)
    {
        if (PreviousOrientation == AspidOrientation.Center)
        {
            if (CurrentOrientation != AspidOrientation.Center)
            {
                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                yield return PlayChangeDirectionClip("Decenterize", DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES);
            }
        }
        else
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                yield return PlayChangeDirectionClip("Centerize", DEFAULT_FPS * speedMultiplier, DEFAULT_CENTERIZE_FRAMES);
            }
            else
            {
                Sprite initialFrame = Animator.SpriteRenderer.sprite;

                yield return PlayChangeDirectionClip("Change Direction", DEFAULT_FPS * speedMultiplier, DEFAULT_CHANGE_DIR_FRAMES);

                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                Animator.SpriteRenderer.sprite = initialFrame;
            }
        }


        /*if (CurrentOrientation != AspidOrientation.Center)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                Animator.SpriteRenderer.flipX == CurrentOrientation != AspidOrientation.Left;
                if (CurrentOrientation == AspidOrientation.Left)
                {
                    Animator.SpriteRenderer.flipX = false;
                }
                else
                {
                    Animator.SpriteRenderer.flipX = true;
                }
                yield return ApplyClip(decenterizeClip);
            }
            else
            {
                Sprite initialFrame = Animator.SpriteRenderer.sprite;

                yield return ApplyClip(changeDirectionClip);

                Animator.SpriteRenderer.flipX = CurrentOrientation == AspidOrientation.Right;
                Animator.SpriteRenderer.sprite = initialFrame;
            }
        }
        else
        {
            yield return ApplyClip(centerizeClip);
        }
        yield break;*/
    }

    public static float GetXForOrientation(float startingX, AspidOrientation orientation)
    {
        switch (orientation)
        {
            case AspidOrientation.Center:
                return 0;
            case AspidOrientation.Right:
                return -startingX;
            default:
                return startingX;
        }
    }

    public float GetXForOrientation(AspidOrientation orientation)
    {
        return GetXForOrientation(StartingLocalX, orientation);
    }

    protected IEnumerator UpdateColliderOffset(float fps, float frameCount)
    {
        if (MainCollider == null)
        {
            yield break;
        }
        float oldX = GetXForOrientation(startingColliderOffsetX, PreviousOrientation);
        float newX = GetXForOrientation(startingColliderOffsetX, CurrentOrientation);

        float secondsPerFrame = 1f / fps;
        float timer = 0;

        for (int i = 0; i < frameCount; i++)
        {
            MainCollider.offset = new Vector2(Mathf.Lerp(oldX, newX, i / (float)(frameCount)), MainCollider.offset.y);
            //yield return new WaitForSeconds(secondsPerFrame);
            while (true)
            {
                yield return null;
                timer += Time.deltaTime;
                if (timer >= secondsPerFrame)
                {
                    timer -= secondsPerFrame;
                    break;
                }
            }
        }

        MainCollider.offset = new Vector2(Mathf.Lerp(oldX, newX, 1f), MainCollider.offset.y);
    }

    protected IEnumerator UpdateLocalPosition(float fps, float frameCount)
    {
        float oldX = GetXForOrientation(StartingLocalX, PreviousOrientation);
        float newX = GetXForOrientation(StartingLocalX, CurrentOrientation);

        return UpdateLocalPosition(fps, frameCount, oldX, newX);
        /*float secondsPerFrame = 1f / fps;
        float timer = 0;


        for (int i = 0; i < frameCount; i++)
        {
            transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)(frameCount)));
            //yield return new WaitForSeconds(secondsPerFrame);
            while (true)
            {
                yield return null;
                timer += Time.deltaTime;
                if (timer >= secondsPerFrame)
                {
                    timer -= secondsPerFrame;
                    break;
                }
            }
        }
        transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, 1f));*/
    }

    protected IEnumerator UpdateLocalPosition(float fps, float frameCount, float oldX, float newX)
    {
        float secondsPerFrame = 1f / fps;
        float timer = 0;


        for (int i = 0; i < frameCount; i++)
        {
            transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)(frameCount)));
            //yield return new WaitForSeconds(secondsPerFrame);
            while (true)
            {
                yield return null;
                timer += Time.deltaTime;
                if (timer >= secondsPerFrame)
                {
                    timer -= secondsPerFrame;
                    break;
                }
            }
        }
        transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, 1f));
    }

    public virtual void OnStun()
    {

    }

    public virtual void OnDeath()
    {

    }

    /// <summary>
    /// This function is used to wait untill all the body parts are ready to change direction mid-air during a ground jump
    /// </summary>
    public abstract IEnumerator WaitTillChangeDirectionMidJump();

    public abstract IEnumerator MidJumpChangeDirection(AspidOrientation oldOrientation, AspidOrientation newOrientation);

    public virtual IEnumerator GroundJumpBeginFalling(bool switchedDirection)
    {
        yield break;
    }

    public virtual IEnumerator GroundMoveCancel(bool onGround)
    {
        yield break;
    }

    /*protected IEnumerator 

    protected IEnumerator ChangeColliderOffset(float fromX, float toX, WeaverAnimationData.Clip clip, Collider2D collider = null)
    {
        return ChangeColliderOffset(fromX, toX, clip.FPS, clip.Frames.Count, collider);
    }

    protected IEnumerator ChangeColliderOffset(float fromX, float toX, float fps, int frameCount, Collider2D collider = null)
    {
        if (collider == null)
        {
            collider = MainCollider;
        }
        if (collider == null)
        {
            yield break;
        }
        float oldX = fromX;
        float newX = toX;

        float secondsPerFrame = 1f / fps;

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForSeconds(secondsPerFrame);

            collider.offset = new Vector2(Mathf.Lerp(oldX, newX, i / (float)(frameCount - 1)), collider.offset.y);
        }
    }

    protected IEnumerator ChangeXPosition(float fromLocalX, float localX, WeaverAnimationData.Clip clip)
    {
        return ChangeXPosition(fromLocalX, localX, clip.FPS, clip.Frames.Count);
    }

    protected IEnumerator ChangeXPosition(float fromLocalX, float localX, float fps, int frameCount)
    {
        float oldX = fromLocalX;
        float newX = localX;

        float secondsPerFrame = 1f / fps;

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForSeconds(secondsPerFrame);
            transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)(frameCount - 1)));
        }
    }*/
}


/*public abstract class AspidBodyPartOLD : MonoBehaviour
{
    public float StartingLocalX { get; private set; }
    public float StartingColliderOffset { get; private set; }
    public AspidOrientation Orientation { get; private set; } = AspidOrientation.Left;

    Collider2D _mainCollider;

    public Collider2D MainCollider => _mainCollider ??= GetComponent<Collider2D>();

    protected virtual void Awake()
    {
        StartingLocalX = transform.GetXLocalPosition();
        if (MainCollider != null)
        {
            StartingColliderOffset = MainCollider.offset.x;
        }
    }

    WeaverAnimationPlayer _animationPlayer;
    public WeaverAnimationPlayer AnimationPlayer => _animationPlayer ??= GetComponent<WeaverAnimationPlayer>();

    protected IEnumerator ChangeColliderOffset(float fromX, float toX, WeaverAnimationData.Clip clip, Collider2D collider = null)
    {
        return ChangeColliderOffset(fromX, toX, clip.FPS, clip.Frames.Count,collider);
    }

    protected IEnumerator ChangeColliderOffset(float fromX, float toX, float fps, int frameCount, Collider2D collider = null)
    {
        if (collider == null)
        {
            collider = MainCollider;
        }
        if (collider == null)
        {
            yield break;
        }
        float oldX = fromX;
        float newX = toX;

        float secondsPerFrame = 1f / fps;

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForSeconds(secondsPerFrame);

            collider.offset = new Vector2(Mathf.Lerp(oldX, newX, i / (float)(frameCount - 1)),collider.offset.y);
            //transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)(frameCount - 1)));
        }
    }

    protected IEnumerator ChangeXPosition(float fromLocalX, float localX, WeaverAnimationData.Clip clip)
    {
        return ChangeXPosition(fromLocalX, localX, clip.FPS, clip.Frames.Count);
    }

    protected IEnumerator ChangeXPosition(float fromLocalX, float localX, float fps, int frameCount)
    {
        float oldX = fromLocalX;
        float newX = localX;

        float secondsPerFrame = 1f / fps;

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForSeconds(secondsPerFrame);
            transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)(frameCount - 1)));
        }
    }

    public IEnumerator ChangeDirection(AspidOrientation newOrientation)
    {
        if (Orientation == newOrientation)
        {
            yield break;
        }
        yield return ChangeDirectionRoutine(newOrientation);
        Orientation = newOrientation;
    }

    public float GetDestinationLocalX(AspidOrientation orientation) => GetDestinationLocalX(StartingLocalX, orientation);

    public float GetDestinationLocalX(float localX, AspidOrientation orientation)
    {
        switch (orientation)
        {
            case AspidOrientation.Center:
                return 0f;
            case AspidOrientation.Right:
                return -localX;
            default:
                return localX;
        }
    }

    protected abstract IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation);
}*/
