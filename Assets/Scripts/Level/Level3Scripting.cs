using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GogoGaga.UWI;
using UnityEngine.SceneManagement;

public class Level3Scripting : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemy1List;
    [SerializeField] private GameObject enemy_2;
    [SerializeField] private List<GameObject> enemy3List;
    [SerializeField] private List<GameObject> enemy4List;
    [SerializeField] private GameObject scientistUnit;
    [SerializeField] private GameObject CagedScientist;


    [SerializeField] private GameObject hider1;

    [SerializeField] private Door door;
    [SerializeField] private Door door_2;
    [SerializeField] private GameObject LeaveCar;
    [SerializeField] private GameObject leaveArea;
    [SerializeField] private GameObject EnemyPlane;


    private WaypointWorldMarker waypointWorldMarker;
    private WaypointWorldMarker waypointWorldMarker_2;
    private WaypointWorldMarker waypointWorldMarker_3;
    private WaypointWorldMarker waypointWorldMarker_4;

    [SerializeField] private WaypointWorldMarker waypointWorldMarker_5;


    private bool hasShownFirstHider = false;
    private bool hasArrivedLeavelArea = false;
    private bool hasLeaveFirstEnemy = false;
    private bool hasLeaveSecondEnemy = false;
    private bool hasLeaveScientist = false;

    //任务系统
    [SerializeField] private GameObject Task1;
    [SerializeField] private GameObject Task2;
    [SerializeField] private GameObject Task3;
    private Animator task1Animator;
    private Animator task2Animator;
    private Animator task3Animator;
    [SerializeField] private GameObject warningUI; // 引用警告 UI 的 GameObject

    private int rescuedHostageCount = 0; // 新增计数器
    private int remainingPlayerTurns = -1; // -1 表示倒计时尚未开始

    [SerializeField] private Image BeforeLeaveUI;
    [SerializeField] private TextMeshProUGUI LeaveNoText;

    [Header("加载页面")]
    [SerializeField] private GameObject loadingPanel; // 加载页面
    [SerializeField] private Slider progressBar; // 进度条
    [SerializeField] private TextMeshProUGUI progressText; // 进度文本
    [SerializeField] private Animation loadingIconAnimation; // 加载图标动画
    [SerializeField] private float fakeLoadSpeed = 0.5f; // 进度条增长速度
    [SerializeField] private string nextSceneName; // 下一个场景的名称
    [SerializeField] private float delayBeforeLoading = 3f; // 开始加载前的延迟时间

    private void Start()
    {
        // 注册事件监听
        LevelGrid.Instance.OnAnyUnitMoveGridPosition += LevelGrid_OnAnyUnitMoveGridPosition;
        // 在此订阅Hostage被救事件
        HostageBehavior.OnHostageRescued += HostageBehavior_OnHostageRescued;
       
        waypointWorldMarker = door.gameObject.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_2 = door_2.gameObject.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_3 = LeaveCar.GetComponent<WaypointWorldMarker>();
        waypointWorldMarker_4 = EnemyPlane.GetComponent<WaypointWorldMarker>();

        waypointWorldMarker.enabled = !waypointWorldMarker.enabled;
        waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
        waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;
        waypointWorldMarker_5.enabled = !waypointWorldMarker_5.enabled;

        //任务系统
        task1Animator = Task1.GetComponent<Animator>();
        task2Animator = Task2.GetComponent<Animator>();
        task3Animator = Task3.GetComponent<Animator>();

        leaveArea.SetActive(false);
        EnemyPlane.SetActive(false);
        BeforeLeaveUI.gameObject.SetActive(false);

        // 订阅 TurnSystem 的回合变化事件
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        door.OnDoorOpened += (object sender, EventArgs e) =>
        {
            waypointWorldMarker.enabled = !waypointWorldMarker.enabled;
            waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
            EnemyAI.isInScoutMode = false;
            enemy_2.SetActive(true);
            AudioManager.Instance.Play("SlideDoorOpen");
        };

        door_2.OnDoorOpened += (object sender, EventArgs e) =>
        {
            waypointWorldMarker_2.enabled = !waypointWorldMarker_2.enabled;
            AudioManager.Instance.Play("SlideDoorOpen");


            StartCoroutine(ShowScientist());
            
        };


    }
    

    private void LevelGrid_OnAnyUnitMoveGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        if (e.toGridPosition.x == 17 && !hasShownFirstHider && !e.unit.IsEnemy())
        {
            hasShownFirstHider = true;
            EnemyAI.ResetScoutMode();
            hider1.SetActive(false);
            SetActiveGameObjectList(enemy1List, true);
            waypointWorldMarker.enabled = !waypointWorldMarker.enabled;
            waypointWorldMarker_5.enabled = !waypointWorldMarker_5.enabled;
        }

        if(e.toGridPosition.x < 6 && e.toGridPosition.z < 4 && !hasArrivedLeavelArea && e.unit.CompareTag("Scientist"))
        {
            hasArrivedLeavelArea = true;
            task3Animator.SetBool("Active", true);

        }

        if (e.toGridPosition.z == 22 && !hasLeaveFirstEnemy && e.unit.CompareTag("Scientist"))
        {
            hasLeaveFirstEnemy = true;
            StartCoroutine(ShowWarningUICoroutine());
            EnemyPlane.SetActive(true);
            StartCoroutine(ShowLeaveEnemy());

        }

        if (e.toGridPosition.x == 14 && !hasLeaveSecondEnemy && e.unit.CompareTag("Scientist"))
        {
            hasLeaveSecondEnemy = true;
            StartCoroutine(ShowWarningUICoroutine());
            SetActiveGameObjectList(enemy4List, true);

        }

        if (e.toGridPosition.x <= 6 && e.toGridPosition.z <= 3 && !hasLeaveScientist && e.unit.CompareTag("Scientist"))
        {
            hasLeaveScientist = true;
            StartCoroutine(TriggerSceneLoading());         
        }


    }

    private void SetActiveGameObjectList(List<GameObject> gameObjectList, bool isActive)
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            gameObject.SetActive(isActive);
        }
    }

    private IEnumerator ShowScientist()
    {
        
        yield return new WaitForSeconds(2f);
     
        CagedScientist.SetActive(false);
        scientistUnit.SetActive(true);
        waypointWorldMarker_3.enabled = !waypointWorldMarker_3.enabled;
        task2Animator.SetBool("Active", true);
        BeforeLeaveUI.gameObject.SetActive(true);

        AudioManager.Instance.Play("LeaveSign");
        remainingPlayerTurns = 12;
        BeforeLeaveUI.gameObject.SetActive(true);
        LeaveNoText.text = $"{remainingPlayerTurns}";

        leaveArea.gameObject.SetActive(true);

    }

    private void HostageBehavior_OnHostageRescued(object sender, EventArgs e)
    {
        rescuedHostageCount++;
      
        if (rescuedHostageCount >= 3)
        {
            task1Animator.SetBool("Active", true);
        }
    }

    private IEnumerator ShowWarningUICoroutine()
    {
        warningUI.SetActive(true);
        AudioManager.Instance.Play("EnemyDetected");
        yield return new WaitForSeconds(3.5f);  // 持续显示 3 秒
        warningUI.SetActive(false);
    }

    private IEnumerator ShowLeaveEnemy()
    {
        yield return new WaitForSeconds(3f);  // 持续显示 3 秒
        EnemyAI.isInScoutMode = false;
        SetActiveGameObjectList(enemy3List, true);
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (hasLeaveFirstEnemy)
        {
            waypointWorldMarker_4.enabled = !waypointWorldMarker_4.enabled;
        }

        // 如果remainingPlayerTurns仍是-1，表示倒计时未开始，不执行下面的逻辑
        if (remainingPlayerTurns == -1)
        {
            return;
        }

        // 检查是否是玩家回合
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            // 在玩家回合开始时显示当前剩余回合数
            LeaveNoText.text = $"{remainingPlayerTurns}";

            // 判断是否剩余回合数用尽
            if (remainingPlayerTurns <= 0)
            {
                Debug.Log("Mission Failed");
                // 可在此执行任务失败逻辑，例如结束游戏或播放失败动画
                TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
                return;
            }

            // 显示完成后，再减少回合数，表示下一个玩家回合开始时会少1
            remainingPlayerTurns--;
        }
        else
        {
            // 敌人回合不更新剩余回合数和UI
            // 留空表示什么都不做
        }
    }

    private IEnumerator TriggerSceneLoading()
    {
        // 1. 等待指定延迟时间
        yield return new WaitForSeconds(delayBeforeLoading);

        // 2. 显示加载页面
        loadingPanel.SetActive(true);
        loadingIconAnimation.gameObject.SetActive(true);
        if (loadingIconAnimation != null)
        {
            loadingIconAnimation.Play();
        }

        // 3. 启动加载进度逻辑
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        float simulatedProgress = 0f;

        // 模拟进度条增长
        while (simulatedProgress < 1f)
        {
            simulatedProgress += fakeLoadSpeed * Time.deltaTime;
            progressBar.value = simulatedProgress;
            progressText.text = $"{Mathf.RoundToInt(simulatedProgress * 100)}%";
            yield return null;
        }

        // 确保进度条增长完成
        progressBar.value = 1f;
        progressText.text = "100%";

        // 延迟片刻，切换到下一个场景
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }
}
