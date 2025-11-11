using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrendAction : BaseAction
{
    [SerializeField] private Transform grenadeProjectilePrefab;
    [SerializeField] private Transform grenadePointTransform;

    private int maxThrowDistance = 8;

    public event EventHandler OnGrenade;
    public event EventHandler OnGrenadeEnded;

    [SerializeField] private int maxGrenadeKits = 3;  // 最大炸弹数量
    private int currentGrenadeKits;

    protected override void Awake()
    {
        base.Awake();
        currentGrenadeKits = maxGrenadeKits;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }
    }

    public override string GetActionName()
    {
        return "Grenade";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }

    public override List<GridPosition> GetValidActionPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxThrowDistance; x <= maxThrowDistance; x++)
        {
            for (int z = -maxThrowDistance; z <= maxThrowDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Math.Abs(x) + Math.Abs(z);
                if (testDistance > maxThrowDistance)
                {
                    continue;
                }                

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        if (currentGrenadeKits <= 0)
        {
            unit.AddActionPointsForShootAction();//补偿消耗的actionpoints
            Debug.Log("No grenade kits left !");
            ActionComplete();
            return;
        }

        currentGrenadeKits--;
        unit.AddActionPointsForShootAction();//补偿消耗的actionpoints

        Transform grenadeProjectileTransform = Instantiate(grenadeProjectilePrefab, grenadePointTransform.position, Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeBehaviorComplete);

        Debug.Log("GrendAction");
    
        //朝向投掷炸弹的grid方向
        Vector3 targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        Vector3 aimDir = (targetPosition - unit.GetWorldPosition()).normalized;
        float rotateSpeed = 200f;
        transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);

        //播放动画
        OnGrenade?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }

    private void OnGrenadeBehaviorComplete()
    {
        OnGrenadeEnded?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }

    public int GetCurrentGrenadeKits()
    {
        return currentGrenadeKits;
    }
}


