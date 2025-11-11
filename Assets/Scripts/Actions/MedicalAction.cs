using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MedicalAction : BaseAction
{
    [SerializeField] private int maxMedicalKits = 3;  // 最大医疗包数量
    private int currentMedicalKits;

    [SerializeField] private Transform dronePrefab;  // 无人机GameObject
    private Transform DroneChild;
    [SerializeField] private Transform healEffectPrefab;  // 治疗特效Prefab

    [SerializeField] private int healAmount = 40;  // 每次治疗恢复的生命值
    private int maxHealDistance = 10;  // 最大治疗范围

    protected override void Awake()
    {
        base.Awake();
        currentMedicalKits = maxMedicalKits;     
       
    }

    private void Update()
    {
       
    }

    public override string GetActionName()
    {
        return "Medical";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }

    //获取可治疗友军
    public override List<GridPosition> GetValidActionPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxHealDistance; x <= maxHealDistance; x++)
        {
            for (int z = -maxHealDistance; z <= maxHealDistance; z++)
            {
                GridPosition testGridPosition = unitGridPosition + new GridPosition(x, z);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue; // 如果网格位置非法，跳过该位置
                }

                //检查网格上是否有友军
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                    if (!targetUnit.IsEnemy() && targetUnit != unit) //只治疗友军，不包括自己
                    {
                        validGridPositionList.Add(testGridPosition);
                    }
                }
            }
        }


        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        if (currentMedicalKits <= 0)
        {
            unit.AddActionPointsForShootAction();//补偿消耗的actionpoints
            Debug.Log("No medical kits left !");
            ActionComplete();
            return;
        }

        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        if (targetUnit == null || targetUnit.IsEnemy())
        {
            Debug.Log("Invalid target!");
            ActionComplete();
            return;
        }

        currentMedicalKits--;
        unit.AddActionPointsForShootAction();//补偿消耗的actionpoints

        // 实例化无人机
        Transform droneTransform = Instantiate(dronePrefab, targetUnit.GetWorldPosition() + Vector3.up * 3.5f, Quaternion.identity);
        DroneChild = droneTransform.GetChild(0);
        DroneChild.gameObject.SetActive(false);

        // 开始淡入无人机的协程
       StartCoroutine(DroneFadeInAndHeal(droneTransform, targetUnit));
        AudioManager.Instance.Play("DroneFlying");


        ActionStart(onActionComplete);
    }

    private IEnumerator DroneFadeInAndHeal(Transform droneTransform, Unit targetUnit)
    {
        float fadeInDuration = 1f; // 调整淡入持续时间
        float fadeOutDuration = 1f; // 淡出持续时间
        float elapsedTime = 0f;

        // 获取无人机的所有Renderer组件
        Renderer[] droneRenderers = droneTransform.GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();

        foreach (Renderer renderer in droneRenderers)
        {
            // 获取每个Renderer的材质实例
            Material mat = renderer.material;
            materials.Add(mat);

            // 设置初始的 _Opacity 为 0（完全透明）
            mat.SetFloat("_Opacity", 0f);
        }


        // 淡入过程
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerp = Mathf.Clamp01(elapsedTime / fadeInDuration);
     
            float opacityValue = lerp;

            foreach (Material mat in materials)
            {
                mat.SetFloat("_Opacity", opacityValue);
            }

            yield return null; // 等待下一帧
        }

       

        // 确保最终的 _Opacity 为 1（完全不透明）
        foreach (Material mat in materials)
        {
            mat.SetFloat("_Opacity", 1f);
            
        }

        AudioManager.Instance.Play("Healing");

        // 显示治疗特效
        DroneChild.gameObject.SetActive(true);
        Transform healEffectTransform = Instantiate(healEffectPrefab, targetUnit.GetWorldPosition() + Vector3.up * 0.1f, Quaternion.identity);

        // 无人机完全显现后，等待1.5秒
        yield return new WaitForSeconds(1.5f);

        // 对目标单位进行治疗
        targetUnit.Heal(healAmount);
       

        // 模拟治疗特效持续时间，可以根据需要调整
        float healEffectDuration = 1f;
        yield return new WaitForSeconds(healEffectDuration);

        //禁用无人机的子对象
        DroneChild.gameObject.SetActive(false);
        AudioManager.Instance.Stop("DroneFlying");

        // 开始淡出过程
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerp = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            float opacityValue = Mathf.Lerp(1f, 0f, lerp); // 从1渐变到0

            foreach (Material mat in materials)
            {
                mat.SetFloat("_Opacity", opacityValue);
            }

            yield return null; // 等待下一帧
        }

        // 确保最终的 _Opacity 为 0（完全透明）
        foreach (Material mat in materials)
        {
            mat.SetFloat("_Opacity", 0f);
        }

        // 删除无人机和治疗特效
        Destroy(droneTransform.gameObject);
        Destroy(healEffectTransform.gameObject);

        ActionComplete();
    }
 

    
    // 医疗包是否可用,这个函数未来可能用到
    public bool HasMedicalKitsLeft()
    {
        return currentMedicalKits > 0;
    }

    public int GetCurrentMedicalKits()
    {
        return currentMedicalKits;
    }

}
