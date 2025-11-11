using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridPosition gridPosition;
    private GridSystem<GridObject> gridSystem;
    private List<Unit> unitlist;
    private IInteractable interactable;


    public GridObject(GridPosition gridPosition, GridSystem<GridObject> gridSystem)
    {
        this.gridPosition = gridPosition;
        this.gridSystem = gridSystem;
        unitlist = new List<Unit>();
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitlist)
        {
            unitString += unit + "\n";

        }
        return gridPosition.ToString() + "\n" + unitString;
    }

    public void AddUnit(Unit unit)
    {
        unitlist.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        unitlist.Remove(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitlist;
    }

    public bool HasAnyUnit()
    {
        return unitlist.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitlist[0];
        }
        else
        {
            return null;
        }
    }

    public IInteractable GetInteractable()
    {
        return interactable;
    }

    public void SetInteractable(IInteractable interactable)
    {
        this.interactable = interactable;
    }
}
