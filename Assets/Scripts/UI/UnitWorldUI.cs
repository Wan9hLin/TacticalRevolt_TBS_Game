using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UnitWorldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    public Slider healthBarSlider;
    [SerializeField] private HealthSystem healthSystem;

    //命中还是未命中
    [SerializeField] private Transform hitImage;
    [SerializeField] private Transform missImage;

    
    [SerializeField] private Transform crosshairImage;

    //掩体状态显示
    [SerializeField] private Image fullCoverImage;
    [SerializeField] private Image halfCoverImage;

    // 新增：行动点数显示的 Toggle
    [SerializeField] private Toggle actionPointToggle1;
    [SerializeField] private Toggle actionPointToggle2;

    private void Start()
    {
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
        healthSystem.OnDamaged += HealthSystem_OnDamaged;
        healthSystem.OnHealed += HealthSystem_OnHealed;
       

        // 设置 Slider 的最大值为最大生命值
        healthBarSlider.maxValue = healthSystem.GetMaxHealth();

        UpdateActionPointsText();
        UpdateHealthBar();

        
        HideCrosshair();  // 初始化时隐藏准星

        UpdateCoverIcons();
    }

  

    private void Update()
    {
        UpdateCoverIcons();
        UpdateHealthBar();
    }

    private void UpdateActionPointsText()
    {
        int actionPoints = unit.GetActionPoints();

        // 更新文本显示
        actionPointsText.text = actionPoints.ToString();

        // 更新 Toggle 状态
        switch (actionPoints)
        {
            case 2:
                actionPointToggle1.isOn = true;
                actionPointToggle2.isOn = true;
                break;
            case 1:
                actionPointToggle1.isOn = true;
                actionPointToggle2.isOn = false;
                break;
            case 0:
                actionPointToggle1.isOn = false;
                actionPointToggle2.isOn = false;
                break;
            default:
                // 处理行动点数超出范围的情况
                actionPointToggle1.isOn = actionPoints >= 1;
                actionPointToggle2.isOn = actionPoints >= 2;
                break;
        }
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPointsText();
    }

    private void UpdateHealthBar()
    {
      //Debug.Log("Health bar value changed by function");
        healthBarSlider.value = healthSystem.GetHealth();
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }
    private void HealthSystem_OnHealed(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

    // 显示命中图片并在几秒后隐藏
    public void ShowHit()
    {
        StartCoroutine(ShowHitImage());
    }

    private IEnumerator ShowHitImage()
    {
        hitImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);  
        hitImage.gameObject.SetActive(false);
    }

    // 显示未命中图片并在几秒后隐藏
    public void ShowMiss()
    {
        StartCoroutine(ShowMissImage());
    }

    private IEnumerator ShowMissImage()
    {
        missImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);  
        missImage.gameObject.SetActive(false);
    }

    // 显示准星
    public void ShowCrosshair()
    {
        crosshairImage.gameObject.SetActive(true);
        
    }

    // 隐藏准星
    public void HideCrosshair()
    {
        crosshairImage.gameObject.SetActive(false);
        
    }

    private void UpdateCoverIcons()
    {
        string coverType = unit.GetCoverType();

        // 根据掩体类型更新图片显示
        if (coverType == "FullCover")
        {
            fullCoverImage.gameObject.SetActive(true);
            halfCoverImage.gameObject.SetActive(false);
        }
        else if (coverType == "HalfCover")
        {
            fullCoverImage.gameObject.SetActive(false);
            halfCoverImage.gameObject.SetActive(true);
        }
        else
        {
            fullCoverImage.gameObject.SetActive(false);
            halfCoverImage.gameObject.SetActive(false);
        }
    }

   
}
