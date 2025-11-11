using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    private const int ACTION_POINTS_MAX = 2;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDied;
    public static event EventHandler OnAnyUnitDamaged;

    [SerializeField] private bool isEnemy;

    private GridPosition gridPosition;
    private HealthSystem healthSystem;
    private BaseAction[] baseActionArray;
    private int actionPoints = ACTION_POINTS_MAX;
    private Animator animator;
    private Unit unit;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        baseActionArray = GetComponents<BaseAction>();
        animator = GetComponentInChildren<Animator>();
        unit = GetComponent<Unit>();
    }

 
    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        healthSystem.OnDead += HealthSystem_OnDead;
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);

        healthSystem.OnDamaged += healthSystem_OnDamaged;
       
    }

  

    private void Update()
    {
        // Check for grid position change and update accordingly
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if(newGridPosition != gridPosition)
        {

            GridPosition oldgridPosition = gridPosition;
            gridPosition = newGridPosition;

            LevelGrid.Instance.UnitMoveGridPosition(this, oldgridPosition, newGridPosition);

        }

    }

    // Get a specific type of action
    public T GetAction<T>() where T : BaseAction
    {
        foreach(BaseAction baseAction in baseActionArray)
        {
            if(baseAction is T)
            {
                return (T)baseAction;
            }
        }
        return null;
    }


    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    // Get array of actions that the unit can perform
    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    // Try to spend action points for an action
    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (unit.CompareTag("Hostage"))
        {
            return true;
        }

        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if(actionPoints >= baseAction.GetActionPointsCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;

        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }


    // Add extra action point for shooting action
    public void AddActionPointsForShootAction()
    {
        actionPoints++;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    // Event handler for turn change to reset action points
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) || 
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = ACTION_POINTS_MAX;

            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    public void SetEnemy(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }


    public void Damage(int damageAmount)
    {
        healthSystem.Damage(damageAmount);
    }

    public void Heal(int healAmount)
    {
        healthSystem.Heal(healAmount);
        Debug.Log($"Healed {healAmount} health points.");
    }

    // Event handler for unit death
    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        Destroy(gameObject);

        OnAnyUnitDied?.Invoke(this, EventArgs.Empty);

        if (unit.IsEnemy())
        {
            AudioManager.Instance.Play("AlienDeath");
        }
        else
        {
            if (gameObject.CompareTag("Medic"))
            {
                AudioManager.Instance.Play("WomanDeath");
            }
            else
            {
                AudioManager.Instance.Play("ManDeath");
            }
        }

  
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
    }

    public float GetHealth()
    {
        return healthSystem.GetHealth();
    }

    // Check if the unit is in cover (valid grid position)
    public bool IsInCover()
    {
        string coverType;
        string coverDirection;
        return LevelGrid.Instance.IsGridPositionInCover(GetGridPosition(), out coverType, out coverDirection);
    }

    // Get the cover type at the unit's current position
    public string GetCoverType()
    {
        LevelGrid.Instance.IsGridPositionInCover(GetGridPosition(), out string coverType, out _);
        return coverType;
    }

    public Animator GetAnimator()
    {
        if(animator != null)
        {
            return animator;
        }
        else
        {
            Debug.LogError("Animator is null");
            return null;
        }
    }

    private void healthSystem_OnDamaged(object sender, EventArgs e)
    {
        OnAnyUnitDamaged?.Invoke(this, EventArgs.Empty);
    }
  


}
