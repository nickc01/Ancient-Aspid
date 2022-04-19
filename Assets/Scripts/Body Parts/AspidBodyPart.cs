using System.Collections;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

public abstract class AspidBodyPart : MonoBehaviour
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
}
