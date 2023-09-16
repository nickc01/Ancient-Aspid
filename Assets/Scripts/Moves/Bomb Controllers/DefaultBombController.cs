﻿
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class DefaultBombController : IBombController
{
    public int BombsToShoot => 1;

    public readonly float ShotSpeed;
    public readonly float GravityScale;

    public DefaultBombController(float shotSpeed, float gravityScale)
    {
        ShotSpeed = shotSpeed;
        GravityScale = gravityScale;
    }

    public void GetBombInfo(int bombIndex, Vector3 sourcePos, out Vector2 velocity, out float bombSize)
    {
        bombSize = float.NaN;

        velocity = MathUtilities.CalculateVelocityToReachPoint(sourcePos, Player.Player1.transform.position, Vector3.Distance(sourcePos, Player.Player1.transform.position) / ShotSpeed, GravityScale);
    }

    public float GetBombZAxis(int bombIndex)
    {
        return float.NaN;
    }
}