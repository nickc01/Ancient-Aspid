using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

public class ClawAnimator : MonoBehaviour
{
    [field: SerializeField]
    public WeaverAnimationData AnimationData { get; private set; }

    [field: SerializeField]
    public string ClipToPlay { get; private set; }

    [field: SerializeField]
    public int FrameToStartOn { get; private set; }

    int frame;
    float secondsPerFrame;
    int frameCount;
    SpriteRenderer spriteRenderer;
    WeaverAnimationData.Clip playingClip;

    float timer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playingClip = AnimationData.GetClip(ClipToPlay);
        frame = FrameToStartOn;
        secondsPerFrame = 1f / playingClip.FPS;
        frameCount = playingClip.Frames.Count;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= secondsPerFrame)
        {
            timer -= secondsPerFrame;
            frame++;
            if (frame >= frameCount)
            {
                frame = 0;
            }
            spriteRenderer.sprite = playingClip.Frames[frame];
        }
    }
}
