using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;


public class ClawAnimator : MonoBehaviour
{
    [field: SerializeField]
    public string ClipToPlay { get; private set; }

    [field: SerializeField]
    public int FrameToStartOn { get; private set; }

    SpriteRenderer _mainRenderer;
    public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

    SpriteFlasher _flasher;
    public SpriteFlasher Flasher => _flasher ??= GetComponent<SpriteFlasher>();

    WeaverAnimationPlayer _animator;
    public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

    public WeaverAnimationData.Clip DefaultClip { get; private set; }

    bool playDefaultAnimation = true;
    public int CurrentFrame { get; set; }
    float secondsPerFrame;
    int frameCount;

    public bool ClawLocked { get; private set; }

    float timer = 0f;

    [SerializeField]
    float attackRotation;

    [SerializeField]
    Vector2Int maxRotationFrameRange;

    [SerializeField]
    string attackAnticAnimation;

    [SerializeField]
    string attackAnimation;

    [SerializeField]
    string attackFullAnimation;

    [SerializeField]
    float anticTime = 0.5f;

    [SerializeField]
    AudioClip anticSoundEffect;

    [SerializeField]
    AudioClip anticSwingSound;

    [SerializeField]
    GameObject slashObject;

    private void Awake()
    {
        DefaultClip = Animator.AnimationData.GetClip(ClipToPlay);
        CurrentFrame = FrameToStartOn;
        secondsPerFrame = 1f / DefaultClip.FPS;
        frameCount = DefaultClip.Frames.Count;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= secondsPerFrame)
        {
            timer -= secondsPerFrame;
            CurrentFrame++;
            if (CurrentFrame >= frameCount)
            {
                CurrentFrame = 0;
            }
            if (playDefaultAnimation)
            {
                MainRenderer.sprite = DefaultClip.Frames[CurrentFrame];
            }
        }
    }
    

    public IEnumerator LockClaw()
    {
        return LockClaw(-1);
    }

    public IEnumerator LockClaw(int targetFrame)
    {
        if (ClawLocked)
        {
            throw new System.Exception("The claw is already locked");
        }
        ClawLocked = true;

        if (targetFrame >= 0)
        {
            yield return new WaitUntil(() => CurrentFrame == targetFrame);
        }

        playDefaultAnimation = false;
    }

    public void UnlockClaw()
    {
        UnlockClaw(-1);
    }

    public void UnlockClaw(int beginningFrame)
    {
        if (!ClawLocked)
        {
            throw new System.Exception("The claw is already unlocked");
        }
        if (beginningFrame >= 0)
        {
            CurrentFrame = beginningFrame % frameCount;
            timer = 0;
        }
        ClawLocked = false;
        playDefaultAnimation = true;
    }



    public IEnumerator PlayAttackAnimationNEW(bool playSounds)
    {
        var defaultRotation = Quaternion.Euler(0f, 0f, transform.GetZLocalRotation());
        var maxRotation = Quaternion.Euler(0f, 0f, attackRotation);
        var attackClip = Animator.AnimationData.GetClip(attackFullAnimation);
        //var attackAnticClip = Animator.AnimationData.GetClip(attackAnticAnimation);

        if (playSounds && anticSoundEffect != null)
        {
            WeaverAudio.PlayAtPoint(anticSoundEffect, transform.position);
        }
        Flasher.FlashColor = Color.white;
        Flasher.DoFlash(0.1f, anticTime / 2f, 1f, 0f);

        for (int i = 0; i < attackClip.Frames.Count; i++)
        {
            MainRenderer.sprite = attackClip.Frames[i];
            if (i < maxRotationFrameRange.x)
            {
                transform.localRotation = Quaternion.Slerp(defaultRotation, maxRotation, i / (float)maxRotationFrameRange.x);
            }
            else if (i >= maxRotationFrameRange.x && i < maxRotationFrameRange.y)
            {
                transform.localRotation = maxRotation;
            }
            else
            {
                transform.localRotation = Quaternion.Slerp(defaultRotation, maxRotation, 1f - ((i - maxRotationFrameRange.y) / (float)(attackClip.Frames.Count - maxRotationFrameRange.y)));
            }
            yield return new WaitForSeconds(1f / attackClip.FPS);

            if (i == maxRotationFrameRange.x)
            {
                yield return new WaitForSeconds(anticTime);
                if (playSounds && anticSwingSound != null)
                {
                    WeaverAudio.PlayAtPoint(anticSwingSound, transform.position);
                }
                slashObject.SetActive(true);
            }

        }
        transform.localRotation = defaultRotation;
    }

    public IEnumerator PlayAttackAnimation(bool playSounds)
    {
        var defaultRotation = Quaternion.Euler(0f, 0f, transform.GetZLocalRotation());
        var maxRotation = Quaternion.Euler(0f,0f, attackRotation);
        var attackClip = Animator.AnimationData.GetClip(attackFullAnimation);

        if (playSounds && anticSoundEffect != null)
        {
            WeaverAudio.PlayAtPoint(anticSoundEffect, transform.position);
        }

        for (int i = 0; i < attackClip.Frames.Count; i++)
        {
            MainRenderer.sprite = attackClip.Frames[i];
            if (i < maxRotationFrameRange.x)
            {
                transform.localRotation = Quaternion.Slerp(defaultRotation,maxRotation,i / maxRotationFrameRange.x);
            }
            else if (i >= maxRotationFrameRange.x && i < maxRotationFrameRange.y)
            {
                transform.localRotation = maxRotation;
            }
            else
            {
                transform.localRotation = Quaternion.Slerp(defaultRotation,maxRotation,1f - ((i - maxRotationFrameRange.y) / (float)(attackClip.Frames.Count - maxRotationFrameRange.y)));
            }

            if (i == 3)
            {
                if (playSounds && anticSwingSound != null)
                {
                    WeaverAudio.PlayAtPoint(anticSwingSound, transform.position);
                }
                slashObject.SetActive(true);
                
            }

            /*if (i == 3)
            {
                firstCollider.enabled = true;
            }
            else if (i == 4)
            {
                firstCollider.enabled = false;
                secondCollider.enabled = true;
            }
            else
            {
                firstCollider.enabled = false;
                secondCollider.enabled = false;
            }*/

            yield return new WaitForSeconds(1f / attackClip.FPS);
        }
        transform.localRotation = defaultRotation;
    }
}
