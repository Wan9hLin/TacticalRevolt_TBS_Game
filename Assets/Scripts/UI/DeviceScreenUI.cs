using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeviceScreenUI : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Start()
    {
        healthSystem.OnDamaged += HealthSystem_OnDamaged;

        // 设置 Slider 的最大值为最大生命值
        healthBarSlider.maxValue = healthSystem.GetMaxHealth();

        UpdateHealthBar();
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBarSlider.value = healthSystem.GetHealth();

        // 更新血量文本，格式为 "当前血量/最大血量"
        healthText.text = $"{healthSystem.GetHealth()}/{healthSystem.GetMaxHealth()}";
    }
}
