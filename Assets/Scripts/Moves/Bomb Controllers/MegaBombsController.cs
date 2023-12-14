
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class MegaBombsController : IBombController
{
    public int BombsToShoot { get; private set; }

    public float BombSize;
    public float BombAirDuration;
    public float BombTargetRandomness;
    public float GravityScale;
    public AncientAspid Boss;

    public float BombFireTimeStamp { get; private set; }

    public float MinTimeToFloor { get; private set; } = float.PositiveInfinity;
    public float MinLandXPos { get; private set; } = float.PositiveInfinity;
    public float FloorHeight { get; private set; }

    public MegaBombsController(AncientAspid boss, int bombsToShoot, float bombSize, float bombAirDuration, float bombTargetRandomness, float gravityScale)
    {
        Boss = boss;
        BombsToShoot = bombsToShoot;
        BombSize = bombSize;
        BombAirDuration = bombAirDuration;
        BombTargetRandomness = bombTargetRandomness;
        GravityScale = gravityScale;
    }

    public void GetBombInfo(int bombIndex, Vector3 sourcePos, out Vector2 velocity, out float bombSize)
    {
        velocity = MathUtilities.CalculateVelocityToReachPoint(sourcePos, (Vector2)Player.Player1.transform.position + (Random.insideUnitCircle * BombTargetRandomness), BombAirDuration, GravityScale);

        float floorHeight = Boss.CurrentRoomRect.yMin + 0.5f;

        FloorHeight = floorHeight;

        float timeToFloor = MathUtilities.CalculateTimeToReachHeight(sourcePos, velocity, floorHeight, GravityScale);

        if (timeToFloor < MinTimeToFloor)
        {
            MinTimeToFloor = timeToFloor;
            MinLandXPos = MathUtilities.PredictPosition(sourcePos, velocity, GravityScale, timeToFloor).x;
        }

        bombSize = BombSize;

        BombFireTimeStamp = Time.time;
    }

    public float GetBombZAxis(int bombIndex)
    {
        return -0.3f;
    }

    public bool DoBombs(AncientAspid Boss)
    {
        return true;
    }
}