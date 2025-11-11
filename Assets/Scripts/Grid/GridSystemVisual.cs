using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { get; private set; }

    [Serializable]
    public struct GridVisualTypeColor
    {
        public GridVisualType gridVisualType;
        public Color color;
    }
    public enum GridVisualType
    {
        White, Blue, Red, RedSoft, Yellow, Green, Orange,
    }

    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    [SerializeField] private List<GridVisualTypeColor> gridVisualTypeColorList;


    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more than one GridSystemVisual!" + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        // Initialize the grid visualization array based on the grid size
        gridSystemVisualSingleArray = new GridSystemVisualSingle[
            LevelGrid.Instance.GetWidth(),
            LevelGrid.Instance.GetHeight()
            ];

        // Instantiate grid visual prefabs at the corresponding grid positions
        for (int x=0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for(int z=0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSystemVisualSingleTransform = 
                    Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);

                gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        LevelGrid.Instance.OnAnyUnitMoveGridPosition += LevelGrid_OnAnyUnitMoveGridPosition;

        UpdateGridVisual();
    }

 
    public void HideAllGridPosition()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                gridSystemVisualSingleArray[x, z].Hide();
                gridSystemVisualSingleArray[x, z].HideCoverUI();
                gridSystemVisualSingleArray[x, z].HideWarningIcon(); 
            }
        }

    }

    // Show a range of grid positions around a given position
    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
       
        List<GridPosition> gridPositionList = new List<GridPosition>();
        gridPositionList = LevelGrid.Instance.GetGridPositionInRange(gridPosition, range);   
        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    private void ShowGridPositionRangeSquare(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    // Show a list of grid positions with a specific visual type (color)
    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        Color color = GetGridVisualTypeColor(gridVisualType);
        foreach (GridPosition gridPosition in gridPositionList)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show(color);
        }
    }

    // Update the grid visuals based on the selected action and unit
    public void UpdateGridVisual()
    {
        HideAllGridPosition();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        if (selectedAction is MoveAction moveAction)
        {
            // Show move action visual feedback (short/long range and cover)
            ShowMoveActionVisuals(moveAction);
            ShowMoveActionCoverVisual(selectedUnit, moveAction);

            // If in scout mode, show enemy vision range
            if (EnemyAI.isInScoutMode)
            {
                foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
                {
                    ShowEnemyVisionWarningIcon(enemyUnit.GetGridPosition(), 7); 
                }
            }
        }
        else if (selectedAction is SpinAction)
        {
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.Blue);
        }
        else if (selectedAction is ShootAction shootAction)
        {
            ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualType.RedSoft);
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.Red);
        }
        else if (selectedAction is GrendAction)
        {
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.Orange);
        }
        else if (selectedAction is SwordAction swordAction)
        {
            ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft);
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.Red);
        }
        else if (selectedAction is InteractAction)
        {
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.Blue);
        }
        else if (selectedAction is MedicalAction)
        {
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.Green);
        }
        else
        {
            // Default to white visual for invalid actions
            ShowGridPositionList(selectedAction.GetValidActionPositionList(), GridVisualType.White);
        }
    }

    private void UnitActionSystem_OnSelectedActionChanged (object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void LevelGrid_OnAnyUnitMoveGridPosition (object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    // Get the color for a specific grid visual type
    private Color GetGridVisualTypeColor(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeColor gridVisualTypeColor in gridVisualTypeColorList)
        {
            if (gridVisualTypeColor.gridVisualType == gridVisualType)
            {
                return gridVisualTypeColor.color;
            }
        }

        Debug.LogError("Could not find GridVisualTypeColor for GridVisualType " + gridVisualType);
        return Color.white;
    }

    // Display cover UI for move action if near cover
    private void ShowMoveActionCoverVisual(Unit selectedUnit, MoveAction moveAction)
    {
        List<GridPosition> validMovePositions = moveAction.GetValidActionPositionList();

        foreach (GridPosition gridPosition in validMovePositions)
        {           
            if (LevelGrid.Instance.IsGridPositionInCover(gridPosition, out string coverType, out string coverDirection))
            {
                gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].ShowCoverUI(coverType, coverDirection);
            }
        }
    }

    // Show move action visuals (short and long range positions)
    private void ShowMoveActionVisuals(MoveAction moveAction)
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        GridPosition unitGridPosition = selectedUnit.GetGridPosition();

        List<GridPosition> validMovePositions = moveAction.GetValidActionPositionList();

        List<GridPosition> shortDistancePositions = new List<GridPosition>();
        List<GridPosition> longDistancePositions = new List<GridPosition>();

        foreach (GridPosition gridPosition in validMovePositions)
        {
            int distance = Mathf.Abs(gridPosition.x - unitGridPosition.x) + Mathf.Abs(gridPosition.z - unitGridPosition.z);
            
            if (distance <= moveAction.GetshortDistanceMax())
            {
                shortDistancePositions.Add(gridPosition);
            }
            else
            {
                longDistancePositions.Add(gridPosition);
            }
        }

        ShowGridPositionList(shortDistancePositions, GridVisualType.White);
        ShowGridPositionList(longDistancePositions, GridVisualType.Yellow);
    }

    public void ShowEnemyVisionWarningIcon(GridPosition enemyGridPosition, int visionRange)
    {
        List<GridPosition> visionRangePositions = LevelGrid.Instance.GetGridPositionInRange(enemyGridPosition, visionRange);

        foreach (GridPosition gridPosition in visionRangePositions)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].ShowWarningIcon();
        }
    }

}
