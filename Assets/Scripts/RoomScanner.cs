using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScanResult
{
    public Rect Rect;
    public RaycastHit2D LeftHit;
    public RaycastHit2D TopHit;
    public RaycastHit2D RightHit;
    public RaycastHit2D BottomHit;
}

public static class RoomScanner
{
    const float maxDistance = 200;

    static RaycastHit2D[] hits = new RaycastHit2D[1];

    public static RoomScanResult GetRoomBoundaries(Vector3 position)
    {
        return GetRoomBoundaries(position, LayerMask.GetMask("Terrain"));
    }

    public static RoomScanResult GetRoomBoundaries(Vector3 position,int layerMask)
    {
        var result = new RoomScanResult();

        float leftSide = position.x - maxDistance;

        float topSide = position.y + maxDistance;

        if (Physics2D.RaycastNonAlloc(position, Vector2.up, hits, maxDistance, layerMask) > 0)
        {
            //Debug.DrawLine(position, hits[0].point, Color.green, 0.25f);
            topSide = hits[0].point.y;
            result.TopHit = hits[0];
        }


        float bottomSide = position.y - maxDistance;

        if (Physics2D.RaycastNonAlloc(position, Vector2.down, hits, maxDistance, layerMask) > 0)
        {
            //Debug.DrawLine(position, hits[0].point, Color.green, 0.25f);
            bottomSide = hits[0].point.y;
            result.BottomHit = hits[0];
        }

        if (Physics2D.RaycastNonAlloc(new Vector2(position.x,Mathf.Lerp(topSide,bottomSide,0.5f)), Vector2.left, hits, maxDistance, layerMask) > 0)
        {
            //Debug.DrawLine(position, hits[0].point, Color.green,0.25f);
            leftSide = hits[0].point.x;
            result.LeftHit = hits[0];
        }

        float rightSide = position.x + maxDistance;

        if (Physics2D.RaycastNonAlloc(new Vector2(position.x, Mathf.Lerp(topSide, bottomSide, 0.5f)), Vector2.right, hits, maxDistance, layerMask) > 0)
        {
            //Debug.DrawLine(position, hits[0].point, Color.green, 0.25f);
            rightSide = hits[0].point.x;
            result.RightHit = hits[0];
        }

        //return new Rect(leftSide, bottomSide, rightSide - leftSide, topSide - bottomSide);
        result.Rect = new Rect(leftSide, bottomSide, rightSide - leftSide, topSide - bottomSide);

        return result;
    }
}
