using UnityEngine;
using WeaverCore;


public class DefaultOffensiveAreaProvider : IModeAreaProvider
{
    AncientAspid boss;

    public DefaultOffensiveAreaProvider(AncientAspid boss)
    {
        this.boss = boss;
    }


    public Vector2 GetModeTarget()
    {
        var rect = RoomScanner.GetRoomBoundaries(Player.Player1.transform.position);

        var newTarget = new Vector3(Mathf.Lerp(rect.Rect.xMin, rect.Rect.xMax, 0.5f), rect.Rect.yMin + boss.OffensiveHeight);

        //newTarget.x = Mathf.Clamp(newTarget.x, Player.Player1.transform.position.x - 10f, Player.Player1.transform.position.x + 10f);
        //newTarget.y = Mathf.Clamp(newTarget.y, Player.Player1.transform.position.y + 4f, Player.Player1.transform.position.y + 20f);

        return newTarget;
    }
}