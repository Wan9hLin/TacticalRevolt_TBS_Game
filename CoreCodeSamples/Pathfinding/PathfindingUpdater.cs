using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingUpdater : MonoBehaviour
{


    private void Start()
    {
        DestructibleCrate.OnAnyDestroyed += DestructibleCrate_OnAnyDestroyed;
    }

    private void DestructibleCrate_OnAnyDestroyed(object sender, EventArgs e)
    {
        DestructibleCrate destructibleCrate = sender as DestructibleCrate;
        List<GridPosition> gridPositions = destructibleCrate.GetGridPositions();

        // Update the positions of these grids to be walkable
        foreach (GridPosition gridPosition in gridPositions)
        {
            PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, true);
        }
    }
}
