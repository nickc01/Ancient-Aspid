using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

public class MantisShotMove : AncientAspidMove
{
    public override bool MoveEnabled => moveEnabled && Boss.AspidMode == AncientAspid.Mode.Tactical &&
        Boss.Claws.claws.All(c => !c.ClawLocked);

    [SerializeField]
    bool moveEnabled = true;

    [SerializeField]
    float postDelay = 0.4f;

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

            var shot = MantisShot.Spawn(Boss.Claws.transform.position, MathUtilities.PolarToCartesian(currentAngle, velocityRange.RandomInRange()));

            shot.Audio.AudioSource.volume = 1f / shotAmount;
        }
    }

    public override float PostDelay => postDelay;

    public override void OnStun()
    {
        
    }
}
