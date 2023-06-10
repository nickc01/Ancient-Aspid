using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Features;
using WeaverCore.Utilities;
using static WeaverCore.Utilities.WeaverAnimationData;


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

    ClawController _controller;
    public ClawController Controller => _controller ??= GetComponentInParent<ClawController>();

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

    [SerializeField]
    [FormerlySerializedAs("groundPosition")]
    Vector2 groundPositionLeft;

    [SerializeField]
    Vector2 groundPositionRight;

    [SerializeField]
    Vector3 groundRotation;

    public bool PlayingSwingAttack { get; private set; }

    /*[SerializeField]
    float rightGroundOffset;*/

    [SerializeField]
    string clawType;

    bool _onGround = false;

    public bool OnGround
    {
        get => _onGround;
        set
        {
            if (_onGround != value)
            {
                _onGround = value;

                //float directionSign = Controller.Boss.Orientation == AspidOrientation.Left ? -1f : 1f;

                if (_onGround)
                {
                    if (Controller.Boss.Orientation == AspidOrientation.Left)
                    {
                        transform.localPosition = transform.localPosition.With(x: groundPositionLeft.x, groundPositionLeft.y);
                    }
                    else
                    {
                        transform.localPosition = transform.localPosition.With(x: groundPositionRight.x, groundPositionRight.y);
                    }
                    transform.localEulerAngles = groundRotation;
                }
                else
                {
                    transform.localPosition = transform.localPosition.With(x: basePosition.x, basePosition.y);
                    transform.localEulerAngles = baseRotation;
                }

                /*if (directionSign == 1f)
                {
                    transform.SetXLocalPosition(transform.GetXLocalPosition() + rightGroundOffset);
                }*/


                /*var oldPosition = transform.localPosition;
                var oldRotation = transform.localEulerAngles;

                transform.localPosition = transform.localPosition.With(x: previousPosition.x, previousPosition.y);
                transform.localEulerAngles = previousRotation;

                previousPosition = oldPosition;
                previousRotation = oldRotation;
                */
                if (attackAnimation == "Attack Right")
                {
                    MainRenderer.flipX = !_onGround;
                }
            }
        }
    }

    Vector2 basePosition;
    Vector3 baseRotation;




    private void Awake()
    {
        basePosition = transform.localPosition;
        baseRotation = transform.localEulerAngles;

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

    public IEnumerator PlayAttackAnimation(bool playAntic, bool playSwingSound, bool doAttack, Action onSwing = null)
    {
        PlayingSwingAttack = true;
        doAttack = doAttack && slashObject != null;
        var defaultRotation = Quaternion.Euler(0f, 0f, transform.GetZLocalRotation());
        var maxRotation = Quaternion.Euler(0f, 0f, attackRotation);
        var attackClip = Animator.AnimationData.GetClip(attackFullAnimation);

        if (playAntic && anticSoundEffect != null)
        {
            WeaverAudio.PlayAtPoint(anticSoundEffect, transform.position);
        }
        Flasher.FlashColor = Color.white;
        //Flasher.DoFlash(maxRotationFrameRange.x * (1f / attackClip.FPS), (attackClip.Frames.Count - maxRotationFrameRange.x) * (1f / attackClip.FPS), 1f, anticTime);
        Flasher.DoFlash(0.1f, anticTime / 2f, 1f, 0.1f);

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
                if (playSwingSound && anticSwingSound != null)
                {
                    WeaverAudio.PlayAtPoint(anticSwingSound, transform.position);
                }
                if (doAttack)
                {
                    slashObject.SetActive(true);
                }
                onSwing?.Invoke();
            }

        }
        transform.localRotation = defaultRotation;
        PlayingSwingAttack = false;
    }

    public void UpdateGroundSprite()
    {
        var clip = Animator.AnimationData.GetClip($"Lunge Land {clawType}");
        if (Controller.Boss.Orientation == AspidOrientation.Left)
        {
            MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", clip.Frames.Count - 1);
        }
        else
        {
            MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", 0);
        }
    }


    /*public void ExitGroundMode()
    {
        OnGround = false;
    }*/

    public void LandImmediately()
    {
        var orientation = Controller.Boss.Orientation;
        var clip = Animator.AnimationData.GetClip($"Lunge Land {clawType}");
        if (orientation == AspidOrientation.Left)
        {
            MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", clip.Frames.Count - 1);
        }
        else
        {
            MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", 0);
        }
    }

    public IEnumerator PlayLanding(bool slide)
    {
        OnGround = true;

        var orientation = Controller.Boss.Orientation;
        var clip = Animator.AnimationData.GetClip($"Lunge Land {clawType}");

        if (!slide)
        {
            if (orientation == AspidOrientation.Left)
            {
                MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", 0);
            }
            else
            {
                MainRenderer.sprite = Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", clip.Frames.Count - 1);
            }
            yield return new WaitForSeconds(Controller.Boss.lungeDownwardsLandDelay);
        }

        float secondsPerFrame = 1f / clip.FPS;
        float timer = 0;

        IEnumerator PlayFrame(int frame)
        {
            MainRenderer.sprite = clip.Frames[frame];
            while (true)
            {
                timer += Time.deltaTime;
                if (timer >= secondsPerFrame)
                {
                    timer -= secondsPerFrame;
                    break;
                }
                yield return null;
            }
        }

        if (orientation == AspidOrientation.Left)
        {
            for (int i = 0; i < clip.Frames.Count; i++)
            {
                yield return PlayFrame(i);
            }
        }
        else
        {
            for (int i = clip.Frames.Count - 1; i >= 0; i--)
            {
                yield return PlayFrame(i);
            }
        }


        //yield return Animator.PlayAnimationTillDone($"Lunge Land {clawType}");
    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        //float oldDirSign = oldDirection == AspidOrientation.Right ? -1f : 1f;
        //float newDirSign = newDirection == AspidOrientation.Right ? -1f : 1f;

        //float oldX = oldDirSign * groundPosition.x;
        //float newX = newDirSign * groundPosition.x;

        Vector2 oldPos = oldDirection == AspidOrientation.Left ? groundPositionLeft : groundPositionRight;
        Vector2 newPos = newDirection == AspidOrientation.Left ? groundPositionLeft : groundPositionRight;

        /*if (newDirection == AspidOrientation.Left)
        {
            //transform.localPosition = transform.localPosition.With(x: groundPositionLeft.x, groundPositionLeft.y);
        }
        else
        {
            //transform.localPosition = transform.localPosition.With(x: groundPositionRight.x, groundPositionRight.y);
        }*/

        for (float t = 0; t < 4; t++)
        {
            transform.localPosition = transform.localPosition.With(x: Mathf.Lerp(oldPos.x, newPos.x,t / 4f), y: Mathf.Lerp(oldPos.y, newPos.y, t / 4f));
            yield return new WaitForSeconds(1f / 8f);
        }

        transform.localPosition = transform.localPosition.With(x: newPos.x, newPos.y);


        /*if (_onGround)
        {
            transform.localPosition = transform.localPosition.With(x: groundPosition.x, groundPosition.y);
            transform.localEulerAngles = groundRotation;
        }
        else
        {
            transform.localPosition = transform.localPosition.With(x: basePosition.x, basePosition.y);
            transform.localEulerAngles = baseRotation;
        }*/

        yield break;
    }

    public IEnumerator FinishLanding(bool slammedIntoWall)
    {
        yield break;
    }

    public void OnStun()
    {
        OnGround = false;

        transform.localPosition = transform.localPosition.With(x: basePosition.x, basePosition.y);
        transform.localEulerAngles = baseRotation;
    }

    /*public IEnumerator PlayAttackAnimation(bool playSounds)
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

            yield return new WaitForSeconds(1f / attackClip.FPS);
        }
        transform.localRotation = defaultRotation;
    }*/
}
