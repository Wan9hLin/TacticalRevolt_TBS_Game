using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectedVisual : MonoBehaviour
{

    [SerializeField] private Unit unit;
    [SerializeField] private GameObject targetgameObject;
    
   
    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        UpdateVisual();
    }

    private void UnitActionSystem_OnSelectedUnitChanged(object sender,EventArgs empty)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {

        if (UnitActionSystem.Instance.GetSelectedUnit() == unit)
        {
            targetgameObject.SetActive(true);
        }
        else
        {
            targetgameObject.SetActive(false);
        }

    }

    private void OnDestroy()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
    }

}
