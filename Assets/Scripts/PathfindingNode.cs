using System.Collections.Generic;
using System.Net;
using UnityEngine;

using V3 = System.Numerics.Vector3;
using V2 = System.Numerics.Vector2;
using System;

[ExecuteAlways]
public class PathfindingNode : MonoBehaviour
{
    [SerializeField]
    float preGenerateRadius = 10f;

    [SerializeField]
    bool doPreGenerate = false;

    [NonSerialized]
    bool preGenerated = false;

    static RaycastHit2D[] singleHitCache = new RaycastHit2D[1];

    [SerializeField]
    List<PathfindingNode> neighbors;

    public List<PathfindingNode> Neighbors => neighbors;

    public static bool IsPathClear(Vector3 start, Vector3 end, int collisionMask, float bossSize = 4.75f)
    {
        var distanceToNode = Vector2.Distance(start, end);

        var directionToPoint = (end - start).normalized;

        var midPoint = Vector2.Lerp(start, end, 0.5f);


        return Physics2D.CapsuleCastNonAlloc(midPoint, new Vector2(distanceToNode, bossSize), CapsuleDirection2D.Horizontal, Mathf.Atan2(directionToPoint.y, directionToPoint.x) * Mathf.Rad2Deg, directionToPoint, singleHitCache, distanceToNode, collisionMask) == 0;

    }

    public static bool IsPathClear(V3 start, V3 end, int collisionMask, float bossSize = 4.75f)
    {
        var distanceToNode = V3.Distance(start, end);

        var directionToPoint = V3.Normalize(end - start);

        var midPoint = V3.Lerp(start, end, 0.5f);

        return Physics2D.CapsuleCastNonAlloc(new Vector2(midPoint.X,midPoint.Y), new Vector2(distanceToNode, bossSize), CapsuleDirection2D.Horizontal, Mathf.Atan2(directionToPoint.Y, directionToPoint.X) * Mathf.Rad2Deg, new Vector2(directionToPoint.X,directionToPoint.Y), singleHitCache, distanceToNode, collisionMask) == 0;

    }

    public void GenerateNeighbors(List<PathfindingNode> allNodes, int collisionMask, float pathGenCircleRadius, float bossSize = 4.75f)
    {
        neighbors = new List<PathfindingNode>();

        for (int i = 0; i < allNodes.Count; i++)
        {
            var distanceToNode = Vector2.Distance(allNodes[i].transform.position, transform.position);

            int nearbyCount = 0;

            if (allNodes[i] != this && distanceToNode <= pathGenCircleRadius)
            {
                nearbyCount++;
                var directionToPoint = (allNodes[i].transform.position - transform.position).normalized;

                var midPoint = Vector2.Lerp(transform.position, allNodes[i].transform.position, 0.5f);

                var hits = Physics2D.CapsuleCastNonAlloc(midPoint, new Vector2(distanceToNode, bossSize), CapsuleDirection2D.Horizontal, Mathf.Atan2(directionToPoint.y, directionToPoint.x) * Mathf.Rad2Deg, directionToPoint, singleHitCache,distanceToNode,collisionMask);
                if (hits == 0)
                {
                    neighbors.Add(allNodes[i]);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }

    private void OnDrawGizmosSelected()
    {
        if (neighbors != null)
        {
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
                }
            }
        }
    }

    private void Update()
    {
        if (doPreGenerate && !preGenerated)
        {
            Debug.Log("GEN TEST");
            preGenerated = true;
            GetComponentInParent<PathfindingSystem>().GenerateNeighborsForNode(this, preGenerateRadius);
        }

        if (!doPreGenerate && preGenerated)
        {
            preGenerated = false;
        }
    }
}
