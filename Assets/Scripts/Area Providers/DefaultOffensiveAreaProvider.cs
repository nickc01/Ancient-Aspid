using UnityEngine;
using WeaverCore;


public class DefaultOffensiveAreaProvider : IModeAreaProvider
{
    AncientAspid boss;

    //BoxCollider2D collider;

    public DefaultOffensiveAreaProvider(AncientAspid boss)
    {
        this.boss = boss;
    }


    public Vector2 GetModeTarget()
    {
        var rect = RoomScanner.GetRoomBoundaries(Player.Player1.transform.position);

        var targetX = boss.transform.position.x;

        if (targetX > rect.Rect.xMax - 7f)
        {
            targetX = rect.Rect.xMax - 7f;
        }

        if (targetX < rect.Rect.xMin + 7f)
        {
            targetX = rect.Rect.xMin + 7f;
        }

        //Mathf.Lerp(rect.Rect.xMin, rect.Rect.xMax, 0.5f)
        var newTarget = new Vector3(targetX, rect.Rect.yMin + boss.OffensiveHeight);

        //newTarget.x = Mathf.Clamp(newTarget.x, Player.Player1.transform.position.x - 10f, Player.Player1.transform.position.x + 10f);
        //newTarget.y = Mathf.Clamp(newTarget.y, Player.Player1.transform.position.y + 4f, Player.Player1.transform.position.y + 20f);

        return newTarget;
    }
}