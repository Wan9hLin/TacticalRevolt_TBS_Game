using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUIController : MonoBehaviour
{
    private Animator damageUIAnimator;


    private void Awake()
    {
        damageUIAnimator = GetComponent<Animator>();
        if (damageUIAnimator == null)
        {
            Debug.LogError("No Animator component found on DamageUI GameObject.");
        }
    }

    private void OnEnable()
    {
        Unit.OnAnyUnitDamaged += Unit_OnAnyUnitDamaged;
    }

    private void OnDisable()
    {
        Unit.OnAnyUnitDamaged -= Unit_OnAnyUnitDamaged;
    }

    private void Unit_OnAnyUnitDamaged(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        if (unit != null && !unit.IsEnemy())
        {
            TriggerDamageAnimation();
        }
    }

    private void TriggerDamageAnimation()
    {
        if (damageUIAnimator != null)
        {
            damageUIAnimator.SetTrigger("Hit");
        }
    }
}
