using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MindControlStatus : MonoBehaviour
{
    private int remainingTurns;
    private Unit unit;
    private GameObject mindControlEffect;

    private UnitWorldUI unitWorldUI;
    private Slider healthSlider;

    private Image fillImage;
    private Sprite OriginalFillSprite;
    private Sprite ControlFillSprite; // 新的图片资源

    public void Initialize(int duration)
    {
        remainingTurns = duration;
        unit = GetComponent<Unit>();

        ControlFillSprite = Resources.Load<Sprite>("ControlFillSprite");

        unitWorldUI = GetComponentInChildren<UnitWorldUI>();
        healthSlider = unitWorldUI.healthBarSlider;

        //获取Fill object
        Transform fillTransform = healthSlider.transform.Find("Fill_Area/Fill");

        // 获取 Fill 对象上的 Image 组件
        fillImage = fillTransform.GetComponent<Image>();
        OriginalFillSprite = fillImage.sprite;
        fillImage.sprite = ControlFillSprite;
        fillImage.color = Color.red;


        // 显示被控制的特效（可以添加特效代码）
        // 加载特效预制体
        GameObject effectPrefab = Resources.Load<GameObject>("MindControlEffect");
        if (effectPrefab != null)
        {
            // 在单位的位置实例化特效
            mindControlEffect = Instantiate(effectPrefab, unit.transform);
        }
        else
        {
            Debug.LogError("MindControlEffect prefab not found in Resources folder.");
        }
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        remainingTurns--;

        if (remainingTurns <= 0)
        {
            // 恢复单位控制权
            RestoreUnitControl();
        }
    }

    private void RestoreUnitControl()
    {
        // 移除敌人列表
        UnitManager.Instance.RemoveEnemyUnit(unit);
        // 添加到友方单位列表
        UnitManager.Instance.AddFriendlyUnit(unit);

        // 设置为友方单位
        unit.SetEnemy(false);

        fillImage.sprite = OriginalFillSprite;
        fillImage.color = Color.white;


        // 移除特效（如果有）
        if (mindControlEffect != null)
        {
            Destroy(mindControlEffect);
        }

        // 重置静态变量，允许再次使用精神控制
        MindControlAction.isAnyUnitMindControlled = false;

        // 销毁此脚本
        Destroy(this);
    }
}
