using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;

    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;


    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    // Abstract method to get the name of the action (e.g., "Move", "Shoot")
    public abstract string GetActionName();

    // Abstract method to perform the action (e.g., move to grid position, shoot)
    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    // Validate if the given grid position is valid for the action
    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionPositionList();
        return validGridPositionList.Contains(gridPosition);
    }


    public abstract List<GridPosition> GetValidActionPositionList();

    public virtual int GetActionPointsCost()
    {
        return 1;
    }

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit()
    {
        return unit;
    }

    // Get the best action for the enemy AI (highest action value)
    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        List<GridPosition> validActionGridPositionList = GetValidActionPositionList();
        
        foreach(GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);                     
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        }
        else
        {
            //No possible Enemy AI Actions
            return null;
        }
    }

    // Abstract method for AI to determine the best action at a given grid position
    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);
}

