using System.Collections.Generic;
using UnityEngine;

public class EnemyAStarMovement : EnemyMovement
{
    public EnemyMovement baseMovement;

    [Header("Pathfinding Settings")]
    public float horizontalCost = 1f;
    public float upCost = 2f;
    public float downCost = 0.5f;
    public float digCost = 10f;
    public int maxPathDepth = 100;
    public int collisionRadius = 0;
    
    [Header("Path Following")]
    public float repathRate = 0.5f;
    public float nextWaypointDistance = 0.5f;

    private List<Vector3> currentPath;
    private float lastRepathTime;
    private int currentPathIndex;

    public override void Move(Vector3 targetPos, DigCallback digCallback)
    {
        if (Time.time - lastRepathTime >= repathRate)
        {
            lastRepathTime = Time.time;
            CalculatePath(targetPos);
        }

        FollowPath(targetPos, digCallback);
    }

    public override void Stop()
    {
        baseMovement.Stop();
        currentPath = null;
    }

    private void CalculatePath(Vector3 targetPos)
    {
        Vector3Int startNode = Level.I.WorldToCell(transform.position);
        Vector3Int targetNode = Level.I.WorldToCell(targetPos);

        if (startNode == targetNode)
        {
            currentPath = new List<Vector3> { targetPos };
            currentPathIndex = 0;
            return;
        }

        currentPath = FindPath(startNode, targetNode);
        currentPathIndex = 0;
    }

    private void FollowPath(Vector3 targetPos, DigCallback digCallback)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            baseMovement.Move(targetPos, digCallback);
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            // Reached end of path
             baseMovement.Move(targetPos, digCallback);
             return;
        }

        Vector3 waypoint = currentPath[currentPathIndex];
        
        // Check if we reached the waypoint
        if (Vector3.Distance(transform.position, waypoint) < nextWaypointDistance)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                 baseMovement.Move(targetPos, digCallback);
                 return;
            }
            waypoint = currentPath[currentPathIndex];
        }

        baseMovement.Move(waypoint, digCallback);
    }

    private List<Vector3> FindPath(Vector3Int start, Vector3Int end)
    {
        PriorityQueue<Node> openSet = new PriorityQueue<Node>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();
        
        Node startNode = new Node(start, 0, GetHeuristic(start, end), null);
        openSet.Enqueue(startNode, startNode.F);
        allNodes[start] = startNode;

        int depth = 0;

        while (openSet.Count > 0 && depth < maxPathDepth)
        {
            depth++;
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode.Position);

            if (currentNode.Position == end)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.Position))
            {
                if (closedSet.Contains(neighborPos))
                {
                    continue;
                }

                float moveCost = GetMoveCost(currentNode.Position, neighborPos);

                bool isDiagonal = neighborPos.x != currentNode.Position.x && neighborPos.y != currentNode.Position.y;
                if (isDiagonal)
                {
                    Vector3Int c1 = new Vector3Int(neighborPos.x, currentNode.Position.y, 0);
                    Vector3Int c2 = new Vector3Int(currentNode.Position.x, neighborPos.y, 0);

                    // "if there exists a non-diagonal path"
                    // We check if we could reach via c1 or c2
                    if (!IsWalkable(c1) && !IsWalkable(c2))
                    {
                        continue;
                    }
                }
                
                if (neighborPos != end && !IsWalkable(neighborPos))
                {
                    moveCost += digCost;
                }

                float newMovementCostToNeighbor = currentNode.G + moveCost;
                
                Node neighborNode;
                bool needsEnqueue = false;

                if (!allNodes.TryGetValue(neighborPos, out neighborNode))
                {
                    neighborNode = new Node(neighborPos, newMovementCostToNeighbor, GetHeuristic(neighborPos, end), currentNode);
                    allNodes[neighborPos] = neighborNode;
                    needsEnqueue = true;
                }
                else if (newMovementCostToNeighbor < neighborNode.G)
                {
                    neighborNode.G = newMovementCostToNeighbor;
                    neighborNode.Parent = currentNode;
                    needsEnqueue = true;
                }

                if (needsEnqueue)
                {
                    openSet.Enqueue(neighborNode, neighborNode.F);
                }
            }
        }

        return null; 
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(Level.I.grid.GetCellCenterWorld(currentNode.Position));
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }

    private bool IsWalkable(Vector3Int center)
    {
        for (int x = -collisionRadius; x <= collisionRadius; x++)
        {
            for (int y = -collisionRadius; y <= collisionRadius; y++)
            {
                if (Level.I.HasTile(center + new Vector3Int(x, y, 0)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int p)
    {
        // 8-neighborhood
        return new List<Vector3Int>
        {
            p + Vector3Int.right,
            p + Vector3Int.left,
            p + Vector3Int.up,
            p + Vector3Int.down,
            p + new Vector3Int(1, 1, 0),
            p + new Vector3Int(1, -1, 0),
            p + new Vector3Int(-1, 1, 0),
            p + new Vector3Int(-1, -1, 0)
        };
    }

    private float GetMoveCost(Vector3Int from, Vector3Int to)
    {
        Vector3Int diff = to - from;
        bool isDiagonal = diff.x != 0 && diff.y != 0;

        if (isDiagonal)
        {
            float verticalCost = diff.y > 0 ? upCost : downCost;
            return Mathf.Sqrt(2) * (horizontalCost + verticalCost) / 2f;
        }

        if (diff.y > 0) return upCost;
        if (diff.y < 0) return downCost;
        return horizontalCost;
    }

    private float GetHeuristic(Vector3Int a, Vector3Int b)
    {
        // Euclidean distance is better for diagonal movement, or just Octile distance. 
        // Using Euclidean as simple approximation since costs vary.
        return Vector3Int.Distance(a, b);
    }

    private class Node
    {
        public Vector3Int Position;
        public float G;
        public float H;
        public Node Parent;

        public float F => G + H;

        public Node(Vector3Int position, float g, float h, Node parent)
        {
            Position = position;
            G = g;
            H = h;
            Parent = parent;
        }
    }

    // Simple Priority Queue implementation since .NET version might be old or not available
    public class PriorityQueue<T>
    {
        private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add(new KeyValuePair<T, float>(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Value < elements[bestIndex].Value)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Key;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            Gizmos.DrawSphere(currentPath[i], 0.1f);
        }
        Gizmos.DrawSphere(currentPath[currentPath.Count - 1], 0.1f);
    }

    public override bool IsCurrentFacingDirectionRight()
    {
        return baseMovement.IsCurrentFacingDirectionRight();
    }
}
