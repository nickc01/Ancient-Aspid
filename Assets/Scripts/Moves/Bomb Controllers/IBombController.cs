using UnityEngine;
using WeaverCore.Features;

public interface IBombController
{
    int BombsToShoot { get; }
    void GetBombInfo(int bombIndex, Vector3 sourcePos, out Vector2 velocity, out float bombSize);

    float GetBombZAxis(int bombIndex);
}
