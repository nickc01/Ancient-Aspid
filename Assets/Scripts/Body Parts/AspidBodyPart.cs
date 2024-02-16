using System;
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

    private float startingColliderOffsetX;

    [NonSerialized]
    private AncientAspid _boss;
    public AncientAspid Boss => _boss ??= GetComponentInParent<AncientAspid>();

    [NonSerialized]
    private Collider2D _mainCollider;
    public Collider2D MainCollider => _mainCollider ??= GetComponent<Collider2D>();

    [NonSerialized]
    private WeaverAnimationPlayer _animator;
    public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

    [NonSerialized]
    private SpriteRenderer _mainRenderer;
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
            return Enumerable.Empty<byte>().GetEnumerator();
        }
        PreviousOrientation = CurrentOrientation;
        CurrentOrientation = newDirection;

        return ChangeDirectionRoutine(speedMultplier);
    }

    protected IEnumerator PlayChangeDirectionClip(string clipName, float fps, int frameCount)
    {
        if (Animator.HasAnimationClip(clipName))
        {
            float clipFPS = Animator.AnimationData.GetClipFPS(clipName);
            int clipFrameCount = Animator.AnimationData.GetClipFrameCount(clipName);

             Boss.StartBoundRoutine(UpdateLocalPosition(clipFPS, clipFrameCount));
             Boss.StartBoundRoutine(UpdateColliderOffset(clipFPS, clipFrameCount));
            Animator.PlaybackSpeed = fps / clipFPS;
            yield return Animator.PlayAnimationTillDone(clipName);
            Animator.PlaybackSpeed = 1f;
        }
        else
        {
             Boss.StartBoundRoutine(UpdateLocalPosition(fps, frameCount));
            yield return UpdateColliderOffset(fps, frameCount);
        }
    }

    public virtual float GetChangeDirectionTime(float speedMultiplier = 1f)
    {
        if (PreviousOrientation == AspidOrientation.Center)
        {
            if (CurrentOrientation != AspidOrientation.Center)
            {
                return 1f / (DEFAULT_FPS * speedMultiplier) * DEFAULT_CENTERIZE_FRAMES;
            }
        }
        else
        {
            return CurrentOrientation == AspidOrientation.Center
                ? 1f / (DEFAULT_FPS * speedMultiplier) * DEFAULT_CENTERIZE_FRAMES
                : 1f / (DEFAULT_FPS * speedMultiplier) * DEFAULT_CHANGE_DIR_FRAMES;
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


    }

    public static float GetXForOrientation(float startingX, AspidOrientation orientation)
    {
        if (orientation == AspidOrientation.Center)
        {
            return 0;
        }
        else if (orientation == AspidOrientation.Right)
        {
            return -startingX;
        }
        else
        {
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
            MainCollider.offset = new Vector2(Mathf.Lerp(oldX, newX, i / (float)frameCount), MainCollider.offset.y);
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
    }

    protected IEnumerator UpdateLocalPosition(float fps, float frameCount, float oldX, float newX)
    {
        float secondsPerFrame = 1f / fps;
        float timer = 0;


        for (int i = 0; i < frameCount; i++)
        {
             transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)frameCount));
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

}


