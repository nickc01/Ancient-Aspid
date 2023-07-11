using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class AspidShotMove : AncientAspidMove
{
    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.6f;

    [SerializeField]
    float climbingPostDelay = 0.8f;

    [SerializeField]
    float minDistance = 5f;

    [SerializeField]
    float headSpeed = 1.5f;

    [SerializeField]
    [Range(1, 2)]
    int attackVariant = 1;

    [SerializeField]
    float shotSpeed = 25f;

    [SerializeField]
    float shotScale = 1.5f;

    [SerializeField]
    GameObject ShotPrefab;

    [SerializeField]
    float shotAngleSeparation = 15f;

    [SerializeField]
    AudioClip fireSound;


    public override bool MoveEnabled => Boss.CanSeeTarget && moveEnabled && Vector2.Distance(Player.Player1.transform.position,Boss.Head.transform.position) >= minDistance;// Boss.AspidMode == AncientAspid.Mode.Tactical || Boss.AspidMode == AncientAspid.Mode.Offensive;

    public override float PostDelay => Boss.InClimbingPhase ? climbingPostDelay : postDelay;

    /*public override IEnumerator DoMove()
    {
        float angle = Boss.GetHeadAngle();

        yield return Boss.Head.DisableFollowPlayer(angle, headSpeed);

        bool oldFlip = Boss.Head.AnimationPlayer.SpriteRenderer.flipX;
        Sprite originalSprite = Boss.Head.AnimationPlayer.SpriteRenderer.sprite;

        Boss.Head.AnimationPlayer.SpriteRenderer.flipX = angle >= 0f;

        yield return Boss.Head.AnimationPlayer.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");

        Fire(angle);

        yield return Boss.Head.AnimationPlayer.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.AnimationPlayer.SpriteRenderer.flipX = oldFlip;
        Boss.Head.AnimationPlayer.SpriteRenderer.sprite = originalSprite;

        Boss.Head.EnableFollowPlayer();

        yield return new WaitForSeconds(0.5f);
    }*/

    public override IEnumerator DoMove()
    {
        yield return Boss.Head.LockHead(Boss.PlayerRightOfBoss ? AspidOrientation.Right : AspidOrientation.Left, headSpeed);

        var oldState = Boss.Head.MainRenderer.TakeSnapshot();

        Boss.Head.MainRenderer.flipX = Boss.Head.LookingDirection >= 0f;

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Prepare");

        Fire(Boss.Head.LookingDirection, 3, 1);
        Fire(Boss.Head.LookingDirection, 2, 0.5f);

        yield return Boss.Head.Animator.PlayAnimationTillDone($"Fire - {attackVariant} - Attack");

        Boss.Head.MainRenderer.Restore(oldState);

        Boss.Head.UnlockHead();
    }

    void Fire(float angle, int shots, float velocityMultplier = 1f)
    {
        if (shotSpeed <= 0)
        {
            shotSpeed = 0.01f;
        }

        var sourcePos = Boss.Head.GetFireSource(angle);

        Blood.SpawnBlood(sourcePos, new Blood.BloodSpawnInfo(3, 7, 10f, 25f, angle - 90f - 40f, angle - 90f + 40f, null));
        //Blood.SpawnBlood(sourcePos, new Blood.BloodSpawnInfo(3, 4, 10f, 15f, 120f, 150f, null));

        float gravityScale = 1;
        if (ShotPrefab.TryGetComponent(out Rigidbody2D rb))
        {
            gravityScale = rb.gravityScale;
        }

        var velocityToPlayer = MathUtilities.CalculateVelocityToReachPoint(sourcePos, Player.Player1.transform.position, Vector3.Distance(sourcePos, Player.Player1.transform.position) / shotSpeed, gravityScale);

        var polarCoordsToPlayer = MathUtilities.CartesianToPolar(velocityToPlayer);

        shots -= 1;

        float lowerShots = -shots / 2;

        for (float i = lowerShots; i <= lowerShots + shots; i++)
        {
            FireShot(polarCoordsToPlayer.x,shotAngleSeparation * i,sourcePos, polarCoordsToPlayer.y * velocityMultplier);
        }

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, sourcePos);
        }
    }

    void FireShot(float playerAngle, float angle, Vector3 sourcePos, float velocity)
    {
        if (!Boss.RiseFromCenterPlatform)
        {
            var instance = Pooling.Instantiate(ShotPrefab, sourcePos, Quaternion.identity);
            if (instance.TryGetComponent(out Rigidbody2D rb))
            {
                rb.velocity = MathUtilities.PolarToCartesian(playerAngle + angle, velocity);
            }
            if (instance.TryGetComponent(out AspidShotBase aspidShot))
            {
                aspidShot.ScaleFactor = shotScale;
            }
            else
            {
                instance.transform.SetLocalScaleXY(shotScale, shotScale);
            }
        }
    }

    public override void OnStun()
    {
        Boss.Head.Animator.StopCurrentAnimation();
        if (Boss.Head.HeadLocked)
        {
            Boss.Head.UnlockHead();
        }
    }
}
