using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class InteractBeam : MonoBehaviour, IInteractable
{

    [SerializeField] private GameObject[] Beamprefabs;
    private GameObject beam1;
    private GameObject beam2;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject CratePart;
    [SerializeField] private VisualEffect ExploedVFX;


    private GridPosition gridPosition;
    private Action onInteractionComplete;
    private bool isActive;
    private float timer;
    private bool isGreen;

    public event EventHandler OnBeamOpened;

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

        // 计算物体前方一格的世界位置
        Vector3 forwardOffset = transform.position + transform.forward * LevelGrid.Instance.GetCellSize();
        GridPosition forwardGridPosition = LevelGrid.Instance.GetGridPosition(forwardOffset);


        LevelGrid.Instance.SetInteractableAtGridPosition(forwardGridPosition, this);
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, false);
      

        SetColorGreen();
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

    private void SetColorGreen()
    {
        isGreen = true;
        beam1 = Instantiate(Beamprefabs[0], spawnPoint.position, spawnPoint.rotation);
        beam1.GetComponent<Animator>().Play("Show");
    }

    private void SetColorRed()
    {
        if (beam1)
        {
            beam1.GetComponent<Animator>().Play("Hide");            
        }
        AudioManager.Instance.Play("Level1Device");
        Invoke("ExecuteAfterDelay", 1.5f); // 1秒延迟      
                
    }
    private void ExecuteAfterDelay()
    {
        beam2 = Instantiate(Beamprefabs[1], spawnPoint.position, spawnPoint.rotation);
        beam2.GetComponent<Animator>().Play("Show");

       

        Invoke("ExplodedDelay", 2f); // 1秒延迟   
    }

    private void ExplodedDelay()
    {
        if (ExploedVFX != null)
        {
            ExploedVFX.Play();
            AudioManager.Instance.Play("Explosion_large");
        }
        Destroy(CratePart);
        
        OnBeamOpened?.Invoke(this, EventArgs.Empty);
    }

    public void Interact(Action onInteractionComplete)
    {
        this.onInteractionComplete = onInteractionComplete;
        isActive = true;
        timer = 0.5f;

        if (isGreen)
        {
            SetColorRed();
        }
        else
        {
            SetColorGreen();
        }
    }

    
}
