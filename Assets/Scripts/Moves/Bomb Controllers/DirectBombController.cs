
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class DirectBombController : IBombController
{
    public int BombsToShoot => 1;

    public float AirTime;
    public float GravityScale;
    public float BombSize;

    public DirectBombController(float airTime, float gravityScale, float bombSize)
    {
        AirTime = airTime;
        GravityScale = gravityScale;
        BombSize = bombSize;
    }

    public void GetBombInfo(int bombIndex, Vector3 sourcePos, out Vector2 velocity, out float bombSize)
    {
        velocity = MathUtilities.CalculateVelocityToReachPoint(sourcePos, Player.Player1.transform.position, AirTime, GravityScale);
        bombSize = BombSize;
    }

    public float GetBombZAxis(int bombIndex)
    {
        return float.NaN;
    }

    public bool DoBombs(AncientAspid Boss)
    {
        return true;
    }
}
