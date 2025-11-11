using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    public static event EventHandler<OnShootEventArgs> OnAnyShoot;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public event EventHandler<OnShootEventArgs> OnShootDamaged;
    public event EventHandler<OnShootEventArgs> OnShootMissed;


    public static event EventHandler<float> OnAnyAimingStarted;  
    public static event EventHandler OnAnyAimingEnded;  

    public event EventHandler OnAimingStarted;
    public event EventHandler OnAimingEnded;


    private bool isHitPass;
    private bool hasAimedStarted = false;

    // Event arguments to hold shooting unit and target info
    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
        public int bulletsPerShot; 
    }

    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }

    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private LayerMask coverLayerMask;

    private State state;
    private int maxShootDistance;

    private float stateTimer;
    private Unit targetUnit;
    private bool canShootBullet;
    private float hitChance; 

    private void Start()
    {
        SetMaxShootDistance();
    }

    private void Update()
    {

        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                HandleAimingState();
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }

    }

    private void HandleAimingState()
    {
        if (!hasAimedStarted)
        {
            hasAimedStarted = true;
            OnAimingStarted?.Invoke(this, EventArgs.Empty);

            if (!unit.IsEnemy())
            {
                AudioManager.Instance.Play("CrossHair");
            }
          
        }

        Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);


        // Show crosshair for player target
        UnitWorldUI targetUnitWorldUI = targetUnit.GetComponentInChildren<UnitWorldUI>();
        if (targetUnitWorldUI != null)
        {
            targetUnitWorldUI.ShowCrosshair();
        }

        if (!unit.IsEnemy())
        {
            // Notify aiming start with hit chance
            OnAnyAimingStarted?.Invoke(this, hitChance);

            if (Input.GetKeyDown(KeyCode.Return)) // Enter to confirm shot
            {
                state = State.Shooting;
                stateTimer = 0.1f;

                // Aiming ends
                OnAnyAimingEnded?.Invoke(this, EventArgs.Empty);

                // Hide crosshair
                if (targetUnitWorldUI != null)
                {
                    targetUnitWorldUI.HideCrosshair();
                }

                hasAimedStarted = false;
            }
            else if (Input.GetMouseButtonDown(1)) // Right click to cancel aim
            {
                OnAimingEnded?.Invoke(this, EventArgs.Empty);
                unit.AddActionPointsForShootAction();

                if (targetUnitWorldUI != null)
                {
                    targetUnitWorldUI.HideCrosshair();
                }

                OnAnyAimingEnded?.Invoke(this, EventArgs.Empty);

                hasAimedStarted = false;

                ActionComplete();
                return;
            }
        }
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                if (unit.IsEnemy()) 
                {
                    state = State.Shooting;
                    float shootingStateTime = 0.1f;
                    stateTimer = shootingStateTime;
                }               
                break;
            case State.Shooting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                OnAnyAimingEnded?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }

        if (state != State.Aiming)
        {
            hasAimedStarted = false;
        }

    }

    private void Shoot()
    {
        // Calculate hit chance
        bool isHit = RollForHit();
        SetitHitToPass(isHit);

        UnitWorldUI targetUnitWorldUI = targetUnit.GetComponentInChildren<UnitWorldUI>();

        if (isHit)
        {
            Debug.Log("Hit!");  // Apply damage based on the unit type
            if (unit.CompareTag("SwordEnemy"))
            {
                targetUnit.Damage(70);
            }
            else if(unit.CompareTag("NormalEnemy"))
            {
                targetUnit.Damage(30);
            }
            else
            {
                targetUnit.Damage(40);
            }

            if (targetUnitWorldUI != null)
            {
                targetUnitWorldUI.ShowHit();  

                OnShootDamaged?.Invoke(this, new OnShootEventArgs
                {
                    targetUnit = targetUnit,
                    shootingUnit = unit,

                });
            }

        }
        else
        {
            Debug.Log("Miss!"); 
            if (targetUnitWorldUI != null)
            {
                targetUnitWorldUI.ShowMiss(); 

                OnShootMissed?.Invoke(this, new OnShootEventArgs
                {
                    targetUnit = targetUnit,
                    shootingUnit = unit,

                });
            }

        }

        // End scout mode regardless of hit or miss
        if (EnemyAI.isInScoutMode)
        {
            EnemyAI.EndScoutMode();
        }

        if (targetUnitWorldUI != null)
        {
            targetUnitWorldUI.HideCrosshair();
        }


        OnAnyShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit,

        });

        int bulletsPerShot = GetBulletsPerShot(); 

        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit,
            bulletsPerShot = bulletsPerShot,
        });
    }

    public bool RollForHit()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        Debug.Log($"Hit Chance: {hitChance}%");
        return randomValue <= hitChance;
    }

    // Calculate base hit chance based on unit type
    private float GetBaseHitChance(Unit shootingUnit)
    {
        switch (shootingUnit.tag)
        {
            case "Commando":  
                return 75f;
            case "Sniper":    
                return 85f;
            case "Heavy":     
                return 65f;
            case "Medic":     
                return 70f;
       
            
            case "NormalEnemy":
                return 70f;
            case "CoverUse":
                return 75f;
            case "Special": 
                return 75f;
            case "SwordEnemy": 
                return 90f;
            default:
                return 75f;
        }
    }

    // Distance modifier for hit chance
    private float GetDistanceModifier(Unit shootingUnit, Unit targetUnit)
    {
        GridPosition shooterGridPos = shootingUnit.GetGridPosition();
        GridPosition targetGridPos = targetUnit.GetGridPosition();

        int distance = Mathf.Abs(shooterGridPos.x - targetGridPos.x) +
            Mathf.Abs(shooterGridPos.z - targetGridPos.z);

        Debug.Log("Distance Modifier£º " + distance);

        if (shootingUnit.CompareTag("Commando"))
        {         
            if (distance >= 1 && distance <= 2) return 20f;
            if (distance >= 3 && distance <= 5) return 10f;
            if (distance >= 6 && distance <= 7) return -10f;
            if (distance == 8) return -25f;
        }

        else if (shootingUnit.CompareTag("Sniper"))
        {
            if (distance >= 1 && distance <= 3) return -30f;
            if (distance >= 4 && distance <= 6) return -15f;
            if (distance >= 7 && distance <= 9) return 5f;
            if (distance >= 10 && distance <= 12) return 15f;
        }

        else if (shootingUnit.CompareTag("Heavy"))
        {
            if (distance >= 1 && distance <= 2) return -20f;
            if (distance >= 3 && distance <= 5) return 10f;
            if (distance >= 6 && distance <= 8) return 15f;
            if (distance >= 9 && distance <= 10) return -15f;
        }

        else if (shootingUnit.CompareTag("Medic"))
        {
            if (distance >= 1 && distance <= 2) return -15f;
            if (distance >= 3 && distance <= 5) return 5f;
            if (distance >= 6 && distance <= 7) return 15f;
            if (distance == 8) return -10f;
        }

        else if (shootingUnit.CompareTag("NormalEnemy"))
        {
            if (distance >= 1 && distance <= 2) return 15f;
            if (distance >= 3 && distance <= 5) return 5f;
            if (distance >= 6 && distance <= 7) return -10f;
            if (distance == 8) return -20f;
        }

        else if (shootingUnit.CompareTag("CoverUse"))
        {
            if (distance >= 1 && distance <= 2) return -10f;
            if (distance >= 3 && distance <= 5) return 10f;
            if (distance >= 6 && distance <= 8) return 15f;
            if (distance >= 9 && distance <= 10) return 0f;
        }
        else if (shootingUnit.CompareTag("Special"))
        {
            if (distance >= 1 && distance <= 2) return -10f;
            if (distance >= 3 && distance <= 5) return 10f;
            if (distance >= 6 && distance <= 8) return 15f;
            if (distance >= 9 && distance <= 10) return 0f;
        }


        return 0f; 
    }

    // Cover modifier for hit chance
    private float GetCoverModifier(Unit shootingUnit, Unit targetUnit)
    {
        if (targetUnit.IsInCover())
        {
            Vector3 shootDirection = (targetUnit.GetWorldPosition() - shootingUnit.GetWorldPosition()).normalized;
            float rayDistance = Vector3.Distance(shootingUnit.GetWorldPosition(), targetUnit.GetWorldPosition());
            RaycastHit hit;

            // Determine whether there is cover between the two unit
            if (Physics.Raycast(shootingUnit.GetWorldPosition() + Vector3.up * 1f, shootDirection, out hit, rayDistance, coverLayerMask))
            {
                if (hit.collider.CompareTag("FullCover"))
                {
                    return -30f; // Full cover, hit rate reduced by 30%
                }
                else if (hit.collider.CompareTag("HalfCover"))
                {
                    return -15f; // Half cover, hit rate reduced by 15%
                }
            }

        }

        return 0f;
    }


    // Calculate the hit rate based on the above three functions
    private float CalculateHitChance(Unit shootingUnit, Unit targetUnit)
    {
        float baseHitChance = GetBaseHitChance(shootingUnit);

        float distanceModifier = GetDistanceModifier(shootingUnit, targetUnit);

        float coverModifier = GetCoverModifier(shootingUnit, targetUnit);

        //final hit rate
        float finalHitChance = Mathf.Clamp(baseHitChance + distanceModifier + coverModifier, 5f, 100f);

        if (targetUnit.CompareTag("CoreDevice"))
        {
            finalHitChance = 100f;
        }

        Debug.Log($"Hit Chance: {finalHitChance}% (Base: {baseHitChance}%, Distance: {distanceModifier}%, Cover: {coverModifier}%)");
        return finalHitChance;
    }

    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionPositionList(unitGridPosition);
    }


    public List<GridPosition> GetValidActionPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Math.Abs(x) + Math.Abs(z);
                if (testDistance > maxShootDistance)
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    //Grid position is empty, no unit
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    //Both unit on the same 'team'
                    continue;
                }

                if (targetUnit.CompareTag("Hostage") && !unit.CompareTag("NormalEnemy"))
                {
                    continue;
                }

                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;


                float unitShoulderHeight = 1.7f;
                if (Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight,
                    shootDir,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstacleLayerMask))
                {
                    //Blocked by an obstacle
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        if (unit.CompareTag("SwordEnemy"))
        {
            maxShootDistance = 1;
        }

        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        hitChance = CalculateHitChance(unit, targetUnit);

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;

        canShootBullet = true;

        ActionStart(onActionComplete);

    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public int GetMaxShootDistance()
    {
        return maxShootDistance;
    }

    // Set max shooting distance based on unit type
    private void SetMaxShootDistance()
    {
        switch (unit.tag)
        {
            case "Commando":
                maxShootDistance = 8;
                break;
            case "Sniper":
                maxShootDistance = 12;
                break;
            case "Medic":
                maxShootDistance = 8;
                break;
            case "Heavy":
                maxShootDistance = 10;
                break;


            case "NormalEnemy":
                maxShootDistance = 8;
                break;
            case "CoverUse":
                maxShootDistance = 10; 
                break;
            case "Speical": 
                maxShootDistance = 10;
                break;
            case "SwordEnemy": 
                maxShootDistance = 1;
                break;

            default:
                maxShootDistance = 8;
                break;
        }
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            // Action value is determined by health and hit rate factors        
            actionValue = Mathf.RoundToInt(100 * (1 - targetUnit.GetHealthNormalized()) + 100 * (CalculateHitChance(unit, targetUnit) / 100)),
        };

    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionPositionList(gridPosition).Count;
    }

    private void SetitHitToPass(bool isHit)
    {
        isHitPass = isHit;
    }

    public bool GetHitToPass()
    {
        return isHitPass;
    }

    // Get bullets per shot based on unit
    private int GetBulletsPerShot()
    {
        switch (unit.tag)
        {
            case "Commando":
                return 4; 
            case "Sniper":
                return 1; 
            case "Medic":
                return 3; 
            case "Heavy":
                return 4; 
           
                //µÐÈËµÄ
            case "NormalEnemy":
                return 4;
            case "CoverUse":
                return 4;
            case "Special":
                return 3;
            default:
                return 1; 
        }
    }
}
