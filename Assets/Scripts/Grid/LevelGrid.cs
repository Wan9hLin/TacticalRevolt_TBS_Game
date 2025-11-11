using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }


    public event EventHandler<OnAnyUnitMovedGridPositionEventArgs> OnAnyUnitMoveGridPosition;
    public class OnAnyUnitMovedGridPositionEventArgs : EventArgs
    {
        public Unit unit;
        public GridPosition fromGridPosition;
        public GridPosition toGridPosition;
    }


    [SerializeField] private Transform gridDebugObjPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    private GridSystem<GridObject> gridSystem;
    [SerializeField] private LayerMask coverLayerMask;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more than one LevelGrid!" + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gridSystem = new GridSystem<GridObject>(width, height, cellSize,
                (GridPosition gridPosition, GridSystem<GridObject> g) => new GridObject(gridPosition, g));
          //gridSystem.CreateDebugObjects(gridDebugObjPrefab);
    }


    private void Start()
    {
        PathFinding.Instance.Setup(width, height, cellSize);
    }


    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }


    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }


    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    // Move a unit from one grid position to another
    public void UnitMoveGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);
        OnAnyUnitMoveGridPosition?.Invoke(this, new OnAnyUnitMovedGridPositionEventArgs
        {
            unit = unit,
            fromGridPosition = fromGridPosition,
            toGridPosition = toGridPosition,
        });
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);
    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);
    public int GetWidth() => gridSystem.GetWidth();
    public int GetHeight() => gridSystem.GetHeight();
    public float GetCellSize() => gridSystem.GetCellSize();


    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    // Set interactable object at the specified grid position
    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    // Determines whether a unit is on the grid near the cover
    public bool IsGridPositionInCover(GridPosition gridPosition, out string coverType, out string coverDirection)
    {
        Vector3 gridWorldPosition = GetWorldPosition(gridPosition);
        float raycastHeight = 1f; // Cover Detection
        float raycastDistance = 2f; // Detect surrounding cover range

        // Raycast from the grid in four directions
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        string[] directionNames = { "North", "South", "West", "East" };

        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics.Raycast(gridWorldPosition + Vector3.up * raycastHeight, directions[i], raycastDistance, coverLayerMask))
            {
                RaycastHit hit;
                if (Physics.Raycast(gridWorldPosition + Vector3.up * raycastHeight, directions[i], out hit, raycastDistance, coverLayerMask))
                {
                    // Determine whether to hit the cover
                    if (hit.collider.CompareTag("HalfCover"))
                    {
                        coverType = "HalfCover";
                        coverDirection = directionNames[i];
                        return true; 
                    }
                    else if (hit.collider.CompareTag("FullCover"))
                    {
                        coverType = "FullCover";
                        coverDirection = directionNames[i];
                        return true; 
                    }
                }
            }
        }
      
        coverType = null;
        coverDirection = null;
        return false; 
    }

    // Get all grid positions within a given range
    public List<GridPosition> GetGridPositionInRange(GridPosition startPosition, int range)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = startPosition + new GridPosition(x, z);

                if (!IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Math.Abs(x) + Math.Abs(z);
                if (testDistance > range)
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }

        return gridPositionList;
    }



}
