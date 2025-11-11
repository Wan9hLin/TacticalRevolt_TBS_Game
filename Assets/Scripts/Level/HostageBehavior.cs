using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostageBehavior : MonoBehaviour
{
    [SerializeField] private GameObject eascapePointObject;
    private GridPosition escapeGridPosition; // 逃生点的 GridPosition，需要在 Inspector 中设置
    private bool isRescued = false; // 是否已经被救
    private Unit unit;
    private MoveAction moveAction;
    [SerializeField] private Animator animator;

    // 用于检测是否已经有单位触发了救援
    private static bool isAnyHostageBeingRescued = false;

    [SerializeField] private float detectionRadius = 5f; // 检测半径，可在 Inspector 中调整

    // 声明一个静态事件用于Hostage被救时通知
    public static event EventHandler OnHostageRescued;


    private void Awake()
    {
        unit = GetComponent<Unit>();
        moveAction = GetComponent<MoveAction>();
     
    }

    private void Start()
    {
        if (eascapePointObject != null)
        {
            Vector3 escapeWorldPosition = eascapePointObject.transform.position;
            escapeGridPosition = LevelGrid.Instance.GetGridPosition(escapeWorldPosition);
        }
        else
        {
            Debug.LogError("EscapePoint not found in the scene.");
        }
    }

    private void Update()
    {
        if (isRescued || isAnyHostageBeingRescued)
        {
            // 已经被救，或者其他 Hostage 正在被救援
            return;
        }

        // 使用 Physics.OverlapSphere 手动检测附近的友方单位
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            Unit otherUnit = hitCollider.GetComponent<Unit>();
            if (otherUnit != null && !otherUnit.IsEnemy() && otherUnit != unit)
            {
                // 检测到友方单位进入范围，且不是自己
                StartRescue(otherUnit);
                break; // 只需要检测到一个友方单位即可
            }
        }
    }

    private void StartRescue(Unit rescuer)
    {
        isRescued = true;
        isAnyHostageBeingRescued = true;

        // 禁用检测范围的视觉效果和碰撞体（如果有）
        Transform detectionRange = transform.Find("DetectionRange");
        if (detectionRange != null)
        {
            detectionRange.gameObject.SetActive(false);
        }

        animator.SetTrigger("Thankful");
        AudioManager.Instance.Play("HostageThx");
        // 立即移动到逃生点
        Invoke("MoveToEscapePoint", 2.5f);
        
       
    }


    private void MoveToEscapePoint()
    {
        // 开始移动
        moveAction.TakeAction(escapeGridPosition, OnRescueComplete);
        AudioManager.Instance.Play("MoveHostage");       

    }

    private void OnRescueComplete()
    {
        AudioManager.Instance.Stop("MoveHostage");

        // 在这里触发 Hostage 被救的事件
        OnHostageRescued?.Invoke(this, EventArgs.Empty);

        // 移除 Hostage，或触发其他逻辑
        Destroy(gameObject);

        // 重置静态变量，允许其他 Hostage 被救
        isAnyHostageBeingRescued = false;
    }

    // 在场景视图中绘制检测范围，方便调整 detectionRadius
    private void OnDrawGizmos()
    {
        // 设置 Gizmo 的颜色
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 绿色，带有一定透明度

        // 绘制半透明的球体表示检测范围
        Gizmos.DrawSphere(transform.position, detectionRadius);

        // 绘制边框
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }




}
