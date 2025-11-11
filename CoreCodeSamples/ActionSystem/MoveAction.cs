using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{

    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    private int maxMoveDistance = 8; // Maximum move distance for normal units
    private const int shortDistanceMax = 5; // Short distance (1-5 Manhattan distance)
    private const int longDistanceMin = 6; // Long distance (6-8 Manhattan distance)
    private GridPosition targetMoveGridPosition;

    private List<Vector3> positionList;
    private int currentPositionIndex;


    private void Start()
    {
        // Set max move distance for enemy units
        if (unit.IsEnemy())
        {
            SetEnemyMaxMoveDistance();
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        if (positionList == null || positionList.Count == 0)
        {
            // End action if no path exists
            Debug.LogError("Position list is empty in MoveAction.");
            OnStopMoving?.Invoke(this, EventArgs.Empty);
            ActionComplete();
            return;
        }

        Vector3 targetPosition = positionList[currentPositionIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;


        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

        float stoppingDistance = 0.1f;
        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
          
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
                   
        }
        else
        {
            currentPositionIndex++;
            if(currentPositionIndex >= positionList.Count)
            {
                OnStopMoving?.Invoke(this, EventArgs.Empty);

                if (EnemyAI.isInScoutMode)
                {                 
                    foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
                    {
                        if (IsPlayerInEnemyVision(enemyUnit))
                        {
                            EnemyAI.EndScoutMode();
                            break;
                        }
                    }
                }

                ActionComplete();
            }         
        }
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathgridPositionList = PathFinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);

        if (pathgridPositionList == null || pathgridPositionList.Count == 0)
        {
            // No valid path found, complete the action
            Debug.LogError("No path found in MoveAction.TakeAction.");
            ActionComplete();
            return;
        }

        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        // Convert path grid positions to world positions
        foreach (GridPosition pathGridPosition in pathgridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }
        
        OnStartMoving?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
  

    }

  

    public override List<GridPosition> GetValidActionPositionList()
    {       
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        // Check all positions within max movement range
        for (int x=-maxMoveDistance; x<= maxMoveDistance; x++)
        {
            for(int z =-maxMoveDistance; z<=maxMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (unitGridPosition == testGridPosition)
                {
                    //same grid position where the unit is already at
                    continue;
                }

                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    //Grid position already occupied with another unit
                    continue;
                }

                if (!PathFinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!PathFinding.Instance.HasPath(unitGridPosition, testGridPosition))
                {
                    continue;
                }

                int pathfindingDistanceMultiplier = 10;
                if(PathFinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier)
                {
                    //Path length is too long
                    continue;
                }
              

                validGridPositionList.Add(testGridPosition);
            }
        }
   
   

        return validGridPositionList;
    }

    public override int GetActionPointsCost()
    {
        if (!unit.IsEnemy())
        {
            targetMoveGridPosition = UnitActionSystem.Instance.GetMoveGridPosition();

            // Calculate action points based on movement distance
            GridPosition unitGridPosition = unit.GetGridPosition();
            int distance = Mathf.Abs(unitGridPosition.x - targetMoveGridPosition.x) + Mathf.Abs(unitGridPosition.z - targetMoveGridPosition.z);


            if (distance <= shortDistanceMax)
            {
                return 1; 
            }
            else if (distance >= longDistanceMin)
            {
                return 2; 
            }

        }

        // Enemies always cost 1 action point
        return base.GetActionPointsCost();
        
    }


    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPosition * 10,
        };
    }

    public int GetMaxMoveDistance()
    {
        return maxMoveDistance;
    }

    public int GetshortDistanceMax()
    {
        return shortDistanceMax;
    }

    // Check if a player is within the enemy's vision range
    private bool IsPlayerInEnemyVision(Unit enemyUnit)
    {
        int visionRange = 7;  // Set vision range
        List<GridPosition> enemyVisionGrids = LevelGrid.Instance.GetGridPositionInRange(enemyUnit.GetGridPosition(), visionRange);

        foreach (Unit playerUnit in UnitManager.Instance.GetFriendlyUnitList())
        {
            if (enemyVisionGrids.Contains(playerUnit.GetGridPosition()) && !playerUnit.CompareTag("Hostage"))
            {
                Debug.Log("Unit Enter!");
                return true;
            }
        }
        return false;
    }

    // Set the max move distance for different types of enemies
    private void SetEnemyMaxMoveDistance()
    {
        switch (unit.tag)
        {               
            case "NormalEnemy":
                maxMoveDistance = 6;
                break;
            case "CoverUse":
                maxMoveDistance = 8;
                break;
            case "Special":
                maxMoveDistance = 8;
                break;
            case "Hostage":
                maxMoveDistance = 50;
                break;
            default:
                maxMoveDistance = 6;
                break;
        }
        Debug.Log("Set Enemy Move distance success");
    }
}
