using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isOpen;
    [SerializeField] private List<Vector3> additionalGridPositions;  // 门占据的额外位置（相对于门的初始位置）
    private List<GridPosition> gridPositions = new List<GridPosition>();

    private Animator animator;
    private Action onInteractionComplete;
    private bool isActive;
    private float timer;


    public static event EventHandler OnAnyDoorOpened;
    public event EventHandler OnDoorOpened;

    [SerializeField] private GameObject Door_2;
    Animator Door_2Animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        Door_2Animator = Door_2.GetComponent<Animator>();
    }

    private void Start()
    {
        // 获取主 grid 位置
        GridPosition mainGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        gridPositions.Add(mainGridPosition);

        // 获取附加的 grid 位置
        foreach (Vector3 offset in additionalGridPositions)
        {
            Vector3 gridWorldPosition = transform.position + offset;
            GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(gridWorldPosition);
            gridPositions.Add(gridPosition);
        }


        // 仅设置主 grid 位置为可交互，但不可通行
        LevelGrid.Instance.SetInteractableAtGridPosition(mainGridPosition, this);
        PathFinding.Instance.SetIsWalkableGridPosition(mainGridPosition, false);



        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isActive = false;
            onInteractionComplete();
        }
    }

    public void Interact(Action onInteractionComplete)
    {
        this.onInteractionComplete = onInteractionComplete;
        isActive = true;
        timer = 0.5f;

        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        animator.SetBool("isOpen", isOpen);
        Door_2Animator.SetTrigger("isOpen");

        // 使除主交互位置外的其他 grid 位置可通行
        for (int i = 1; i < gridPositions.Count; i++)
        {
            PathFinding.Instance.SetIsWalkableGridPosition(gridPositions[i], true);
          //  Debug.Log("Open grid at " + gridPositions[i]);
        }

        OnDoorOpened?.Invoke(this, EventArgs.Empty);
        OnAnyDoorOpened?.Invoke(this, EventArgs.Empty);
    }

    private void CloseDoor()
    {
        isOpen = false;
        animator.SetBool("isOpen", isOpen);

        // 使除主交互位置外的其他 grid 位置不可通行
        for (int i = 1; i < gridPositions.Count; i++)
        {
            PathFinding.Instance.SetIsWalkableGridPosition(gridPositions[i], false);
           // Debug.Log("Close grid at " + gridPositions[i]);
        }
    }
}
