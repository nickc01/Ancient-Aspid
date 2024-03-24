
using UnityEngine;
using WeaverCore.Utilities;

public class TargetBombController : IBombController
{
    public int BombsToShoot => 1;

    public float AirTime;
    public float GravityScale;
    public float BombSize;
    public Vector3 target;

    public TargetBombController(float airTime, float gravityScale, float bombSize, Vector3 target)
    {
        AirTime = airTime;
        GravityScale = gravityScale;
        BombSize = bombSize;
        this.target = target;
    }

    public void GetBombInfo(int bombIndex, Vector3 sourcePos, out Vector2 velocity, out float bombSize)
    {
        velocity = MathUtilities.CalculateVelocityToReachPoint(sourcePos, target, AirTime, GravityScale);
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
