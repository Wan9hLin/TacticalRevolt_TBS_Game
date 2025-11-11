using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject> 
{
    private int width;
    private int height; 
    private float cellSize;
    private TGridObject[,] gridObjectArray;


    // Constructor to initialize the grid with specified dimensions and grid object creation function
    public GridSystem(int width, int height, float cellSize, Func<GridPosition, GridSystem<TGridObject>, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridObjectArray = new TGridObject[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int z=0; z< height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                gridObjectArray[x,z] = createGridObject(gridPosition, this);
            }
        }       
    }


    // Convert grid position to world position for object placement
    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, 0, gridPosition.z) * cellSize;
    }


    // Convert world position to grid position (rounded)
    public GridPosition GetGridPosition(Vector3 wroldPosition)
    {
        return new GridPosition(

            Mathf.RoundToInt(wroldPosition.x / cellSize),
            Mathf.RoundToInt(wroldPosition.z / cellSize)
            );         
    }


    // Create debug objects at each grid position for visualization
    public void CreateDebugObjects(Transform debugPrefab)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
              GridPosition gridPosition = new GridPosition(x, z); 
              Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
              GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
              gridDebugObject.SetGridObject(GetGridObject(gridPosition));


            }
        }
    }
    

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.z];
    }



    // Check if a grid position is within valid bounds
    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.x >= 0 && 
               gridPosition.z >= 0 && 
               gridPosition.x < width && 
               gridPosition.z < height;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

}
