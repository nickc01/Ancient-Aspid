using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class BeBetweenPlayerAndAspid : MonoBehaviour
{
    public Vector2 FarDistanceRange = new Vector2(10f,13f);
    public Vector2 NearDistanceRange = new Vector2(7.5f,10f);

    public float ShortenDistanceRate = 0.75f;
    public float LengthenDistanceRate = 4.5f;

    public float DistanceFromPlayerWhenAbove = 2f;

    public float AboveMinAngle = 45;
    public float AboveMaxAngle = 90 + 45;

    public float minDistanceFromFloor = 5f;
    public float minDistanceFromRightWall = 5f;
    public float minDistanceFromLeftWall = 5f;
    public float minDistanceFromCeiling = 5f;

    public float currentDistanceToPlayer = 0f;

    public Vector2 angleRange = new Vector2(5f, 180f - 5f);

    public AncientAspid Aspid;

    float floorLevel = 0;

    RaycastHit2D[] resultStorage = new RaycastHit2D[1];

    bool doFarDistance = false;


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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }

    private void Update()
    {
        var playerPos = Player.Player1.transform.position;

        var dirToAspid = Aspid.transform.position - playerPos;

        var angleToAspid = (MathUtilities.CartesianToPolar(dirToAspid).x + 360f) % 360f;

        float magnitude;

        if (angleToAspid > AboveMinAngle && angleToAspid < AboveMaxAngle)
        {
            magnitude = DistanceFromPlayerWhenAbove;
        }
        else
        {
            magnitude = currentDistanceToPlayer;
        }

        if (angleToAspid > 270f)
        {
            angleToAspid -= 360f;
        }

        angleToAspid = Mathf.Clamp(angleToAspid, angleRange.x, angleRange.y);

        transform.position = playerPos + (Vector3)MathUtilities.PolarToCartesian(angleToAspid, magnitude);

        var roomBoundaries = Aspid.CurrentRoomRect;

        if (transform.position.y <= roomBoundaries.yMin + minDistanceFromFloor)
        {
            transform.SetPositionY(roomBoundaries.yMin + minDistanceFromFloor);
        }
    }
}
