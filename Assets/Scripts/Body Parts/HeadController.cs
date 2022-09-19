using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class HeadController : AspidBodyPart
{
    public struct IdleSprite
    {
        public Sprite Sprite;
        public bool XFlipped;
        public float Degrees;
    }

    [SerializeField]
    [Tooltip("When the boss is in offensive mode, the head will look towards the player if this is set to true")]
    bool lookAtPlayer = true;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_Sprites")]
    List<Sprite> idle_Sprites;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_HorizFlip")]
    List<bool> idle_Flip;

    [SerializeField]
    [FormerlySerializedAs("followPlayer_Degrees")]
    List<float> idle_Degrees;

    bool currentlyLookingAtPlayer = false;
    float currentHeadAngle = -60f;

    /// <summary>
    /// The current direction the head is looking in.
    /// 
    /// If the value is -60, the head is looking left
    /// If the value is 60, the head is looking right
    /// If the value is 0, the head is looking center
    /// </summary>
    public float LookingDirection => currentHeadAngle;

    public bool HeadLocked { get; private set; } = false;

    uint unlockRoutine = 0;

    private void Update()
    {
        if (currentlyLookingAtPlayer && !HeadLocked)
        {
            var angleToPlayer = GetDownwardAngleToPlayer();
            if (angleToPlayer >= currentHeadAngle)
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, 0f, 120f * Time.deltaTime);
            }
            else
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, -120f * Time.deltaTime, 0f);
            }

            //Debug.DrawRay(transform.position,MathUtilities.PolarToCartesian(new Vector2(currentHeadAngle - 90f,5f)),Color.red);

            var index = GetIdleIndexForAngle(currentHeadAngle);

            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
        }
    }

    public override float GetChangeDirectionTime()
    {
        var time = base.GetChangeDirectionTime();

        if (lookAtPlayer)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                //yield return StartFollowingPlayer();
                time += (1f / DEFAULT_FPS) * DEFAULT_CENTERIZE_FRAMES;
            }
            else if (CurrentOrientation != AspidOrientation.Center && PreviousOrientation == AspidOrientation.Center)
            {
                //yield return StopFollowingPlayer();
                time += (1f / DEFAULT_FPS) * DEFAULT_CENTERIZE_FRAMES;
            }
        }

        return time;
    }

    protected override IEnumerator ChangeDirectionRoutine()
    {
        if (HeadLocked)
        {
            throw new Exception("The head wasn't unlocked when the last move ended");
        }
        if (lookAtPlayer)
        {
            if (CurrentOrientation == AspidOrientation.Center)
            {
                yield return StartFollowingPlayer();
                yield break;
            }
            else if (CurrentOrientation != AspidOrientation.Center && PreviousOrientation == AspidOrientation.Center)
            {
                yield return StopFollowingPlayer();
                yield break;
            }
        }

        currentHeadAngle = OrientationToAngle(CurrentOrientation);

        yield return base.ChangeDirectionRoutine();
    }

    IEnumerator StartFollowingPlayer()
    {
        MainRenderer.flipX = false;
        StartCoroutine(UpdateColliderOffset(DEFAULT_FPS,DEFAULT_CENTERIZE_FRAMES));

        yield return InterpolateToFollowPlayer(DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS);
        currentlyLookingAtPlayer = true;
    }

    IEnumerator StopFollowingPlayer()
    {
        StartCoroutine(UpdateColliderOffset(DEFAULT_FPS, DEFAULT_CENTERIZE_FRAMES));
        yield return InterpolateToNewAngle(currentHeadAngle, () => OrientationToAngle(CurrentOrientation), DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS);

        MainRenderer.sprite = idle_Sprites[0];
        MainRenderer.flipX = CurrentOrientation == AspidOrientation.Right;

        currentlyLookingAtPlayer = false;
    }

    IEnumerator InterpolateToFollowPlayer(float increments, float incrementsPerSecond)
    {
        return InterpolateToNewAngle(OrientationToAngle(CurrentOrientation), GetDownwardAngleToPlayer, increments, incrementsPerSecond);
    }

    IEnumerator InterpolateToNewAngle(float oldAngle, Func<float> destAngle, float increments, float incrementsPerSecond)
    {
        for (int i = 1; i <= increments; i++)
        {
            currentHeadAngle = Mathf.Lerp(oldAngle, destAngle(), i / (float)increments);
            int index = GetIdleIndexForAngle(currentHeadAngle);
            MainRenderer.sprite = idle_Sprites[index];
            MainRenderer.flipX = idle_Flip[index];
            if (i != increments)
            {
                yield return new WaitForSeconds(1f / incrementsPerSecond);
            }
        }
    }

    public IEnumerator LockHead(float lockSpeed = 1f)
    {
        return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left,lockSpeed);
    }

    /// <summary>
    /// Locks the head so other scripts can play animations and other effects on it. Make sure to call <see cref="UnlockHead"/> when done
    /// 
    /// NOTE: It isn't guaranteed that the head will be in the direction you intend if it's not possible to do so. Always use <see cref="LookingDirection"/> after calling this
    /// 
    /// In Offensive Mode, you can pick whatever intendedDirection you want, but in defensive mode, the orientation can't be chosen, and it can be either left or right
    /// </summary>
    public IEnumerator LockHead(AspidOrientation intendedOrientation, float lockSpeed = 1f)
    {
        return LockHead(OrientationToAngle(intendedOrientation),lockSpeed);
        /*if (headLocked)
        {
            throw new Exception("The head is already locked");
        }

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            currentlyLookingAtPlayer = false;
            yield return InterpolateToNewAngle(currentHeadAngle, () => OrientationToAngle(intendedOrientation), DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS);
        }
        headLocked = true;*/
    }

    public IEnumerator LockHead(float intendedDirection, float lockSpeed = 1f)
    {
        unlockRoutine = 0;
        if (HeadLocked)
        {
            throw new Exception("The head is already locked");
        }

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            currentlyLookingAtPlayer = false;
            yield return InterpolateToNewAngle(currentHeadAngle, () => intendedDirection, DEFAULT_CENTERIZE_FRAMES, DEFAULT_FPS * lockSpeed);
        }
        HeadLocked = true;
    }

    /// <summary>
    /// Unlocks the head and allows its default animations to resume
    /// </summary>
    public void UnlockHead()
    {
        UnlockHead(currentHeadAngle);
    }

    /// <summary>
    /// Unlocks the head and allows its default animations to resume. 
    /// 
    /// This variant allows you to set the new direction the head should be facing
    /// 
    /// For example, if you have a move finish with the head facing to the right, set the angle to 60 degrees.
    /// 
    /// If it finishes facing left, set the angle to -60
    /// 
    /// If it finishes facing towards the center, set the angle to 0
    /// </summary>
    public void UnlockHead(float newHeadDirection)
    {
        if (unlockRoutine != 0)
        {
            throw new Exception("The head is already being unlocked");
        }
        if (!HeadLocked)
        {
            throw new Exception("The head is already unlocked");
        }

        currentHeadAngle = newHeadDirection;

        if (CurrentOrientation == AspidOrientation.Center && lookAtPlayer)
        {
            currentlyLookingAtPlayer = true;
            HeadLocked = false;
        }
        else if (CurrentOrientation != AspidOrientation.Center)
        {
            unlockRoutine = Boss.StartBoundRoutine(UnlockTowardsDirection(newHeadDirection, CurrentOrientation));
        }
    }

    IEnumerator UnlockTowardsDirection(float oldHeadDirection, AspidOrientation orientation)
    {
        float trueNewDirection = orientation == AspidOrientation.Left ? -60f : 60f;
        var oldIndex = GetIdleIndexForAngle(oldHeadDirection);
        var newIndex = GetIdleIndexForAngle(trueNewDirection);

        yield return InterpolateToNewAngle(oldHeadDirection, () => trueNewDirection, Math.Abs(newIndex - oldIndex) + 1, DEFAULT_FPS);

        MainRenderer.sprite = idle_Sprites[0];
        MainRenderer.flipX = orientation == AspidOrientation.Right;

        HeadLocked = false;
        unlockRoutine = 0;
    }

    public void UnlockHead(AspidOrientation newHeadOrientation)
    {
        UnlockHead(OrientationToAngle(newHeadOrientation));
    }

    /// <summary>
    /// Returns the downward angle to the player. 90* means the player is to the right of the boss. 0 means the player is above or below the boss. -90* means the player is to the left of the boss
    /// </summary>
    float GetDownwardAngleToPlayer()
    {
        return GetDownwardAngleToPosition(Player.Player1.transform.position);
        //return 90f * Vector2.Dot(Vector2.right,Player.Player1.transform.position - transform.position);
    }

    float GetDownwardAngleToPosition(Vector3 pos)
    {
        return 90f * Vector2.Dot(Vector2.right, (pos - transform.position).normalized);
    }

    float OrientationToAngle(AspidOrientation orientation)
    {
        switch (orientation)
        {
            case AspidOrientation.Left:
                return idle_Degrees[0];
            case AspidOrientation.Right:
                return idle_Degrees[idle_Degrees.Count - 1];
            default:
                return 0;
        }
    }

    public int GetIdleIndexForAngle(float angle)
    {
        for (int i = idle_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == idle_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(idle_Degrees[i - 1], idle_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(idle_Degrees[i], idle_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(idle_Degrees[i], idle_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(idle_Degrees[i - 1], idle_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public Vector3 GetFireSource(float angleORPosition)
    {
        if (angleORPosition >= 0f)
        {
            return Boss.SpitTargetRight;
        }
        else
        {
            return Boss.SpitTargetLeft;
        }
    }

    public IdleSprite GetIdleSprite(int index)
    {
        return new IdleSprite
        {
            Sprite = idle_Sprites[index],
            Degrees = idle_Degrees[index],
            XFlipped = idle_Flip[index]
        };
    }
}

/*public class HeadControllerOLD : AspidBodyPartOLD
{
    public struct IdleSprite
    {
        public Sprite Sprite;
        public bool XFlipped;
        public float Degrees;
    }

    [SerializeField]
    [Tooltip("When the boss is in offensive mode, the head will look towards the player if this is set to true")]
    bool lookAtPlayer = true;

    public bool FollowPlayer { get; set; } = false;

    [SerializeField]
    [Tooltip("How fast the head of the boss moves when FollowPlayer is set to true")]
    float headMovementSpeed = 5f;

    bool changingSettings = false;

    WeaverAnimationData.Clip changeDirectionClip;
    WeaverAnimationData.Clip centerizeClip;
    WeaverAnimationData.Clip decenterizeClip;

    [SerializeField]
    List<Sprite> followPlayer_Sprites;

    [SerializeField]
    List<bool> followPlayer_HorizFlip;

    [SerializeField]
    List<float> followPlayer_Degrees;

    public bool HeadFacingRight
    {
        get
        {
            if (FollowPlayer)
            {
                return currentHeadAngle >= 0f;
            }
            else
            {
                return Orientation == AspidOrientation.Right;
            }
        }
    }

    public IEnumerable<IdleSprite> IdleSprites
    {
        get
        {
            for (int i = 0; i < followPlayer_Sprites.Count; i++)
            {
                yield return new IdleSprite
                {
                    Sprite = followPlayer_Sprites[i],
                    Degrees = followPlayer_Degrees[i],
                    XFlipped = followPlayer_HorizFlip[i]
                };
            }
        }
    }

    public int IdleSpriteCount => followPlayer_Sprites.Count;

    public SpriteRenderer MainRenderer => AnimationPlayer.SpriteRenderer;

    float currentHeadAngle;


    protected override void Awake()
    {
        base.Awake();
        changeDirectionClip = AnimationPlayer.AnimationData.GetClip("Change Direction");
        centerizeClip = AnimationPlayer.AnimationData.GetClip("Centerize");
        decenterizeClip = AnimationPlayer.AnimationData.GetClip("Decenterize");
    }

    private void Update()
    {
        if (FollowPlayer)
        {
            var angleToPlayer = GetDownwardAngleToPlayer();
            if (angleToPlayer >= currentHeadAngle)
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, 0f, headMovementSpeed * Time.deltaTime);
            }
            else
            {
                currentHeadAngle += Mathf.Clamp(angleToPlayer - currentHeadAngle, -headMovementSpeed * Time.deltaTime, 0f);
            }

            var index = GetFollowPlayerIndexForAngle(currentHeadAngle);

            AnimationPlayer.SpriteRenderer.sprite = followPlayer_Sprites[index];
            AnimationPlayer.SpriteRenderer.flipX = followPlayer_HorizFlip[index];
        }
    }

    /// <summary>
    /// Disables the head from automatically looking towards the player, and forces it to look at a specific direction
    /// 
    /// The facing angle determines where the head should look. 
    /// 
    /// Only works if the Boss's Orientation is set to Center
    /// 
    /// Example angles:
    /// 
    /// Any angle less than or equal to -60 degrees will make the head face left
    /// Any angle greater than or equal to 60 degrees will make the head face right
    /// An angle equal to 0 degrees will make the head face center
    /// </summary>
    public IEnumerator DisableFollowPlayer(float facingAngle, float speed = 1f)
    {
        if (Orientation == AspidOrientation.Center)
        {
            FollowPlayer = false;
            yield return InterpolateToNewAngle(currentHeadAngle, () => facingAngle, Mathf.RoundToInt(Mathf.Abs(facingAngle - currentHeadAngle) / 30f), 1f / (centerizeClip.FPS * speed));
            currentHeadAngle = facingAngle;
        }
    }

    float GetHeadAngle()
    {
        if (Orientation == AspidOrientation.Center)
        {
            if (Player.Player1.transform.position.x < transform.position.x)
            {
                return -60f;
            }
            else
            {
                return 60f;
            }
        }
        else
        {
            if (Orientation == AspidOrientation.Left)
            {
                return -60f;
            }
            else
            {
                return 60f;
            }
        }
    }

    public IEnumerator DisableFollowPlayer(float speed = 1f)
    {
        return DisableFollowPlayer(GetHeadAngle(), speed);
    }

    public IEnumerator DisableFollowPlayer(bool headFacesRight, float speed = 1f)
    {
        return DisableFollowPlayer(headFacesRight ? 60f : -60f,speed);
    }

    public void EnableFollowPlayer()
    {
        if (Orientation == AspidOrientation.Center)
        {
            FollowPlayer = true;
        }
    }



    protected override IEnumerator ChangeDirectionRoutine(AspidOrientation newOrientation)
    {
        if (lookAtPlayer)
        {
            if (newOrientation == AspidOrientation.Center)
            {
                yield return FollowPlayerEnable();
                yield break;
            }
            else if (newOrientation != AspidOrientation.Center && Orientation == AspidOrientation.Center)
            {
                yield return FollowPlayerDisable(newOrientation);
                yield break;
            }
        }
        if (changingSettings)
        {
            throw new System.Exception("Attempting to change head settings while it's already being changed");
        }
        changingSettings = true;
        IEnumerator ApplyClip(WeaverAnimationData.Clip clip)
        {
            StartCoroutine(ChangeXPosition(GetDestinationLocalX(Orientation), GetDestinationLocalX(newOrientation), clip));
            StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, Orientation), GetDestinationLocalX(StartingColliderOffset, newOrientation), clip));
            yield return AnimationPlayer.PlayAnimationTillDone(clip.Name);
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
                yield return ApplyClip(decenterizeClip);
            }
            else
            {
                Sprite initialFrame = AnimationPlayer.SpriteRenderer.sprite;
                yield return ApplyClip(changeDirectionClip);
                AnimationPlayer.SpriteRenderer.flipX = newOrientation == AspidOrientation.Right;

                AnimationPlayer.SpriteRenderer.sprite = initialFrame;
            }
        }
        else
        {
            yield return ApplyClip(centerizeClip);
        }
        changingSettings = false;
    }

    public IEnumerator FollowPlayerEnable()
    {

        if (changingSettings)
        {
            throw new System.Exception("Attempting to change head settings while it's already being changed");
        }
        changingSettings = true;

        //TODO TODo TODO 

        //ALSO MAKE SURE SPRITE LISTS GET FILLED OUT

        AnimationPlayer.SpriteRenderer.flipX = false;
        StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, Orientation), GetDestinationLocalX(StartingColliderOffset, AspidOrientation.Center), centerizeClip));
        yield return InterpolateToFollowPlayer(OrientationToAngle(Orientation), centerizeClip.Frames.Count, 1f / centerizeClip.FPS);
        currentHeadAngle = GetDownwardAngleToPlayer();

        FollowPlayer = true;
        changingSettings = false;
    }

    public IEnumerator FollowPlayerDisable(AspidOrientation newOrientation)
    {
        if (changingSettings)
        {
            throw new System.Exception("Attempting to change head settings while it's already being changed");
        }
        changingSettings = true;

        //TODO TODO TODO
        StartCoroutine(ChangeColliderOffset(GetDestinationLocalX(StartingColliderOffset, AspidOrientation.Center), GetDestinationLocalX(StartingColliderOffset, newOrientation), centerizeClip));
        yield return InterpolateToNewAngle(currentHeadAngle, () => OrientationToAngle(newOrientation), centerizeClip.Frames.Count, 1f / centerizeClip.FPS);

        AnimationPlayer.SpriteRenderer.sprite = followPlayer_Sprites[0];
        AnimationPlayer.SpriteRenderer.flipX = newOrientation == AspidOrientation.Right;

        FollowPlayer = false;
        changingSettings = false;
    }

    int GetCurrentFollowPlayerIndex()
    {
        var currentSprite = AnimationPlayer.SpriteRenderer.sprite;
        var xflipped = AnimationPlayer.SpriteRenderer.flipX;
        for (int i = followPlayer_Sprites.Count - 1; i >= 0; i--)
        {
            if (followPlayer_Sprites[i].name == currentSprite.name && followPlayer_HorizFlip[i] == xflipped)
            {
                return i;
            }
        }
        return -1;
    }

    IEnumerator InterpolateToFollowPlayer(float oldAngle, float incrementCount, float secondsPerIncrement)
    {
        return InterpolateToNewAngle(oldAngle, GetDownwardAngleToPlayer, incrementCount, secondsPerIncrement);
    }

    IEnumerator InterpolateToNewAngle(float oldAngle, Func<float> destAngle, float incrementCount, float secondsPerIncrement)
    {
        for (int i = 1; i <= incrementCount; i++)
        {
            float newAngle = Mathf.Lerp(oldAngle, destAngle(), i / (float)incrementCount);
            int index = GetFollowPlayerIndexForAngle(newAngle);
            AnimationPlayer.SpriteRenderer.sprite = followPlayer_Sprites[index];
            AnimationPlayer.SpriteRenderer.flipX = followPlayer_HorizFlip[index];
            if (i != incrementCount)
            {
                yield return new WaitForSeconds(secondsPerIncrement);
            }
        }
    }

    /// <summary>
    /// Returns the downward angle to the player. 90* means the player is to the right of the boss. 0 means the player is above or below the boss. -90* means the player is to the left of the boss
    /// </summary>
    float GetDownwardAngleToPlayer()
    {
        return GetDownwardAngleToPosition(Player.Player1.transform.position);
        //return 90f * Vector2.Dot(Vector2.right,Player.Player1.transform.position - transform.position);
    }

    float GetDownwardAngleToPosition(Vector3 pos)
    {
        return 90f * Vector2.Dot(Vector2.right, (pos - transform.position).normalized);
    }

    float OrientationToAngle(AspidOrientation orientation)
    {
        switch (orientation)
        {
            case AspidOrientation.Left:
                return followPlayer_Degrees[0];
            case AspidOrientation.Right:
                return followPlayer_Degrees[followPlayer_Degrees.Count - 1];
            default:
                return 0;
        }
    }

    int GetFollowPlayerIndexForAngle(float angle)
    {
        for (int i = followPlayer_Degrees.Count - 1; i >= 0; i--)
        {
            if (i == followPlayer_Degrees.Count - 1)
            {
                if (angle >= Mathf.Lerp(followPlayer_Degrees[i - 1], followPlayer_Degrees[i],0.5f))
                {
                    return i;
                }
            }
            else if (i == 0)
            {
                if (angle < Mathf.Lerp(followPlayer_Degrees[i], followPlayer_Degrees[i + 1], 0.5f))
                {
                    return i;
                }
            }
            else
            {
                if (angle < Mathf.Lerp(followPlayer_Degrees[i], followPlayer_Degrees[i + 1], 0.5f) && angle >= Mathf.Lerp(followPlayer_Degrees[i - 1], followPlayer_Degrees[i], 0.5f))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public IdleSprite GetIdleSprite(int index)
    {
        return new IdleSprite
        {
            Sprite = followPlayer_Sprites[index],
            Degrees = followPlayer_Degrees[index],
            XFlipped = followPlayer_HorizFlip[index]
        };
    }
}*/
