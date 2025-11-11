using System;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleCrate : MonoBehaviour
{
    public static event EventHandler OnAnyDestroyed;

    [SerializeField] private Transform crateDestroyedPrefab;
    private List<GridPosition> gridPositions;

    private void Start()
    {
        // 获取物体占据的所有格子
        gridPositions = CalculateGridPositions();
    }

    // 获取物体占据的所有格子位置
    public List<GridPosition> GetGridPositions()
    {
        return gridPositions;
    }


    public void Damage()
    {
        Transform crateDestroyTransform = Instantiate(crateDestroyedPrefab, transform.position, transform.rotation);
        ApplyExplosionToChildren(crateDestroyTransform, 200f, transform.position, 15f);
        Destroy(gameObject);
      
        OnAnyDestroyed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyExplosionToChildren(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody))
            {
                childRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }

            ApplyExplosionToChildren(child, explosionForce, explosionPosition, explosionRange);

        }

    }
    // 计算物体占据的所有格子位置
    private List<GridPosition> CalculateGridPositions()
    {
        List<GridPosition> positions = new List<GridPosition>();

        // 获取物体的边界范围
        Bounds bounds = GetComponent<Collider>().bounds;

        // 获取 LevelGrid 的实例
        LevelGrid levelGrid = LevelGrid.Instance;

        // 计算物体所覆盖的最小和最大 GridPosition
        GridPosition minGridPosition = levelGrid.GetGridPosition(bounds.min);
        GridPosition maxGridPosition = levelGrid.GetGridPosition(bounds.max);

        // 遍历所有被覆盖的格子
        for (int x = minGridPosition.x; x <= maxGridPosition.x; x++)
        {
            for (int z = minGridPosition.z; z <= maxGridPosition.z; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);

                if (levelGrid.IsValidGridPosition(gridPosition))
                {
                    positions.Add(gridPosition);
                }
            }
        }

        return positions;
    }
}
