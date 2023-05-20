using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class BeBetweenPlayerAndAspid : MonoBehaviour
{
    //public float DistanceFromPlayer = 5f;
    //public Vector2 DistanceFromPlayerMinMax = new Vector2(7f,12f);

    public Vector2 FarDistanceRange = new Vector2(10f,13f);
    public Vector2 NearDistanceRange = new Vector2(7.5f,10f);

    public float ShortenDistanceRate = 0.75f;
    public float LengthenDistanceRate = 4.5f;

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

    public float currentDistanceToPlayer = 0f;

    public AncientAspid Aspid;

    float floorLevel = 0;

    RaycastHit2D[] resultStorage = new RaycastHit2D[1];

    bool doFarDistance = false;


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

    private void Awake()
    {
        currentDistanceToPlayer = NearDistanceRange.RandomInRange();
        StartCoroutine(DistanceRefresher());
    }

    IEnumerator DistanceRefresher()
    {
        while (true)
        {
            if (doFarDistance)
            {
                yield return new WaitForSeconds(ShortenDistanceRate);
            }
            else
            {
                yield return new WaitForSeconds(LengthenDistanceRate);
            }
            doFarDistance = !doFarDistance;
            if (doFarDistance)
            {
                currentDistanceToPlayer = FarDistanceRange.RandomInRange();
            }
            else
            {
                currentDistanceToPlayer = NearDistanceRange.RandomInRange();
            }
        }
    }

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
            transform.position = playerPos + (vectorToObject.normalized * currentDistanceToPlayer);
        }

        var roomBoundaries = Aspid.CurrentRoomRect;

        if (transform.position.y < roomBoundaries.Rect.yMin + minDistanceFromFloor)
        {
            transform.SetPositionY(roomBoundaries.Rect.yMin + minDistanceFromFloor);
        }

        if (transform.position.y > roomBoundaries.Rect.xMax - minDistanceFromCeiling)
        {
            transform.SetPositionY(roomBoundaries.Rect.xMax - minDistanceFromCeiling);
        }

        if (transform.position.x < roomBoundaries.Rect.xMin + minDistanceFromLeftWall)
        {
            transform.SetPositionX(roomBoundaries.Rect.xMin + minDistanceFromLeftWall);
        }

        if (transform.position.x > roomBoundaries.Rect.xMax - minDistanceFromRightWall)
        {
            transform.SetPositionX(roomBoundaries.Rect.xMax - minDistanceFromRightWall);
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
