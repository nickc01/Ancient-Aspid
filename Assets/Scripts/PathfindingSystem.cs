using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Utilities;

using V3 = System.Numerics.Vector3;
using V2 = System.Numerics.Vector2;

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
        //Awake();
        //node.GenerateNeighbors(nodeObjects, collisionMask, radius, BossColliderSize);
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
        //nodes.ForEach(n => Debug.Log($"DISTANCE = {V3.Distance(n.pos, pos)}"));
        /*foreach (var n in nodes)
        {
            Debug.Log("SOURCE POS = " + n.pos);
            Debug.Log("DEST POS = " + pos);
            Debug.Log($"DISTANCE = {V3.Distance(n.pos, pos)}");
        }*/
        var nearbyNodes = nodes.AsParallel().Where(n => V3.Distance(n.pos, pos) <= PathGenCircleRadius).ToList();

        //Debug.Log("Pos = " + pos);
        //Debug.Log("GENERATED NEIGHBOR COUNT = " + nearbyNodes.Count);

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
            //nearbyNodes.Add(nodes.AsParallel().Min());
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
            /*return new List<Vector3>()
            {
                start,
                end
            };*/
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
            /*onFinish(new List<Vector3>()
            {
                start,
                end
            });*/
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

    /*class AStarNode : IComparable<AStarNode>
    {
        //static Comparer<float> floatComparer = Comparer<float>.Default;

        public AStarNode prevNode; //Used to traverse backwards back to the start when the destination is found
        public StaticNode source; //The static node at this aStar Node

        public readonly float distanceTravelled;
        public readonly float distanceToDest;
        public readonly float Heuristic;

        public AStarNode(AStarNode prevNode, StaticNode source, float distanceTravelled, float distanceToDest)
        {
            this.prevNode = prevNode;
            this.source = source;
            this.distanceTravelled = distanceTravelled;
            this.distanceToDest = distanceToDest;

            Heuristic = distanceTravelled;
        }

        //The heuristic of this node. The lower the value, the better this node is at reaching the destination
        //public float Heuristic => distanceToDest + distanceTravelled;
        //public float Heuristic => distanceTravelled;

        public override bool Equals(object obj)
        {
            if (obj is StaticNode sNode)
            {
                return source == sNode;
            }
            else if (obj is AStarNode aNode)
            {
                return source == aNode.source;
                //return base.Equals(obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return source.GetHashCode();
        }

        public int CompareTo(AStarNode other)
        {
            return Heuristic.CompareTo(other.Heuristic);
            //return floatComparer.Compare(Heuristic, other.Heuristic);
        }
    }*/

    /*class AStarNodeSorter : IComparer<AStarNode>
    {
        Comparer<float> floatSorter = Comparer<float>.Default;

        public int Compare(AStarNode x, AStarNode y)
        {
            return floatSorter.Compare(y.Heuristic, x.Heuristic);
        }
    }*/


    void GeneratePath(StaticNode start, StaticNode end, List<Vector3> output)
    {
        void generatePath(StaticNode start, StaticNode end, List<Vector3> output)
        {
            start.prev = null;
            start.Heuristic = 0;
            //AStarNode neighborDummy = new AStarNode(null, null, 0, 0);

            //var sorter = new AStarNodeSorter();

            //The list of nodes to select
            /*List<AStarNode> openList = new List<AStarNode>()
            {
                new AStarNode(null, start, 0f, Vector2.Distance(start.pos, end.pos))
            };*/

            //The list of nodes that have already been traversed
            //List<AStarNode> closedList = new List<AStarNode>();

            LinkedList<StaticNode> openList = new LinkedList<StaticNode>();

            openList.AddFirst(start);

            /*HashSet<StaticNode> openSet = new HashSet<StaticNode>
            {
                //new AStarNode(null, start, 0f, V3.Distance(start.pos, end.pos))
                start
            };*/

            HashSet<StaticNode> closedSet = new HashSet<StaticNode>();

            StaticNode foundDest = null;

            while (openList.Count > 0)
            {
                //Debug.Log("OPEN SET COUNT = " + openList.Count);
                //openList.Sort(sorter);

                //var currentNode = openSet.AsParallel().Min();
                var currentNode = openList.First.Value;
                openList.RemoveFirst();
                //var currentNode = openList[openList.Count - 1];
                //openList.RemoveAt(openList.Count - 1);
                //openSet.Remove(currentNode);

                if (currentNode == end)
                {
                    foundDest = currentNode;
                }

                //closedList.Add(currentNode);
                closedSet.Add(currentNode);

                //Debug.Log("NODE NEIGHBORs = " + currentNode.neighbors.Count);

                foreach (var neighbor in currentNode.neighbors)
                {
                    //neighborDummy.source = neighbor;

                    //bool addable = false;

                    var travel = currentNode.Heuristic + V3.Distance(neighbor.pos, currentNode.pos);
                    //var destDistance = Vector2.Distance(neighbor.pos, end.pos);

                    //var h = travel;

                    //var neighborNode = new AStarNode(currentNode, neighbor, travel, destDistance);

                    //var foundIndex = openList.FindIndex(n => n.source == neighbor);


                    //if (foundIndex >= 0)
                    //if (openSet.Contains(neighborDummy))

                    var neighborNode = openList.Find(neighbor);

                    //Debug.Log("FOUND = " + (neighborNode != null));

                    //if (openSet.TryGetEquivalent(neighborNode, out var openNeighbor))
                    if (neighborNode != null)
                    {
                        if (travel < neighborNode.Value.Heuristic)
                        {
                            neighborNode.Value.Heuristic = travel;
                            neighborNode.Value.prev = currentNode;
                            //openSet.Remove(openNeighbor);
                            //openSet.Add(neighborNode);
                            //openList[foundIndex] = new AStarNode(currentNode, neighbor, travel, destDistance);
                        }
                        //openList[foundIndex] = new AStarNode(currentNode, neighbor, travel, destDistance);
                    }
                    else
                    {
                        //Todo - do the same thing for the closed list. remove it from the closed list and add it to the open list

                        //var closedFoundIndex = closedList.FindIndex(n => n.source == neighbor);

                        //if (closedFoundIndex >= 0)
                        //if (closedSet.Contains(neighborDummy))
                        //if (closedSet.TryGetEquivalent(neighborNode, out var closedNeighbor))
                        if (closedSet.Contains(neighbor))
                        {
                            if (travel < neighbor.Heuristic)
                            {
                                //closedList.RemoveAt(closedFoundIndex);
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

                                /*foreach (var n in openList)
                                {
                                    if (n.Heuristic > travel)
                                    {
                                        openLis
                                        added = true;
                                    }
                                }*/
                                //openSet.Add(neighborNode);
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
                            //openSet.Add(neighborNode);
                        }
                    }

                    /*foreach (var node in openList)
                    {
                        if (node.source == neighbor && h > node.Heuristic)
                        {
                            addable = false;
                            break;
                        }
                    }

                    if (!addable)
                    {
                        foreach (var node in closedList)
                        {
                            if (node.source == neighbor && h > node.Heuristic)
                            {
                                addable = false;
                                break;
                            }
                        }
                    }*/

                    //bool addable = openList.AsParallel().All(n => !(n.source == neighbor && ))

                    /*if (addable)
                    {
                        openList.Add(new AStarNode(currentNode, neighbor, travel, destDistance));
                    }*/

                    //If the open and closed lists don't contain the neighboring node, then add it to the open list
                    /*if (!openList.Any(n => n.source == neighbor) && !closedList.Any(n => n.source == neighbor))
                    {
                        openList.Add(new AStarNode(currentNode, neighbor, currentNode.distanceTravelled + Vector2.Distance(neighbor.pos, currentNode.source.pos), Vector2.Distance(neighbor.pos, end.pos)));
                    }*/
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


        //Inject and Uninject here
    }

    /*public List<PathfindingNode> GeneratePath(PathfindingNode start, PathfindingNode end)
    {

    }

    public List<PathfindingNode> GeneratePath(Vector3 start, Vector3 end)
    {

    }*\

    /*PathfindingNode GeneratePositionNode(Vector3 pos)
    {
        var newObj = new GameObject("TEMP_NODE");
        newObj.transform.position = pos;
        var node = newObj.AddComponent<PathfindingNode>();
        node.GenerateNeighbors(Nodes, LayerMask.GetMask("Terrain"));
    }*/
}
