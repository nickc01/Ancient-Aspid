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


    public override bool MoveEnabled => moveEnabled;// Boss.AspidMode == AncientAspid.Mode.Tactical || Boss.AspidMode == AncientAspid.Mode.Offensive;

    public override IEnumerator DoMove()
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
    }

    void Fire(float angle)
    {
        if (shotSpeed <= 0)
        {
            shotSpeed = 0.01f;
        }

        var sourcePos = GetFireSource(angle);

        float gravityScale = 1;
        if (ShotPrefab.TryGetComponent(out Rigidbody2D rb))
        {
            gravityScale = rb.gravityScale;
        }

        var velocityToPlayer = MathUtilities.CalculateVelocityToReachPoint(sourcePos, Player.Player1.transform.position, Vector3.Distance(sourcePos, Player.Player1.transform.position) / shotSpeed, gravityScale);

        var polarCoordsToPlayer = MathUtilities.CartesianToPolar(velocityToPlayer);

        for (int i = -1; i <= 1; i++)
        {
            FireShot(polarCoordsToPlayer.x,shotAngleSeparation * i,sourcePos, polarCoordsToPlayer.y);
        }

        if (fireSound != null)
        {
            WeaverAudio.PlayAtPoint(fireSound, sourcePos);
        }
    }

    Vector3 GetFireSource(float angle)
    {
        if (angle >= 0f)
        {
            return Boss.SpitTargetRight;
        }
        else
        {
            return Boss.SpitTargetLeft;
        }
    }

    void FireShot(float playerAngle, float angle, Vector3 sourcePos, float velocity)
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

    public override void OnStun()
    {
        Boss.Head.EnableFollowPlayer();
    }
}
