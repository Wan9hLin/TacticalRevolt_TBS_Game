using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using INab.WorldAlchemy;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }

    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStrated;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction selectedAction;
    private bool isBusy;

    private GridPosition TargetMoveGridPosition;

    [SerializeField] private SeeThroughDetect seethrough;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more than one UnitActionSystem!" + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetSelectedUnit(selectedUnit);           
    }


    private void Update()
    {

        // If system is busy or it's not the player's turn, skip the update
        if (isBusy || !TurnSystem.Instance.IsPlayerTurn() || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (TryHandleUnitSelection())
        {
            return;
        }



        HandleSelectedAction();      
    }

     private void HandleSelectedAction()
     {
        // Handle action when mouse button is clicked
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
         {

             GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
             TargetMoveGridPosition = mouseGridPosition;// Pass to move action

            // Validate action and spend action points
            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition) ||
                !selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
            {
                return;
            }

            // Set system to busy and execute the action
            SetBusy();
            selectedAction.TakeAction(mouseGridPosition, ClearBusy);

            // Notify action started
            OnActionStrated?.Invoke(this, EventArgs.Empty);
         }
     }

     private void SetBusy()
     {
         isBusy = true;
         OnBusyChanged?.Invoke(this, isBusy);
     }

     private void ClearBusy()
     {
         isBusy = false;
         OnBusyChanged?.Invoke(this, isBusy);
     }

     private bool TryHandleUnitSelection()
     {

         if (InputManager.Instance.IsMouseButtonDownThisFrame())
         {
             Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
             if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
             {
                 if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                 {
                     if (unit == selectedUnit)
                     {
                         //unit already selected
                         return false;
                     }

                    if (unit.CompareTag("Hostage"))
                    {
                        return false;
                    }

                     if (unit.IsEnemy())
                     {
                         //clicked on an enemy
                         return false;
                     }

                     SetSelectedUnit(unit);
                     return true;
                 }

             }
         }

         return false;
     }

     private void SetSelectedUnit(Unit unit)
     {
         selectedUnit = unit;

        // Set the see-through target (if applicable)
        if (seethrough != null)
        {
            seethrough.targetTransform = selectedUnit.transform;
        }

        // Set the default selected action to MoveAction
         SetSelectedAction(unit.GetAction<MoveAction>());     
         OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
     }

     public void SetSelectedAction(BaseAction baseAction)
     {
         selectedAction = baseAction;
         OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
     }

     public Unit GetSelectedUnit()
     {
         return selectedUnit;
     }

     public BaseAction GetSelectedAction()
     {
         return selectedAction;
     }

     //For Moveaction
     public GridPosition GetMoveGridPosition()
     {
         return TargetMoveGridPosition;
     }

}
