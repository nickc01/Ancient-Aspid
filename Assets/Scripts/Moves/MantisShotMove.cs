using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

public class MantisShotMove : AncientAspidMove
{
    public override bool MoveEnabled => Boss.CanSeeTarget && moveEnabled && Boss.AspidMode == AncientAspid.Mode.Tactical &&
        Boss.Claws.claws.All(c => !c.ClawLocked);

    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.4f;

    [SerializeField]
    float climbingPostDelay = 0.4f;

    [SerializeField]
    int shotAmount = 3;

    [SerializeField]
    Vector2 velocityRange = new Vector2(5f,10f);

    [SerializeField]
    Vector2 angleRange = new Vector2(-30f,30f);

    [SerializeField]
    Vector2 scaleRange = new Vector2(0.75f,1.25f);

    [SerializeField]
    float angleOffset = 0f;

    public void EnableMove(bool enabled)
    {
        moveEnabled = enabled;
    }

    public override IEnumerator DoMove()
    {
        yield return Boss.Claws.DoMantisShots(OnSwing);
    }

    void OnSwing()
    {
        var playerAngle = Boss.GetAngleToPlayer();

        for (int i = 0; i < shotAmount; i++)
        {
            var fireAngleRange = new Vector2(angleRange.x + playerAngle,angleRange.y + playerAngle);

            var currentAngle = Mathf.Lerp(fireAngleRange.x, fireAngleRange.y,i / (shotAmount - 1f));

            if (!Boss.RiseFromCenterPlatform)
            {
                var shot = MantisShot.Spawn(Boss.Claws.transform.position, MathUtilities.PolarToCartesian(currentAngle, velocityRange.RandomInRange()));

                shot.Audio.AudioSource.volume = 1f / shotAmount;
            }
        }
    }

    public override float PostDelay => Boss.InClimbingPhase ? climbingPostDelay : postDelay;

    public override void OnStun()
    {
        foreach (var claw in Boss.Claws.claws)
        {
            if (claw.ClawLocked)
            {
                claw.UnlockClaw();
            }
        }
    }
}
