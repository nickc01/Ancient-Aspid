using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;
using static WeaverCore.Utilities.WeaverAnimationData;


public class ClawAnimator : MonoBehaviour
{
    [field: SerializeField]
    public string ClipToPlay { get; private set; }

    [field: SerializeField]
    public int FrameToStartOn { get; private set; }

    private SpriteRenderer _mainRenderer;
    public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

    private SpriteFlasher _flasher;
    public SpriteFlasher Flasher => _flasher ??= GetComponent<SpriteFlasher>();

    private WeaverAnimationPlayer _animator;
    public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();

    private ClawController _controller;
    public ClawController Controller => _controller ??= GetComponentInParent<ClawController>();

    public WeaverAnimationData.Clip DefaultClip { get; private set; }

    private bool playDefaultAnimation = true;
    public int CurrentFrame { get; set; }

    private float secondsPerFrame;
    private int frameCount;

    public bool ClawLocked { get; private set; }

    private float timer = 0f;

    [SerializeField]
    private float attackRotation;

    [SerializeField]
    private Vector2Int maxRotationFrameRange;

    [SerializeField]
    private string attackAnticAnimation;

    [SerializeField]
    private string attackAnimation;

    [SerializeField]
    private string attackFullAnimation;

    [SerializeField]
    private float anticTime = 0.5f;

    [SerializeField]
    private AudioClip anticSoundEffect;

    [SerializeField]
    private AudioClip anticSwingSound;

    [SerializeField]
    private GameObject slashObject;

    [SerializeField]
    [FormerlySerializedAs("groundPosition")]
    private Vector2 groundPositionLeft;

    [SerializeField]
    private Vector2 groundPositionRight;

    [SerializeField]
    private Vector3 groundRotation;

    public bool PlayingSwingAttack { get; private set; }

    [SerializeField]
    private string clawType;
    private bool _onGround = false;

    public bool OnGround
    {
        get => _onGround;
        set
        {
            if (_onGround != value)
            {
                _onGround = value;

                if (_onGround)
                {
                    transform.localPosition = Controller.Boss.Orientation == AspidOrientation.Left
                        ? transform.localPosition.With(x: groundPositionLeft.x, groundPositionLeft.y)
                        : transform.localPosition.With(x: groundPositionRight.x, groundPositionRight.y);
                    transform.localEulerAngles = groundRotation;
                }
                else
                {
                    transform.localPosition = transform.localPosition.With(x: basePosition.x, basePosition.y);
                    transform.localEulerAngles = baseRotation;
                }


                if (attackAnimation == "Attack Right")
                {
                    MainRenderer.flipX = !_onGround;
                }
            }
        }
    }

    private Vector2 basePosition;
    private Vector3 baseRotation;




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
        Quaternion defaultRotation = Quaternion.Euler(0f, 0f, transform.GetZLocalRotation());
        Quaternion maxRotation = Quaternion.Euler(0f, 0f, attackRotation);
        Clip attackClip = Animator.AnimationData.GetClip(attackFullAnimation);

        if (playAntic && anticSoundEffect != null)
        {
             WeaverAudio.PlayAtPoint(anticSoundEffect, transform.position);
        }
        Flasher.FlashColor = Color.white;
        Flasher.DoFlash(0.1f, anticTime / 2f, 1f, 0.1f);

        for (int i = 0; i < attackClip.Frames.Count; i++)
        {
            MainRenderer.sprite = attackClip.Frames[i];
            if (i < maxRotationFrameRange.x)
            {
                transform.localRotation = Quaternion.Slerp(defaultRotation, maxRotation, i / (float)maxRotationFrameRange.x);
            }
            else
            {
                transform.localRotation = i >= maxRotationFrameRange.x && i < maxRotationFrameRange.y
                    ? maxRotation
                    : Quaternion.Slerp(defaultRotation, maxRotation, 1f - ((i - maxRotationFrameRange.y) / (float)(attackClip.Frames.Count - maxRotationFrameRange.y)));
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
        Clip clip = Animator.AnimationData.GetClip($"Lunge Land {clawType}");
        MainRenderer.sprite = Controller.Boss.Orientation == AspidOrientation.Left
            ? Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", clip.Frames.Count - 1)
            : Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", 0);
    }


    public void LandImmediately()
    {
        AspidOrientation orientation = Controller.Boss.Orientation;
        Clip clip = Animator.AnimationData.GetClip($"Lunge Land {clawType}");
        MainRenderer.sprite = orientation == AspidOrientation.Left
            ? Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", clip.Frames.Count - 1)
            : Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", 0);
    }

    public IEnumerator PlayLanding(bool slide)
    {
        OnGround = true;

        AspidOrientation orientation = Controller.Boss.Orientation;
        Clip clip = Animator.AnimationData.GetClip($"Lunge Land {clawType}");

        if (!slide)
        {
            MainRenderer.sprite = orientation == AspidOrientation.Left
                ? Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", 0)
                : Animator.AnimationData.GetFrameFromClip($"Lunge Land {clawType}", clip.Frames.Count - 1);
            yield return new WaitForSeconds(Controller.Boss.GroundMode.lungeDownwardsLandDelay);
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


    }

    public IEnumerator SlideSwitchDirection(AspidOrientation oldDirection, AspidOrientation newDirection)
    {
        Vector2 oldPos = oldDirection == AspidOrientation.Left ? groundPositionLeft : groundPositionRight;
        Vector2 newPos = newDirection == AspidOrientation.Left ? groundPositionLeft : groundPositionRight;

        for (float t = 0; t < 4; t++)
        {
            transform.localPosition = transform.localPosition.With(x: Mathf.Lerp(oldPos.x, newPos.x, t / 4f), y: Mathf.Lerp(oldPos.y, newPos.y, t / 4f));
            yield return new WaitForSeconds(1f / 8f);
        }

        transform.localPosition = transform.localPosition.With(x: newPos.x, newPos.y);


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

}
