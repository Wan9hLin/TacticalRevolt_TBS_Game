using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitActionSysUI : MonoBehaviour
{
    [SerializeField] private Transform actionBtnPrefab;
    [SerializeField] private Transform actionBtnContainerTransform;
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private TextMeshProUGUI healthNumberText;


    //新的引用：shootaction确认的UI和显示的概率字体
    [SerializeField] private Transform shootActionUI;
    [SerializeField] private TextMeshProUGUI hitRateText;
    [SerializeField] private Transform isBusyUI;

    //新的引用：unit的detail pannel引用
    [SerializeField] private GameObject commonDetailPanel;
    [SerializeField] private GameObject medicalDetailPanel;
    [SerializeField] private GameObject grenadeDetailPanel;
    [SerializeField] private GameObject sniperDetailPanel;
    [SerializeField] private GameObject scientistDetailPanel;


    private List<ActionBtnUI> actionButtonUIList;


    private void Awake()
    {
        actionButtonUIList = new List<ActionBtnUI>();
        // 订阅 ShootAction 的事件
        ShootAction.OnAnyAimingStarted += ShootAction_OnAnyAimingStarted;
        ShootAction.OnAnyAimingEnded += ShootAction_OnAnyAimingEnded;


    }

    private void OnDestroy()
    {
        // 在对象销毁时取消订阅，避免内存泄漏
        ShootAction.OnAnyAimingStarted -= ShootAction_OnAnyAimingStarted;
        ShootAction.OnAnyAimingEnded -= ShootAction_OnAnyAimingEnded;

        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;
    }

 
    private void Start()
    {     
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnselectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnselectedActionChanged;
        UnitActionSystem.Instance.OnActionStrated += UnitActionSystem_OnActionStrated;
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
      

        // 监听行动完成事件
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

        UpdateActionPoints();
        CreateUnitActionBtns();
        UpdateSelectedVisual();
        UpdateDetailPanel(); // panel更新
    }

   

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        // 行动完成后更新按钮显示
        CreateUnitActionBtns();
        UpdateSelectedVisual();
    }

    private void ShootAction_OnAnyAimingStarted(object sender, float hitChance)
    {
        // 显示 ShootAction UI 并更新命中率
        ShowShootActionUI(hitChance);
        
        isBusyUI.gameObject.SetActive(false);
    }

    private void ShootAction_OnAnyAimingEnded(object sender, EventArgs e)
    {
        // 隐藏 ShootAction UI
        HideShootActionUI();
    }

    public void ShowShootActionUI(float hitChance)
    {
        shootActionUI.gameObject.SetActive(true);
        int roundedHitChance = Mathf.RoundToInt(hitChance);
        hitRateText.text = $"{roundedHitChance}%";
       


    }

    public void HideShootActionUI()
    {
        shootActionUI.gameObject.SetActive(false);
    }

    private void CreateUnitActionBtns()
    {
        foreach(Transform buttonTransform in actionBtnContainerTransform)
        {
            Destroy(buttonTransform.gameObject);
        }

        actionButtonUIList.Clear();
         
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

        foreach (BaseAction baseAction in selectedUnit.GetBaseActionArray())
        {
            Transform actionBtnTransform = Instantiate(actionBtnPrefab, actionBtnContainerTransform);
            ActionBtnUI actionBtnUI = actionBtnTransform.GetComponent<ActionBtnUI>();
            actionBtnUI.SetBaseAction(baseAction);// 这里会判断是否为 MedicalAction 并更新显示

            actionButtonUIList.Add(actionBtnUI);
        }
    }

    private void UnitActionSystem_OnselectedUnitChanged(object sender, EventArgs e)
    {
        CreateUnitActionBtns();
        UpdateSelectedVisual();
        UpdateActionPoints();
        UpdateDetailPanel(); // panel更新

        AudioManager.Instance.Stop("Button_1");
       
    }

    private void UnitActionSystem_OnselectedActionChanged(object sender, EventArgs e)
    {
        UpdateSelectedVisual();
        AudioManager.Instance.Play("Button_1");
    }

    private void UnitActionSystem_OnActionStrated(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }

    private void UpdateSelectedVisual()
    {
        foreach(ActionBtnUI actionBtnUI in actionButtonUIList)
        {
            actionBtnUI.UpdateSelectedVisual();
        }
    }

    private void UpdateActionPoints()
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
       
        if (selectedUnit == null)
        {
            // 如果没有选中的单位，隐藏行动点或显示默认值
            actionPointsText.text = "0";
            healthNumberText.text = "0";
            return;
        }

        // 获取选中单位的标签
        string unitTag = selectedUnit.tag;

        // 根据标签显示对应的详情面板
        switch (unitTag)
        {
            case "Commando":
                actionPointsText.color = ChangeTextColor(207, 231, 247, 255);
                healthNumberText.color = ChangeTextColor(33, 174, 255, 255);
                break;
            case "Medic":
                actionPointsText.color = ChangeTextColor(133, 252, 80, 255);
                healthNumberText.color = ChangeTextColor(80, 252, 149, 255);
                break;
            case "Heavy":
                actionPointsText.color = ChangeTextColor(255, 148, 84, 255);
                healthNumberText.color = ChangeTextColor(255, 147, 85, 255);
                break;
            case "Sniper":
                actionPointsText.color = ChangeTextColor(236, 174, 255, 255);
                healthNumberText.color = ChangeTextColor(155, 60, 244, 255);
                break;
            case "Scientist":
                actionPointsText.color = ChangeTextColor(247, 212, 207, 255);
                healthNumberText.color = ChangeTextColor(255, 70, 33, 255);
                break;
            default:
                actionPointsText.color = ChangeTextColor(133, 252, 80, 255);
                healthNumberText.color = ChangeTextColor(33, 174, 255, 255);
                break;
        }

        actionPointsText.text = selectedUnit.GetActionPoints().ToString();
        healthNumberText.text = selectedUnit.GetHealth().ToString();

    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPoints();
    }

    private void UpdateDetailPanel()
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
      

        // 首先隐藏所有详情面板
        commonDetailPanel.SetActive(false);
        medicalDetailPanel.SetActive(false);
        grenadeDetailPanel.SetActive(false);
        sniperDetailPanel.SetActive(false);
        scientistDetailPanel.SetActive(false);

        // 获取选中单位的标签
        string unitTag = selectedUnit.tag;

        // 根据标签显示对应的详情面板     
            switch (unitTag)
            {
                case "Commando":
                    commonDetailPanel.SetActive(true);
                    AudioManager.Instance.Play("CommondoVoice");            
                break;
                case "Medic":
                    medicalDetailPanel.SetActive(true);
                    AudioManager.Instance.Play("MedicVoice");
                    break;
                case "Heavy":
                    grenadeDetailPanel.SetActive(true);
                    AudioManager.Instance.Play("GrendierVoice");
                    break;
                case "Sniper":
                    sniperDetailPanel.SetActive(true);
                    AudioManager.Instance.Play("SniperVoice");
                    break;
                case "Scientist":
                    scientistDetailPanel.SetActive(true);
                    AudioManager.Instance.Play("ScientistVoice");
                    break;
            default:
                    // 如果没有匹配的标签，可以选择显示一个默认的面板或不显示任何面板
                    commonDetailPanel.SetActive(false);
                    medicalDetailPanel.SetActive(false);
                    grenadeDetailPanel.SetActive(false);
                    sniperDetailPanel.SetActive(false);
                    break;
            }
      
            
        
    }

    // 使用整数 RGB (0-255) 设置颜色
    public Color ChangeTextColor(int r, int g, int b, int a = 255)
    {
        if (actionPointsText != null && healthNumberText != null)
        {
            // 将 0-255 转换为 0.0-1.0f 的范围
            float normalizedR = r / 255f;
            float normalizedG = g / 255f;
            float normalizedB = b / 255f;
            float normalizedA = a / 255f;

            // 设置颜色
            return new Color(normalizedR, normalizedG, normalizedB, normalizedA);
            
        }
        return new Color(1, 1, 1, 1);
    }

    
}
