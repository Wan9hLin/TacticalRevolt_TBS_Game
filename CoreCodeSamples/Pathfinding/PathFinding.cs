using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public static PathFinding Instance { get; private set; }

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    [SerializeField] private Transform gridDebugObjPrefab;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private LayerMask coverLayerMask;

    private int width;
    private int height;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more than one PathFinding!" + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;

     
    }

    // Setup grid system with size and obstacles
    public void Setup(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridSystem = new GridSystem<PathNode>(width, height, cellSize,
         (GridPosition gridPosition, GridSystem<PathNode> g) => new PathNode(gridPosition));

        // Mark obstacles and cover as non-walkable
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                float ratcastOffsetDistance = 5f;
                if(Physics.Raycast(worldPosition + Vector3.down * ratcastOffsetDistance, 
                    Vector3.up, 
                    ratcastOffsetDistance * 2,
                    obstaclesLayerMask)
                    ||                     
                    Physics.Raycast(worldPosition + Vector3.down * ratcastOffsetDistance,
                    Vector3.up,
                    ratcastOffsetDistance * 2,
                    coverLayerMask))
                {
                    GetNode(x, z).SetIsWalkable(false);
                }
            }
        }
    }

    // Find the shortest path between two grid positions using A* algorithm
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);
        openList.Add(startNode);

        for (int x = 0; x< gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();
            }
        }

        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        // A* pathfinding loop
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);

            if(currentNode == endNode)
            {
                //Reached Final node
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach(PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.IsWalkable())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                // Calculate tentative G cost
                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());
                
                if(tentativeGCost < neighbourNode.GetGCost())
                {
                    // Update path node with better path
                    neighbourNode.SetCameFromPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

        }

        //No path found
        pathLength = 0;
        return null;


    }

    // Calculate distance using Manhattan distance (for grid movement)
    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        int totalDistance = Mathf.Abs(gridPositionDistance.x) + Mathf.Abs(gridPositionDistance.z);
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    // Get the path node with the lowest F cost
    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        for(int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }

        return lowestFCostPathNode;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }

    // Get neighboring nodes for a given node
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        if (gridPosition.x - 1 >= 0)
        {
            //Left 
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 0));

            if (gridPosition.z - 1 >= 0)
            {
                //Left Down
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < gridSystem.GetHeight())
            {
                //Left Up
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1));
            }
        }

        if (gridPosition.x + 1 < gridSystem.GetWidth())
        {
            //Right
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 0));

            if (gridPosition.z - 1 >= 0)
            {
                //Right Down
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < gridSystem.GetHeight())
            {
                //Right Up
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1));
            }
        }

        if (gridPosition.z - 1 >= 0)
        {
            //Down
            neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z - 1));
        }

        if (gridPosition.z + 1 < gridSystem.GetHeight())
        {
            //Up
            neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z + 1));
        }
        

        return neighbourList;
    }

    // Reconstruct path from end node to start node
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;

        while (currentNode.GetCameFromPathNode() != null)
        {
            pathNodeList.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
        }

        pathNodeList.Reverse();

        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach(PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());

        }

        return gridPositionList;
    }

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable)
    {
        gridSystem.GetGridObject(gridPosition).SetIsWalkable(isWalkable);
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).IsWalkable();
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}
