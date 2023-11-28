using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Utilities;

using V3 = System.Numerics.Vector3;

public class PathfindingSystem : MonoBehaviour
{
    List<PathfindingNode> nodeObjects = new List<PathfindingNode>();

    List<StaticNode> nodes = new List<StaticNode>();

    [field: SerializeField]
    public float PathGenCircleRadius = 5f;

    [field: SerializeField]
    public float BossColliderSize = 4.75f;

    [field: SerializeField]
    LayerMask collisionMask;

    class StaticNode : IComparable<StaticNode>
    {
        static V3 dest;

        public PathfindingNode SourceNode;
        public V3 pos;
        public List<StaticNode> neighbors;
        public StaticNode prev;
        public float Heuristic;

        public int CompareTo(StaticNode other)
        {
            return V3.Distance(pos, dest).CompareTo(V3.Distance(other.pos, dest));
        }
    }


    private void Awake()
    {
        if (nodeObjects == null || nodeObjects.Count == 0)
        {
            GetComponentsInChildren(nodeObjects);

            if (Application.isPlaying)
            {
                foreach (var node in nodeObjects)
                {
                    node.GenerateNeighbors(nodeObjects, collisionMask, PathGenCircleRadius, BossColliderSize);
                }
                FixNeighbors();
                SetupStaticNodes();
            }
        }
    }

    public void GenerateNeighborsForNode(PathfindingNode node, float radius)
    {
    }

    void FixNeighbors()
    {
        foreach (var node in nodeObjects)
        {
            foreach (var neighbor in node.Neighbors)
            {
                if (neighbor.Neighbors.Contains(node))
                {
                    neighbor.Neighbors.Add(node);
                }
            }
        }
    }

    void SetupStaticNodes()
    {
        Dictionary<PathfindingNode, StaticNode> nodeMappings = new Dictionary<PathfindingNode, StaticNode>();

        foreach (var node in nodeObjects)
        {
            var pos = node.transform.position;
            nodeMappings.Add(node, new StaticNode()
            {
                neighbors = new List<StaticNode>(),
                SourceNode = node,
                pos = new V3(pos.x,pos.y,pos.z)
            });
        }

        foreach (var pair in nodeMappings)
        {
            pair.Value.neighbors.AddRange(pair.Key.Neighbors.Select(n => nodeMappings[n]));
        }

        nodes.AddRange(nodeMappings.Values);
    }

    Vector3 ToVector3(V3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    V3 ToV3(Vector3 v)
    {
        return new V3(v.x, v.y, v.z);
    }

    StaticNode GenerateNode(V3 pos) => new StaticNode
    {
        neighbors = FindNeighbors(pos),
        pos = pos
    };

    List<StaticNode> FindNeighbors(V3 pos)
    {
        var nearbyNodes = nodes.AsParallel().Where(n => V3.Distance(n.pos, pos) <= PathGenCircleRadius).ToList();

        nearbyNodes.RemoveAll(n => !PathfindingNode.IsPathClear(pos, n.pos, collisionMask, BossColliderSize));

        if (nearbyNodes.Count == 0)
        {
            float minDistance = float.PositiveInfinity;
            int minIndex = -1;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var dist = V3.DistanceSquared(nodes[i].pos, pos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    minIndex = i;
                }
            }

            if (minIndex > -1)
            {
                nearbyNodes.Add(nodes[minIndex]);
            }
        }

        return nearbyNodes;
    }

    void InjectIntoNeighbors(StaticNode node)
    {
        foreach (var neighbor in node.neighbors)
        {
            neighbor.neighbors.AddIfNotContained(node);
        }
    }

    void UninjectFromNeighbors(StaticNode node)
    {
        foreach (var neighbor in node.neighbors)
        {
            neighbor.neighbors.Remove(node);
        }
    }

    public bool CanDirectConnect(Vector3 start, Vector3 end)
    {
        return PathfindingNode.IsPathClear(start, end, collisionMask);
    }

