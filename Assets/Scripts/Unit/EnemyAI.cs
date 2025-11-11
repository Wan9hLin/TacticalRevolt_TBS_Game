using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        WaitingForEnemyTurn,
        TakingTurn,
        Busy,
    }

    private State state;
    private float timer;
    public static bool isInScoutMode = true;  
    private const float scoutModeWaitTime = 1.5f; 

    public static event EventHandler OnScoutModeEnded;


    private void Awake()
    {
        state = State.WaitingForEnemyTurn;
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void Update()
    {      
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        // Handle scout mode actions
        if (isInScoutMode)
        {          
            if (state == State.TakingTurn)
            {
                state = State.Busy;
                timer = scoutModeWaitTime;  // Set scout mode wait time
            }
            else if (state == State.Busy)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    TurnSystem.Instance.NextTurn();
                }
            }

            return;
        }

        switch (state)
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.TakingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {

                    if (TryTakeEnemeyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        // No more enemies have actions they can take, end enemy turn
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                break;
        }


    }

    private void SetStateTakingTurn()
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }

    public static void EndScoutMode()
    {
        isInScoutMode = false;  
        GridSystemVisual.Instance.UpdateGridVisual();  
        Debug.Log("Scout Mode has been Exit.");
        OnScoutModeEnded?.Invoke(null, EventArgs.Empty);
    }

    public static void ResetScoutMode()
    {
        isInScoutMode = true;
        Debug.Log("Scout Mode has been reset.");
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TakingTurn;
            timer = 2f;

            if (!isInScoutMode)
            {
                AudioManager.Instance.Play("Level1AlienTurn");
            }

        }

    }

    private bool TryTakeEnemeyAIAction(Action onEnemyAIActionComplete)
    {
        Debug.Log("Take emeny AI action");

        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if (enemyUnit.CompareTag("CoverUse") || enemyUnit.CompareTag("Special"))
            {
                if (TryTakeCoverUseEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
                {
                    return true;
                }
            }

            else
            {
                if (TryTakeStandardEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
                {
                    return true;
                }
            }

        }

        return false;
    }

    // Standard enemy AI actions
    private bool TryTakeStandardEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {      
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;

        foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {
            if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                //Enemy cannot afford this action
                continue;
            }

            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            }
            else
            {
                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
            }
        }

        if (bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction))
        {
            bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition, onEnemyAIActionComplete);
            return true;
        }

        else
        {
            return false;
        }

    }

    // Advanced enemy AI actions
    private bool TryTakeCoverUseEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        // Check for available mind control action
        MindControlAction mindControlAction = enemyUnit.GetAction<MindControlAction>();
        if (mindControlAction != null && !MindControlAction.isAnyUnitMindControlled && mindControlAction.IsAvailable())
        {
            if (enemyUnit.CanSpendActionPointsToTakeAction(mindControlAction))
            {
                List<GridPosition> validActionPositions = mindControlAction.GetValidActionPositionList();
                if (validActionPositions.Count > 0)
                {
                    // Choose the best mind control target
                    EnemyAIAction bestMindControlAction = mindControlAction.GetBestEnemyAIAction();
                    if (bestMindControlAction != null)
                    {
                        if (enemyUnit.TrySpendActionPointsToTakeAction(mindControlAction))
                        {
                            mindControlAction.TakeAction(bestMindControlAction.gridPosition, onEnemyAIActionComplete);
                            return true;
                        }
                    }
                }
            }
        }

        // Attack action if no mind control available
        if (enemyUnit.CanSpendActionPointsToTakeAction(enemyUnit.GetAction<ShootAction>()))
        {
            List<GridPosition> validShootPositions = enemyUnit.GetAction<ShootAction>().GetValidActionPositionList();
            if (validShootPositions.Count > 0)
            {
                // Choose the best attack target
                EnemyAIAction bestShootAction = enemyUnit.GetAction<ShootAction>().GetBestEnemyAIAction();
                if (bestShootAction != null)
                {
                    if (enemyUnit.TrySpendActionPointsToTakeAction(enemyUnit.GetAction<ShootAction>()))
                    {
                        enemyUnit.GetAction<ShootAction>().TakeAction(bestShootAction.gridPosition, onEnemyAIActionComplete);
                        return true;
                    }
                }
            }
        }

        // Move towards cover if attack isn't possible
        if (enemyUnit.CanSpendActionPointsToTakeAction(enemyUnit.GetAction<MoveAction>()))
        {
            bool foundCover;
            GridPosition bestMovePosition = FindBestCoverPosition(enemyUnit, out foundCover);

            if (foundCover)
            {
                if (enemyUnit.TrySpendActionPointsToTakeAction(enemyUnit.GetAction<MoveAction>()))
                {
                    enemyUnit.GetAction<MoveAction>().TakeAction(bestMovePosition, onEnemyAIActionComplete);
                    return true;
                }
            }
            else
            {
                // Move towards the nearest player if no cover is found
                GridPosition closestPlayerPosition = GetClosestPlayerUnitPosition(enemyUnit.GetGridPosition());
                List<GridPosition> pathToPlayer = PathFinding.Instance.FindPath(enemyUnit.GetGridPosition(), closestPlayerPosition, out int pathLength);
                if (pathToPlayer != null && pathToPlayer.Count > 1)
                {
                    GridPosition moveToPosition = pathToPlayer[1]; 
                    if (enemyUnit.GetAction<MoveAction>().IsValidActionGridPosition(moveToPosition))
                    {
                        if (enemyUnit.TrySpendActionPointsToTakeAction(enemyUnit.GetAction<MoveAction>()))
                        {
                            enemyUnit.GetAction<MoveAction>().TakeAction(moveToPosition, onEnemyAIActionComplete);
                            return true;
                        }
                    }
                }
            }
        }

        // No valid action to take
        return false;
    }


    // Find the best cover position for the enemy unit
    private GridPosition FindBestCoverPosition(Unit enemyUnit, out bool foundCover)
    {
        List<GridPosition> validMovePositions = enemyUnit.GetAction<MoveAction>().GetValidActionPositionList();
        GridPosition bestCoverPosition = new GridPosition(0, 0);
        int bestCoverScore = int.MinValue;
        foundCover = false;

        Unit closestPlayerUnit = GetClosestPlayerUnit(enemyUnit.GetGridPosition());
        int currentDistanceToPlayer = GetDistanceToUnit(enemyUnit.GetGridPosition(), closestPlayerUnit);

        foreach (GridPosition movePosition in validMovePositions)
        {
            int coverScore = 0;
            int newDistanceToPlayer = GetDistanceToUnit(movePosition, closestPlayerUnit);

            if (newDistanceToPlayer > currentDistanceToPlayer)
            {
                continue; // Skip if the move brings the enemy closer to the player

            }

            // Check if the position has cover
            if (LevelGrid.Instance.IsGridPositionInCover(movePosition, out string coverType, out string coverDirection))
            {
                if (IsCoverEffective(movePosition, coverDirection, closestPlayerUnit))
                {
                    coverScore += coverType == "FullCover" ? 100 : 50;  
                }
                else
                {
                    coverScore += coverType == "FullCover" ? 2 : 1;
                }
            }
            else
            {
                continue;
            }

            coverScore -= newDistanceToPlayer; // Penalize moves closer to the player

            if (coverScore > bestCoverScore)
            {
                bestCoverScore = coverScore;
                bestCoverPosition = movePosition;
                foundCover = true;
            }
        }

        return bestCoverPosition;
    }

    // Get the closest player unit's grid position
    private GridPosition GetClosestPlayerUnitPosition(GridPosition fromPosition)
    {
        GridPosition closestPosition = new GridPosition(0, 0);
        int shortestDistance = int.MaxValue;
        foreach (Unit playerUnit in UnitManager.Instance.GetFriendlyUnitList())
        {
            int distance = GridPosition.CalculateDistance(fromPosition, playerUnit.GetGridPosition());
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestPosition = playerUnit.GetGridPosition();
            }
        }
        return closestPosition;
    }

    // Convert cover direction string to vector
    private Vector3 DirectionStringToVector(string direction)
    {
        switch (direction)
        {
            case "North":
                return Vector3.back;
            case "South":
                return Vector3.forward;
            case "East":
                return Vector3.left;
            case "West":
                return Vector3.right;
            default:
                return Vector3.zero;
        }
    }

    // Get the closest player unit from a given position
    private Unit GetClosestPlayerUnit(GridPosition fromPosition)
    {
        Unit closestUnit = null;
        int shortestDistance = int.MaxValue;
        foreach (Unit playerUnit in UnitManager.Instance.GetFriendlyUnitList())
        {
            int distance = GridPosition.CalculateDistance(fromPosition, playerUnit.GetGridPosition());
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestUnit = playerUnit;
            }
        }
        return closestUnit;
    }


    private int GetDistanceToUnit(GridPosition fromPosition, Unit unit)
    {
        return GridPosition.CalculateDistance(fromPosition, unit.GetGridPosition());
    }

    // Check if the cover is effective in blocking the player's line of sight
    private bool IsCoverEffective(GridPosition movePosition, string coverDirection, Unit closestPlayerUnit)
    {
        Vector3 coverDirVector = DirectionStringToVector(coverDirection);
        Vector3 movePositionWorld = LevelGrid.Instance.GetWorldPosition(movePosition);
        Vector3 playerPositionWorld = closestPlayerUnit.GetWorldPosition();
        Vector3 directionToPlayer = (playerPositionWorld - movePositionWorld).normalized;

        float dotProduct = Vector3.Dot(coverDirVector, directionToPlayer);

        // Effective cover if the dot product is below the threshold
        return dotProduct < -0.5f; 
    }

}