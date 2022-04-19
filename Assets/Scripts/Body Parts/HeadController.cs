using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        /*if (FollowPlayer)
        {
            yield break;
        }*/

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
        /*if (!FollowPlayer)
        {
            yield break;
        }*/
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
    /*#if UNITY_EDITOR
        Player currentPlayer;

        private void OnDrawGizmos()
        {
            if (currentPlayer == null)
            {
                currentPlayer = GameObject.FindObjectOfType<Player>();
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentPlayer.transform.position);
            Debug.Log("Angle to player = " + GetDownwardAngleToPosition(currentPlayer.transform.position));
        }
    #endif*/
}
