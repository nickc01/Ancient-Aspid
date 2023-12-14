using UnityEngine;
using WeaverCore;


public class DefaultOffensiveAreaProvider : IModeAreaProvider
{
    public float OffensiveHeight { get; set; }

    public DefaultOffensiveAreaProvider(float offensiveHeight)
    {
        OffensiveHeight = offensiveHeight;
    }


    public Vector2 GetModeTarget(AncientAspid boss)
    {
        RoomScanResult rect = RoomScanner.GetRoomBoundaries(Player.Player1.transform.position);

        float targetX = boss.transform.position.x;

        if (targetX > rect.Rect.xMax - 7f)
        {
            targetX = rect.Rect.xMax - 7f;
        }

        if (targetX < rect.Rect.xMin + 7f)
        {
            targetX = rect.Rect.xMin + 7f;
        }

        return new Vector2(targetX, rect.Rect.yMin + OffensiveHeight);
    }

    public bool IsTargetActive(AncientAspid boss)
    {
        return Vector3.Distance(boss.transform.position, Player.Player1.transform.position) <= 30f;
    }

    public Vector2 GetLockAreaOverride(Vector2 oldPos, out bool clampWithinArea)
    {
        clampWithinArea = true;
        return oldPos;
    }
}