    public void GeneratePath(Vector3 start, Vector3 end, List<Vector3> output)
    {
        if (CanDirectConnect(start, end))
        {
            output.Clear();
            output.Add(start);
            output.Add(end);
        }
        else
        {
            GeneratePath(GenerateNode(new V3(start.x, start.y, start.z)), GenerateNode(new V3(end.x,end.y,end.z)), output);
        }
    }

    public void GeneratePathAsync(Vector3 start, Vector3 end, List<Vector3> output, Action onFinish)
    {
        if (CanDirectConnect(start, end))
        {
            output.Clear();
            output.Add(start);
            output.Add(end);
            onFinish();
        }
        else
        {
            var startNode = GenerateNode(new V3(start.x, start.y, start.z));
            var endNode = GenerateNode(new V3(end.x, end.y, end.z));

            void GenAsync()
            {
                try
                {
                    GeneratePath(startNode, endNode, output);
                    onFinish();
                }
                catch (Exception e)
                {
                    Debug.LogError("PATH GEN FAILED");
                    Debug.LogException(e);
                }
            }

            Task.Run(GenAsync);
        }
    }


    void GeneratePath(StaticNode start, StaticNode end, List<Vector3> output)
    {
        void generatePath(StaticNode start, StaticNode end, List<Vector3> output)
        {
            start.prev = null;
            start.Heuristic = 0;
            LinkedList<StaticNode> openList = new LinkedList<StaticNode>();

            openList.AddFirst(start);

            HashSet<StaticNode> closedSet = new HashSet<StaticNode>();

            StaticNode foundDest = null;

            while (openList.Count > 0)
            {
                var currentNode = openList.First.Value;
                openList.RemoveFirst();
                if (currentNode == end)
                {
                    foundDest = currentNode;
                }

                closedSet.Add(currentNode);

                foreach (var neighbor in currentNode.neighbors)
                {
                    var travel = currentNode.Heuristic + V3.Distance(neighbor.pos, currentNode.pos);

                    var neighborNode = openList.Find(neighbor);

                    if (neighborNode != null)
                    {
                        if (travel < neighborNode.Value.Heuristic)
                        {
                            neighborNode.Value.Heuristic = travel;
                            neighborNode.Value.prev = currentNode;
                        }
                    }
                    else
                    {
                        if (closedSet.Contains(neighbor))
                        {
                            if (travel < neighbor.Heuristic)
                            {
                                closedSet.Remove(neighbor);

                                neighbor.Heuristic = travel;
                                neighbor.prev = currentNode;

                                bool added = false;

                                var first = openList.First;

                                for (int i = openList.Count - 1; i >= 0; i--)
                                {
                                    if (first.Value.Heuristic > travel)
                                    {
                                        openList.AddBefore(first, neighbor);
                                        added = true;
                                    }

                                    if (i > 0)
                                    {
                                        first = first.Next;
                                    }
                                }

                                if (!added)
                                {
                                    openList.AddLast(neighbor);
                                }

                            }
                        }
                        else
                        {
                            neighbor.Heuristic = travel;
                            neighbor.prev = currentNode;

                            bool added = false;

                            var first = openList.First;

                            for (int i = openList.Count - 1; i >= 0; i--)
                            {
                                if (first.Value.Heuristic > travel)
                                {
                                    openList.AddBefore(first, neighbor);
                                    added = true;
                                }

                                if (i > 0)
                                {
                                    first = first.Next;
                                }
                            }

                            if (!added)
                            {
                                openList.AddLast(neighbor);
                            }
                        }
                    }

                }
            }

            if (foundDest != null)
            {
                output.Clear();

                while (foundDest != null)
                {
                    output.Add(new Vector3(foundDest.pos.X, foundDest.pos.Y, foundDest.pos.Z));
                    var prev = foundDest.prev;
                    foundDest.prev = null;
                    foundDest = prev;
                }

                output.Reverse();
            }
            else
            {
                output.Clear();
                output.Add(ToVector3(start.pos));
                output.Add(ToVector3(end.pos));
            }
        }

        try
        {
            InjectIntoNeighbors(start);
            InjectIntoNeighbors(end);
            generatePath(start, end, output);
        }
        finally
        {
            UninjectFromNeighbors(start);
            UninjectFromNeighbors(end);
        }


    }

}
