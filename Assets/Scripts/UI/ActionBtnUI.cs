using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ActionBtnUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    //医疗包数量显示
    [SerializeField] private TextMeshProUGUI medicalKitsText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedGameObject;

    [Header("Action Icon List")]
    [SerializeField] private List<ActionIconPair> actionIconPairs;

    private BaseAction baseAction;

    public void SetBaseAction(BaseAction baseAction)
    {
        this.baseAction = baseAction;
        textMeshPro.text = baseAction.GetActionName().ToUpper();

        button.onClick.AddListener(() =>
        {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });

        // 更新技能图标
        UpdateSkillIcons();

        // 判断是否为 MedicalAction，并更新医疗包数量显示
        if (baseAction is MedicalAction medicalAction)
        {
            medicalKitsText.gameObject.SetActive(true);  // 显示医疗包文本
            medicalKitsText.text = $"x{medicalAction.GetCurrentMedicalKits()}";  // 设置文本为医疗包数量
        }
        else if(baseAction is GrendAction grendAction)
        {
            medicalKitsText.gameObject.SetActive(true);  // 显示医疗包文本
            medicalKitsText.text = $"x{grendAction.GetCurrentGrenadeKits()}";  // 设置文本为医疗包数量
        }
        else
        {
            medicalKitsText.gameObject.SetActive(false);  // 隐藏医疗包文本
        }
    }

    public void UpdateSelectedVisual()
    {
        BaseAction selectedBaseAction = UnitActionSystem.Instance.GetSelectedAction();
        bool isSelected = selectedBaseAction == baseAction;
        selectedGameObject.SetActive(isSelected);

        // 更新技能图标的选中状态
        UpdateSkillIcons();
    }

    private void UpdateSkillIcons()
    {
        // 先隐藏所有图标
        foreach (var pair in actionIconPairs)
        {
            pair.unselectedIcon.SetActive(false);
            pair.selectedIcon.SetActive(false);
        }

        // 获取当前技能的名称
        string currentActionName = baseAction.GetActionName();

        // 遍历图标列表，匹配技能名称
        foreach (var pair in actionIconPairs)
        {
            if (pair.actionName == currentActionName)
            {
                BaseAction selectedBaseAction = UnitActionSystem.Instance.GetSelectedAction();
                bool isSelected = selectedBaseAction == baseAction;

                pair.unselectedIcon.SetActive(!isSelected);
                pair.selectedIcon.SetActive(isSelected);
                break; // 找到匹配的技能，停止循环
            }
        }
    }

}

[Serializable]
public class ActionIconPair
{
    public string actionName;              // 技能名称，用于匹配
    public GameObject unselectedIcon;      // 未选中状态的图标
    public GameObject selectedIcon;        // 选中状态的图标
}

