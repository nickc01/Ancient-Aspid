using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    Vector2 startingVelocity = new Vector2();

    private void Awake()
    {
        //mainRigidbody.velocity = startingVelocity;
        //mainCollider.enabled = false;
    }

    private void LateUpdate()
    {
        transform.position += (Vector3)startingVelocity * Time.deltaTime;
    }

    /*private void FixedUpdate()
    {
        var worldBottomLeft = transform.TransformPoint(colliderBottomLeft);
        var worldTopRight = transform.TransformPoint(colliderTopRight);

        Debug.DrawLine(worldBottomLeft, worldTopRight, Color.blue, 5f);

        Debug.DrawLine(new Vector2(worldBottomLeft.x - nearColliderThreshold, worldBottomLeft.y - nearColliderThreshold), new Vector2(worldBottomLeft.x - nearColliderThreshold, worldTopRight.y + nearColliderThreshold), Color.red, 1f);

        if (Player.Player1.transform.position.y >= worldBottomLeft.y - nearColliderThreshold && Player.Player1.transform.position.y <= worldTopRight.y + nearColliderThreshold && Player.Player1.transform.position.x <= worldTopRight.x + nearColliderThreshold && Player.Player1.transform.position.x >= worldBottomLeft.x - nearColliderThreshold)
        {
            mainCollider.enabled = true;
        }
        else
        {
            mainCollider.enabled = false;
        }
    }*/
}
