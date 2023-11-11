using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public abstract class GroundModeVariationBase
{
    public readonly GroundMode Mode;

    public AncientAspid Boss => Mode.Boss;
    public Transform transform => Mode.transform;
    public GameObject gameObject => Mode.gameObject;

    public GroundModeVariationBase(GroundMode mode)
    {
        Mode = mode;
    }

    public virtual bool VariationEnabled => true;

    public virtual IEnumerator OnBegin() { yield break; }

    public virtual IEnumerator LungeAntic() { yield break; }

    //public virtual IEnumerator BeforeLunging() { yield break; }

    public virtual Vector3 GetLungeTarget()
    {
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            return Player.Player1.transform.position;
        }
        else
        {
            return transform.position.With(y: Boss.CurrentRoomRect.yMin);
        }
    }

    public virtual bool DoSlide(Vector3 lungeTarget)
    {
        var downwardAngle = Vector3.Dot(Vector3.right, (lungeTarget - transform.position).normalized) * 90f;
        return Mathf.Abs(downwardAngle) >= Mode.DegreesSlantThreshold;
    }

    public virtual void LungeStart() { }
    public virtual void LungeLand(bool sliding) { }
    public virtual void LungeCancel() { }

    public virtual void SlidingStart() { }

    /// <summary>
    /// When the boss is sliding, this is called whenever the boss enters the ground or exits the ground
    /// </summary>
    /// <param name="onGround"></param>
    public virtual void SlidingOnGround(bool onGround) { }

    /// <summary>
    /// Called when the boss switches direction during a slide
    /// </summary>
    public virtual void OnSlideSwitchDirection(AspidOrientation oldOrientation) { }

    /// <summary>
    /// Called when the boss stops sliding by slamming into a wall
    /// </summary>
    public virtual void OnSlideSlam() { }

    /// <summary>
    /// Called when the boss stops sliding by gracefully stopping
    /// </summary>
    public virtual void OnSlideStop() { }

    /// <summary>
    /// Called when the slide gets cancelled
    /// </summary>
    public virtual void SlideCancel() { }

    public virtual bool FireGroundLaser(out Quaternion laserDirection)
    {
        var playerAngle = Boss.GetAngleToPlayer();

        const float angleLimit = 45f;


        if (Boss.Orientation == AspidOrientation.Left)
        {
            if (playerAngle < 0f)
            {
                playerAngle += 360f;
            }
            playerAngle = Mathf.Clamp(playerAngle, 180 - angleLimit, 180 + angleLimit);
            if (playerAngle > 180)
            {
                playerAngle = 180f;
            }
        }
        else
        {
            if (playerAngle >= 180f)
            {
                playerAngle -= 360f;
            }
            playerAngle = Mathf.Clamp(playerAngle, -angleLimit, angleLimit);
            if (playerAngle < 0f)
            {
                playerAngle = 0f;
            }
        }

        laserDirection = Quaternion.Euler(0f, 0f, playerAngle);

        return Boss.CanSeeTarget && ((Boss.Orientation == AspidOrientation.Left && Player.Player1.transform.position.x <= transform.position.x) || (Boss.Orientation == AspidOrientation.Right && Player.Player1.transform.position.x >= transform.position.x));
    }

    public virtual IEnumerator OnEnd() { yield break; }

    /// <summary>
    /// Called when the boss starts falling off of the platform. This forces the boss to start flying again
    /// </summary>
    public virtual void OnFallCancel() { }

    /// <summary>
    /// Called when a ground mode move gets cancelled, thus cancelling the whole mode
    /// </summary>
    public virtual void OnMoveCancel() { }

    /// <summary>
    /// Called when the boss dies during the ground mode
    /// </summary>
    public virtual void OnDeath() { }
}

