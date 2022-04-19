/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

public abstract class BodyPartAnimator : MonoBehaviour
{
    WeaverAnimationPlayer _animationPlayer;
    public WeaverAnimationPlayer AnimationPlayer
    {
        get
        {
            if (_animationPlayer == null)
            {
                _animationPlayer = GetComponent<WeaverAnimationPlayer>();
            }
            return _animationPlayer;
        }
    }

    protected IEnumerator FlipXPosition(WeaverAnimationData.Clip clip)
    {
        return FlipXPosition(clip.FPS, clip.Frames.Count);
    }

    protected IEnumerator FlipXPosition(float fps, int frameCount)
    {
        float oldX = transform.localPosition.x;
        float newX = -oldX;

        float secondsPerFrame = 1f / fps;

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForSeconds(secondsPerFrame);
            transform.SetXLocalPosition(Mathf.Lerp(oldX, newX, i / (float)(frameCount - 1)));
        }
    }

    public abstract IEnumerator ChangeDirection();
    public abstract float GetChangeDirectionDelay();

    public abstract IEnumerator Centerize();
}*/
