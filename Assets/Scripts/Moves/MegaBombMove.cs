/*using System.Collections;
using UnityEngine;

public class MegaBombMove : AncientAspidMove
{
    [SerializeField]
    int bombsToShoot = 3;

    [SerializeField]
    float bombSize = 2f;

    [SerializeField]
    float bombTargetRandomness = 1f;

    [SerializeField]
    float bombAirDuration = 1f;

    public override bool MoveEnabled => false;

    BombMove bombMove;

    MegaBombsController lastController;

    public float FireTimeStamp => lastController?.BombFireTimeStamp ?? 0f;

    //public float MinAirTime => 

    private void Awake()
    {
        bombMove = GetComponent<BombMove>();
    }

    public override IEnumerator DoMove()
    {
        lastController = new MegaBombsController(bombsToShoot, bombSize, bombAirDuration, bombTargetRandomness, bombMove.BombGravityScale);
        yield return bombMove.FireBombs(lastController, true);
    }

    public override void OnStun()
    {
        bombMove.OnStun();
    }
}*/
