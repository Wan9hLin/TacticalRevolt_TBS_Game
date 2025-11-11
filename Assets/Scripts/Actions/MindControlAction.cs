using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindControlAction : BaseAction
{
    public static event EventHandler<OnMindControlEventArgs> OnAnyMindControl;

    public class OnMindControlEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit controllingUnit;
        public MindControlAction mindControlAction; // 新增
    }

    private enum State
    {
        Aiming,
        Casting,
        WaitingForAnimation, // 新增
        Cooldown,
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private bool canCastSpell;
    private int maxUseCount = 1;
    private int currentUseCount = 0;
    private int maxMindControlRange = 8;

    public static bool isAnyUnitMindControlled = false;
    private int mindControlDuration = 2; // 持续两个敌人回合
    private int remainingMindControlTurns = 0;

    protected override void Awake()
    {
        base.Awake();
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
                HandleAiming();
                break;
            case State.Casting:
                if (canCastSpell)
                {
                    CastMindControl();
                    canCastSpell = false;
                }
                break;
            case State.WaitingForAnimation:
                // 等待动画完成，不执行任何操作
                break;
            case State.Cooldown:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Casting;
                stateTimer = 0.5f;
                break;
            case State.Casting:
                // 不再自动转换到 Cooldown，等待动画完成后再转换
                break;
            case State.Cooldown:
                ActionComplete();
                break;
        }
    }


    public override string GetActionName()
    {
        return "Mind Control";
    }

    public override List<GridPosition> GetValidActionPositionList()
    {
        if (currentUseCount >= maxUseCount)
        {
            return new List<GridPosition>(); // 返回空列表
        }

        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxMindControlRange; x <= maxMindControlRange; x++)
        {
            for (int z = -maxMindControlRange; z <= maxMindControlRange; z++)
            {
                GridPosition testGridPosition = unitGridPosition + new GridPosition(x, z);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Math.Abs(x) + Math.Abs(z);
                if (testDistance > maxMindControlRange)
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                // 添加以下检查，确保潜在目标不为空
                if (targetUnit == null)
                {
                    continue;
                }

                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    continue;
                }

                // 添加以下检查，排除带有 "hostage" 和 "scientist" 标签的单位
                if (targetUnit.CompareTag("Hostage") || targetUnit.CompareTag("Scientist"))
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
        if (currentUseCount >= maxUseCount)
        {
            ActionComplete();
            return;
        }

        if (isAnyUnitMindControlled)
        {
            ActionComplete();
            return;
        }

        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        // 添加以下检查，确保目标单位不为空
        if (targetUnit == null)
        {
            ActionComplete();
            return;
        }

        state = State.Aiming;
        stateTimer = 1f; // 瞄准时间

        canCastSpell = true;

        ActionStart(onActionComplete);
    }

    private void HandleAiming()
    {
        Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
    }

    private void CastMindControl()
    {
        // 触发动画和特效
        OnAnyMindControl?.Invoke(this, new OnMindControlEventArgs
        {
            targetUnit = targetUnit,
            controllingUnit = unit,
            mindControlAction = this, // 传递自身引用
        });

        // 转换到等待动画完成的状态
        state = State.WaitingForAnimation;


    }

    public void OnMindControlAnimationComplete()
    {
        currentUseCount++;
        isAnyUnitMindControlled = true;
        remainingMindControlTurns = mindControlDuration;

        // 将目标单位转换为敌人
        targetUnit.SetEnemy(true);

        // 添加 MindControlStatus 脚本，处理持续时间
        targetUnit.gameObject.AddComponent<MindControlStatus>().Initialize(remainingMindControlTurns);

        // 从友方单位列表中移除
        UnitManager.Instance.RemoveFriendlyUnit(targetUnit);
        // 添加到敌人单位列表中
        UnitManager.Instance.AddEnemyUnit(targetUnit);

        // 转换到 Cooldown 状态
        state = State.Cooldown;
        stateTimer = 0.5f;
    }



    public override int GetActionPointsCost()
    {
        return 2;
    }


    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        if (currentUseCount >= maxUseCount)
        {
            return null;
        }

        Unit potentialTarget = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        // 添加以下检查，确保潜在目标不为空
        if (potentialTarget == null)
        {
            return null;
        }

        // 排除 "hostage" 和 "scientist" 标签的单位
        if (potentialTarget.CompareTag("Hostage") || potentialTarget.CompareTag("Scientist"))
        {
            return null;
        }

        int actionValue = Mathf.RoundToInt(100 * potentialTarget.GetHealthNormalized());

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = actionValue,
        };
    }

    public bool IsAvailable()
    {
        return currentUseCount < maxUseCount;
    }



}
