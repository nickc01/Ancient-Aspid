using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;

public class GroundModeExplosiveVariation : GroundModeVariationBase
{
    MegaBombsController bombController;
    BombMove bombMove;

    public GroundModeExplosiveVariation(GroundMode mode) : base(mode) { }

    IEnumerator ShootExplosives(BombMove move, IBombController controller, float preDelay = 0.25f)
    {
        yield return new WaitForSeconds(preDelay);
        //move.CustomController = controller;
        //yield return Boss.RunMove(move);

        yield return Mode.RunAspidMove(move, new Dictionary<string, object>
        {
            { BombMove.CUSTOM_BOMB_CONTROLLER, controller }
        });

    }

    public override IEnumerator OnBegin()
    {
        bombMove = Boss.GetComponent<BombMove>();
        bombController = new MegaBombsController(Boss, 3, 2f, 1f, 0.25f, bombMove.BombGravityScale);
        yield return ShootExplosives(bombMove, bombController);
    }

    public override Vector3 GetLungeTarget()
    {
        return new Vector3(bombController.MinLandXPos, bombController.FloorHeight);
    }

    public override bool DoSlide(Vector3 lungeTarget)
    {
        return false;
    }

    public override void LungeLand(bool sliding)
    {
        foreach (var bomb in bombMove.LastFiredBombs)
        {
            if (bomb != null)
            {
                bomb.Explode();
            }
        }

        if (Mode.megaExplosionSound != null)
        {
            WeaverAudio.PlayAtPoint(Mode.megaExplosionSound, transform.position);
        }

        InfectedExplosion.Spawn(transform.position, Mode.megaExplosionSize);
    }
}

