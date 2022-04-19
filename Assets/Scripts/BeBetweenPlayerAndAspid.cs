using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class BeBetweenPlayerAndAspid : MonoBehaviour
{
    public float DistanceFromPlayer = 5f;

    public float DistanceFromPlayerWhenAbove = 2f;

    public float AboveMinAngle = 45;
    public float AboveMaxAngle = 90 + 45;

    /*[Tooltip("If set to true, then this will make sure this object stays above the player at a certain height")]
    public bool KeepAboveFloorHeight = true;

    [Tooltip("The minimum height this object can be above the player")]
    public float HeightAboveFloor = 5f;*/

    public float minDistanceFromFloor = 5f;
    public float minDistanceFromRightWall = 5f;
    public float minDistanceFromLeftWall = 5f;
    public float minDistanceFromCeiling = 5f;

    public AncientAspid Aspid;

    float floorLevel = 0;

    RaycastHit2D[] resultStorage = new RaycastHit2D[1];


    /*private void Awake()
    {
        StartCoroutine(FloorCheckRoutine());
    }

    IEnumerator FloorCheckRoutine()
    {
        while (true)
        {
            if (Physics2D.RaycastNonAlloc(OtherObject.transform.position, Vector2.down, resultStorage,100,LayerMask.GetMask("Terrain")) > 0)
            {
                floorLevel = resultStorage[0].point.y;
                Debug.DrawLine(OtherObject.transform.position, resultStorage[0].point, Color.green, 0.5f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }*/

    private void Update()
    {
        var playerPos = Player.Player1.transform.position;

        var vectorToObject = (Aspid.transform.position - playerPos);


        var angle = 180f + (Mathf.Rad2Deg * Mathf.Atan2(vectorToObject.y, vectorToObject.x));

        //Debug.DrawLine(OtherObject.transform.position, OtherObject.transform.position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)));

        if (angle > AboveMinAngle && angle < AboveMaxAngle)
        {
            transform.position = playerPos + (vectorToObject.normalized * DistanceFromPlayerWhenAbove);
        }
        else
        {
            transform.position = playerPos + (vectorToObject.normalized * DistanceFromPlayer);
        }

        var roomBoundaries = Aspid.CurrentRoomRect;

        if (transform.position.y < roomBoundaries.yMin + minDistanceFromFloor)
        {
            transform.SetPositionY(roomBoundaries.yMin + minDistanceFromFloor);
        }

        if (transform.position.y > roomBoundaries.xMax - minDistanceFromCeiling)
        {
            transform.SetPositionY(roomBoundaries.xMax - minDistanceFromCeiling);
        }

        if (transform.position.x < roomBoundaries.xMin + minDistanceFromLeftWall)
        {
            transform.SetPositionX(roomBoundaries.xMin + minDistanceFromLeftWall);
        }

        if (transform.position.x > roomBoundaries.xMax - minDistanceFromRightWall)
        {
            transform.SetPositionX(roomBoundaries.xMax - minDistanceFromRightWall);
        }

        /*if (KeepAboveFloorHeight)
        {
            if (transform.position.y < floorLevel + HeightAboveFloor)
            {
                transform.SetPositionY(floorLevel + HeightAboveFloor);
            }
        }*/
    }
}
